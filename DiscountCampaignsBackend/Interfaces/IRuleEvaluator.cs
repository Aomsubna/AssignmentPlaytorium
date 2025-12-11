public interface IRuleEvaluator
{
    string RuleType { get; }
    decimal Apply(decimal currentTotal, DiscountRequestDto request, string ruleJson);
}