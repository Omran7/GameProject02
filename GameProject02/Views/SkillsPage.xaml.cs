using GameProject02.Models;
using GameProject02.Services;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GameProject02.Views;

public partial class SkillsPage : ContentPage
{
    public ObservableCollection<SkillViewModel> Skills { get; } = new();
    public int PlayerSkillPoints { get; set; }
    public ICommand UpgradeCommand { get; }
    public ICommand EquipCommand { get; }
    public ICommand SaveCommand { get; }

    public SkillsPage()
    {
        InitializeComponent();
        BindingContext = this;
        UpgradeCommand = new Command<SkillViewModel>(OnUpgrade);
        EquipCommand = new Command<SkillViewModel>(OnEquip);
        SaveCommand = new Command(OnSave);
        LoadSkills();
    }

    private void LoadSkills()
    {
        var player = AccountService.GetCurrentPlayer();
        if (player == null) return;

        SkillService.InitializeDefaultSkills(player);
        PlayerSkillPoints = player.Merits; // Merits are used for skills now
        Skills.Clear();

        foreach (var skill in player.Skills)
        {
            var def = SkillDatabase.GetSkill(skill.Id);
            Skills.Add(new SkillViewModel(skill, def));
        }
    }

    private void OnUpgrade(SkillViewModel vm)
    {
        var player = AccountService.GetCurrentPlayer();
        if (player == null) return;

        // Pass 1 as points (cost is calculated server-side)
        var (success, msg) = SkillService.UpgradeSkill(player, vm.Skill.Id, 1);
        if (success)
        {
            PlayerSkillPoints = player.Merits;
            vm.Refresh(player);
        }
        else DisplayAlert("تنبيه", msg, "موافق");
    }

    private void OnEquip(SkillViewModel vm)
    {
        var player = AccountService.GetCurrentPlayer();
        if (player != null)
        {
            SkillService.ToggleEquip(player, vm.Skill.Id);
            vm.Refresh(player);
        }
    }

    private async void OnSave()
    {
        var player = AccountService.GetCurrentPlayer();
        if (player == null) return;

        await SkillService.SaveSkillsToFirestoreAsync(player.PlayerId, player.Skills);
        await DisplayAlert("✅ تم الحفظ", "تم تطبيق المهارات بنجاح", "موافق");
        MessagingCenter.Send(this, "RefreshMainUI");
    }
}

// ✅ VIEWMODEL FOR SKILL DISPLAY
public class SkillViewModel : BindableObject
{
    public Skill Skill { get; }
    public SkillDefinition Def { get; }
    public string Name => Def?.Name ?? Skill.Name;
    public string Description => Def?.Description ?? "";
    public string LevelString => $"المستوى {Skill.Level}/20";
    public string EquipText => Skill.IsEquipped ? "🔓" : "🔒";
    public string EquipBg => Skill.IsEquipped ? "#e74c3c" : "#3498db";

    public SkillViewModel(Skill skill, SkillDefinition def)
    {
        Skill = skill;
        Def = def;
    }

    public void Refresh(PlayerAccount player)
    {
        OnPropertyChanged(nameof(LevelString));
        OnPropertyChanged(nameof(EquipText));
        OnPropertyChanged(nameof(EquipBg));
    }
}