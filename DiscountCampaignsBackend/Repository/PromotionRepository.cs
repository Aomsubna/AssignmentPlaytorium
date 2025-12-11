public interface IPromotionRepository
{
    Task<List<Promotion>> GetActiveAsync();
    Task<Promotion?> GetByCodeAsync(string code);
    Task<List<PromotionTypeTemplate>> GetTemplatesAsync();
}

public class PromotionRepository : IPromotionRepository
{
    public Task<List<Promotion>> GetActiveAsync()
    {
        // filter เฉพาะโปรที่อยู่ในช่วงวัน หรือเป็น always active
        var today = DateTime.UtcNow.Date;

        var result = Mockdatabase.Promotions
            .Where(p =>
                (p.StartAt == null || p.StartAt.Value.Date <= today) &&
                (p.EndAt == null || p.EndAt.Value.Date >= today)
            ).ToList();

        return Task.FromResult(result);
    }

    public Task<Promotion?> GetByCodeAsync(string code)
    {
        var promo = Mockdatabase.Promotions.FirstOrDefault(p => p.Code == code);
        return Task.FromResult(promo);
    }

    public Task<List<PromotionTypeTemplate>> GetTemplatesAsync()
    {
        return Task.FromResult(Mockdatabase.PromotionTemplates);
    }
}
