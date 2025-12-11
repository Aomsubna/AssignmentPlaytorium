using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;

namespace DiscountCampaignsBackend.Services
{
    public class DiscountCalculator
    {
        private readonly IEnumerable<IDiscountCategory> _discountSteps;
        private readonly IEnumerable<IRuleEvaluator> _ruleEvaluators;
        private readonly IEnumerable<IConditionEvaluator> _conditionEvaluators;

        public DiscountCalculator(IEnumerable<IDiscountCategory> discountSteps, IEnumerable<IRuleEvaluator> ruleEvaluators, IEnumerable<IConditionEvaluator> conditionEvaluators)
        {
            _discountSteps = discountSteps;
            _ruleEvaluators = ruleEvaluators;
            _conditionEvaluators = conditionEvaluators;
        }

        public decimal CalculateTotal(DiscountRequestDto request)
        {
            decimal originalTotal = request.SelectedProduct.Sum(p => p.Product.Price * p.Quantity);
            decimal currentTotal = originalTotal;

            foreach (var step in _discountSteps)
            {
                currentTotal = step.Apply(currentTotal, request);
            }

            return currentTotal;
        }

        public (decimal finalTotal, List<(string promoCode, decimal amount)> applied) Calculate(DiscountRequestDto req)
        {
            // Starting total = sum of line items
            decimal originalTotal = req.SelectedProduct.Sum(i => i.Product.Price * i.Quantity);

            // maintain per-line current subtotals so we can allocate fixed discounts proportionally
            var lineSubtotals = req.SelectedProduct
                .Select(p => (Item: p, Subtotal: p.Product.Price * p.Quantity))
                .ToList();

            decimal total = lineSubtotals.Sum(x => x.Subtotal);
            var applied = new List<(string promoCode, decimal amount)>();

            // choose promotions: if a SelectedCampaignCode provided, filter to that, else use all active in FakeDatabase + date filter
            var promos = Mockdatabase.Promotions
                .Where(p => IsActiveNow(p))
                .OrderBy(p => p.Priority)
                .ToList();

            // Check all campaign selections (Coupon, OnTop, Seasonal)
            var selectedCampaignNames = new List<string>();

            var couponName = req.CampaignsSelected?.SelectedCampaignCoupon?.CampaignName;
            if (!string.IsNullOrWhiteSpace(couponName))
                selectedCampaignNames.Add(couponName);

            var onTopName = req.CampaignsSelected?.SelectedCampaignOnTop?.CampaignName;
            if (!string.IsNullOrWhiteSpace(onTopName))
                selectedCampaignNames.Add(onTopName);

            var seasonalName = req.CampaignsSelected?.SelectedCampaignSeasonal?.CampaignName;
            if (!string.IsNullOrWhiteSpace(seasonalName))
                selectedCampaignNames.Add(seasonalName);

            // If any campaigns selected, filter promotions to only those
            if (selectedCampaignNames.Any())
            {
                var selectedCodes = new List<string>();
                foreach (var name in selectedCampaignNames)
                {
                    if (Mockdatabase.CampaignNameToPromoCode.TryGetValue(name, out var promoCode))
                    {
                        selectedCodes.Add(promoCode);
                    }
                }

                if (selectedCodes.Any())
                {
                    promos = promos.Where(p => selectedCodes.Contains(p.Code, StringComparer.OrdinalIgnoreCase)).ToList();
                }
                else
                {
                    promos = new List<Promotion>();
                }
            }

            foreach (var promo in promos)
            {
                // Condition checks
                bool conditionsOk = true;
                foreach (var cond in promo.Conditions ?? Enumerable.Empty<PromotionCondition>())
                {
                    var evaluator = _conditionEvaluators.FirstOrDefault(e => e.ConditionType == cond.Type);
                    if (evaluator == null) { conditionsOk = false; break; }
                    if (!evaluator.IsValid(req, cond.ConditionJson)) { conditionsOk = false; break; }
                }
                if (!conditionsOk) continue;

                // Check stack/exclusive rules: simple handling
                if (promo.StackMode == "EXCLUSIVE_ALL" && applied.Any()) continue;
                if (promo.StackMode == "EXCLUSIVE" && applied.Any(a => Mockdatabase.Promotions.FirstOrDefault(p => p.Code == a.promoCode)?.Category == promo.Category)) continue;

                // Apply each rule sequentially. If the promo has a CATEGORY_IN_CART condition,
                // apply the rule only to the subtotal of items in that category and deduct
                // the resulting discount from the overall total.
                string? categoryCondition = null;
                var catCond = (promo.Conditions ?? Enumerable.Empty<PromotionCondition>()).FirstOrDefault(c => string.Equals(c.Type, "CATEGORY_IN_CART", StringComparison.OrdinalIgnoreCase));
                if (catCond != null)
                {
                    try
                    {
                        var merged = JsonTemplateMerger.Merge(catCond.ConditionJson, req.UserInput);
                        var jobj = JsonConvert.DeserializeObject(merged) as Newtonsoft.Json.Linq.JObject;
                        categoryCondition = jobj?["category"]?.ToString();
                    }
                    catch
                    {
                        categoryCondition = null;
                    }
                }

                foreach (var rule in promo.Rules)
                {
                    var evaluator = _ruleEvaluators.FirstOrDefault(r => r.RuleType == rule.Type);
                    if (evaluator == null) continue;

                    if (!string.IsNullOrWhiteSpace(categoryCondition))
                    {
                        // compute subtotal for the matching category
                        decimal categorySubtotal = lineSubtotals
                            .Where(ls => string.Equals(ls.Item.Product.Category, categoryCondition, StringComparison.OrdinalIgnoreCase))
                            .Sum(ls => ls.Subtotal);

                        if (categorySubtotal <= 0) continue;

                        // apply evaluator to the category subtotal and compute discount amount
                        var afterCategory = evaluator.Apply(categorySubtotal, req, rule.RuleJson);
                        decimal discountAmount = Math.Max(0, categorySubtotal - afterCategory);
                        if (discountAmount > 0)
                        {
                            // allocate discount across items in that category proportionally
                            var indices = lineSubtotals
                                .Select((ls, idx) => (ls, idx))
                                .Where(t => string.Equals(t.ls.Item.Product.Category, categoryCondition, StringComparison.OrdinalIgnoreCase))
                                .ToList();
                            decimal allocated = 0;
                            for (int k = 0; k < indices.Count; k++)
                            {
                                var (ls, idx) = indices[k];
                                decimal share = (categorySubtotal > 0) ? ls.Subtotal / categorySubtotal : 0m;
                                decimal part = Math.Round(discountAmount * share, 2);
                                // last item takes the remainder to avoid rounding loss
                                if (k == indices.Count - 1) part = discountAmount - allocated;
                                allocated += part;
                                lineSubtotals[idx] = (ls.Item, Math.Max(0, ls.Subtotal - part));
                            }
                            applied.Add((promo.Code, allocated));
                            total = lineSubtotals.Sum(x => x.Subtotal);
                        }
                    }
                    else
                    {
                        // fallback: apply against whole total. If rule JSON contains an explicit amount
                        // (fixed discount), allocate it proportionally across line items.
                        decimal currentTotal = lineSubtotals.Sum(x => x.Subtotal);

                        // try to detect fixed amount in rule JSON
                        decimal? fixedAmount = null;
                        try
                        {
                            var rj = !string.IsNullOrWhiteSpace(rule.RuleJson) ? Newtonsoft.Json.Linq.JObject.Parse(rule.RuleJson) : null;
                            if (rj != null && rj["amount"] != null)
                            {
                                var token = rj["amount"];
                                decimal parsed;
                                if (token != null && decimal.TryParse(token.ToString(), out parsed)) fixedAmount = parsed;
                            }
                        }
                        catch
                        {
                            fixedAmount = null;
                        }

                        if (fixedAmount.HasValue)
                        {
                            var discountAmount = Math.Min(fixedAmount.Value, currentTotal);
                            if (discountAmount <= 0) continue;
                            // allocate across all lines proportionally
                            decimal allocated = 0;
                            for (int k = 0; k < lineSubtotals.Count; k++)
                            {
                                var ls = lineSubtotals[k];
                                decimal share = (currentTotal > 0) ? ls.Subtotal / currentTotal : 0m;
                                decimal part = Math.Round(discountAmount * share, 2);
                                if (k == lineSubtotals.Count - 1) part = discountAmount - allocated;
                                allocated += part;
                                lineSubtotals[k] = (ls.Item, Math.Max(0, ls.Subtotal - part));
                            }
                            applied.Add((promo.Code, allocated));
                            total = lineSubtotals.Sum(x => x.Subtotal);
                        }
                        else
                        {
                            var after = evaluator.Apply(currentTotal, req, rule.RuleJson);
                            decimal discountAmount = Math.Max(0, currentTotal - after);
                            if (discountAmount > 0)
                            {
                                // distribute proportionally across lines
                                decimal allocated = 0;
                                for (int k = 0; k < lineSubtotals.Count; k++)
                                {
                                    var ls = lineSubtotals[k];
                                    decimal share = (currentTotal > 0) ? ls.Subtotal / currentTotal : 0m;
                                    decimal part = Math.Round(discountAmount * share, 2);
                                    if (k == lineSubtotals.Count - 1) part = discountAmount - allocated;
                                    allocated += part;
                                    lineSubtotals[k] = (ls.Item, Math.Max(0, ls.Subtotal - part));
                                }
                                applied.Add((promo.Code, allocated));
                                total = lineSubtotals.Sum(x => x.Subtotal);
                            }
                        }
                    }
                }
            }

            return (total, applied: applied);
        }

        private bool IsActiveNow(Promotion p)
        {
            var today = DateTime.UtcNow.Date;
            if (p.StartAt.HasValue && p.StartAt.Value.Date > today) return false;
            if (p.EndAt.HasValue && p.EndAt.Value.Date < today) return false;
            return true;
        }
    }
}
