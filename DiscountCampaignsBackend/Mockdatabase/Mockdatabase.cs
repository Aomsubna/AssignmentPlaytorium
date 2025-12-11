public static class Mockdatabase
{
    // =======================================================================
    // 1) Promotion Template (Schema สำหรับหน้า Admin UI)
    // =======================================================================
    public static List<PromotionTypeTemplate> PromotionTemplates = new()
    {
        new PromotionTypeTemplate {
            Code = "PERCENT",
            Name = "Percent Discount",
            SchemaJson = @"{
                ""properties"": {
                    ""percent"": {""type"":""number"", ""minimum"":1, ""maximum"":100},
                    ""maxDiscount"": {""type"":""number""},
                    ""minSpend"": {""type"":""number""}
                }
            }"
        },

        new PromotionTypeTemplate {
            Code = "FIXED",
            Name = "Fixed Discount",
            SchemaJson = @"{
                ""properties"": {
                    ""amount"": {""type"":""number"", ""minimum"":1},
                    ""minSpend"": {""type"":""number""}
                }
            }"
        },

        new PromotionTypeTemplate {
            Code = "TIER_PERCENT",
            Name = "Tier % Discount",
            SchemaJson = @"{
                ""properties"": {
                    ""tiers"": {
                        ""type"":""array"",
                        ""items"": {
                            ""type"":""object"",
                            ""properties"": {
                                ""min"":{""type"":""number""},
                                ""max"":{""type"":""number""},
                                ""percent"":{""type"":""number""}
                            }
                        }
                    }
                }
            }"
        },

        new PromotionTypeTemplate {
            Code = "STEP_DISCOUNT",
            Name = "Step Discount",
            SchemaJson = @"{
                ""properties"": {
                    ""step"": {""type"":""number""},
                    ""discount"": {""type"":""number""}
                }
            }"
        },

        new PromotionTypeTemplate {
            Code = "POINT_DISCOUNT",
            Name = "Point Discount",
            SchemaJson = @"{
                ""properties"": {
                    ""pointValue"": {""type"":""number""},
                    ""maxPercent"": {""type"":""number""}
                }
            }"
        },

        new PromotionTypeTemplate {
            Code = "BUY_X_GET_Y",
            Name = "Buy X Get Y",
            SchemaJson = @"{
                ""properties"": {
                    ""buyQty"": {""type"":""number""},
                    ""getQty"": {""type"":""number""},
                    ""sku"": {""type"":""string""}
                }
            }"
        }
    };



    // =======================================================================
    // 2) Campaign Name to Promotion Code Mapping (supports all campaign types)
    // =======================================================================
    public static Dictionary<string, string> CampaignNameToPromoCode = new(StringComparer.OrdinalIgnoreCase)
    {
        // Coupon campaigns
        { "Fix amount", "COUPON100" },
        { "Percentage discount", "COUPON10" },
        // On Top campaigns
        { "Percentage discount by item category", "ONTOP-ACC-12" },
        { "Discount by points", "POINT" },
        // Seasonal campaigns
        { "Special campaigns", "STEP300-40" }
    };

    // =======================================================================
    // 3) Promotions (ALL promotions we talked about)
    // =======================================================================
    public static List<Promotion> Promotions = new()
    {

        // ------------------------------------------------------------
        // Assignment: Coupon 10% (Exclusive) - uses user input percent
        // ------------------------------------------------------------
        new Promotion
        {
            Id = Guid.NewGuid(),
            Code = "COUPON10",
            Name = "10% Coupon",
            TypeCode = "PERCENT",
            Category = "COUPON",
            StackMode = "EXCLUSIVE",
            Priority = 1,
            Rules = new()
            {
                new PromotionRule
                {
                    Type = "PERCENT",
                    RuleJson = @"{ ""percent"": {{input.percentcoupon}} }"
                }
            }
        },

        // ------------------------------------------------------------
        // Assignment: Coupon 100 THB (Exclusive) - uses user input amount
        // ------------------------------------------------------------
        new Promotion
        {
            Id = Guid.NewGuid(),
            Code = "COUPON100",
            Name = "100 THB Coupon",
            TypeCode = "FIXED",
            Category = "COUPON",
            StackMode = "EXCLUSIVE",
            Priority = 2,
            Rules = new()
            {
                new PromotionRule
                {
                    Type = "FIXED",
                    RuleJson = @"{ ""amount"": {{input.amount}} }"
                }
            }
        },

        // ------------------------------------------------------------
        // On-Top Discount Category (Clothing 20%)
        // ------------------------------------------------------------
        new Promotion
        {
            Id = Guid.NewGuid(),
            Code = "ONTOP-ACC-12",
            Name = "Accessories 12% On-Top",
            TypeCode = "PERCENT",
            Category = "ON_TOP",
            StackMode = "STACKABLE",
            Priority = 5,
            Conditions = new()
            {
                new PromotionCondition
                {
                    Type = "CATEGORY_IN_CART",
                    ConditionJson = @"{ ""category"": ""{{input.category}}"" }"
                }
            },
            Rules = new()
            {
                new PromotionRule
                {
                    Type = "PERCENT",
                    RuleJson = @"{ ""percent"": {{input.percent}} }"
                }
            }
        },

        // ------------------------------------------------------------
        // Seasonal: Step Discount (Every 100 → -20)
        // ------------------------------------------------------------
        new Promotion
        {
            Id = Guid.NewGuid(),
            Code = "STEP300-40",
            Name = "Step Discount 300-40",
            TypeCode = "STEP_DISCOUNT",
            Category = "SEASONAL",
            StackMode = "STACKABLE",
            Priority = 10,
            Rules = new()
            {
                new PromotionRule
                {
                    Type = "STEP_DISCOUNT",
                    RuleJson = @"{ ""step"": {{input.step}}, ""discount"": {{input.discount}} }"
                }
            }
        },

        // ------------------------------------------------------------
        // Tier Discount: 500–2000 = 10%, 2001–2500 = 25%
        // ------------------------------------------------------------
        new Promotion
        {
            Id = Guid.NewGuid(),
            Code = "TIER-PRICE",
            Name = "Tier Price Discount",
            TypeCode = "TIER_PERCENT",
            Category = "ON_TOP",
            StackMode = "STACKABLE",
            Priority = 15,
            Rules = new()
            {
                new PromotionRule
                {
                    Type = "TIER",
                    RuleJson = @"[
                        { ""min"":500, ""max"":2000, ""percent"":10 },
                        { ""min"":2001, ""max"":2500, ""percent"":25 }
                    ]"
                }
            }
        },

        // ------------------------------------------------------------
        // Point Discount: points from user input, cap = 20%
        // The evaluator reads points from userInput.points
        new Promotion
        {
            Id = Guid.NewGuid(),
            Code = "POINT",
            Name = "Point Discount",
            TypeCode = "POINT_DISCOUNT",
            Category = "POINT",
            StackMode = "STACKABLE",
            Priority = 20,
            Rules = new()
            {
                new PromotionRule
                {
                    Type = "POINT",
                    RuleJson = @"{ ""pointValue"": {{input.pointValue}}, ""maxPercent"": {{input.maxPercent}} }"
                }
            }
        },

        // ------------------------------------------------------------
        // Date-based: 12.12 Sale 15%
        // ------------------------------------------------------------
        new Promotion
        {
            Id = Guid.NewGuid(),
            Code = "12-12",
            Name = "12.12 Mega Sale",
            TypeCode = "PERCENT",
            Category = "SEASONAL",
            StackMode = "STACKABLE",
            Priority = 30,
            Conditions = new()
            {
                new PromotionCondition
                {
                    Type = "DATE_RANGE",
                    ConditionJson = @"{
                        ""start"": ""2025-12-12"",
                        ""end"": ""2025-12-12""
                    }"
                }
            },
            Rules = new()
            {
                new PromotionRule
                {
                    Type = "PERCENT",
                    RuleJson = @"{ ""percent"":15 }"
                }
            }
        },

        // ------------------------------------------------------------
        // Date-based: Wednesday 10%
        // ------------------------------------------------------------
        new Promotion
        {
            Id = Guid.NewGuid(),
            Code = "WED-10",
            Name = "Wednesday Discount 10%",
            TypeCode = "PERCENT",
            Category = "SEASONAL",
            StackMode = "STACKABLE",
            Priority = 40,
            Conditions = new()
            {
                new PromotionCondition
                {
                    Type = "DAY_OF_WEEK",
                    ConditionJson = @"{ ""days"":[3] }"
                }
            },
            Rules = new()
            {
                new PromotionRule
                {
                    Type = "PERCENT",
                    RuleJson = @"{ ""percent"":10 }"
                }
            }
        },

        // ------------------------------------------------------------
        // Event-based: Mother's Day 20%
        // ------------------------------------------------------------
        new Promotion
        {
            Id = Guid.NewGuid(),
            Code = "MOM-20",
            Name = "Mother's Day 20%",
            TypeCode = "PERCENT",
            Category = "EVENT",
            StackMode = "EXCLUSIVE_ALL",
            Priority = 50,
            Conditions = new()
            {
                new PromotionCondition
                {
                    Type = "ANNUAL_DATE",
                    ConditionJson = @"{ ""month"":8, ""day"":12 }"
                }
            },
            Rules = new()
            {
                new PromotionRule
                {
                    Type = "PERCENT",
                    RuleJson = @"{ ""percent"":20 }"
                }
            }
        },

        // ------------------------------------------------------------
        // Mix & Match: Buy 2 Get 1 (SKU)
        // ------------------------------------------------------------
        new Promotion
        {
            Id = Guid.NewGuid(),
            Code = "BXGY-SKU",
            Name = "Buy 2 Get 1 Free",
            TypeCode = "BUY_X_GET_Y",
            Category = "ON_TOP",
            StackMode = "STACKABLE",
            Priority = 60,
            Rules = new()
            {
                new PromotionRule
                {
                    Type = "BUY_X_GET_Y",
                    RuleJson = @"{
                        ""buyQty"":2,
                        ""getQty"":1,
                        ""sku"":""SKU-ABC""
                    }"
                }
            }
        }
    };
}
