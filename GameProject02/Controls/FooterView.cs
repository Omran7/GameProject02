using Microsoft.Maui.Controls;
using System;

namespace GameProject02.Controls;

// ═══════════════════════════════════════════════════════════════════
//  حاوية الفوتر (Slot)
//  ─────────────────────────────────────────────────────────────────
//  كل صفحة تضع <controls:FooterView x:Name="PageFooter"/> كما في
//  السابق وتستدعي نفس الدوال (SetContent / CreateFooterButton...)،
//  لكن الفوتر الحقيقي (FooterCore) نسخة واحدة مشتركة تنتقل بين
//  الصفحات: الخلفية والتصميم ثابتان لا يُعاد تحميلهما، وكل صفحة
//  تضع أزرارها الإضافية الخاصة داخل الفوتر المشترك.
// ═══════════════════════════════════════════════════════════════════
public class FooterView : ContentView
{
    private static FooterCore? _core;

    /// <summary>النسخة الوحيدة المشتركة من الفوتر (تُنشأ عند أول استخدام).</summary>
    public static FooterCore Core => _core ??= new FooterCore();

    // محتوى هذه الصفحة بالتحديد (أزرارها الخاصة)
    private View? _pageContent;
    private bool _autoSizeButtons = true;

    private Page? _hostPage;

    // ==================== نفس إعدادات الفوتر السابقة ====================
    public double FooterHeightRatio
    {
        get => Core.FooterHeightRatio;
        set => Core.FooterHeightRatio = value;
    }

    public double MinFooterHeight
    {
        get => Core.MinFooterHeight;
        set => Core.MinFooterHeight = value;
    }

    public double MaxFooterHeight
    {
        get => Core.MaxFooterHeight;
        set => Core.MaxFooterHeight = value;
    }

    public double ButtonSizeRatio
    {
        get => Core.ButtonSizeRatio;
        set => Core.ButtonSizeRatio = value;
    }

    public Color ButtonBackgroundColor
    {
        get => Core.ButtonBackgroundColor;
        set => Core.ButtonBackgroundColor = value;
    }

    public double ButtonFontSize
    {
        get => Core.ButtonFontSize;
        set => Core.ButtonFontSize = value;
    }
    // ====================================================================

    public FooterView()
    {
        Loaded += OnSlotLoaded;
    }

    private void OnSlotLoaded(object? sender, EventArgs e)
    {
        AttachCore();
        HookHostPage();
    }

    // نربط حدث ظهور الصفحة حتى نستعيد الفوتر أيضاً عند العودة من
    // الصفحات المنبثقة (Modal) التي لا تعيد تحميل الصفحة السفلية
    private void HookHostPage()
    {
        var page = FindHostPage();
        if (page == null || page == _hostPage) return;

        if (_hostPage != null)
            _hostPage.Appearing -= OnHostPageAppearing;

        _hostPage = page;
        _hostPage.Appearing += OnHostPageAppearing;
    }

    private void OnHostPageAppearing(object? sender, EventArgs e) => AttachCore();

    private Page? FindHostPage()
    {
        Element? current = Parent;
        while (current != null && current is not Page)
            current = current.Parent;
        return current as Page;
    }

    private void AttachCore()
    {
        var core = Core;

        // إذا كان الفوتر معروضاً داخل حاوية أخرى، افصله عنها أولاً
        if (core.Parent is FooterView previousOwner && previousOwner != this)
            previousOwner.Content = null;

        if (Content != core)
            Content = core;

        // اعرض محتوى هذه الصفحة (أو فرّغه إذا لم تحدد الصفحة أزراراً
        // حتى لا تبقى أزرار الصفحة السابقة ظاهرة)
        ApplyPageContent();
    }

    private void ApplyPageContent()
    {
        if (Content != Core) return;

        if (_autoSizeButtons)
            Core.SetContent(_pageContent);
        else
            Core.SetCustomContent(_pageContent);
    }

    /// <summary>
    /// تعيين المحتوى الداخلي للفوتر مع تطبيق حجم الزر التلقائي
    /// </summary>
    public void SetContent(View content)
    {
        _pageContent = content;
        _autoSizeButtons = true;
        ApplyPageContent();
    }

    /// <summary>
    /// تعيين محتوى مخصص للفوتر دون تطبيق حجم الزر التلقائي
    /// </summary>
    public void SetCustomContent(View content)
    {
        _pageContent = content;
        _autoSizeButtons = false;
        ApplyPageContent();
    }

    /// <summary>
    /// دالة مساعدة لإنشاء زر فوتر جاهز (نفس التصميم السابق)
    /// </summary>
    public Border CreateFooterButton(string text, EventHandler<TappedEventArgs> tappedHandler,
                                     string buttonImageSource = null,
                                     LayoutOptions? horizontalOptions = null)
        => Core.CreateFooterButton(text, tappedHandler, buttonImageSource, horizontalOptions);
}
