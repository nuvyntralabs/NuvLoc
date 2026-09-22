using NuvLoc.Config;
using NuvLoc.Resources;

namespace NuvLoc.Diff;

public sealed class LocalizationPlanner
{
    readonly IResourceAdapter _adapter;

    public LocalizationPlanner(IResourceAdapter? adapter = null) =>
        _adapter = adapter ?? new ResxAdapter();

    public LocalizationPlan Build(I18nConfig config, IEnumerable<string>? languages = null)
    {
        var source = _adapter.Read(config.SourceFullPath);
        var cache = TranslationCache.Load(config.CachePath);
        var wanted = FilterLanguages(config, languages);
        var perLanguage = new List<LanguagePlan>(wanted.Count);

        foreach (var language in wanted)
        {
            var targetPath = config.TargetPath(language);
            var target = _adapter.Read(targetPath).ToDictionary(e => e.Key, StringComparer.Ordinal);
            var missing = new List<WorkItem>();
            var stale = new List<WorkItem>();
            var broken = new List<WorkItem>();
            var current = 0;

            foreach (var entry in source)
            {
                if (!target.TryGetValue(entry.Key, out var existing))
                {
                    missing.Add(new WorkItem(entry.Key, entry.Value, entry.Comment));
                    continue;
                }

                if (!PlaceholderValidator.Matches(entry.Value, existing.Value))
                    broken.Add(new WorkItem(entry.Key, entry.Value, entry.Comment, existing.Value));

                var hash = SourceHash.Compute(entry.Value);
                var cached = cache.Get(language, entry.Key);
                if (cached is not null && !string.Equals(cached, hash, StringComparison.OrdinalIgnoreCase))
                {
                    stale.Add(new WorkItem(entry.Key, entry.Value, entry.Comment, existing.Value));
                    continue;
                }

                current++;
            }

            var extra = target.Keys
                .Where(key => source.All(s => !string.Equals(s.Key, key, StringComparison.Ordinal)))
                .OrderBy(k => k, StringComparer.Ordinal)
                .ToArray();

            perLanguage.Add(new LanguagePlan
            {
                Language = language,
                TargetPath = targetPath,
                Missing = missing,
                Stale = stale,
                PlaceholderBroken = broken,
                Extra = extra,
                Current = current,
            });
        }

        return new LocalizationPlan
        {
            Source = config.Source,
            SourceFullPath = config.SourceFullPath,
            Languages = perLanguage,
        };
    }

    public void WriteAcceptedHashes(I18nConfig config, LocalizationPlan plan)
    {
        var cache = TranslationCache.Load(config.CachePath);
        var source = _adapter.Read(config.SourceFullPath).ToDictionary(e => e.Key, StringComparer.Ordinal);

        foreach (var language in plan.Languages)
        {
            var target = _adapter.Read(language.TargetPath);
            foreach (var entry in target)
            {
                if (!source.TryGetValue(entry.Key, out var sourceEntry))
                    continue;
                if (!PlaceholderValidator.Matches(sourceEntry.Value, entry.Value))
                    continue;

                cache.Set(language.Language, entry.Key, SourceHash.Compute(sourceEntry.Value));
            }
        }

        cache.Write(config.CachePath);
    }

    static IReadOnlyList<string> FilterLanguages(I18nConfig config, IEnumerable<string>? languages)
    {
        if (languages is null)
            return config.Languages;

        var filter = languages
            .Where(static l => !string.IsNullOrWhiteSpace(l))
            .Select(static l => l.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (filter.Count == 0)
            return config.Languages;

        return config.Languages.Where(filter.Contains).ToArray();
    }
}

public sealed class LocalizationPlan
{
    public required string Source { get; init; }
    public required string SourceFullPath { get; init; }
    public required IReadOnlyList<LanguagePlan> Languages { get; init; }

    public bool HasGaps => Languages.Any(static l => l.HasGaps);
}
