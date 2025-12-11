public class PromotionTypeTemplate
{
    public string Code { get; set; }       // e.g. "TIER_PERCENT"
    public string Name { get; set; }
    public string SchemaJson { get; set; } // form schema
}

public class Promotion
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Code { get; set; }
    public string Name { get; set; }

    public string TypeCode { get; set; }      // e.g. "TIER_PERCENT"
    public string Category { get; set; }      // COUPON / ON_TOP / SEASONAL / POINT
    public string StackMode { get; set; }     // STACKABLE / EXCLUSIVE / EXCLUSIVE_ALL
    public int Priority { get; set; }

    public DateTime? StartAt { get; set; }
    public DateTime? EndAt { get; set; }

    public List<PromotionCondition> Conditions { get; set; } = new();
    public List<PromotionRule> Rules { get; set; } = new();
}

public class PromotionCondition
{
    public string Type { get; set; }          // "DATE_RANGE", "DAY_OF_WEEK", etc.
    public string ConditionJson { get; set; } // JSON payload
}

public class PromotionRule
{
    public string Type { get; set; }          // "PERCENT", "TIER", etc.
    public string RuleJson { get; set; }
}
