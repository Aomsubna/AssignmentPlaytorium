using System;
using System.Globalization;
using System.Linq;

namespace DiscountCampaignsBackend.Services
{
    public class CouponDiscount : IDiscountCategory
    {
        public decimal Apply(decimal currentTotal, DiscountRequestDto dto)
        {
            if (dto.CampaignsSelected.SelectedCampaignCoupon == null)
                return currentTotal;

            var campaignName = dto.CampaignsSelected.SelectedCampaignCoupon.CampaignName;
            var discountStr = dto.CampaignsSelected.DiscountCoupon ?? "0";

            decimal.TryParse(discountStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var discount);

            if (campaignName == "Fix amount")
            {
                return Math.Max(0, currentTotal - discount);
            }
            else if (campaignName == "Percentage discount")
            {
                return Math.Max(0, currentTotal * (1 - (discount / 100)));
            }
            return currentTotal;
        }
    }

    public class OnTopDiscount : IDiscountCategory
    {
        public decimal Apply(decimal currentTotal, DiscountRequestDto dto)
        {
            if (dto.CampaignsSelected.SelectedCampaignOnTop == null)
                return currentTotal;

            var campaignName = dto.CampaignsSelected.SelectedCampaignOnTop.CampaignName;
            var discountStr = dto.CampaignsSelected.DiscountOnTopValue ?? "0";
            decimal.TryParse(discountStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var discountValue);
            if (campaignName == "Discount by points")
            {
                var originalPrice = dto.SelectedProduct.Sum(p => p.Product.Price * p.Quantity);
                var maxPointDiscount = originalPrice * (2m / 10);
                var finalDiscount = discountValue >= maxPointDiscount ? maxPointDiscount : discountValue;
                return Math.Max(0, currentTotal - finalDiscount);
            }
            else if (campaignName == "Percentage discount by item category")
            {
                var discountStrClassify = dto.CampaignsSelected.DiscountOnTopClassify;
                var productByCategory = dto.SelectedProduct.Where(i => i.Product.Category == discountStrClassify);
                var originalPrice = dto.SelectedProduct.Sum(p => p.Product.Price * p.Quantity);
                var discountedBefore = originalPrice - currentTotal;
                var discountedProducts = productByCategory
                                            .Select(i => new
                                            {
                                                Product = i.Product,
                                                Quantity = i.Quantity,
                                                PriceAfterDiscount = i.Product.Price * i.Quantity * (1 - (discountedBefore / originalPrice))
                                            })
                                            .ToList();
                var totalDiscountAmount = discountedProducts
                                            .Select(i => (i.PriceAfterDiscount) * (discountValue / 100))
                                            .Sum();
                return Math.Max(0, currentTotal - totalDiscountAmount);
            }
            // var classify = dto.CampaignsSelected.DiscountOnTopClassify?.ToLower();
            // var discountStr = dto.CampaignsSelected.DiscountOnTopValue ?? "0";
            // decimal.TryParse(discountStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var discountValue);

            // var subtotal = dto.SelectedProduct.Sum(p => p.Product.Price * p.Quantity);

            // if (classify == "percentage")
            //     return Math.Max(0, subtotal * (1 - discountValue / 100));

            return currentTotal; // fallback as fix amount
        }
    }

    public class SeasonalDiscount : IDiscountCategory
    {
        public decimal Apply(decimal currentTotal, DiscountRequestDto dto)
        {
            if (dto.CampaignsSelected.SelectedCampaignSeasonal == null)
                return currentTotal;

            var campaignName = dto.CampaignsSelected.SelectedCampaignSeasonal.CampaignName;

            var classify = dto.CampaignsSelected.DiscountSeasonalClassify ?? "0";
            var discountStr = dto.CampaignsSelected.DiscountSeasonalValue ?? "0";
            decimal.TryParse(classify, NumberStyles.Any, CultureInfo.InvariantCulture, out var classifyValue);
            decimal.TryParse(discountStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var discountValue);

            if (campaignName == "Special campaigns")
            {
                var numberMultiply = Math.Floor(currentTotal / classifyValue);
                return Math.Max(0, currentTotal - (discountValue * numberMultiply));
            }

            return currentTotal;
        }
    }
}