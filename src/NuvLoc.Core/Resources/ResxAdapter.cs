using System.Xml.Linq;

namespace NuvLoc.Resources;

public sealed class ResxAdapter : IResourceAdapter
{
    static readonly XNamespace XmlNs = "http://www.w3.org/XML/1998/namespace";

    public IReadOnlyList<ResourceEntry> Read(string path)
    {
        if (!File.Exists(path))
            return [];

        var doc = XDocument.Load(path);
        var entries = new List<ResourceEntry>();
        if (doc.Root is null)
            return entries;

        foreach (var data in doc.Root.Elements("data"))
        {
            var name = (string?)data.Attribute("name");
            if (string.IsNullOrWhiteSpace(name) || IsHeader(name))
                continue;

            var value = data.Element("value")?.Value ?? "";
            var comment = data.Element("comment")?.Value;
            entries.Add(new ResourceEntry(name, value, string.IsNullOrWhiteSpace(comment) ? null : comment));
        }

        return entries;
    }

    public void Write(string path, IReadOnlyList<ResourceEntry> entries, string? templatePath = null)
    {
        XDocument doc;
        if (File.Exists(path))
            doc = XDocument.Load(path);
        else if (!string.IsNullOrWhiteSpace(templatePath) && File.Exists(templatePath))
            doc = CloneTemplate(templatePath);
        else
            doc = NewDocument();

        if (doc.Root is null)
            throw new InvalidOperationException("Invalid .resx document.");

        var byKey = entries.ToDictionary(e => e.Key, StringComparer.Ordinal);
        foreach (var data in doc.Root.Elements("data").ToList())
        {
            var name = (string?)data.Attribute("name");
            if (string.IsNullOrWhiteSpace(name) || IsHeader(name))
                continue;

            if (byKey.TryGetValue(name, out var entry))
            {
                SetValue(data, entry);
                byKey.Remove(name);
            }
        }

        foreach (var entry in byKey.Values)
            doc.Root.Add(CreateData(entry));

        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        doc.Save(path);
    }

    static bool IsHeader(string name) =>
        name.StartsWith(">>", StringComparison.Ordinal);

    static void SetValue(XElement data, ResourceEntry entry)
    {
        var value = data.Element("value");
        if (value is null)
        {
            value = new XElement("value");
            data.Add(value);
        }

        value.Value = entry.Value;

        var comment = data.Element("comment");
        if (!string.IsNullOrWhiteSpace(entry.Comment))
        {
            if (comment is null)
            {
                comment = new XElement("comment");
                data.Add(comment);
            }

            comment.Value = entry.Comment;
        }
    }

    static XElement CreateData(ResourceEntry entry)
    {
        var data = new XElement("data",
            new XAttribute("name", entry.Key),
            new XAttribute(XmlNs + "space", "preserve"),
            new XElement("value", entry.Value));
        if (!string.IsNullOrWhiteSpace(entry.Comment))
            data.Add(new XElement("comment", entry.Comment));
        return data;
    }

    static XDocument CloneTemplate(string templatePath)
    {
        var doc = XDocument.Load(templatePath);
        if (doc.Root is null)
            return NewDocument();

        foreach (var data in doc.Root.Elements("data").ToList())
        {
            var name = (string?)data.Attribute("name");
            if (string.IsNullOrWhiteSpace(name) || IsHeader(name))
                continue;
            data.Remove();
        }

        return doc;
    }

    static XDocument NewDocument() =>
        new(
            new XElement("root",
                new XElement("resheader",
                    new XAttribute("name", "resmimetype"),
                    new XElement("value", "text/microsoft-resx")),
                new XElement("resheader",
                    new XAttribute("name", "version"),
                    new XElement("value", "2.0")),
                new XElement("resheader",
                    new XAttribute("name", "reader"),
                    new XElement("value", "System.Resources.ResXResourceReader, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")),
                new XElement("resheader",
                    new XAttribute("name", "writer"),
                    new XElement("value", "System.Resources.ResXResourceWriter, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"))));
}
