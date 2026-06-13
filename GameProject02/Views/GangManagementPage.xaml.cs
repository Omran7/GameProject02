using GameProject02.Models;
using GameProject02.Services;
using Microsoft.Maui.Controls;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Storage;
using System.IO;

namespace GameProject02.Views;

public partial class GangManagementPage : ContentPage
{
    private PlayerAccount _player;
    private GangObject _gang;
    private ObservableCollection<GangManagementItem> _items;
    private string _currentTab = "requests";

    public GangManagementPage()
    {
        InitializeComponent();
        _ = LoadData();
    }

    private async Task LoadData()
    {
        try
        {
            _player = AccountService.CurrentPlayer;
            if (_player == null)
            {
                await DisplayAlert("خطأ", "الرجاء تسجيل الدخول أولاً", "موافق");
                await Navigation.PopToRootAsync();
                return;
            }
            if (string.IsNullOrEmpty(_player.GangId))
            {
                await DisplayAlert("خطأ", "أنت لست عضواً في أي عصابة", "موافق");
                await Navigation.PopToRootAsync();
                return;
            }
            _gang = await GangDatabaseService.GetGangAsync(_player.GangId);
            if (_gang == null)
            {
                await DisplayAlert("خطأ", "لم يتم العثور على بيانات العصابة", "موافق");
                await Navigation.PopToRootAsync();
                return;
            }
            await RefreshContent();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[MANAGEMENT] LoadData error: {ex}");
            await DisplayAlert("خطأ", "فشل تحميل البيانات", "موافق");
        }
    }

    private void OnTabClicked(object sender, EventArgs e)
    {
        if (sender == TabMembers) _currentTab = "members";
        else if (sender == TabRequests) _currentTab = "requests";
        else _currentTab = "settings";

        TabMembers.BackgroundColor = _currentTab == "members" ? Color.FromArgb("#3498db") : Color.FromArgb("#2c2c2c");
        TabRequests.BackgroundColor = _currentTab == "requests" ? Color.FromArgb("#3498db") : Color.FromArgb("#2c2c2c");
        TabSettings.BackgroundColor = _currentTab == "settings" ? Color.FromArgb("#3498db") : Color.FromArgb("#2c2c2c");
        _ = RefreshContent();
    }

    private async Task RefreshContent()
    {
        _items = new ObservableCollection<GangManagementItem>();

        if (_currentTab == "members")
        {
            var currentPlayerId = _player.PlayerId;
            var currentPos = _gang.GetPosition(currentPlayerId);
            bool isLeader = currentPos == GangPosition.Leader;
            bool isCoLeader = currentPos == GangPosition.CoLeader;

            foreach (var kvp in _gang.MembersWithPositions)
            {
                string targetId = kvp.Key;
                GangPosition targetPos = kvp.Value;
                bool isCurrentPlayer = targetId == currentPlayerId;
                string displayName = isCurrentPlayer ? "أنت" : (AccountService.GetPlayerById(targetId)?.Username ?? targetId);
                var item = new GangManagementItem { Label1 = displayName, Label2 = GetPositionName(targetPos) };

                if (CanKickTarget(currentPos, targetPos, isCurrentPlayer))
                {
                    item.Btn1Text = "طرد"; item.Action1 = $"kick:{targetId}"; item.HasBtn1 = true;
                }
                if (!isCurrentPlayer && targetPos != GangPosition.Leader && (isLeader || isCoLeader))
                {
                    if (isLeader && targetPos != GangPosition.CoLeader)
                    {
                        item.Btn2Text = "نائب قائد"; item.Action2 = $"promoteToCoLeader:{targetId}"; item.HasBtn2 = true;
                    }
                    if (targetPos != GangPosition.Vice && targetPos != GangPosition.CoLeader)
                    {
                        item.Btn3Text = "نائب"; item.Action3 = $"promoteToVice:{targetId}"; item.HasBtn3 = true;
                    }
                    if (targetPos != GangPosition.Elder && targetPos != GangPosition.Vice && targetPos != GangPosition.CoLeader)
                    {
                        item.Btn4Text = "حكيم"; item.Action4 = $"promoteToElder:{targetId}"; item.HasBtn4 = true;
                    }
                }
                bool canDemote = !isCurrentPlayer && targetPos != GangPosition.Leader &&
                                 (isLeader || (isCoLeader && targetPos != GangPosition.CoLeader && targetPos != GangPosition.Leader));
                if (canDemote) { item.Btn6Text = "تنزيل رتبة"; item.Action6 = $"demote:{targetId}"; item.HasBtn6 = true; }
                if (isLeader && !isCurrentPlayer && targetPos != GangPosition.Leader) { item.Btn5Text = "نقل الزعامة"; item.Action5 = $"transfer:{targetId}"; item.HasBtn5 = true; }
                _items.Add(item);
            }
        }
        else if (_currentTab == "requests")
        {
            bool canAccept = GangService.CanPerformAction(_gang, _player.PlayerId, GangAction.AcceptJoinRequest);
            if (canAccept)
            {
                var reqs = await GangDatabaseService.GetJoinRequestsAsync(_gang.GangId);
                foreach (var req in reqs)
                {
                    string displayName = string.IsNullOrEmpty(req.PlayerName) ? req.PlayerId : req.PlayerName;
                    _items.Add(new GangManagementItem
                    {
                        Label1 = displayName,
                        Btn1Text = "قبول",
                        Action1 = $"accept:{req.PlayerId}",
                        HasBtn1 = true,
                        Btn2Text = "رفض",
                        Action2 = $"reject:{req.PlayerId}",
                        HasBtn2 = true
                    });
                }
                if (_items.Count == 0) _items.Add(new GangManagementItem { Label1 = "لا توجد طلبات" });
            }
            else _items.Add(new GangManagementItem { Label1 = "ليس لديك صلاحية لعرض الطلبات" });
        }
        else // settings
        {
            bool canChangeData = GangService.CanPerformAction(_gang, _player.PlayerId, GangAction.ChangeGangData);
            bool canDisband = GangService.CanPerformAction(_gang, _player.PlayerId, GangAction.DisbandGang);
            if (canChangeData) _items.Add(new GangManagementItem { Label1 = "تغيير الاسم", Btn1Text = "تعديل", Action1 = "change_name", HasBtn1 = true });
            if (canChangeData) _items.Add(new GangManagementItem { Label1 = "تغيير الرمز", Btn1Text = "تعديل", Action1 = "change_tag", HasBtn1 = true });
            if (canChangeData) _items.Add(new GangManagementItem { Label1 = "تغيير الصورة", Btn1Text = "اختيار", Action1 = "change_image", HasBtn1 = true });
            if (canDisband) _items.Add(new GangManagementItem { Label1 = "حل العصابة", Btn1Text = "حذف", Action1 = "dissolve", HasBtn1 = true });
            if (_items.Count == 0) _items.Add(new GangManagementItem { Label1 = "ليس لديك صلاحيات إدارة" });
        }
        ContentList.ItemsSource = _items;
    }

    private bool CanKickTarget(GangPosition currentPos, GangPosition targetPos, bool isCurrentPlayer)
    {
        if (isCurrentPlayer) return false;
        if (targetPos == GangPosition.Leader) return false;
        switch (currentPos)
        {
            case GangPosition.Leader: return true;
            case GangPosition.CoLeader: return targetPos != GangPosition.CoLeader;
            case GangPosition.Vice: return targetPos != GangPosition.CoLeader && targetPos != GangPosition.Vice;
            case GangPosition.Elder: return targetPos == GangPosition.Member || targetPos == GangPosition.Officer;
            default: return false;
        }
    }

    private string GetPositionName(GangPosition pos) => pos switch
    {
        GangPosition.Leader => "زعيم",
        GangPosition.CoLeader => "نائب القائد",
        GangPosition.Vice => "نائب",
        GangPosition.Elder => "حكيم",
        GangPosition.Officer => "ضابط",
        GangPosition.Member => "عضو",
        _ => "لا شيء"
    };

    private async void OnActionClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is string action)
        {
            // Safety checks
            if (_gang == null) { await DisplayAlert("خطأ", "بيانات العصابة غير متوفرة", "موافق"); return; }
            if (_player == null) { await DisplayAlert("خطأ", "يرجى تسجيل الدخول", "موافق"); return; }
            if (string.IsNullOrEmpty(_gang.GangId)) { await DisplayAlert("خطأ", "معرف العصابة غير صالح", "موافق"); return; }

            // Accept
            if (action.StartsWith("accept:"))
            {
                var parts = action.Split(':');
                if (parts.Length < 2) { await DisplayAlert("خطأ", "بيانات الطلب غير صالحة", "موافق"); return; }
                string playerId = parts[1];
                if (string.IsNullOrEmpty(playerId)) { await DisplayAlert("خطأ", "معرف اللاعب غير صالح", "موافق"); return; }

                bool confirm = await DisplayAlert("تأكيد", "قبول طلب الانضمام؟", "نعم", "لا");
                if (!confirm) return;

                try
                {
                    bool success = await GangDatabaseService.ProcessJoinRequestAsync(_gang.GangId, playerId, true);
                    if (success)
                    {
                        // Reload gang and player data
                        _gang = await GangDatabaseService.GetGangAsync(_gang.GangId);
                        if (_gang != null)
                        {
                            _player.GangObject = _gang;
                            AccountService.CurrentPlayer = _player;
                        }
                        await RefreshContent();
                        // Send refresh message
                        try { MessagingCenter.Send(this, "RefreshGangProfile"); } catch { }
                        await DisplayAlert("✅ نجاح", "تم قبول العضو", "موافق");
                    }
                    else
                    {
                        await DisplayAlert("❌ فشل", "حدث خطأ أثناء قبول الطلب", "موافق");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[ACCEPT ERROR] {ex}");
                    await DisplayAlert("خطأ", $"حدث خطأ: {ex.Message}", "موافق");
                }
            }
            // Reject
            else if (action.StartsWith("reject:"))
            {
                var parts = action.Split(':');
                if (parts.Length < 2) return;
                string playerId = parts[1];
                try
                {
                    bool success = await GangDatabaseService.ProcessJoinRequestAsync(_gang.GangId, playerId, false);
                    if (success) await RefreshContent();
                    await DisplayAlert(success ? "✅ تم الرفض" : "❌ فشل", "", "موافق");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[REJECT ERROR] {ex}");
                    await DisplayAlert("خطأ", ex.Message, "موافق");
                }
            }
            // Kick
            else if (action.StartsWith("kick:"))
            {
                var parts = action.Split(':');
                if (parts.Length < 2) return;
                string pid = parts[1];
                bool confirm = await DisplayAlert("تأكيد", "هل تريد طرد هذا العضو؟", "نعم", "لا");
                if (confirm)
                {
                    GangDatabaseService.UpdateMemberPosition(_gang.GangId, pid, null);
                    _gang = await GangDatabaseService.GetGangAsync(_gang.GangId);
                    await RefreshContent();
                    try { MessagingCenter.Send(this, "RefreshGangProfile"); } catch { }
                }
            }
            // Promote to CoLeader
            else if (action.StartsWith("promoteToCoLeader:"))
            {
                var parts = action.Split(':');
                if (parts.Length < 2) return;
                string pid = parts[1];
                if (GangService.PromoteTo(_gang, pid, GangPosition.CoLeader, _player))
                {
                    await SaveGangAndRefresh();
                    await DisplayAlert("نجاح", "تمت الترقية إلى نائب القائد", "موافق");
                }
                else await DisplayAlert("فشل", "", "موافق");
            }
            // Promote to Vice
            else if (action.StartsWith("promoteToVice:"))
            {
                var parts = action.Split(':');
                if (parts.Length < 2) return;
                string pid = parts[1];
                if (GangService.PromoteTo(_gang, pid, GangPosition.Vice, _player))
                {
                    await SaveGangAndRefresh();
                    await DisplayAlert("نجاح", "تمت الترقية إلى نائب", "موافق");
                }
                else await DisplayAlert("فشل", "", "موافق");
            }
            // Promote to Elder
            else if (action.StartsWith("promoteToElder:"))
            {
                var parts = action.Split(':');
                if (parts.Length < 2) return;
                string pid = parts[1];
                if (GangService.PromoteTo(_gang, pid, GangPosition.Elder, _player))
                {
                    await SaveGangAndRefresh();
                    await DisplayAlert("نجاح", "تمت الترقية إلى حكيم", "موافق");
                }
                else await DisplayAlert("فشل", "", "موافق");
            }
            // Demote
            else if (action.StartsWith("demote:"))
            {
                var parts = action.Split(':');
                if (parts.Length < 2) return;
                string pid = parts[1];
                var targetPos = _gang.GetPosition(pid);
                GangPosition newPos = targetPos switch
                {
                    GangPosition.CoLeader => GangPosition.Vice,
                    GangPosition.Vice => GangPosition.Elder,
                    GangPosition.Elder => GangPosition.Officer,
                    GangPosition.Officer => GangPosition.Member,
                    _ => GangPosition.Member
                };
                if (GangService.PromoteTo(_gang, pid, newPos, _player))
                {
                    await SaveGangAndRefresh();
                    await DisplayAlert("نجاح", $"تم تنزيل العضو إلى {GetPositionName(newPos)}", "موافق");
                }
                else await DisplayAlert("فشل", "", "موافق");
            }
            // Transfer leadership
            else if (action.StartsWith("transfer:"))
            {
                var parts = action.Split(':');
                if (parts.Length < 2) return;
                string pid = parts[1];
                bool confirm = await DisplayAlert("نقل الزعامة", "هل أنت متأكد؟ ستصبح عضواً عادياً", "نعم", "لا");
                if (confirm && GangService.TransferLeadership(_gang, pid, _player))
                {
                    await SaveGangAndRefresh();
                    await DisplayAlert("نجاح", "تم نقل الزعامة", "موافق");
                    await Navigation.PopToRootAsync();
                }
                else await DisplayAlert("فشل", "", "موافق");
            }
            // Change name
            else if (action == "change_name")
            {
                string newName = await DisplayPromptAsync("تغيير الاسم", "أدخل الاسم الجديد (4-15 حرف)", maxLength: 15);
                if (!string.IsNullOrEmpty(newName) && GangService.ChangeGangName(_gang, newName, _player))
                {
                    await SaveGangAndRefresh();
                    await DisplayAlert("نجاح", "تم تغيير الاسم", "موافق");
                }
                else await DisplayAlert("خطأ", "الاسم غير صالح", "موافق");
            }
            // Change tag
            else if (action == "change_tag")
            {
                string newTag = await DisplayPromptAsync("تغيير الرمز", "أدخل 3 أحرف", maxLength: 3);
                if (!string.IsNullOrEmpty(newTag) && GangService.ChangeGangTag(_gang, newTag, _player))
                {
                    await SaveGangAndRefresh();
                    await DisplayAlert("نجاح", "تم تغيير الرمز", "موافق");
                }
                else await DisplayAlert("خطأ", "الرمز غير صالح", "موافق");
            }
            // Change image (stub)
            else if (action == "change_image")
            {
                try
                {
                    // Request permission
                    if (DeviceInfo.Platform == DevicePlatform.Android)
                    {
                        var status = await Permissions.RequestAsync<Permissions.StorageRead>();
                        if (status != PermissionStatus.Granted)
                        {
                            await DisplayAlert("خطأ", "لم يتم منح صلاحية الوصول إلى الصور", "موافق");
                            return;
                        }
                    }

                    var result = await MediaPicker.PickPhotoAsync(new MediaPickerOptions
                    {
                        Title = "اختر صورة للعصابة"
                    });
                    if (result != null)
                    {
                        bool success = await GangService.ChangeGangImageAsync(_gang, result.FullPath, _player);
                        if (success)
                        {
                            _gang = await GangDatabaseService.GetGangAsync(_gang.GangId);
                            _player.GangObject = _gang;
                            AccountService.CurrentPlayer = _player;
                            await RefreshContent();
                            MessagingCenter.Send(this, "RefreshGangProfile");
                            await DisplayAlert("نجاح", "تم تغيير صورة العصابة", "موافق");
                        }
                        else
                        {
                            await DisplayAlert("فشل", "حدث خطأ أثناء تغيير الصورة. تأكد من أن الصورة صالحة.", "موافق");
                        }
                    }
                }
                catch (Exception ex)
                {
                    await DisplayAlert("خطأ", $"فشل اختيار الصورة: {ex.Message}", "موافق");
                }
            }
            // Dissolve gang
            else if (action == "dissolve")
            {
                bool confirm = await DisplayAlert("حل العصابة", "هل أنت متأكد؟ لا يمكن التراجع", "نعم", "لا");
                if (confirm && GangService.DisbandGang(_gang, _player))
                {
                    await Navigation.PopToRootAsync();
                }
            }
        }
    }

    private async Task SaveGangAndRefresh()
    {
        await GangDatabaseService.SaveGangAsync(_gang);
        _gang = await GangDatabaseService.GetGangAsync(_gang.GangId);
        _player.GangObject = _gang;
        AccountService.CurrentPlayer = _player;
        await RefreshContent();
        try { MessagingCenter.Send(this, "RefreshGangProfile"); } catch { }
    }

    private async void OnBackClicked(object sender, EventArgs e) => await Navigation.PopAsync();
    private async void OnHomeClicked(object sender, EventArgs e) => await Navigation.PopToRootAsync();
}

public class GangManagementItem
{
    public string Label1 { get; set; } = "";
    public string Label2 { get; set; } = "";
    public string Btn1Text { get; set; } = "";
    public string Action1 { get; set; } = "";
    public bool HasBtn1 { get; set; } = false;
    public string Btn2Text { get; set; } = "";
    public string Action2 { get; set; } = "";
    public bool HasBtn2 { get; set; } = false;
    public string Btn3Text { get; set; } = "";
    public string Action3 { get; set; } = "";
    public bool HasBtn3 { get; set; } = false;
    public string Btn4Text { get; set; } = "";
    public string Action4 { get; set; } = "";
    public bool HasBtn4 { get; set; } = false;
    public string Btn5Text { get; set; } = "";
    public string Action5 { get; set; } = "";
    public bool HasBtn5 { get; set; } = false;
    public string Btn6Text { get; set; } = "";
    public string Action6 { get; set; } = "";
    public bool HasBtn6 { get; set; } = false;
}