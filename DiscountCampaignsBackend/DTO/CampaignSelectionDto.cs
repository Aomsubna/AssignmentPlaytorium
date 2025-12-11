public class CampaignSelectionDto
{
    public CampaignDto? SelectedCampaignCoupon { get; set; }
    public string? DiscountCoupon { get; set; }  // รองรับ string หรือ number

    public CampaignDto? SelectedCampaignOnTop { get; set; }
    public string? DiscountOnTopClassify { get; set; }
    public string? DiscountOnTopValue { get; set; }  // รองรับ string หรือ number

    public CampaignDto? SelectedCampaignSeasonal { get; set; }
    public string? DiscountSeasonalClassify { get; set; }  // รองรับ string หรือ number
    public string? DiscountSeasonalValue { get; set; }     // รองรับ string หรือ number
}