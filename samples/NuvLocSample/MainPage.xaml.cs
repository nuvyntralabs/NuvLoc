using System.Globalization;
using NuvLocSample.Resources.Strings;

namespace NuvLocSample;

public partial class MainPage : ContentPage
{
    int _count;

    public MainPage()
    {
        InitializeComponent();
    }

    void OnCounterClicked(object? sender, EventArgs e)
    {
        _count++;
        var format = _count == 1 ? AppResources.ClickedOnce : AppResources.ClickedMany;
        CounterBtn.Text = string.Format(CultureInfo.CurrentCulture, format, _count);
        SemanticScreenReader.Announce(CounterBtn.Text);
    }
}
