using GameProject02.Helpers;
using GameProject02.Models;
using GameProject02.Services;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;
using System;
using System.Timers;
using static GameProject02.Helpers.PopupService;

namespace GameProject02.Views;

public partial class TopHeaderView : ContentView
{
    private System.Timers.Timer _refreshTimer;
    private bool _sizesApplied = false;

    private const double TopBarPercent = 0.11;
    private const double LevelBarPercent = 0.025;
    private const double FieldHeightRatio = 0.20;
    private const double BarHeightRatio = 0.07;
    private const double BarWidthRatio = 0.13;
    private const double IconSizeRatio = 0.22;
    private const double FontSizeRatio = 0.12;
    private const double FontSizeSmRatio = 0.11;
    private const double AvatarSizeRatio = 0.65;
    private const double NameFieldHeightRatio = 0.23;
    private const double XpBarHeightRatio = 0.9;
    private const double PlayerNameFontRatio = 0.13;
    private const double XPLabelFontRatio = 0.6;
    private const double LvlTextIconWidthRatio = 1.0;
    private const double LvlTextIconHeightRatio = 1.0;
    private const double FieldWidthRatio = 0.37;
    private const double NameFieldWidthRatio = 0.25;
    private const double GoldIconWidthRatio = 1.2;
    private const double DiamondIconWidthRatio = 1.2;
    private const double SecondaryIconSizeRatio = 0.18;
    private const double EditHintSizeRatio = 0.20;
    private const double FramePaddingRatio = 0.06;
    private const double CourageIntervalSec = 120;
    private const double EnergyIntervalSec = 120;
    private const double NobilityIntervalSec = 300;
    private const double HealthIntervalSec = 120;
    private const double BarBorderThickness = 1;   // ← زيادة لإظهار كنار الفارغة
    private const double XpBarMarginRatio = 0.215;

    private double _scrW, _scrH;
    private double _fillMaxWidth = 0;
    private double _levelDigitSize;
    private double _maxDigitSize;

    public TopHeaderView()
    {
        InitializeComponent();

        var info = DeviceDisplay.MainDisplayInfo;
        _scrW = info.Width / info.Density;
        _scrH = info.Height / info.Density;

        ApplyResponsiveSizes();
        LoadSavedAvatar();

        PlayerAccount.AvatarChanged += OnAvatarChanged;
        StartRefreshTimer();
    }

    // ═══════════════════════════════════════════════════
    //  تحميل الصورة عند بدء التشغيل
    // ═══════════════════════════════════════════════════
    private void LoadSavedAvatar()
    {
        string saved = Preferences.Get("AvatarPath", string.Empty);

        if (!string.IsNullOrEmpty(saved)
            && System.IO.Path.IsPathRooted(saved)
            && System.IO.File.Exists(saved))
            AvatarImage.Source = ImageSource.FromFile(saved);
        else
            AvatarImage.Source = "avatar_player.png";
    }

    // ═══════════════════════════════════════════════════
    //  الضغط على الصورة — يفتح نافذة الخيارات
    // ═══════════════════════════════════════════════════
    private async void OnAvatarTapped(object sender, TappedEventArgs e)
    {
        string currentPath = Preferences.Get("AvatarPath", string.Empty);
        var result = await PopupService.ShowAvatarPopupAsync(currentPath);

        switch (result)
        {
            case AvatarPopupResult.Change:
                await PickAndSaveAvatar();
                break;
            case AvatarPopupResult.Remove:
                RemoveAvatar();
                break;
            case AvatarPopupResult.Cancel:
                break;
        }
    }

    // ═══════════════════════════════════════════════════
    //  اختيار صورة جديدة من المعرض
    // ═══════════════════════════════════════════════════
    private async Task PickAndSaveAvatar()
    {
        try
        {
            var photo = await MediaPicker.Default.PickPhotoAsync(new MediaPickerOptions
            {
                Title = "اختر صورة الملف الشخصي"
            });

            if (photo == null) return;

            string localPath = await SaveAvatarLocallyAsync(photo);
            AvatarImage.Source = ImageSource.FromFile(localPath);

            var player = AccountService.GetCurrentPlayer();
            if (player != null)
                player.AvatarPath = localPath;
            else
                Preferences.Set("AvatarPath", localPath);
        }
        catch (PermissionException)
        {
            await Application.Current!.MainPage!
                .DisplayAlert("صلاحية مرفوضة",
                    "يرجى السماح للتطبيق بالوصول إلى معرض الصور من إعدادات الجهاز.",
                    "موافق");
        }
        catch (Exception)
        {
            await Application.Current!.MainPage!
                .DisplayAlert("خطأ", "تعذّر تغيير الصورة، حاول مرة أخرى.", "موافق");
        }
    }

    // ═══════════════════════════════════════════════════
    //  حذف الصورة والعودة للافتراضية
    // ═══════════════════════════════════════════════════
    private void RemoveAvatar()
    {
        string oldPath = Preferences.Get("AvatarPath", string.Empty);
        if (!string.IsNullOrEmpty(oldPath)
            && System.IO.Path.IsPathRooted(oldPath)
            && System.IO.File.Exists(oldPath))
        {
            try { System.IO.File.Delete(oldPath); } catch { /* تجاهل */ }
        }

        var player = AccountService.GetCurrentPlayer();
        if (player != null)
            player.ClearAvatar();
        else
            Preferences.Remove("AvatarPath");

        AvatarImage.Source = "avatar_player.png";
    }

    // ═══════════════════════════════════════════════════
    //  حفظ الصورة محلياً وحذف القديمة
    // ═══════════════════════════════════════════════════
    private static async Task<string> SaveAvatarLocallyAsync(FileResult photo)
    {
        string folder = System.IO.Path.Combine(FileSystem.AppDataDirectory, "Avatars");
        System.IO.Directory.CreateDirectory(folder);

        string oldPath = Preferences.Get("AvatarPath", string.Empty);
        if (!string.IsNullOrEmpty(oldPath)
            && System.IO.File.Exists(oldPath)
            && oldPath.Contains("Avatars"))
        {
            try { System.IO.File.Delete(oldPath); } catch { /* تجاهل */ }
        }

        string destPath = System.IO.Path.Combine(folder, $"avatar_{DateTime.Now.Ticks}.jpg");

        using var src = await photo.OpenReadAsync();
        using var dest = System.IO.File.OpenWrite(destPath);
        await src.CopyToAsync(dest);

        return destPath;
    }

    // ═══════════════════════════════════════════════════
    //  استقبال تغيير الصورة من أي مكان آخر
    // ═══════════════════════════════════════════════════
    private void OnAvatarChanged(string newPath)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (!string.IsNullOrEmpty(newPath)
                && System.IO.Path.IsPathRooted(newPath)
                && System.IO.File.Exists(newPath))
                AvatarImage.Source = ImageSource.FromFile(newPath);
            else
                AvatarImage.Source = "avatar_player.png";
        });
    }

    // ═══════════════════════════════════════════════════
    //  الأحجام
    // ═══════════════════════════════════════════════════
    private void ApplyResponsiveSizes()
    {
        double topH = _scrH * TopBarPercent;
        double levelH = _scrH * LevelBarPercent;

        double fieldH = topH * FieldHeightRatio;
        double barH = topH * BarHeightRatio;
        double barW = _scrW * BarWidthRatio;
        double iconSz = topH * IconSizeRatio;
        double secondaryIconSz = topH * SecondaryIconSizeRatio;
        double fontSize = topH * FontSizeRatio;
        double fontSzSm = topH * FontSizeSmRatio;
        double fieldContainerW = _scrW * FieldWidthRatio;

        DiamondField.WidthRequest = GoldField.WidthRequest = fieldContainerW;
        NameField.WidthRequest = _scrW * NameFieldWidthRatio;
        DiamondField.HeightRequest = GoldField.HeightRequest = fieldH;

        DiamondIcon.WidthRequest = DiamondIcon.HeightRequest = iconSz * DiamondIconWidthRatio;
        GoldIcon.WidthRequest = GoldIcon.HeightRequest = iconSz * GoldIconWidthRatio;
        DiamondSecondaryIcon.WidthRequest = DiamondSecondaryIcon.HeightRequest = secondaryIconSz;
        GoldSecondaryIcon.WidthRequest = GoldSecondaryIcon.HeightRequest = secondaryIconSz;

        DiamondLabel.FontSize = GoldLabel.FontSize = fontSize;

        TopBarGrid.HeightRequest = topH;
        LevelBarGrid.HeightRequest = levelH;

        CourageIcon.WidthRequest = EnergyIcon.WidthRequest =
        NobilityIcon.WidthRequest = HealthIcon.WidthRequest = iconSz;
        CourageIcon.HeightRequest = EnergyIcon.HeightRequest =
        NobilityIcon.HeightRequest = HealthIcon.HeightRequest = iconSz;

        CourageBarGrid.WidthRequest = EnergyBarGrid.WidthRequest =
        NobilityBarGrid.WidthRequest = HealthBarGrid.WidthRequest = barW;
        CourageBarGrid.HeightRequest = EnergyBarGrid.HeightRequest =
        NobilityBarGrid.HeightRequest = HealthBarGrid.HeightRequest = barH;

        // ═══ حجم الـ Fill مع مراعاة الكنار من الأربع جهات ═══
        _fillMaxWidth = barW - 2 * BarBorderThickness;
        double fillH = barH - 2 * BarBorderThickness;

        CourageFill.HeightRequest = fillH;
        EnergyFill.HeightRequest = fillH;
        NobilityFill.HeightRequest = fillH;
        HealthFill.HeightRequest = fillH;

        // Margin لإبعاد الـ Fill عن حواف الـ Bar الفارغة
        var fillMargin = new Thickness(0.5, 0, 0, 0);
        CourageFill.Margin = fillMargin;
        EnergyFill.Margin = fillMargin;
        NobilityFill.Margin = fillMargin;
        HealthFill.Margin = fillMargin;

        // ═══ شريط الخبرة ═══
        double xpBarH = levelH * XpBarHeightRatio - 2 * BarBorderThickness;
        double xpMarginH = _scrW * XpBarMarginRatio;

        XPBarContainer.HeightRequest = xpBarH;
        XPBarContainer.Margin = new Thickness(xpMarginH, 0, xpMarginH, 0);
        XPFill.HeightRequest = xpBarH;

        // أيقونات LVL و MAX
        LvlTextIcon.WidthRequest = iconSz * LvlTextIconWidthRatio;
        LvlTextIcon.HeightRequest = iconSz * LvlTextIconHeightRatio;
        IconMax.WidthRequest = IconMax.HeightRequest = iconSz;

        _levelDigitSize = _maxDigitSize = iconSz * 0.5;

        PlayerNameLabel.FontSize = topH * PlayerNameFontRatio;
        XPLabel.FontSize = levelH * XPLabelFontRatio;

        double s = fontSzSm;
        CourageText.FontSize = EnergyText.FontSize =
        NobilityText.FontSize = HealthText.FontSize = s;
        CourageNameLabel.FontSize = EnergyNameLabel.FontSize =
        NobilityNameLabel.FontSize = HealthNameLabel.FontSize = s * 0.85;
        CourageTimer.FontSize = EnergyTimer.FontSize =
        NobilityTimer.FontSize = HealthTimer.FontSize = s * 0.9;

        // ═══ الصورة الرمزية + الكنار ═══
        double avatarSz = topH * AvatarSizeRatio;
        double framePadding = avatarSz * FramePaddingRatio;

        AvatarFrameImage.WidthRequest = avatarSz;
        AvatarFrameImage.HeightRequest = avatarSz;

        AvatarImageFrame.WidthRequest = avatarSz - framePadding * 2;
        AvatarImageFrame.HeightRequest = avatarSz - framePadding * 2;
        AvatarImageFrame.CornerRadius = 0;

        double hintSz = avatarSz * EditHintSizeRatio;
        EditAvatarHint.WidthRequest = hintSz;
        EditAvatarHint.HeightRequest = hintSz;

        NameField.HeightRequest = topH * NameFieldHeightRatio;

        _sizesApplied = true;
    }

    // ═══════════════════════════════════════════════════
    //  تحديث الإحصائيات
    // ═══════════════════════════════════════════════════
    private void StartRefreshTimer()
    {
        Task.Delay(500).ContinueWith(_ =>
            MainThread.BeginInvokeOnMainThread(UpdateStats));

        _refreshTimer = new System.Timers.Timer(1000);
        _refreshTimer.Elapsed += (s, e) =>
            MainThread.BeginInvokeOnMainThread(UpdateStats);
        _refreshTimer.AutoReset = true;
        _refreshTimer.Enabled = true;
    }

    private void UpdateStats()
    {
        var player = AccountService.GetCurrentPlayer();
        if (player == null) return;

        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        PlayerNameLabel.Text = player.Username?.ToUpper() ?? "PLAYER";
        UpdateLevelDigits(player.Level);
        UpdateMaxDigits(100);
        GoldLabel.Text = player.Gold.ToString("N0");
        DiamondLabel.Text = player.Diamonds.ToString("N0");
        XPLabel.Text = $"{player.CurrentXP:N0}/{player.MaxXP:N0}";

        CourageText.Text = $"{player.Courage}/{player.MaxCourage}";
        EnergyText.Text = $"{player.Energy}/{player.MaxEnergy}";
        NobilityText.Text = $"{player.NobilityCurrent}/100";
        HealthText.Text = $"{player.Health}/{player.MaxHealth}";

        UpdateBarWithTimer(player.Courage, player.MaxCourage, CourageIntervalSec,
            "regen_last_courage", now, CourageNameLabel, CourageTimer, CourageFill, _fillMaxWidth);
        UpdateBarWithTimer(player.Energy, player.MaxEnergy, EnergyIntervalSec,
            "regen_last_energy", now, EnergyNameLabel, EnergyTimer, EnergyFill, _fillMaxWidth);
        UpdateBarWithTimer(player.NobilityCurrent, 100, NobilityIntervalSec,
            "regen_last_nobility", now, NobilityNameLabel, NobilityTimer, NobilityFill, _fillMaxWidth);
        UpdateBarWithTimer(player.Health, player.MaxHealth, HealthIntervalSec,
            "regen_last_health", now, HealthNameLabel, HealthTimer, HealthFill, _fillMaxWidth);

        // ═══ شريط الخبرة ═══
        if (XPBarContainer.Width > 10)
            XPFill.WidthRequest = XPBarContainer.Width *
                (player.MaxXP > 0
                    ? Math.Clamp((double)player.CurrentXP / player.MaxXP, 0, 1)
                    : 0);
    }

    private void UpdateLevelDigits(int level)
    {
        LevelDigitsContainer.Children.Clear();
        foreach (char c in level.ToString())
            if (c >= '0' && c <= '9')
                LevelDigitsContainer.Children.Add(CreateDigitImage(c, _levelDigitSize));
    }

    private void UpdateMaxDigits(int number)
    {
        MaxDigitsContainer.Children.Clear();
        foreach (char c in number.ToString())
            if (c >= '0' && c <= '9')
                MaxDigitsContainer.Children.Add(CreateDigitImage(c, _maxDigitSize));
    }

    private static Image CreateDigitImage(char digit, double size) =>
        new Image
        {
            Source = ImageSource.FromFile($"digit_{digit}.png"),
            WidthRequest = size,
            HeightRequest = size,
            Aspect = Aspect.AspectFit
        };

    private static void UpdateBarWithTimer(
        int current, int max, double intervalSec, string prefKey, long nowSec,
        Label nameLabel, Label timerLabel, Image fillImage, double maxWidth)
    {
        bool isFull = current >= max;
        nameLabel.IsVisible = isFull;
        timerLabel.IsVisible = !isFull;

        if (!isFull)
        {
            long last = Preferences.Get(prefKey, nowSec);
            double elapsed = Math.Clamp(nowSec - last, 0, intervalSec);
            double remaining = intervalSec - elapsed;
            timerLabel.Text = $"{(int)(remaining / 60):D2}:{(int)(remaining % 60):D2}";
        }
        else timerLabel.Text = "";

        if (maxWidth <= 0 || max <= 0) return;

        double ratio = isFull ? 1.0 :
            Math.Clamp(
                (current + Math.Clamp(nowSec - Preferences.Get(prefKey, nowSec), 0, intervalSec)
                         / intervalSec) / max,
                0, 1);

        fillImage.WidthRequest = maxWidth * ratio;
    }

    ~TopHeaderView()
    {
        PlayerAccount.AvatarChanged -= OnAvatarChanged;
        _refreshTimer?.Stop();
        _refreshTimer?.Dispose();
    }
}