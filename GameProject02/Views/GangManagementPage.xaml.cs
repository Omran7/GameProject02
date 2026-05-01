using GameProject02.Models;
using GameProject02.Services;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Maui.Media;

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
        LoadData();
    }

    private void LoadData()
    {
        _player = AccountService.GetCurrentPlayer();
        if (_player == null || _player.GangObject == null) return;
        _gang = _player.GangObject;
        RefreshContent();
    }

    private void OnTabClicked(object sender, EventArgs e)
    {
        _currentTab = sender == TabMembers ? "members" : sender == TabRequests ? "requests" : "settings";
        var activeColor = Color.FromArgb("#3498db");
        var inactiveColor = Color.FromArgb("#2c2c2c");
        TabMembers.BackgroundColor = _currentTab == "members" ? activeColor : inactiveColor;
        TabRequests.BackgroundColor = _currentTab == "requests" ? activeColor : inactiveColor;
        TabSettings.BackgroundColor = _currentTab == "settings" ? activeColor : inactiveColor;
        RefreshContent();
    }

    private void RefreshContent()
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
                var item = new GangManagementItem
                {
                    Label1 = displayName,
                    Label2 = GetPositionName(targetPos),
                };

                // زر الطرد
                if (CanKickTarget(currentPos, targetPos, isCurrentPlayer))
                {
                    item.Btn1Text = "طرد";
                    item.Action1 = $"kick:{targetId}";
                    item.HasBtn1 = true;
                }

                // أزرار الترقية (فقط للقائد ونائب القائد، ولا يمكن ترقية القائد أو النفس)
                if (!isCurrentPlayer && targetPos != GangPosition.Leader && (isLeader || isCoLeader))
                {
                    // ترقية إلى نائب القائد (CoLeader) – فقط القائد
                    if (isLeader && targetPos != GangPosition.CoLeader)
                    {
                        item.Btn2Text = "نائب قائد";
                        item.Action2 = $"promoteToCoLeader:{targetId}";
                        item.HasBtn2 = true;
                    }
                    // ترقية إلى نائب (Vice) – القائد ونائب القائد
                    if (targetPos != GangPosition.Vice && targetPos != GangPosition.CoLeader)
                    {
                        item.Btn3Text = "نائب";
                        item.Action3 = $"promoteToVice:{targetId}";
                        item.HasBtn3 = true;
                    }
                    // ترقية إلى حكيم (Elder) – القائد ونائب القائد
                    if (targetPos != GangPosition.Elder && targetPos != GangPosition.Vice && targetPos != GangPosition.CoLeader)
                    {
                        item.Btn4Text = "حكيم";
                        item.Action4 = $"promoteToElder:{targetId}";
                        item.HasBtn4 = true;
                    }
                }

                // زر التنزيل (Demote) – فقط للقائد ونائب القائد، ولا يمكن تنزيل القائد أو النفس
                bool canDemote = false;
                if (!isCurrentPlayer && targetPos != GangPosition.Leader)
                {
                    if (isLeader)
                        canDemote = true;
                    else if (isCoLeader && targetPos != GangPosition.CoLeader && targetPos != GangPosition.Leader)
                        canDemote = true;
                }
                if (canDemote)
                {
                    item.Btn6Text = "تنزيل رتبة";
                    item.Action6 = $"demote:{targetId}";
                    item.HasBtn6 = true;
                }

                // زر نقل الزعامة (فقط للقائد)
                if (isLeader && !isCurrentPlayer && targetPos != GangPosition.Leader)
                {
                    item.Btn5Text = "نقل الزعامة";
                    item.Action5 = $"transfer:{targetId}";
                    item.HasBtn5 = true;
                }

                _items.Add(item);
            }
        }
        else if (_currentTab == "requests")
        {
            bool canAccept = GangService.CanPerformAction(_gang, _player.PlayerId, GangAction.AcceptJoinRequest);
            if (canAccept)
            {
                var reqs = GangDatabaseService.GetJoinRequests(_gang.GangId);
                foreach (var req in reqs)
                {
                    string displayName = !string.IsNullOrEmpty(req.PlayerName) ? req.PlayerName : req.PlayerId;
                    _items.Add(new GangManagementItem
                    {
                        Label1 = displayName,
                        Label2 = "",
                        Btn1Text = "قبول",
                        Action1 = $"accept:{req.PlayerId}",
                        HasBtn1 = true,
                        Btn2Text = "رفض",
                        Action2 = $"reject:{req.PlayerId}",
                        HasBtn2 = true
                    });
                }
                if (_items.Count == 0)
                    _items.Add(new GangManagementItem { Label1 = "لا توجد طلبات" });
            }
            else
            {
                _items.Add(new GangManagementItem { Label1 = "ليس لديك صلاحية لعرض الطلبات" });
            }
        }
        else // settings
        {
            bool canChangeData = GangService.CanPerformAction(_gang, _player.PlayerId, GangAction.ChangeGangData);
            bool canDisband = GangService.CanPerformAction(_gang, _player.PlayerId, GangAction.DisbandGang);
            if (canChangeData)
                _items.Add(new GangManagementItem { Label1 = "تغيير الاسم", Btn1Text = "تعديل", Action1 = "change_name", HasBtn1 = true });
            if (canChangeData)
                _items.Add(new GangManagementItem { Label1 = "تغيير الرمز", Btn1Text = "تعديل", Action1 = "change_tag", HasBtn1 = true });
            if (canChangeData)
                _items.Add(new GangManagementItem { Label1 = "تغيير الصورة", Btn1Text = "اختيار", Action1 = "change_image", HasBtn1 = true });
            if (canDisband)
                _items.Add(new GangManagementItem { Label1 = "حل العصابة", Btn1Text = "حذف", Action1 = "dissolve", HasBtn1 = true });
            if (_items.Count == 0)
                _items.Add(new GangManagementItem { Label1 = "ليس لديك صلاحيات إدارة" });
        }
        ContentList.ItemsSource = _items;
    }
    // دالة مساعدة للتحقق من إمكانية طرد عضو معين
    private bool CanKickTarget(GangPosition currentPos, GangPosition targetPos, bool isCurrentPlayer)
    {
        if (isCurrentPlayer) return false; // لا يمكن طرد النفس
        if (targetPos == GangPosition.Leader) return false; // لا يمكن طرد القائد

        switch (currentPos)
        {
            case GangPosition.Leader:
                return true;
            case GangPosition.CoLeader:
                return targetPos != GangPosition.CoLeader;
            case GangPosition.Vice:
                return targetPos != GangPosition.CoLeader && targetPos != GangPosition.Vice;
            case GangPosition.Elder:
                return targetPos == GangPosition.Member || targetPos == GangPosition.Officer;
            default:
                return false;
        }
    }

    private string GetPositionName(GangPosition pos)
    {
        return pos switch
        {
            GangPosition.Leader => "زعيم",
            GangPosition.CoLeader => "نائب القائد",
            GangPosition.Vice => "نائب",
            GangPosition.Elder => "حكيم",
            GangPosition.Officer => "ضابط",
            GangPosition.Member => "عضو",
            _ => "لا شيء"
        };
    }

    private async void OnActionClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is string action)
        {
            // قبول / رفض طلبات الانضمام
            if (action.StartsWith("accept:") || action.StartsWith("reject:"))
            {
                if (!GangService.CanPerformAction(_gang, _player.PlayerId, GangAction.AcceptJoinRequest))
                {
                    await DisplayAlert("⚠️", "ليس لديك صلاحية لقبول أو رفض الطلبات", "موافق");
                    return;
                }
                var pid = action.Split(':')[1];
                bool accept = action.StartsWith("accept:");
                bool success = GangDatabaseService.ProcessJoinRequest(_gang.GangId, pid, accept);
                await DisplayAlert(success ? "✅ نجاح" : "❌ فشل", success ? "تمت المعالجة" : "فشل المعالجة", "موافق");
            }
            // طرد عضو
            else if (action.StartsWith("kick:"))
            {
                var pid = action.Split(':')[1];
                var targetPos = _gang.GetPosition(pid);
                if (!CanKickTarget(_gang.GetPosition(_player.PlayerId), targetPos, false))
                {
                    await DisplayAlert("⚠️", "ليس لديك صلاحية لطرد هذا العضو", "موافق");
                    return;
                }
                bool confirm = await DisplayAlert("تأكيد الطرد", "هل أنت متأكد من طرد هذا العضو؟", "نعم", "لا");
                if (confirm)
                {
                    GangDatabaseService.UpdateMemberPosition(_gang.GangId, pid, null);
                    await DisplayAlert("✅ تم الطرد", "تم طرد العضو من العصابة", "موافق");
                }
            }
            // ترقية إلى نائب القائد
            else if (action.StartsWith("promoteToCoLeader:"))
            {
                var pid = action.Split(':')[1];
                if (!GangService.CanPerformAction(_gang, _player.PlayerId, GangAction.PromoteMember))
                {
                    await DisplayAlert("⚠️", "ليس لديك صلاحية للترقية", "موافق");
                    return;
                }
                if (GangService.PromoteTo(_gang, pid, GangPosition.CoLeader, _player))
                    await DisplayAlert("نجاح", "تمت الترقية إلى نائب القائد", "موافق");
                else
                    await DisplayAlert("فشل", "لا يمكن الترقية", "موافق");
            }
            // ترقية إلى نائب
            else if (action.StartsWith("promoteToVice:"))
            {
                var pid = action.Split(':')[1];
                if (!GangService.CanPerformAction(_gang, _player.PlayerId, GangAction.PromoteMember))
                {
                    await DisplayAlert("⚠️", "ليس لديك صلاحية للترقية", "موافق");
                    return;
                }
                if (GangService.PromoteTo(_gang, pid, GangPosition.Vice, _player))
                    await DisplayAlert("نجاح", "تمت الترقية إلى نائب", "موافق");
                else
                    await DisplayAlert("فشل", "لا يمكن الترقية", "موافق");
            }
            // ترقية إلى حكيم
            else if (action.StartsWith("promoteToElder:"))
            {
                var pid = action.Split(':')[1];
                if (!GangService.CanPerformAction(_gang, _player.PlayerId, GangAction.PromoteMember))
                {
                    await DisplayAlert("⚠️", "ليس لديك صلاحية للترقية", "موافق");
                    return;
                }
                if (GangService.PromoteTo(_gang, pid, GangPosition.Elder, _player))
                    await DisplayAlert("نجاح", "تمت الترقية إلى حكيم", "موافق");
                else
                    await DisplayAlert("فشل", "لا يمكن الترقية", "موافق");
            }
            // تنزيل رتبة (Demote)
            else if (action.StartsWith("demote:"))
            {
                var pid = action.Split(':')[1];
                var targetPos = _gang.GetPosition(pid);
                var currentPos = _gang.GetPosition(_player.PlayerId);
                bool canDemote = false;
                if (currentPos == GangPosition.Leader)
                    canDemote = true;
                else if (currentPos == GangPosition.CoLeader && targetPos != GangPosition.CoLeader && targetPos != GangPosition.Leader)
                    canDemote = true;
                if (!canDemote)
                {
                    await DisplayAlert("⚠️", "ليس لديك صلاحية لتنزيل هذا العضو", "موافق");
                    return;
                }
                // تحديد الرتبة الجديدة (درجة واحدة للأسفل)
                GangPosition newPos = targetPos switch
                {
                    GangPosition.CoLeader => GangPosition.Vice,
                    GangPosition.Vice => GangPosition.Elder,
                    GangPosition.Elder => GangPosition.Officer,
                    GangPosition.Officer => GangPosition.Member,
                    _ => GangPosition.Member
                };
                if (GangService.PromoteTo(_gang, pid, newPos, _player))
                    await DisplayAlert("نجاح", $"تم تنزيل العضو إلى {GetPositionName(newPos)}", "موافق");
                else
                    await DisplayAlert("فشل", "لا يمكن تنزيل هذا العضو", "موافق");
            }
            // نقل الزعامة
            else if (action.StartsWith("transfer:"))
            {
                var pid = action.Split(':')[1];
                if (_gang.GetPosition(_player.PlayerId) != GangPosition.Leader)
                {
                    await DisplayAlert("⚠️", "فقط القائد يمكنه نقل الزعامة", "موافق");
                    return;
                }
                bool confirm = await DisplayAlert("نقل الزعامة", "هل أنت متأكد؟ ستصبح عضواً عادياً", "نعم", "لا");
                if (confirm && GangService.TransferLeadership(_gang, pid, _player))
                {
                    await DisplayAlert("نجاح", "تم نقل الزعامة", "موافق");
                    await Navigation.PopToRootAsync();
                }
                else
                    await DisplayAlert("فشل", "حدث خطأ", "موافق");
            }
            // تغيير الاسم
            else if (action == "change_name")
            {
                if (!GangService.CanPerformAction(_gang, _player.PlayerId, GangAction.ChangeGangData))
                {
                    await DisplayAlert("⚠️", "ليس لديك صلاحية لتغيير الاسم", "موافق");
                    return;
                }
                string newName = await DisplayPromptAsync("تغيير الاسم", "أدخل الاسم الجديد (4-15 حرف)", maxLength: 15);
                if (!string.IsNullOrEmpty(newName))
                {
                    if (GangService.ChangeGangName(_gang, newName, _player))
                        await DisplayAlert("نجاح", "تم تغيير الاسم", "موافق");
                    else
                        await DisplayAlert("خطأ", "الاسم غير صالح أو ليس لديك صلاحية", "موافق");
                }
            }
            // تغيير الرمز
            else if (action == "change_tag")
            {
                if (!GangService.CanPerformAction(_gang, _player.PlayerId, GangAction.ChangeGangData))
                {
                    await DisplayAlert("⚠️", "ليس لديك صلاحية لتغيير الرمز", "موافق");
                    return;
                }
                string newTag = await DisplayPromptAsync("تغيير الرمز", "أدخل 3 أحرف", maxLength: 3);
                if (!string.IsNullOrEmpty(newTag))
                {
                    if (GangService.ChangeGangTag(_gang, newTag, _player))
                        await DisplayAlert("نجاح", "تم تغيير الرمز", "موافق");
                    else
                        await DisplayAlert("خطأ", "الرمز غير صالح (3 أحرف فقط)", "موافق");
                }
            }
            // حل العصابة
            else if (action == "dissolve")
            {
                if (!GangService.CanPerformAction(_gang, _player.PlayerId, GangAction.DisbandGang))
                {
                    await DisplayAlert("⚠️", "فقط القائد يمكنه حل العصابة", "موافق");
                    return;
                }
                bool confirm = await DisplayAlert("حل العصابة", "هل أنت متأكد؟ لا يمكن التراجع", "نعم", "لا");
                if (confirm && GangService.DisbandGang(_gang, _player))
                {
                    await DisplayAlert("تم الحل", "تم حل العصابة", "موافق");
                    await Navigation.PopToRootAsync();
                }
            }
            // تغيير الصورة
            else if (action == "change_image")
            {
                if (!GangService.CanPerformAction(_gang, _player.PlayerId, GangAction.ChangeGangData))
                {
                    await DisplayAlert("⚠️", "ليس لديك صلاحية لتغيير الصورة", "موافق");
                    return;
                }
                try
                {
                    var result = await MediaPicker.PickPhotoAsync();
                    if (result != null)
                    {
                        string localPath = result.FullPath;
                        if (GangService.ChangeGangImage(_gang, localPath, _player))
                        {
                            await DisplayAlert("نجاح", "تم تغيير صورة العصابة", "موافق");
                            MessagingCenter.Send(this, "GangImageUpdated");
                        }
                        else
                            await DisplayAlert("خطأ", "ليس لديك صلاحية", "موافق");
                    }
                }
                catch (Exception ex)
                {
                    await DisplayAlert("خطأ", $"فشل اختيار الصورة: {ex.Message}", "موافق");
                }
            }
            RefreshContent();
        }
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