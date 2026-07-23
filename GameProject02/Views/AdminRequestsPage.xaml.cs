using GameProject02.Models;
using GameProject02.Services;
using Microsoft.Maui.Controls;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GameProject02.Views;

public partial class AdminRequestsPage : ContentPage
{
    public ObservableCollection<AdminRequest> Requests { get; } = new();
    public ICommand ApproveCommand { get; }
    public ICommand RejectCommand { get; }
    public ICommand ViewImageCommand { get; }

    public AdminRequestsPage()
    {
        InitializeComponent();
        BindingContext = this;
        ApproveCommand = new Command<AdminRequest>(OnApprove);
        RejectCommand = new Command<AdminRequest>(OnReject);
        ViewImageCommand = new Command<AdminRequest>(OnViewImage);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadRequests();
    }

    private async Task LoadRequests()
    {
        try
        {
            Requests.Clear();
            var pending = await AdminService.GetPendingRequestsAsync();
            Debug.WriteLine($"[REQUESTS] Found {pending.Count} pending requests");
            foreach (var req in pending)
            {
                Debug.WriteLine($"[REQUESTS] Adding: {req.PlayerName}, Type={req.RequestType}, Target={req.TargetPlayerName}, Id={req.Id}");
                Requests.Add(req);
            }
            RequestsList.ItemsSource = null;
            RequestsList.ItemsSource = Requests;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[REQUESTS] Load error: {ex.Message}");
            await DisplayAlert("خطأ", "فشل تحميل الطلبات", "موافق");
        }
    }

    private async void OnApprove(AdminRequest req)
    {
        try
        {
            var admin = AccountService.GetCurrentPlayer();
            if (admin == null || !AdminService.IsPlayerAdmin(admin))
            {
                await DisplayAlert("خطأ", "ليس لديك صلاحية", "موافق");
                return;
            }

            List<string> banTypes = null;

            if (req.RequestType == AdminRequestType.BanRequest)
            {
                var banOptions = new string[]
                {
                    "حظر الدردشة",
                    "حظر تغيير الصورة",
                    "حظر الأخبار",
                    "حظر الرسائل الخاصة",
                    "إلغاء"
                };
                var choice = await DisplayActionSheet("اختر نوع الحظر", "إلغاء", null, banOptions);
                if (choice == "إلغاء" || choice == null)
                    return;

                string banType = choice switch
                {
                    "حظر الدردشة" => "chat",
                    "حظر تغيير الصورة" => "profile",
                    "حظر الأخبار" => "news",
                    "حظر الرسائل الخاصة" => "messages",
                    _ => "chat"
                };
                banTypes = new List<string> { banType };
            }

            string note = await DisplayPromptAsync("قبول الطلب", "إضافة ملاحظة (اختياري):", "تمت المراجعة");
            if (note == null)
                return;

            Debug.WriteLine($"[REQUESTS] Approving request {req.Id} by admin {admin.PlayerId}");
            bool success = await AdminService.ReviewAdminRequestAsync(
                req.Id,
                admin.PlayerId,
                true,
                note,
                banTypes
            );

            await DisplayAlert(success ? "نجاح" : "فشل",
                               success ? "تم معالجة الطلب" : "حدث خطأ أثناء المعالجة",
                               "موافق");
            if (success)
                await LoadRequests();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[REQUESTS] Approve exception: {ex.Message}");
            await DisplayAlert("خطأ", $"حدث استثناء: {ex.Message}", "موافق");
        }
    }

    private async void OnReject(AdminRequest req)
    {
        try
        {
            var admin = AccountService.GetCurrentPlayer();
            if (admin == null || !AdminService.IsPlayerAdmin(admin))
            {
                await DisplayAlert("خطأ", "ليس لديك صلاحية", "موافق");
                return;
            }

            string note = await DisplayPromptAsync("رفض الطلب", "سبب الرفض:", "غير مطابق للشروط");
            if (note == null)
                return;

            Debug.WriteLine($"[REQUESTS] Rejecting request {req.Id} by admin {admin.PlayerId}");
            bool success = await AdminService.ReviewAdminRequestAsync(
                req.Id,
                admin.PlayerId,
                false,
                note
            );

            await DisplayAlert(success ? "نجاح" : "فشل",
                               success ? "تم رفض الطلب" : "حدث خطأ أثناء الرفض",
                               "موافق");
            if (success)
                await LoadRequests();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[REQUESTS] Reject exception: {ex.Message}");
            await DisplayAlert("خطأ", $"حدث استثناء: {ex.Message}", "موافق");
        }
    }

    private async void OnViewImage(AdminRequest req)
    {
        try
        {
            if (string.IsNullOrEmpty(req.ImageBase64))
            {
                await DisplayAlert("تنبيه", "لا توجد صورة مرفقة", "موافق");
                return;
            }
            var bytes = Convert.FromBase64String(req.ImageBase64);
            var stream = new MemoryStream(bytes);
            var image = ImageSource.FromStream(() => stream);
            var page = new ContentPage();
            page.Content = new Image { Source = image, Aspect = Aspect.AspectFit };
            await Navigation.PushAsync(page, false);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[REQUESTS] ViewImage error: {ex.Message}");
            await DisplayAlert("خطأ", "تعذر عرض الصورة", "موافق");
        }
    }
}