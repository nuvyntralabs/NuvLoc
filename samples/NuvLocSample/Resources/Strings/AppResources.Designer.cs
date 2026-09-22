using System.Globalization;
using System.Resources;

namespace NuvLocSample.Resources.Strings;

public static class AppResources
{
    static readonly ResourceManager Manager = new("NuvLocSample.Resources.Strings.AppResources", typeof(AppResources).Assembly);

    public static CultureInfo? Culture { get; set; }

    public static string AppTitle => Get(nameof(AppTitle));
    public static string HelloWorld => Get(nameof(HelloWorld));
    public static string Welcome => Get(nameof(Welcome));
    public static string ClickMe => Get(nameof(ClickMe));
    public static string ClickedOnce => Get(nameof(ClickedOnce));
    public static string ClickedMany => Get(nameof(ClickedMany));
    public static string ReviewHint => Get(nameof(ReviewHint));

    static string Get(string name) =>
        Manager.GetString(name, Culture) ?? name;
}
