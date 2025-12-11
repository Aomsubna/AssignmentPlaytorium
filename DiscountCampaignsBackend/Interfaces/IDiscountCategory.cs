public interface IDiscountCategory
{
    decimal Apply(decimal currentTotal, DiscountRequestDto discountRequestDto);
}
