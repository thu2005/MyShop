using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace MyShop.App.Controls;

/// <summary>
/// Stat card control displaying a metric with icon, value, and change percentage
/// </summary>
public sealed partial class StatCard : UserControl
{
    public StatCard()
    {
        this.InitializeComponent();
    }

    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(nameof(Title), typeof(string), typeof(StatCard), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(nameof(Value), typeof(string), typeof(StatCard), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty ChangePercentageProperty =
        DependencyProperty.Register(nameof(ChangePercentage), typeof(double), typeof(StatCard), new PropertyMetadata(0.0));

    public static readonly DependencyProperty IconGlyphProperty =
        DependencyProperty.Register(nameof(IconGlyph), typeof(string), typeof(StatCard), new PropertyMetadata("\uE8F1"));

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string Value
    {
        get => (string)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public double ChangePercentage
    {
        get => (double)GetValue(ChangePercentageProperty);
        set => SetValue(ChangePercentageProperty, value);
    }

    public string IconGlyph
    {
        get => (string)GetValue(IconGlyphProperty);
        set => SetValue(IconGlyphProperty, value);
    }

    public string ChangeText => $"{(ChangePercentage >= 0 ? "+" : "")}{ChangePercentage:F1}%";
    
    public bool IsPositiveChange => ChangePercentage >= 0;
}
