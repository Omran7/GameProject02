using GameProject02.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GameProject02.Models;

public class PlayerAccount : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string name = "") =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    // ══════════════════════════════════════════════════════════════════
    //  معلومات الحساب الأساسية
    // ══════════════════════════════════════════════════════════════════
    public string PlayerId { get; set; } = Guid.NewGuid().ToString();
    public string Username { get; set; } = "";
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    private string _avatarPath = "player_avatar.png";
    public string AvatarPath
    {
        get => _avatarPath;
        set
        {
            if (_avatarPath != value)
            {
                _avatarPath = value;
                OnPropertyChanged();
                Preferences.Set("AvatarPath", value);
                AvatarChanged?.Invoke(value);
            }
        }
    }

    public void ClearAvatar()
    {
        _avatarPath = string.Empty;
        Preferences.Remove("AvatarPath");
        AvatarChanged?.Invoke(string.Empty);
    }

    public static event Action<string> AvatarChanged;

    // ══════════════════════════════════════════════════════════════════
    //  معلومات اللاعب
    // ══════════════════════════════════════════════════════════════════
    public int Level { get => MainStatesObject.Level; set => MainStatesObject.Level = value; }
    public string Gender { get; set; } = "ذكر";
    public string City { get; set; } = "مدينة العصابات";
    public bool IsVIP { get; set; } = false;
    public int AchievementPoints { get; set; } = 0;
    public int Medals { get; set; } = 0;

    public MainStatesObject MainStatesObject { get; set; } = new MainStatesObject();

    // ══════════════════════════════════════════════════════════════════
    //  العملات
    // ══════════════════════════════════════════════════════════════════
    public int Gold { get; set; } = 100;
    public int Diamonds { get; set; } = 0;
    public long Checks { get; set; } = 0;

    // ══════════════════════════════════════════════════════════════════
    //  البارات الأربعة — القيم والحدود
    //  التجديد يُدار بالكامل من RegenerationService
    // ══════════════════════════════════════════════════════════════════
    public int Energy { get; set; } = 100;
    public int MaxEnergy { get; set; } = 100;

    public int Courage { get => MainStatesObject.CourageCurrent; set => MainStatesObject.CourageCurrent = value; }
    public int MaxCourage { get => MainStatesObject.CourageMax; set => MainStatesObject.CourageMax = value; }

    public int NobilityCurrent { get; set; } = 100;

    public int Health { get; set; } = 500;
    public int MaxHealth { get; set; } = 500;

    // ══════════════════════════════════════════════════════════════════
    //  إحصائيات القتال
    // ══════════════════════════════════════════════════════════════════
    public int Strength { get; set; } = 5;
    public int Defense { get; set; } = 5;
    public int Speed { get; set; } = 5;
    public int Dexterity { get; set; } = 5;
    public int Intelligence { get; set; } = 5;

    // ══════════════════════════════════════════════════════════════════
    //  الخبرة والمستوى
    // ══════════════════════════════════════════════════════════════════
    public new long CurrentXP
    {
        get => MainStatesObject.CurrentExperience;
        set => MainStatesObject.CurrentExperience = value;
    }

    public int MaxXP { get; set; } = 100;

    private double _levelProgress = 0.0;
    public double LevelProgress
    {
        get => _levelProgress;
        set { _levelProgress = value; OnPropertyChanged(); }
    }

    private string _xpText = "0/100";
    public string XPText
    {
        get => _xpText;
        set { _xpText = value; OnPropertyChanged(); }
    }

    // ══════════════════════════════════════════════════════════════════
    //  نبل / شهامة UI
    // ══════════════════════════════════════════════════════════════════
    public long NobilityChangeTimeInMilli { get; set; } = -101;

    private double _nobilityProgress = 1.0;
    public double NobilityProgress
    {
        get => _nobilityProgress;
        set { _nobilityProgress = value; OnPropertyChanged(); }
    }

    private string _nobilityText = "100/100";
    public string NobilityText
    {
        get => _nobilityText;
        set { _nobilityText = value; OnPropertyChanged(); }
    }

    // ══════════════════════════════════════════════════════════════════
    //  إحصائيات عامة
    // ══════════════════════════════════════════════════════════════════
    public long PersonalContribution { get; set; } = 0;
    public long PersonalLoyalty
    {
        get => _personalLoyalty;
        set { _personalLoyalty = value; OnPropertyChanged(); }
    }
    private long _personalLoyalty = 0;

    public int CrystalCount
    {
        get => _crystalCount;
        set { _crystalCount = value; OnPropertyChanged(); }
    }
    private int _crystalCount = 0;

    public int CrimeAttempts { get; set; } = 0;
    public int Shovels { get; set; } = 0;
    public int HospitalVisits { get; set; } = 0;
    public int JailTimes { get; set; } = 0;
    public int Flights { get; set; } = 0;
    public int HerbsUsed { get; set; } = 0;
    public int ItemsFound { get; set; } = 0;

    // ══════════════════════════════════════════════════════════════════
    //  مهارات المدرسة
    // ══════════════════════════════════════════════════════════════════
    public int CrimeSuccessRate { get; set; } = 0;
    public int CrimeGoldYield { get; set; } = 0;
    public int CrimeExperienceYield { get; set; } = 0;
    public int CrimePunishmentReduction { get; set; } = 0;
    public int HospitalTimeMultiplier { get; set; } = 0;
    public int FirearmEfficiency { get; set; } = 0;
    public int MeleeWeaponEfficiency { get; set; } = 0;
    public int BodyguardHPBonus { get; set; } = 0;
    public int LootBoxChance { get; set; } = 0;
    public int ArtifactCrimeSuccess { get; set; } = 0;
    public int StallTaxReduction { get; set; } = 0;
    public int DamageReduction { get; set; } = 0;
    public int HackingCrimeSuccess { get; set; } = 0;
    public int CarCrimeSuccess { get; set; } = 0;
    public int EstateModificationCostReduction { get; set; } = 0;
    public int HappinessMultiplier { get; set; } = 1;
    public int EstateHappinessBonus { get; set; } = 0;
    public int GymEfficiency { get; set; } = 0;

    // ══════════════════════════════════════════════════════════════════
    //  مهارات
    // ══════════════════════════════════════════════════════════════════
    public Skill Greatness { get; set; } = new Skill("عظمة", 0, 0, 0);
    public Skill KillingDifficulty { get; set; } = new Skill("صعوبة القتل", 0, 0, 0);
    public Skill FastGhost { get; set; } = new Skill("شبح سريع", 0, 0, 0);
    public Skill LightMovement { get; set; } = new Skill("خفيف الحركة", 0, 0, 0);

    // ══════════════════════════════════════════════════════════════════
    //  الأنظمة
    // ══════════════════════════════════════════════════════════════════
    public string EstateType { get; set; } = "لا يوجد";
    public string EstateOwner { get; set; } = "لا يوجد";
    public int EstateHours { get; set; } = 0;
    public int EstateUpgrades { get; set; } = 0;
    public int EstateWorkers { get; set; } = 0;

    public string ImageResource { get; set; } = "default_avatar.png";

    public string PrisonRemainingTime => PrisonService.GetRemainingTime(CrimeObject.PrisonReleaseTime);
    public string HospitalRemainingTime => HospitalService.GetRemainingTime(CrimeObject.HospitalReleaseTime);

    public SchoolObject School { get; set; } = new SchoolObject();
    public GymObject Gym { get; set; } = new GymObject();
    public StockObject StockObject { get; set; } = new StockObject();
    public MuseumObject Museum { get; set; } = new MuseumObject();
    public EstateObject Estate { get; set; } = new EstateObject();
    public WorkObject WorkObject { get; set; } = new WorkObject();

    public List<EstateObject> Estates { get; set; } = new List<EstateObject>();

    public int PrimaryResidenceEstateId { get; set; } = 0;
    public string PrimaryResidenceEstateInstanceId { get; set; } = string.Empty;

    public CrimeObject CrimeObject { get; set; }
    public CombatObject Combat { get; set; } = new CombatObject();
    public ArmingObject ArmingObject { get; set; } = new ArmingObject();
    public GangObject GangObject { get; set; } = null;

    public List<string> MilitiaMemberIds { get; set; } = new List<string>();
    public string MembersIdsJoinedMilitia { get; set; } = string.Empty;

    // ✅ NEW: Notifications embedded directly in the player document
    public List<NotificationItem> Notifications { get; set; } = new List<NotificationItem>();

    // ══════════════════════════════════════════════════════════════════
    //  المُنشئ
    // ══════════════════════════════════════════════════════════════════
    public PlayerAccount()
    {
        Estates = new List<EstateObject>
        {
            new EstateObject
            {
                Id             = 0,
                InstanceId     = Guid.NewGuid().ToString().Substring(0, 8),
                EstateOwnerId  = PlayerId,
                IsUsed         = true,
                LastTaxPaidTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                FixedModifications = new List<bool> { true, false, false }
            }
        };

        PrimaryResidenceEstateInstanceId = Estates[0].InstanceId;
        PrimaryResidenceEstateId = 0;
        Estate = Estates[0];
        EstateType = "عشة";
        EstateOwner = "انت";

        StockObject = new StockObject();

        CrimeObject = new CrimeObject
        {
            MaxCourage = 100,
            LastCourageRechargeTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        };
        ArmingObject = new ArmingObject();
        Notifications = new List<NotificationItem>(); // ✅ initialize notifications list
        AvatarPath = Preferences.Get("AvatarPath", "player_avatar.png");
    }
}

// ══════════════════════════════════════════════════════════════════════
//  Skill class
// ══════════════════════════════════════════════════════════════════════
public class Skill
{
    public string Name { get; set; }
    public int Percentage { get; set; }
    public int BaseValue { get; set; }
    public int BonusValue { get; set; }

    public Skill(string name, int percentage, int baseValue, int bonusValue)
    {
        Name = name;
        Percentage = percentage;
        BaseValue = baseValue;
        BonusValue = bonusValue;
    }

    public string GetDescription()
    {
        if (BaseValue > 0)
            return $"({BaseValue}:مجردة + {BonusValue}:مصاحبة)";
        return $"({BonusValue}:مصاحبة)";
    }
}