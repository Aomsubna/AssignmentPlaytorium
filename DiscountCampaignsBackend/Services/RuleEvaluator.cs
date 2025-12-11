using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;


// PERCENT
public class PercentRuleEvaluator : IRuleEvaluator
{
    public string RuleType => "PERCENT";
    public decimal Apply(decimal currentTotal, DiscountRequestDto request, string ruleJson)
    {
        var merged = JsonTemplateMerger.Merge(ruleJson, request.UserInput);
        dynamic obj = JsonConvert.DeserializeObject(merged);
        decimal percent = 0;
        if ((obj?.percent) != null)
            percent = (decimal)obj.percent;
        var discount = currentTotal * (percent / 100m);
        return Math.Max(0, currentTotal - discount);
    }
}

// FIXED
public class FixedRuleEvaluator : IRuleEvaluator
{
    public string RuleType => "FIXED";
    public decimal Apply(decimal currentTotal, DiscountRequestDto request, string ruleJson)
    {
        var merged = JsonTemplateMerger.Merge(ruleJson, request.UserInput);
        dynamic obj = JsonConvert.DeserializeObject(merged);
        decimal amount = 0;
        if ((obj?.amount) != null)
            amount = (decimal)obj.amount;
        return Math.Max(0, currentTotal - amount);
    }
}

// TIER
public class TierRuleEvaluator : IRuleEvaluator
{
    public string RuleType => "TIER";
    public decimal Apply(decimal currentTotal, DiscountRequestDto request, string ruleJson)
    {
        var merged = JsonTemplateMerger.Merge(ruleJson, request.UserInput);
        var tiers = JsonConvert.DeserializeObject<List<TierItem>>(merged) ?? new List<TierItem>();
        foreach (var t in tiers.OrderBy(x => x.Min))
        {
            if (currentTotal >= t.Min && currentTotal <= t.Max)
            {
                var discount = currentTotal * (t.Percent / 100m);
                return Math.Max(0, currentTotal - discount);
            }
        }
        return currentTotal;
    }

    private class TierItem { public decimal Min { get; set; } public decimal Max { get; set; } public decimal Percent { get; set; } }
}

// STEP_DISCOUNT
public class StepDiscountEvaluator : IRuleEvaluator
{
    public string RuleType => "STEP_DISCOUNT";
    public decimal Apply(decimal currentTotal, DiscountRequestDto request, string ruleJson)
    {
        var merged = JsonTemplateMerger.Merge(ruleJson, request.UserInput);
        dynamic obj = JsonConvert.DeserializeObject(merged);
        decimal step = 0;
        decimal disc = 0;
        if ((obj?.step) != null) step = (decimal)obj.step;
        if ((obj?.discount) != null) disc = (decimal)obj.discount;
        if (step <= 0) return currentTotal;
        var times = Math.Floor(currentTotal / step);
        var totalDiscount = times * disc;
        return Math.Max(0, currentTotal - totalDiscount);
    }
}

// POINT
public class PointRuleEvaluator : IRuleEvaluator
{
    public string RuleType => "POINT";
    public decimal Apply(decimal currentTotal, DiscountRequestDto request, string ruleJson)
    {
        var merged = JsonTemplateMerger.Merge(ruleJson, request.UserInput);
        dynamic obj = JsonConvert.DeserializeObject(merged);
        decimal pointValue = 1m;
        decimal maxPercent = 100m;
        if ((obj?.pointValue) != null) pointValue = (decimal)obj.pointValue;
        if ((obj?.maxPercent) != null) maxPercent = (decimal)obj.maxPercent;

        // read user provided points (from UserInput dict or campaignsSelected)
        decimal usedPoints = 0;
        if (request.UserInput.TryGetValue("points", out var v))
        {
            decimal.TryParse(v.ToString(), out usedPoints);
        }

        decimal desired = usedPoints * pointValue;
        decimal cap = currentTotal * (maxPercent / 100m);
        decimal final = Math.Min(desired, cap);
        return Math.Max(0, currentTotal - final);
    }
}

// BUY_X_GET_Y
public class BuyXGetYRuleEvaluator : IRuleEvaluator
{
    public string RuleType => "BUY_X_GET_Y";
    public decimal Apply(decimal currentTotal, DiscountRequestDto request, string ruleJson)
    {
        var merged = JsonTemplateMerger.Merge(ruleJson, request.UserInput);
        dynamic obj = JsonConvert.DeserializeObject(merged);
        int buyQty = 0;
        int getQty = 0;
        string sku = "";
        if ((obj?.buyQty) != null) buyQty = (int)obj.buyQty;
        if ((obj?.getQty) != null) getQty = (int)obj.getQty;
        if ((obj?.sku) != null) sku = (string)obj.sku;

        if (buyQty <= 0 || getQty <= 0 || string.IsNullOrWhiteSpace(sku))
            return currentTotal;

        var items = request.SelectedProduct.Where(x => x.Product.Sku == sku).ToList();
        if (!items.Any()) return currentTotal;

        int totalQty = items.Sum(i => i.Quantity);
        // number of free groups = totalQty / (buyQty + getQty)
        int groupSize = buyQty + getQty;
        if (groupSize <= 0) return currentTotal;

        int groups = totalQty / groupSize;
        int freeItems = groups * getQty;

        if (freeItems <= 0) return currentTotal;

        decimal unitPrice = items.First().Product.Price;
        decimal discount = freeItems * unitPrice;
        return Math.Max(0, currentTotal - discount);
    }
}

