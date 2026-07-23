using Microsoft.Maui.Controls;
using System;

namespace GameProject02.Views;

// ═══════════════════════════════════════════════════════════════════
//  حاوية الهيدر (Slot)
//  ─────────────────────────────────────────────────────────────────
//  كل صفحة تضع <views:TopHeaderView/> كما في السابق، لكن الهيدر
//  الحقيقي (TopHeaderCore) نسخة واحدة مشتركة تنتقل بين الصفحات.
//  النتيجة: الهيدر لا يُعاد إنشاؤه ولا "يرمش" عند التنقل — فقط
//  القيم بداخله (الذهب، الطاقة...) تتحدث عبر مؤقّته الداخلي.
// ═══════════════════════════════════════════════════════════════════
public class TopHeaderView : ContentView
{
    private static TopHeaderCore? _core;

    /// <summary>النسخة الوحيدة المشتركة من الهيدر (تُنشأ عند أول استخدام).</summary>
    public static TopHeaderCore Core => _core ??= new TopHeaderCore();

    private Page? _hostPage;

    public TopHeaderView()
    {
        Loaded += OnSlotLoaded;
    }

    private void OnSlotLoaded(object? sender, EventArgs e)
    {
        AttachCore();
        HookHostPage();
    }

    // نربط حدث ظهور الصفحة حتى نستعيد الهيدر أيضاً عند العودة من
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

        // إذا كان الهيدر معروضاً داخل حاوية أخرى، افصله عنها أولاً
        if (core.Parent is TopHeaderView previousOwner && previousOwner != this)
            previousOwner.Content = null;

        if (Content != core)
            Content = core;
    }
}
