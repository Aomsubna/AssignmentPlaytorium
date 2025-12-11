public interface IConditionEvaluator
{
    string ConditionType { get; }
    bool IsValid(DiscountRequestDto request, string conditionJson);
}