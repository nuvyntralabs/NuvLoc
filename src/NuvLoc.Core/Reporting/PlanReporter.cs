using System.Text;
using System.Text.Json;
using NuvLoc.Diff;

namespace NuvLoc.Reporting;

public static class PlanReporter
{
    public static string Human(LocalizationPlan plan)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Source: {plan.Source}");
        foreach (var language in plan.Languages)
        {
            sb.AppendLine();
            sb.AppendLine($"{language.Language}: {language.Current} current, {language.Missing.Count} missing, {language.Stale.Count} stale, {language.PlaceholderBroken.Count} placeholder, {language.Extra.Count} extra");
            foreach (var item in language.Missing)
                sb.AppendLine($"  missing   {item.Key}");
            foreach (var item in language.Stale)
                sb.AppendLine($"  stale     {item.Key}");
            foreach (var item in language.PlaceholderBroken)
                sb.AppendLine($"  placeholder {item.Key}");
            foreach (var extra in language.Extra)
                sb.AppendLine($"  extra     {extra}");
        }

        sb.AppendLine();
        sb.AppendLine(Disclaimer.Text);
        return sb.ToString();
    }

    public static string Json(LocalizationPlan plan) =>
        JsonSerializer.Serialize(ToDto(plan), new JsonSerializerOptions { WriteIndented = true });

    public static object ToDto(LocalizationPlan plan) =>
        new
        {
            source = plan.Source,
            disclaimer = Disclaimer.Text,
            languages = plan.Languages.ToDictionary(
                l => l.Language,
                l => new
                {
                    target = l.TargetPath,
                    current = l.Current,
                    missing = l.Missing.Select(Item).ToArray(),
                    stale = l.Stale.Select(Item).ToArray(),
                    placeholderBroken = l.PlaceholderBroken.Select(Item).ToArray(),
                    extra = l.Extra.ToArray(),
                },
                StringComparer.OrdinalIgnoreCase),
        };

    static object Item(WorkItem item) =>
        new
        {
            key = item.Key,
            value = item.Value,
            comment = item.Comment ?? "",
            previousSource = item.PreviousSource,
        };
}
