using GameProject02.Models;
using GameProject02.Services;
using Microsoft.Maui.Controls;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GameProject02.Views;

public partial class GangSearchPage : ContentPage
{
    private PlayerAccount _player;

    public GangSearchPage()
    {
        InitializeComponent();
        _player = AccountService.GetCurrentPlayer();
    }

    // ✅ SEARCH BUTTON HANDLER
    private async void OnSearchClicked(object sender, EventArgs e)
    {
        string query = SearchEntry.Text?.Trim() ?? "";

        if (string.IsNullOrEmpty(query))
        {
            StatusLabel.Text = "❌ الرجاء إدخال اسم أو رمز العصابة";
            return;
        }

        StatusLabel.Text = "⏳ جاري البحث...";
        await Task.Delay(300); // Small delay for UX

        try
        {
            var results = GangDatabaseService.SearchGangs(query);
            ResultsList.ItemsSource = results;

            if (results.Count == 0)
                StatusLabel.Text = "❌ لم يتم العثور على عصابة بهذا الاسم";
            else
                StatusLabel.Text = $"✅ تم العثور على {results.Count} عصابة";
        }
        catch (Exception ex)
        {
            StatusLabel.Text = $"❌ خطأ في البحث: {ex.Message}";
        }
    }

    // ✅ JOIN REQUEST HANDLER (WHEN GANG IS SELECTED)
    private async void OnGangSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is GangObject gang)
        {
            if (_player == null)
            {
                await DisplayAlert("خطأ", "لم يتم تسجيل الدخول", "موافق");
                return;
            }

            // Send join request
            bool sent = GangDatabaseService.SendJoinRequest(gang.GangId, _player.PlayerId, _player.Username);

            await DisplayAlert(
                sent ? "✅ تم الإرسال" : "❌ فشل",
                sent ? "تم إرسال طلب الانضمام بنجاح. انتظر موافقة الزعيم." : "أنت عضو بالفعل أو أرسلت طلباً مسبقاً.",
                "موافق");

            // Clear selection
            ResultsList.SelectedItem = null;
        }
    }

    private async void OnBackClicked(object sender, EventArgs e) => await Navigation.PopAsync();
    private async void OnHomeClicked(object sender, EventArgs e) => await Navigation.PopToRootAsync();
}