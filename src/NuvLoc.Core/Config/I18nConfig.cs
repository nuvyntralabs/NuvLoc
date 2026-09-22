namespace NuvLoc.Config;

public sealed class I18nConfig
{
    public const string DefaultFileName = "i18n.json";
    public const string DefaultPlatform = "maui";

    public required string Platform { get; init; }
    public required string Source { get; init; }
    public required IReadOnlyList<string> Languages { get; init; }
    public required string ConfigPath { get; init; }

    public string ConfigDirectory => Path.GetDirectoryName(ConfigPath) ?? ".";

    public string SourceFullPath => Path.GetFullPath(Path.Combine(ConfigDirectory, Source));

    public string TargetPath(string language)
    {
        var fileName = Path.GetFileName(SourceFullPath);
        var ext = Path.GetExtension(fileName);
        var stem = Path.GetFileNameWithoutExtension(fileName);
        return Path.Combine(Path.GetDirectoryName(SourceFullPath) ?? ".", $"{stem}.{language}{ext}");
    }

    public string NuvLocDirectory => Path.Combine(ConfigDirectory, ".nuvloc");

    public string CachePath => Path.Combine(NuvLocDirectory, "cache.json");
}
