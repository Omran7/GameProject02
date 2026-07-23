using GameProject02.Models;
using GameProject02.Services;
using Microsoft.Maui.Controls;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GameProject02.Views
{
    public partial class GangSearchPage : ContentPage
    {
        private PlayerAccount _player;

        public GangSearchPage()
        {
            InitializeComponent();
            _player = AccountService.CurrentPlayer;
        }

        private async void OnSearchClicked(object sender, EventArgs e)
        {
            string query = SearchEntry.Text?.Trim() ?? "";
            if (string.IsNullOrEmpty(query))
            {
                StatusLabel.Text = "❌ الرجاء إدخال اسم أو رمز العصابة";
                return;
            }

            StatusLabel.Text = "⏳ جاري البحث...";
            ResultsList.ItemsSource = null;

            try
            {
                var results = await GangDatabaseService.SearchGangsAsync(query);
                ResultsList.ItemsSource = results;
                if (results.Count == 0)
                    StatusLabel.Text = "❌ لم يتم العثور على عصابة بهذا الاسم أو الرمز";
                else
                    StatusLabel.Text = $"✅ تم العثور على {results.Count} عصابة";
            }
            catch (Exception ex)
            {
                StatusLabel.Text = "⚠️ خطأ في البحث. حاول مرة أخرى.";
                System.Diagnostics.Debug.WriteLine($"[SEARCH] Error: {ex.Message}");
            }
        }

        private async void OnGangSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is GangObject gang)
            {
                if (_player == null)
                {
                    await DisplayAlert("خطأ", "لم يتم تسجيل الدخول", "موافق");
                    return;
                }

                if (!string.IsNullOrEmpty(_player.GangId))
                {
                    await DisplayAlert("تنبيه", "أنت بالفعل عضو في عصابة أخرى", "موافق");
                    return;
                }

                try
                {
                    bool sent = await GangDatabaseService.SendJoinRequestAsync(gang.GangId, _player.PlayerId, _player.Username);
                    await DisplayAlert(sent ? "✅ تم الإرسال" : "❌ فشل", sent ? "تم إرسال طلب الانضمام بنجاح. انتظر موافقة الزعيم." : "حدث خطأ أو طلب مسبق موجود", "موافق");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[JOIN] Exception: {ex.Message}");
                    await DisplayAlert("خطأ", "فشل إرسال الطلب", "موافق");
                }
                ResultsList.SelectedItem = null;
            }
        }

        private async void OnBackClicked(object sender, EventArgs e) => await Navigation.PopAsync(false);
        private async void OnHomeClicked(object sender, EventArgs e) => await Navigation.PopToRootAsync(false);
    }
}