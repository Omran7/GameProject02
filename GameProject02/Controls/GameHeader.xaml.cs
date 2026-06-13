using Microsoft.Maui.Controls;

namespace GameProject02.Views;

public partial class GameHeader : ContentView
{
    public GameHeader()
    {
        InitializeComponent();
        BindingContext = this;
    }

    // ── خاصية العنوان ──────────────────────────────────────────
    public static readonly BindableProperty HeaderTitleProperty =
        BindableProperty.Create(
            nameof(HeaderTitle),
            typeof(string),
            typeof(GameHeader),
            defaultValue: "عنوان الصفحة");

    public string HeaderTitle
    {
        get => (string)GetValue(HeaderTitleProperty);
        set => SetValue(HeaderTitleProperty, value);
    }

    // ── صورة اليمين ────────────────────────────────────────────
    public static readonly BindableProperty RightImageProperty =
        BindableProperty.Create(
            nameof(RightImage),
            typeof(string),
            typeof(GameHeader),
            defaultValue: "header_right.png");

    public string RightImage
    {
        get => (string)GetValue(RightImageProperty);
        set => SetValue(RightImageProperty, value);
    }

    // ── صورة اليسار ────────────────────────────────────────────
    public static readonly BindableProperty LeftImageProperty =
        BindableProperty.Create(
            nameof(LeftImage),
            typeof(string),
            typeof(GameHeader),
            defaultValue: "header_left.png");

    public string LeftImage
    {
        get => (string)GetValue(LeftImageProperty);
        set => SetValue(LeftImageProperty, value);
    }
}