using GameProject02.Models;
using GameProject02.Services;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Linq;

namespace GameProject02.Views;

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
    public bool IsJoined { get; set; }
    public List<string> AdditionalRewards { get; set; } = new();
    public bool HasAdditionalRewards => AdditionalRewards != null && AdditionalRewards.Count > 0;
    public string AdditionalRewardsText => AdditionalRewards != null ? string.Join(" - ", AdditionalRewards) : "";

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
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
        _player = AccountService.CurrentPlayer;
        BindingContext = this;
        LoadUnits();
    }

    private void LoadUnits()
    {
        Units.Clear();
        if (_currentGang == null || _player == null) return;

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
            bool isJoined = (joinedUnitId == i);

            var unit = new MilitiaUnit
            {
                Id = i,
                Name = $"الميليشيا {i}",
                CourageReq = i * 10,
                RespectReward = i * 50,
                MaxMembers = maxCount,
                CurrentMembers = currentCount,
                IsJoined = isJoined,
                AdditionalRewards = GetRewardsForUnit(i)
            };
            Units.Add(unit);
        }
    }

    private List<string> GetRewardsForUnit(int unitId)
    {
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

            bool isMemberOfAnyUnit = _currentGang.MilitiaMembersByUnit.Any(u => u.Value.Contains(_player.PlayerId));
            bool isMemberOfThisUnit = unit.IsJoined;

            if (isMemberOfAnyUnit && !isMemberOfThisUnit)
            {
                await DisplayAlert("تنبيه", "أنت بالفعل عضو في ميليشيا أخرى. لا يمكنك الانضمام إلى أكثر من وحدة.", "موافق");
                return;
            }
            if (isMemberOfThisUnit)
            {
                await DisplayAlert("تنبيه", "أنت بالفعل عضو في هذه الميليشيا.", "موافق");
                return;
            }

            btn.IsEnabled = false;
            btn.Text = "جاري الانضمام...";

            try
            {
                var result = await GangDatabaseService.JoinMilitiaAsync(_player.PlayerId, _currentGang, unitId, unit.CourageReq, unit.RespectReward);
                if (result.IsAllProcessSuccess)
                {
                    // The gang object (_currentGang) is already updated inside JoinMilitiaAsync
                    // Update the UI for this unit
                    ReloadUnit(unitId);
                    // Notify profile page to refresh its UI (without reloading from Firestore)
                    MessagingCenter.Send(this, "MilitiaJoined");
                    MessagingCenter.Send(_currentGang, "GangDataUpdated");

                    string rewardMsg = $"✅ انضممت إلى {unit.Name}\n" +
                                       $"🎁 حصلت على: {unit.RespectReward} احترام, {unit.RespectReward / 2} ولاء";
                    if (!string.IsNullOrEmpty(result.CrystalId) && result.CrystalId != "0")
                        rewardMsg += $", {result.CrystalId}";
                    await DisplayAlert("✅ انضمام ناجح", rewardMsg, "موافق");
                }
                else
                {
                    await DisplayAlert("❌ فشل الانضمام", result.ErrorMessage, "موافق");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MILITIA] Error: {ex}");
                await DisplayAlert("خطأ", "حدث خطأ غير متوقع", "موافق");
            }
            finally
            {
                btn.IsEnabled = true;
                btn.Text = "انضمام";
            }
        }
    }

    private void ReloadUnit(int unitId)
    {
        if (_currentGang.MilitiaMembersByUnit.TryGetValue(unitId, out var members))
        {
            var unit = Units.FirstOrDefault(u => u.Id == unitId);
            if (unit != null)
            {
                unit.CurrentMembers = members.Count;
                unit.IsJoined = members.Contains(_player.PlayerId);
                int index = Units.IndexOf(unit);
                if (index >= 0) Units[index] = unit;
            }
        }
    }

    private async void OnBackClicked(object sender, EventArgs e) => await Navigation.PopAsync();
    private async void OnHomeClicked(object sender, EventArgs e) => await Navigation.PopToRootAsync();
}