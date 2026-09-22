using System.Text.Json;

namespace NuvLoc.Diff;

public sealed class TranslationCache
{
    readonly Dictionary<string, Dictionary<string, string>> _entries;

    TranslationCache(Dictionary<string, Dictionary<string, string>> entries) =>
        _entries = entries;

    public static TranslationCache Load(string path)
    {
        if (!File.Exists(path))
            return new TranslationCache(new(StringComparer.OrdinalIgnoreCase));

        try
        {
            using var doc = JsonDocument.Parse(File.ReadAllText(path));
            var map = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
            if (!doc.RootElement.TryGetProperty("entries", out var entries)
                || entries.ValueKind != JsonValueKind.Object)
            {
                return new TranslationCache(map);
            }

            foreach (var lang in entries.EnumerateObject())
            {
                if (lang.Value.ValueKind != JsonValueKind.Object)
                    continue;

                var keys = new Dictionary<string, string>(StringComparer.Ordinal);
                foreach (var key in lang.Value.EnumerateObject())
                {
                    var hash = key.Value.GetString();
                    if (!string.IsNullOrWhiteSpace(hash))
                        keys[key.Name] = hash;
                }

                map[lang.Name] = keys;
            }

            return new TranslationCache(map);
        }
        catch (JsonException)
        {
            return new TranslationCache(new(StringComparer.OrdinalIgnoreCase));
        }
    }

    public string? Get(string language, string key) =>
        _entries.TryGetValue(language, out var keys) && keys.TryGetValue(key, out var hash)
            ? hash
            : null;

    public void Set(string language, string key, string hash)
    {
        if (!_entries.TryGetValue(language, out var keys))
        {
            keys = new Dictionary<string, string>(StringComparer.Ordinal);
            _entries[language] = keys;
        }

        keys[key] = hash;
    }

    public void Write(string path)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true }))
        {
            writer.WriteStartObject();
            writer.WriteStartObject("entries");
            foreach (var lang in _entries.OrderBy(p => p.Key, StringComparer.OrdinalIgnoreCase))
            {
                writer.WriteStartObject(lang.Key);
                foreach (var key in lang.Value.OrderBy(p => p.Key, StringComparer.Ordinal))
                    writer.WriteString(key.Key, key.Value);
                writer.WriteEndObject();
            }

            writer.WriteEndObject();
            writer.WriteEndObject();
        }

        var tmp = path + ".tmp";
        File.WriteAllBytes(tmp, stream.ToArray());
        File.Move(tmp, path, overwrite: true);
    }
}
