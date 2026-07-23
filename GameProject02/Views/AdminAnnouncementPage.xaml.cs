using GameProject02.Services;
using Microsoft.Maui.Controls;
using System;
using System.Threading.Tasks;

namespace GameProject02.Views;

public partial class AdminAnnouncementPage : ContentPage
{
    public AdminAnnouncementPage()
    {
        InitializeComponent();
    }

    private async void OnSendClicked(object sender, EventArgs e)
    {
        var message = MessageEntry.Text?.Trim();
        if (string.IsNullOrEmpty(message))
        {
            await DisplayAlert("خطأ", "الرجاء كتابة نص الإعلان", "موافق");
            return;
        }

        var admin = AccountService.GetCurrentPlayer();
        var adminName = admin?.Username ?? "System Admin";

        var (success, resultMessage) = await AdminService.SendSystemAnnouncementAsync(message, adminName);

        if (success)
        {
            MessageEntry.Text = "";
            await DisplayAlert("✅ نجاح", resultMessage, "موافق");
            MessagingCenter.Send(this, "RefreshNews");
            await Navigation.PopAsync(false);
        }
        else
        {
            await DisplayAlert("❌ فشل", resultMessage, "موافق");
        }
    }
}