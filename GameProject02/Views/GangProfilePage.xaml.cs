using GameProject02.Models;
using GameProject02.Services;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace GameProject02.Views;

public partial class GangProfilePage : ContentPage
{
    private PlayerAccount _player;
    private GangObject _gang;

    public GangProfilePage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _player = AccountService.GetCurrentPlayer();
        if (_player != null && _player.GangObject != null)
        {
            _gang = _player.GangObject;
            BindingContext = _gang;
            LoadMembers();

            // ✅ إظهار زر الإعدادات إذا كان المستخدم لديه صلاحية قبول الطلبات (أي ليس عضواً عادياً)
            bool hasManagementRights = GangService.CanPerformAction(_gang, _player.PlayerId, GangAction.AcceptJoinRequest);
            SettingsButton.IsVisible = hasManagementRights;

            MessagingCenter.Subscribe<GangObject>(this, "GangDataUpdated", (gang) =>
            {
                if (_gang != null && _gang.GangId == gang.GangId)
                {
                    LoadMembers();
                    SettingsButton.IsVisible = GangService.CanPerformAction(_gang, _player.PlayerId, GangAction.AcceptJoinRequest);
                }
            });
        }
        else
        {
            SettingsButton.IsVisible = false;
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        MessagingCenter.Unsubscribe<GangObject>(this, "GangDataUpdated");
    }

    private void OnGangPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // إذا تغيرت قائمة الأعضاء، نعيد تحميل القائمة
        if (e.PropertyName == nameof(GangObject.MembersWithPositions) ||
            e.PropertyName == nameof(GangObject.MembersCount))
        {
            LoadMembers();
        }
    }

    private void LoadMembers()
    {
        if (_gang == null) return;
        var members = GangService.GetGangMembers(_gang);
        MembersList.ItemsSource = new ObservableCollection<GangMemberInfo>(members);
    }

    // جميع الأحداث (Buttons clicks)
    private async void OnGangMarketClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new GangMarketCategoriesPage()); // تأكد من وجود الصفحة
    }

    private async void OnDonateClicked(object sender, EventArgs e)
    {
        if (_gang == null || _player == null) return;

        string amountStr = await DisplayPromptAsync("تبرع للعصابة", "أدخل المبلغ الذي تريد التبرع به (ذهب):", keyboard: Keyboard.Numeric);
        if (string.IsNullOrEmpty(amountStr)) return;

        if (!int.TryParse(amountStr, out int amount) || amount <= 0)
        {
            await DisplayAlert("خطأ", "الرجاء إدخال مبلغ صحيح", "موافق");
            return;
        }

        var result = GangService.DonateToGang(_player, _gang, amount);
        await DisplayAlert(result.success ? "✅ نجاح" : "❌ فشل", result.message, "موافق");
    }

    private async void OnBackClicked(object sender, EventArgs e) => await Navigation.PopAsync();

    private async void OnHomeClicked(object sender, EventArgs e) => await Navigation.PopToRootAsync();

    private async void OnLevelUpgradeClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new GangLevelUpgradePage());
    }

    private async void OnSettingsClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new GangManagementPage());
    }

    private async void OnOpenMilitiaPageClicked(object sender, EventArgs e)
    {
        if (_gang != null)
            await Navigation.PushAsync(new GangMilitiaPage(_gang));
        else
            await DisplayAlert("خطأ", "بيانات العصابة غير متوفرة", "موافق");
    }

    private async void OnSkillsClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new GangSkillsPage());
    }
}