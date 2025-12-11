public class DiscountRequestDto
{
    public CampaignSelectionDto? CampaignsSelected { get; set; }
    public SelectedProductDto[] SelectedProduct { get; set; }
    public Dictionary<string, object> UserInput { get; set; } = new();
}