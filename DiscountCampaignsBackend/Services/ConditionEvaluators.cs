using System;
using System.Linq;
using Newtonsoft.Json;


public class DateRangeConditionEvaluator : IConditionEvaluator
{
    public string ConditionType => "DATE_RANGE";
    public bool IsValid(DiscountRequestDto request, string conditionJson)
    {
        var merged = JsonTemplateMerger.Merge(conditionJson, request.UserInput);
        dynamic obj = JsonConvert.DeserializeObject(merged);
        if (obj == null) return true;
        DateTime start = DateTime.Parse((string)(obj.start ?? DateTime.MinValue.ToString()));
        DateTime end = DateTime.Parse((string)(obj.end ?? DateTime.MaxValue.ToString()));
        var today = DateTime.UtcNow.Date;
        return today >= start.Date && today <= end.Date;
    }
}

public class DayOfWeekConditionEvaluator : IConditionEvaluator
{
    public string ConditionType => "DAY_OF_WEEK";
    public bool IsValid(DiscountRequestDto request, string conditionJson)
    {
        var merged = JsonTemplateMerger.Merge(conditionJson, request.UserInput);
        dynamic obj = JsonConvert.DeserializeObject(merged);
        if (obj == null) return true;
        var days = ((Newtonsoft.Json.Linq.JArray)obj.days).Select(x => (int)x).ToArray();
        int today = (int)DateTime.UtcNow.DayOfWeek;
        return days.Contains(today);
    }
}

public class AnnualDateConditionEvaluator : IConditionEvaluator
{
    public string ConditionType => "ANNUAL_DATE";
    public bool IsValid(DiscountRequestDto request, string conditionJson)
    {
        var merged = JsonTemplateMerger.Merge(conditionJson, request.UserInput);
        dynamic obj = JsonConvert.DeserializeObject(merged);
        if (obj == null) return true;
        int month = (int)(obj.month ?? 0);
        int day = (int)(obj.day ?? 0);
        var today = DateTime.UtcNow;
        return today.Month == month && today.Day == day;
    }
}

public class CategoryInCartConditionEvaluator : IConditionEvaluator
{
    public string ConditionType => "CATEGORY_IN_CART";
    public bool IsValid(DiscountRequestDto request, string conditionJson)
    {
        var merged = JsonTemplateMerger.Merge(conditionJson, request.UserInput);
        dynamic obj = JsonConvert.DeserializeObject(merged);
        if (obj == null) return true;
        string category = (string)(obj.category ?? "");
        return request.SelectedProduct.Any(p => string.Equals(p.Product.Category, category, StringComparison.OrdinalIgnoreCase));
    }
}

