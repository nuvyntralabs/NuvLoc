namespace NuvyntraLabs.NuvLoc.Cli.Scaffolding;

public static class PayloadInstaller
{
    public static IReadOnlyList<string> InstallRules(string payloadRoot, string projectDir, bool force)
    {
        var written = new List<string>();
        var destRoot = Path.Combine(projectDir, ".nuvloc");
        Directory.CreateDirectory(destRoot);

        CopyFile(
            Path.Combine(payloadRoot, "nuvloc", "rules.md"),
            Path.Combine(destRoot, "rules.md"),
            projectDir,
            written,
            overwrite: force);

        var referenceSrc = Path.Combine(payloadRoot, "nuvloc", "reference");
        var referenceDest = Path.Combine(destRoot, "reference");
        if (Directory.Exists(referenceSrc))
        {
            foreach (var file in Directory.GetFiles(referenceSrc, "*", SearchOption.AllDirectories))
            {
                var relative = Path.GetRelativePath(referenceSrc, file);
                var dest = Path.Combine(referenceDest, relative);
                CopyFile(file, dest, projectDir, written, overwrite: true);
            }
        }

        return written;
    }

    static void CopyFile(string source, string dest, string projectDir, List<string> written, bool overwrite)
    {
        if (!File.Exists(source))
            return;
        if (File.Exists(dest) && !overwrite)
            return;

        Directory.CreateDirectory(Path.GetDirectoryName(dest)!);
        File.Copy(source, dest, overwrite: true);
        written.Add(Path.GetRelativePath(projectDir, dest));
    }
}
