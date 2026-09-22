using System.Security.Cryptography;
using System.Text;

namespace NuvLoc.Diff;

public static class SourceHash
{
    public static string Compute(string value)
    {
        var normalized = (value ?? "").Replace("\r\n", "\n").Replace('\r', '\n');
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(normalized));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
