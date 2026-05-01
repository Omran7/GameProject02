using GameProject02.Models;
using GameProject02.Services;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GameProject02.Views;

// Model for a single Militia Unit
public class MilitiaUnit : INotifyPropertyChanged
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string ImageSource { get; set; } = "gang_militia_default";
    public int CourageReq { get; set; }
    public int RespectReward { get; set; }
    public int MaxMembers { get; set; }
    private int _currentMembers;
    public int CurrentMembers
    {
        get => _currentMembers;
        set { _currentMembers = value; OnPropertyChanged(); OnPropertyChanged(nameof(MemberProgress)); OnPropertyChanged(nameof(MemberCountText)); }
    }
    public double MemberProgress => (double)CurrentMembers / MaxMembers;
    public string MemberCountText => $"{CurrentMembers}/{MaxMembers}";
    public string JoinButtonText { get; set; } = "انضمام";
    public bool IsJoined { get; set; }
    public bool HasAdditionalRewards => AdditionalRewards != null && AdditionalRewards.Count > 0;
    public List<string> AdditionalRewards { get; set; } = new List<string>();
    public string MembersIdsString { get; set; } = ""; // comma separated list of player IDs

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    public string AdditionalRewardsText => AdditionalRewards != null ? string.Join(" - ", AdditionalRewards) : "";
}

public partial class GangMilitiaPage : ContentPage
{
    private PlayerAccount _player;
    private GangObject _currentGang;
    public ObservableCollection<MilitiaUnit> Units { get; set; } = new();

    public GangMilitiaPage(GangObject gang)
    {
        InitializeComponent();
        _currentGang = gang;
        _player = AccountService.GetCurrentPlayer();
        BindingContext = this;  // هذا السطر مهم جداً
        LoadUnits();
    }

    private void LoadUnits()
    {
        Units.Clear();
        if (_currentGang == null || _player == null) return;

        // ✅ تحديد الوحدة التي ينتمي إليها اللاعب (إن وجدت)
        int joinedUnitId = -1;
        foreach (var unit in _currentGang.MilitiaMembersByUnit)
        {
            if (unit.Value.Contains(_player.PlayerId))
            {
                joinedUnitId = unit.Key;
                break;
            }
        }

        int maxUnits = Math.Min(_currentGang.Level, 7);
        for (int i = 1; i <= maxUnits; i++)
        {
            if (!_currentGang.MilitiaMembersByUnit.ContainsKey(i))
                _currentGang.MilitiaMembersByUnit[i] = new List<string>();

            var members = _currentGang.MilitiaMembersByUnit[i];
            int currentCount = members.Count;
            int maxCount = i * 10;
            bool isJoined = (joinedUnitId == i); // ✅ فقط الوحدة التي ينتمي إليها اللاعب تكون true

            var unit = new MilitiaUnit
            {
                Id = i,
                Name = $"الميليشيا {i}",
                CourageReq = i * 10,
                RespectReward = i * 50,
                MaxMembers = maxCount,
                CurrentMembers = currentCount,
                IsJoined = isJoined,
                JoinButtonText = "انضمام",
                AdditionalRewards = GetRewardsForUnit(i)
            };
            Units.Add(unit);
        }
    }
    private List<string> GetRewardsForUnit(int unitId)
    {
        // Simulate additional rewards like old game (Crystal, gold, etc.)
        var rewards = new List<string>();
        if (unitId == 1) rewards.Add("بلورة +1");
        if (unitId == 2) rewards.Add("بلورة +2");
        if (unitId == 3) rewards.Add("ذهب +500");
        if (unitId == 4) rewards.Add("احترام +200");
        return rewards;
    }

    private async void OnJoinMilitiaClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is int unitId)
        {
            var unit = Units.FirstOrDefault(u => u.Id == unitId);
            if (unit == null) return;

            // ✅ التحقق: هل اللاعب عضو بالفعل في أي وحدة ميليشيا أخرى؟
            bool isMemberOfAnyUnit = _currentGang.MilitiaMembersByUnit.Any(u => u.Value.Contains(_player.PlayerId));
            bool isMemberOfThisUnit = unit.IsJoined;

            // إذا كان اللاعب عضواً في وحدة أخرى (وليس هذه الوحدة)، نمنعه من الانضمام
            if (isMemberOfAnyUnit && !isMemberOfThisUnit)
            {
                await DisplayAlert("تنبيه", "أنت بالفعل عضو في ميليشيا أخرى. لا يمكنك الانضمام إلى أكثر من وحدة.", "موافق");
                return;
            }

            // إذا كان اللاعب عضواً في هذه الوحدة بالفعل، لا نسمح بالمغادرة (حسب آلية اللعبة)
            if (isMemberOfThisUnit)
            {
                await DisplayAlert("تنبيه", "أنت بالفعل عضو في هذه الميليشيا. لا يمكنك المغادرة يدوياً، سيتم طردك تلقائياً عند اكتمال الوحدة.", "موافق");
                return;
            }

            // محاولة الانضمام
            var result = GangDatabaseService.JoinMilitia(_player.PlayerId, _currentGang, unitId, unit.CourageReq, unit.RespectReward);
            if (result.IsAllProcessSuccess)
            {
                // إعادة تحميل بيانات الوحدة (قد تتغير بسبب إعادة التعيين عند الامتلاء)
                ReloadUnit(unitId);

                // بناء رسالة المكافآت
                string rewardMsg = $"✅ انضممت إلى {unit.Name}\n";
                rewardMsg += $"🎁 حصلت على: {unit.RespectReward} احترام, {unit.RespectReward / 2} ولاء";
                if (!string.IsNullOrEmpty(result.CrystalId) && result.CrystalId != "0")
                    rewardMsg += $", {result.CrystalId} بلورة";
                rewardMsg += $"\n💰 تبرعت العصابة بمكافأة إضافية: {unit.RespectReward * 5} نقد (عند الامتلاء)";

                await DisplayAlert("✅ انضمام ناجح", rewardMsg, "موافق");
            }
            else
            {
                await DisplayAlert("❌ فشل الانضمام", result.ErrorMessage, "موافق");
            }
        }
    }

    // دالة مساعدة لإعادة تحميل بيانات وحدة محددة بعد التغيير
    private void ReloadUnit(int unitId)
    {
        if (_currentGang.MilitiaMembersByUnit.TryGetValue(unitId, out var members))
        {
            var unit = Units.FirstOrDefault(u => u.Id == unitId);
            if (unit != null)
            {
                unit.CurrentMembers = members.Count;
                unit.IsJoined = members.Contains(_player.PlayerId);
                // تحديث الواجهة (إعادة تعيين العنصر في القائمة)
                var index = Units.IndexOf(unit);
                if (index >= 0) Units[index] = unit;
            }
        }
    }
    private async void OnBackClicked(object sender, EventArgs e) => await Navigation.PopAsync();
    private async void OnHomeClicked(object sender, EventArgs e) => await Navigation.PopToRootAsync();
}