using GameProject02.Services;
using GameProject02.Models;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using System;
using System.IO;
using System.Linq;
using Microsoft.Maui.ApplicationModel;

namespace GameProject02.Views;

public partial class ProfilePage : ContentPage
{
    private PlayerAccount _player;

    public ProfilePage()
    {
        InitializeComponent();
        LoadPlayerData();
        ApplyLanguage();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadPlayerData();
        ApplyLanguage();
    }

    private void LoadPlayerData()
    {
        RentalService.ProcessExpiredRentals();
        _player = AccountService.GetCurrentPlayer();
        if (_player == null) return;

        // Update top bar resources
        GoldLabel.Text = $"{LanguageManager.Gold}: {_player.Gold:N0}";
        DiamondsLabel.Text = $"{LanguageManager.Diamonds}: {_player.Diamonds:N0}";
        EnergyLabel.Text = $"{LanguageManager.Energy}: {_player.Energy}/{_player.MaxEnergy}";
        HealthLabel.Text = $"{LanguageManager.Health}: {_player.Health}/{_player.MaxHealth}";

        // Update level info
        LevelLabel.Text = $"{LanguageManager.Level} {_player.Level}";
        LevelProgressBar.Progress = _player.LevelProgress;
        XPLabel.Text = $"{_player.CurrentXP}/{_player.MaxXP}";

        // Update basic info
        NameLabel.Text = LanguageManager.CurrentLanguage == LanguageManager.Language.Arabic ?
            $"الاسم : {_player.Username}" : $"Name : {_player.Username}";
        LevelInfoLabel.Text = $"{LanguageManager.Level} : {_player.Level}";
        AgeLabel.Text = LanguageManager.CurrentLanguage == LanguageManager.Language.Arabic ?
            $"العمر : {AccountService.GetPlayerAgeInDays()} يوم" : $"Age : {AccountService.GetPlayerAgeInDays()} days";
        GenderLabel.Text = LanguageManager.CurrentLanguage == LanguageManager.Language.Arabic ?
            $"الجنس : {_player.Gender}" : $"Gender : {_player.Gender}";

        // Show primary residence info
        if (_player.Estates != null && _player.Estates.Count > 0)
        {
            var primaryEstate = _player.Estates.FirstOrDefault(e => e.Id == _player.PrimaryResidenceEstateId);
            if (primaryEstate != null)
            {
                MedalsLabel.Text = $"{LanguageManager.Medals} : {_player.Medals} | مقر إقامتك: {primaryEstate.GetEstateTypeName()} ({primaryEstate.GetHappiness(_player):N0} 😊)";
            }
            else
            {
                MedalsLabel.Text = $"{LanguageManager.Medals} : {_player.Medals} | مقر إقامتك: عشة (0 😊)";
            }
        }
        else
        {
            MedalsLabel.Text = $"{LanguageManager.Medals} : {_player.Medals} | لم تشتري عقار بعد";
        }

        CityLabel.Text = $"{LanguageManager.City} : {_player.City}";
        VIPLabel.Text = $"{LanguageManager.VIP} : {(_player.IsVIP ? LanguageManager.IsVIP : LanguageManager.NotVIP)}";
        AchievementLabel.Text = $"{LanguageManager.AchievementPoints} : {_player.AchievementPoints:N0}";

        // Crime stats
        CrimeAttemptsLabel.Text = _player.CrimeObject.TotalCrimesAttempted.ToString("N0");
        JailTimesLabel.Text = _player.CrimeObject.TotalPrisonVisits.ToString("N0");
        HospitalVisitsLabel.Text = _player.CrimeObject.TotalHospitalVisits.ToString("N0");

        ShovelsLabel.Text = _player.Shovels.ToString("N0");
        FlightsLabel.Text = _player.Flights.ToString("N0");
        HerbsUsedLabel.Text = _player.HerbsUsed.ToString("N0");
        ItemsFoundLabel.Text = _player.ItemsFound.ToString("N0");

        // Battle stats
        SpeedStatLabel.Text = $"{LanguageManager.Speed} : {_player.Speed:N2}";
        SkillStatLabel.Text = $"المهارة : {_player.Intelligence:N2}";
        StrengthStatLabel.Text = $"{LanguageManager.Strength} : {_player.Strength:N2}";
        DefenseStatLabel.Text = $"{LanguageManager.Defense} : {_player.Defense:N2}";
        HealthPointsLabel.Text = $"{LanguageManager.Health} : {_player.Health:N0}";

        // Skills display (example values)
        GreatnessPercentLabel.Text = $"90% : زادت بمعدل";
        GreatnessDescLabel.Text = "(15:مجردة + 75:مصاحبة)";
        KillingDifficultyPercentLabel.Text = $"75% : زاد الدفع بمعدل";
        KillingDifficultyDescLabel.Text = "(15:مجردة + 60:مصاحبة)";
        FastGhostPercentLabel.Text = $"90% : زادت السرعة بمعدل";
        FastGhostDescLabel.Text = "(15:مجردة + 75:مصاحبة)";
        LightMovementPercentLabel.Text = $"75% : زادت المهارة بمعدل";
        LightMovementDescLabel.Text = "(75:مصاحبة)";

        // Update profile avatar if the image control exists
        if (ProfileAvatarImage != null)
        {
            ProfileAvatarImage.Source = ImageSource.FromFile(_player.AvatarPath);
        }

        // Apply RTL/LTR layout
        this.FlowDirection = LanguageManager.CurrentLanguage == LanguageManager.Language.Arabic
            ? FlowDirection.RightToLeft
            : FlowDirection.LeftToRight;
    }

    private void ApplyLanguage()
    {
        // Optional: translate static labels
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private void OnSwitchLanguageClicked(object sender, EventArgs e)
    {
        LanguageManager.CurrentLanguage =
            LanguageManager.CurrentLanguage == LanguageManager.Language.Arabic ?
            LanguageManager.Language.English : LanguageManager.Language.Arabic;
        LoadPlayerData();
    }

    private string GetEstateImageSource(EstateObject estate)
    {
        if (estate.Id == 16 && !string.IsNullOrEmpty(estate.EstateImageUrl) &&
            estate.EstateImageUrl != "no_private_domain_image")
        {
            return estate.EstateImageUrl;
        }
        return estate.GetImageSource();
    }

    private async void OnArmingClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new ArmingPage());
    }

    private async void OnSignOutClicked(object sender, EventArgs e)
    {
        var confirm = await DisplayAlert("تسجيل الخروج", "هل تريد تسجيل الخروج الآن؟", "نعم", "إلغاء");
        if (!confirm) return;

        try
        {
            AccountService.Logout();
            Application.Current.MainPage = new NavigationPage(new LoginPage());
        }
        catch (Exception ex)
        {
            await DisplayAlert("❌ خطأ", $"فشل تسجيل الخروج: {ex.Message}", "موافق");
        }
    }

    // NEW: Change avatar method
    private async void OnChangeAvatarClicked(object sender, EventArgs e)
    {
        try
        {
            // Request permission for Android
            if (DeviceInfo.Current.Platform == DevicePlatform.Android)
            {
                PermissionStatus status;
                if (OperatingSystem.IsAndroidVersionAtLeast(33))
                    status = await Permissions.RequestAsync<Permissions.Media>();
                else
                    status = await Permissions.RequestAsync<Permissions.StorageRead>();

                if (status != PermissionStatus.Granted)
                {
                    await DisplayAlert("⚠️ تنبيه", "يجب منح صلاحية الوصول للصور", "حسنا");
                    return;
                }
            }

            // Pick image using MediaPicker (simpler and more reliable)
            var result = await MediaPicker.PickPhotoAsync();
            if (result == null) return;

            // Copy to app's local storage
            var destinationPath = Path.Combine(FileSystem.AppDataDirectory, $"{_player.PlayerId}_avatar.png");
            using (var srcStream = await result.OpenReadAsync())
            using (var destStream = File.Create(destinationPath))
            {
                await srcStream.CopyToAsync(destStream);
            }

            // Update player's avatar path (this triggers the AvatarChanged event)
            _player.AvatarPath = destinationPath;

            // Refresh the profile page UI
            LoadPlayerData();

            await DisplayAlert("تم", "تم تغيير الصورة بنجاح", "حسنا");
        }
        catch (Exception ex)
        {
            await DisplayAlert("خطأ", $"لا يمكن تغيير الصورة: {ex.Message}", "حسنا");
        }
    }
}