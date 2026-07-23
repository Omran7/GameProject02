using GameProject02.Services;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using System;
using System.Threading.Tasks;

namespace GameProject02.Views;

public partial class ManagerReportPage : ContentPage
{
    private string _imageBase64 = string.Empty;

    public ManagerReportPage()
    {
        InitializeComponent();
    }

    private async void OnPickImageClicked(object sender, EventArgs e)
    {
        try
        {
            var result = await MediaPicker.PickPhotoAsync(new MediaPickerOptions { Title = "اختر صورة للإثبات" });
            if (result == null) return;

            using var stream = await result.OpenReadAsync();
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            var bytes = memoryStream.ToArray();
            _imageBase64 = Convert.ToBase64String(bytes);

            ImagePathLabel.Text = "✅ تم اختيار صورة";
            ImagePathLabel.TextColor = Color.FromArgb("#2ecc71");
        }
        catch (Exception ex)
        {
            await DisplayAlert("خطأ", $"فشل اختيار الصورة: {ex.Message}", "موافق");
        }
    }

    private async void OnSubmitClicked(object sender, EventArgs e)
    {
        var targetUsername = TargetUsernameEntry.Text?.Trim();
        var reason = ReasonEditor.Text?.Trim();

        if (string.IsNullOrEmpty(targetUsername))
        {
            await DisplayAlert("خطأ", "الرجاء إدخال اسم اللاعب المستهدف", "موافق");
            return;
        }
        if (string.IsNullOrEmpty(reason))
        {
            await DisplayAlert("خطأ", "الرجاء كتابة سبب التبليغ", "موافق");
            return;
        }

        var manager = AccountService.GetCurrentPlayer();
        if (manager == null || !AdminService.IsPlayerManager(manager))
        {
            await DisplayAlert("خطأ", "ليس لديك صلاحية لتقديم هذا الطلب", "موافق");
            return;
        }

        var target = await AccountService.GetPlayerByUsernameAsync(targetUsername);
        if (target == null)
        {
            await DisplayAlert("خطأ", "لم يتم العثور على لاعب بهذا الاسم", "موافق");
            return;
        }

        LoadingIndicator.IsRunning = true;
        LoadingIndicator.IsVisible = true;

        try
        {
            bool success = await AdminService.SubmitBanRequestAsync(manager, target.PlayerId, target.Username, reason, _imageBase64);
            await DisplayAlert(success ? "✅ تم الإرسال" : "❌ فشل",
                success ? "تم إرسال طلب الحظر إلى الإدارة للمراجعة" : "حدث خطأ أثناء الإرسال، حاول مرة أخرى",
                "موافق");
            if (success)
            {
                TargetUsernameEntry.Text = "";
                ReasonEditor.Text = "";
                _imageBase64 = string.Empty;
                ImagePathLabel.Text = "لم يتم اختيار صورة";
                ImagePathLabel.TextColor = Color.FromArgb("#bdc3c7");
                await Navigation.PopAsync(false);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("خطأ", $"فشل الإرسال: {ex.Message}", "موافق");
        }
        finally
        {
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
        }
    }
}