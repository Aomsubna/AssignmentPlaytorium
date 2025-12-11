public static class JsonTemplateMerger
{
    // Simple placeholder replacement: "{{input.key}}"
    // Handles context-aware escaping: if placeholder is inside quotes, only substitute the value
    // If placeholder is bare, wrap strings in JSON quotes
    public static string Merge(string jsonTemplate, Dictionary<string, object>? inputs)
    {
        if (string.IsNullOrWhiteSpace(jsonTemplate) || inputs == null || inputs.Count == 0)
            return jsonTemplate ?? string.Empty;

        var merged = jsonTemplate;
        foreach (var kv in inputs)
        {
            var placeholder = "{{input." + kv.Key + "}}";

            // Get JSON-safe value
            string jsonVal = ConvertToJsonValue(kv.Value);

            // Check if placeholder is inside quotes: "{{input.key}}"
            var quotedPattern = "\"" + placeholder + "\"";
            if (merged.Contains(quotedPattern))
            {
                // Inside quotes: just use the raw string value without adding quotes
                string rawVal = kv.Value?.ToString() ?? "";
                merged = merged.Replace(quotedPattern, "\"" + rawVal + "\"");
            }
            else
            {
                // Bare: use full JSON value
                merged = merged.Replace(placeholder, jsonVal);
            }
        }
        return merged;
    }

    private static string ConvertToJsonValue(object? value)
    {
        if (value == null)
            return "null";

        if (value is bool boolVal)
            return boolVal.ToString().ToLower();

        if (value is int || value is long || value is decimal || value is double || value is float)
            return value.ToString() ?? "0";

        // String: escape and quote
        if (value is string strVal)
            return "\"" + strVal.Replace("\"", "\\\"") + "\"";

        // Fallback
        return "\"" + value.ToString()?.Replace("\"", "\\\"") + "\"";
    }
}