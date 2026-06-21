using GameProject02.Models;
using GameProject02.Services;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GameProject02.Views;

public partial class AdminBanPlayerPage : ContentPage
{
    private PlayerAccount _foundPlayer;
    private Dictionary<string, bool> _originalBanStates = new();

    public AdminBanPlayerPage()
    {
        InitializeComponent();
    }

    private async void OnSearchClicked(object sender, EventArgs e)
    {
        var username = UsernameEntry.Text?.Trim();
        if (string.IsNullOrEmpty(username))
        {
            await DisplayAlert("خطأ", "الرجاء إدخال اسم المستخدم", "موافق");
            return;
        }

        LoadingIndicator.IsRunning = true;
        LoadingIndicator.IsVisible = true;

        try
        {
            _foundPlayer = await AccountService.GetPlayerByUsernameAsync(username);
            if (_foundPlayer == null)
            {
                await DisplayAlert("خطأ", "لم يتم العثور على لاعب بهذا الاسم", "موافق");
                PlayerInfoLabel.IsVisible = false;
                ResetSwitches();
                return;
            }

            PlayerInfoLabel.Text = $"✅ {_foundPlayer.Username} (المستوى {_foundPlayer.Level}) - ID: {_foundPlayer.PlayerId.Substring(0, 8)}...";
            PlayerInfoLabel.IsVisible = true;

            // Load current ban states
            LoadBanStates();
        }
        catch (Exception ex)
        {
            await DisplayAlert("خطأ", $"فشل البحث: {ex.Message}", "موافق");
        }
        finally
        {
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
        }
    }

    private void LoadBanStates()
    {
        if (_foundPlayer == null) return;

        BanChat.IsToggled = _foundPlayer.IsBannedFromChat;
        BanProfile.IsToggled = _foundPlayer.IsBannedFromChangeProfilePic;
        BanNews.IsToggled = _foundPlayer.IsBannedFromNews;
        BanMessages.IsToggled = _foundPlayer.IsBannedFromPrivateMessages;

        // Save original states for "Reset" functionality
        _originalBanStates["chat"] = _foundPlayer.IsBannedFromChat;
        _originalBanStates["profile"] = _foundPlayer.IsBannedFromChangeProfilePic;
        _originalBanStates["news"] = _foundPlayer.IsBannedFromNews;
        _originalBanStates["messages"] = _foundPlayer.IsBannedFromPrivateMessages;
    }

    private void ResetSwitches()
    {
        BanChat.IsToggled = false;
        BanProfile.IsToggled = false;
        BanNews.IsToggled = false;
        BanMessages.IsToggled = false;
        _originalBanStates.Clear();
    }

    private async void OnApplyChangesClicked(object sender, EventArgs e)
    {
        if (_foundPlayer == null)
        {
            await DisplayAlert("خطأ", "الرجاء البحث عن لاعب أولاً", "موافق");
            return;
        }

        LoadingIndicator.IsRunning = true;
        LoadingIndicator.IsVisible = true;

        try
        {
            var changes = new List<(string banType, bool newState)>();
            bool hasChanges = false;

            // Check each ban type and collect changes
            if (BanChat.IsToggled != _originalBanStates.GetValueOrDefault("chat", false))
            {
                changes.Add(("chat", BanChat.IsToggled));
                hasChanges = true;
            }
            if (BanProfile.IsToggled != _originalBanStates.GetValueOrDefault("profile", false))
            {
                changes.Add(("profile", BanProfile.IsToggled));
                hasChanges = true;
            }
            if (BanNews.IsToggled != _originalBanStates.GetValueOrDefault("news", false))
            {
                changes.Add(("news", BanNews.IsToggled));
                hasChanges = true;
            }
            if (BanMessages.IsToggled != _originalBanStates.GetValueOrDefault("messages", false))
            {
                changes.Add(("messages", BanMessages.IsToggled));
                hasChanges = true;
            }

            if (!hasChanges)
            {
                await DisplayAlert("تنبيه", "لا توجد تغييرات لتطبيقها", "موافق");
                return;
            }

            // Confirm changes
            var confirm = await DisplayAlert("تأكيد التغييرات",
                $"سيتم تحديث {changes.Count} إجراء(ات) للاعب {_foundPlayer.Username}.\nهل أنت متأكد؟",
                "نعم", "إلغاء");
            if (!confirm) return;

            // Apply each change
            bool allSuccess = true;
            foreach (var (banType, newState) in changes)
            {
                bool success = await AdminService.BanPlayerAsync(_foundPlayer.PlayerId, banType, newState);
                if (!success) allSuccess = false;
            }

            if (allSuccess)
            {
                await DisplayAlert("✅ نجاح", "تم تطبيق جميع التغييرات بنجاح", "موافق");
                // Refresh the player data to update original states
                _foundPlayer = await AccountService.GetPlayerByUsernameAsync(_foundPlayer.Username);
                LoadBanStates(); // update the UI and original states
            }
            else
            {
                await DisplayAlert("❌ فشل جزئي", "حدثت بعض الأخطاء أثناء تطبيق التغييرات، يرجى المحاولة مرة أخرى", "موافق");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("خطأ", $"فشل تطبيق التغييرات: {ex.Message}", "موافق");
        }
        finally
        {
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
        }
    }

    private async void OnResetClicked(object sender, EventArgs e)
    {
        if (_foundPlayer == null)
        {
            ResetSwitches();
            return;
        }

        var confirm = await DisplayAlert("إعادة تعيين", "هل تريد إعادة تعيين جميع المفاتيح إلى حالتها الأصلية؟", "نعم", "إلغاء");
        if (confirm)
        {
            LoadBanStates(); // reload from found player
        }
    }
}