using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using System;

namespace GameProject02.Controls;

// ═══════════════════════════════════════════════════════════════════
//  الفوتر الحقيقي — نسخة واحدة مشتركة في كل التطبيق (Singleton)
//  الصفحات تستخدم FooterView (الحاوية) التي تستعير هذه النسخة
//  وتضع أزرارها الخاصة داخل ContentContainer
// ═══════════════════════════════════════════════════════════════════
public partial class FooterCore : ContentView
{
    // ==================== إعدادات خاصة بالفوتر ====================
    public double FooterHeightRatio { get; set; } = 0.08;
    public double MinFooterHeight { get; set; } = 0;
    public double MaxFooterHeight { get; set; } = 0;
    public double ButtonSizeRatio { get; set; } = 0.85;
    public Color ButtonBackgroundColor { get; set; } = Colors.Transparent;
    public double ButtonFontSize { get; set; } = Device.GetNamedSize(NamedSize.Medium, typeof(Label));
    // =============================================================

    private double _buttonSize = 60;

    public FooterCore()
    {
        InitializeComponent();
        this.SizeChanged += OnFooterViewSizeChanged;
    }

    /// <summary>حجم الزر الحالي (يُستخدم من FooterView لإنشاء أزرار بالحجم الصحيح)</summary>
    public double CurrentButtonSize => _buttonSize;

    private void OnFooterViewSizeChanged(object sender, EventArgs e)
    {
        double screenHeight = DeviceDisplay.MainDisplayInfo.Height / DeviceDisplay.MainDisplayInfo.Density;

        if (screenHeight <= 0) return;

        double calculatedHeight = screenHeight * FooterHeightRatio;

        if (MinFooterHeight > 0 && calculatedHeight < MinFooterHeight)
            calculatedHeight = MinFooterHeight;

        if (MaxFooterHeight > 0 && calculatedHeight > MaxFooterHeight)
            calculatedHeight = MaxFooterHeight;

        if (calculatedHeight < 1)
            calculatedHeight = 1;

        RootGrid.HeightRequest = calculatedHeight;

        _buttonSize = calculatedHeight * ButtonSizeRatio;
        UpdateExistingButtonsSize();
    }

    // ✅ تبحث في كل المستويات بشكل تلقائي
    private void UpdateExistingButtonsSize()
    {
        UpdateBordersInView(ContentContainer.Content);
    }

    private void UpdateBordersInView(View view)
    {
        if (view == null) return;

        if (view is Border border)
        {
            border.WidthRequest = _buttonSize;
            border.HeightRequest = _buttonSize;
        }

        if (view is Layout layout)
        {
            foreach (var child in layout.Children)
                if (child is View childView)
                    UpdateBordersInView(childView);
        }
    }

    /// <summary>
    /// تعيين محتوى مخصص للفوتر دون تطبيق حجم الزر التلقائي
    /// </summary>
    public void SetCustomContent(View content)
    {
        ContentContainer.Content = content;
    }

    /// <summary>
    /// تعيين المحتوى الداخلي للفوتر مع تطبيق حجم الزر التلقائي
    /// </summary>
    public void SetContent(View content)
    {
        ContentContainer.Content = content;
        UpdateExistingButtonsSize();
    }

    /// <summary>
    /// دالة مساعدة لإنشاء زر فوتر جاهز
    /// </summary>
    public Border CreateFooterButton(string text, EventHandler<TappedEventArgs> tappedHandler,
                                     string buttonImageSource = null,
                                     LayoutOptions? horizontalOptions = null)
    {
        string imageSource = string.IsNullOrEmpty(buttonImageSource)
            ? "button_background.png"
            : buttonImageSource;

        var border = new Border
        {
            Stroke = Colors.Transparent,
            StrokeShape = new RoundRectangle { CornerRadius = 0 },
            Padding = 0,
            HorizontalOptions = horizontalOptions ?? LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            BackgroundColor = ButtonBackgroundColor,
            WidthRequest = _buttonSize,
            HeightRequest = _buttonSize
        };

        var grid = new Grid();

        // الصورة تملأ كل المساحة
        grid.Add(new Image
        {
            Source = imageSource,
            Aspect = Aspect.Fill
        });

        // النص بداخل الصورة في الأسفل
        if (!string.IsNullOrEmpty(text))
        {
            grid.Add(new Label
            {
                Text = text,
                TextColor = Colors.White,
                FontAttributes = FontAttributes.Bold,
                FontSize = ButtonFontSize * 0.8,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.End,
                HorizontalTextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 4)
            });
        }

        border.Content = grid;

        var tapGesture = new TapGestureRecognizer();
        tapGesture.Tapped += tappedHandler;
        border.GestureRecognizers.Add(tapGesture);

        return border;
    }
}