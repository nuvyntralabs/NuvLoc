namespace NuvLoc.Resources;

public interface IResourceAdapter
{
    IReadOnlyList<ResourceEntry> Read(string path);

    void Write(string path, IReadOnlyList<ResourceEntry> entries, string? templatePath = null);
}
