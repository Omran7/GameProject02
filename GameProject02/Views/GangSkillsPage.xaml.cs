using GameProject02.Models;
using GameProject02.Services;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GameProject02.Views;

public class GangSkill : INotifyPropertyChanged
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Effect { get; set; } = "";
    private int _level = 0;
    public int Level
    {
        get => _level;
        set { _level = value; OnPropertyChanged(); OnPropertyChanged(nameof(RespectCost)); OnPropertyChanged(nameof(CashCost)); }
    }
    public long RespectCost => (Level + 1) * 100L;
    public long CashCost => (Level + 1) * 200L;
    public int MaxLevelForCurrentGang { get; set; } = 2; // يتم تحديثه حسب مستوى العصابة

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

public partial class GangSkillsPage : ContentPage
{
    private GangObject _gang;
    private PlayerAccount _player;
    public ObservableCollection<GangSkill> Skills { get; set; } = new();

    public GangSkillsPage()
    {
        InitializeComponent();
        LoadData();
        BindingContext = this;
    }

    private void LoadData()
    {
        _player = AccountService.GetCurrentPlayer();
        _gang = _player?.GangObject;
        if (_gang == null)
        {
            Navigation.PopAsync(false);
            return;
        }

        Skills.Clear();

        // المهارات الست كما في اللعبة القديمة
        var skillsData = new[]
        {
            new { Name = "قوة خاصة", Effect = "+5% ضرر", Id = 0 },
            new { Name = "سرعة خارقة", Effect = "+5% سرعة", Id = 1 },
            new { Name = "دفاع مدرع", Effect = "+5% دفاع", Id = 2 },
            new { Name = "دقة قناص", Effect = "+5% دقة", Id = 3 },
            new { Name = "تعافي سريع", Effect = "-10% وقت المستشفى", Id = 4 },
            new { Name = "تحمل عالي", Effect = "+10% صحة", Id = 5 }
        };

        // أقصى مستوى للمهارة حسب مستوى العصابة (مطابق للعبة القديمة)
        int maxSkillLevel = GetMaxSkillLevelForGangLevel(_gang.Level);

        foreach (var data in skillsData)
        {
            int currentLevel = _gang.SkillsLevel.ContainsKey(data.Id.ToString()) ? _gang.SkillsLevel[data.Id.ToString()] : 0;
            Skills.Add(new GangSkill
            {
                Id = data.Id,
                Name = data.Name,
                Effect = data.Effect,
                Level = currentLevel,
                MaxLevelForCurrentGang = maxSkillLevel
            });
        }

        SkillsListView.ItemsSource = Skills;
    }

    private int GetMaxSkillLevelForGangLevel(int gangLevel)
    {
        // مطابق للجدول من اللعبة القديمة
        return gangLevel switch
        {
            1 => 2,
            2 => 4,
            3 => 6,
            4 => 8,
            5 => 10,
            6 => 15,
            >= 7 => 16,
            _ => 2
        };
    }

    private async void OnUpgradeClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is int skillId)
        {
            var skill = Skills.FirstOrDefault(s => s.Id == skillId);
            if (skill == null) return;

            // التحقق من صلاحية القائد أو نائب القائد
            var playerPos = _gang.GetPosition(_player.PlayerId);
            if (playerPos != GangPosition.Leader && playerPos != GangPosition.CoLeader)
            {
                await DisplayAlert("⚠️", "فقط القائد أو نائب القائد يمكنه ترقية المهارات", "موافق");
                return;
            }

            // التحقق من الحد الأقصى حسب مستوى العصابة
            if (skill.Level >= GetMaxSkillLevelForGangLevel(_gang.Level))
            {
                await DisplayAlert("❌ فشل", $"تم الوصول إلى الحد الأقصى للمهارة (المستوى {skill.Level})", "موافق");
                return;
            }

            // التحقق من وجود الموارد الكافية
            if (_gang.AvailableRespect < skill.RespectCost)
            {
                await DisplayAlert("❌ فشل", $"تحتاج {skill.RespectCost} احترام متاح (لديك {_gang.AvailableRespect})", "موافق");
                return;
            }
            if (_gang.GangCash < skill.CashCost)
            {
                await DisplayAlert("❌ فشل", $"تحتاج {skill.CashCost} نقد عصابة (لديك {_gang.GangCash})", "موافق");
                return;
            }

            // تأكيد الترقية (كما في اللعبة القديمة)
            bool confirm = await DisplayAlert("تأكيد الترقية",
                $"هل أنت متأكد من ترقية {skill.Name} إلى المستوى {skill.Level + 1}؟\n" +
                $"التكلفة: {skill.RespectCost} احترام + {skill.CashCost} نقد",
                "نعم", "لا");
            if (!confirm) return;

            // تنفيذ الترقية
            bool success = GangDatabaseService.UpgradeSkill(
                _gang.GangId,
                skillId,
                skill.RespectCost,
                skill.CashCost,
                1 // المعامل maxLevelPerGangLevel لا يُستخدم هنا لأننا نتحقق يدوياً
            );

            if (success)
            {
                skill.Level++;
                // تحديث الكائن الأصلي في _gang
                if (_gang.SkillsLevel.ContainsKey(skillId.ToString()))
                    _gang.SkillsLevel[skillId.ToString()] = skill.Level;
                else
                    _gang.SkillsLevel.Add(skillId.ToString(), skill.Level);

                await DisplayAlert("✅ نجاح", $"تمت ترقية {skill.Name} إلى المستوى {skill.Level}", "موافق");
                // تحديث واجهة ملف العصابة (إذا كانت مفتوحة)
                MessagingCenter.Send(_gang, "GangDataUpdated");
            }
            else
            {
                await DisplayAlert("❌ فشل", "حدث خطأ أثناء الترقية", "موافق");
            }
        }
    }

    private async void OnBackClicked(object sender, EventArgs e) => await Navigation.PopAsync(false);
    private async void OnHomeClicked(object sender, EventArgs e) => await Navigation.PopToRootAsync(false);
}