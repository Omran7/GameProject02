using GameProject02.Models;
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
    // Core account info
    public string PlayerId { get; set; } = Guid.NewGuid().ToString();
    public string Username { get; set; } = "LORD";
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
                // Save to preferences for persistence
                Preferences.Set("AvatarPath", value);
                // Notify subscribers (e.g., TopHeaderView)
                AvatarChanged?.Invoke(value);
            }
        }
    }

    public static event Action<string> AvatarChanged;

    // Basic Info
    public int Level
    {
        get => MainStatesObject.Level;
        set => MainStatesObject.Level = value;
    }
    public string Gender { get; set; } = "ذكر";
    public string City { get; set; } = "مدينة العصابات";
    public bool IsVIP { get; set; } = false;
    public int AchievementPoints { get; set; } = 0;
    public int Medals { get; set; } = 0;
    public MainStatesObject MainStatesObject { get; set; } = new MainStatesObject();
    // Resources (Top bar)
    public int Gold { get; set; } = 100;
    public int Diamonds { get; set; } = 0;

    public long Checks { get; set; } = 0;

    // ⚡ ENERGY SYSTEM (FOR GYM TRAINING)
    public int Energy { get; set; } = 100;
    public int MaxEnergy { get; set; } = 100;
    public long LastEnergyRechargeTime { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    public int Health { get; set; } = 100;
    public int MaxHealth { get; set; } = 100;

    // Battle Stats
    public int Strength { get; set; } = 5;
    public int Defense { get; set; } = 5;
    public int Speed { get; set; } = 5;
    public int Dexterity { get; set; } = 5;
    public int Intelligence { get; set; } = 5;

    // ✅ ADD TO PlayerAccount CLASS
    public int NobilityCurrent { get; set; } = 100; // Max 100
    public long NobilityChangeTimeInMilli { get; set; } = -101; // -101 = uninitialized


    // ✅ ADD THESE PROPERTIES WITH PROPERTY CHANGE NOTIFICATION (MUST HAVE FOR UI UPDATES)
    private double _levelProgress = 0.0;
    private string _xpText = "0/100";
    private double _nobilityProgress = 1.0;
    private string _nobilityText = "100/100";

    public long PersonalContribution { get; set; } = 0; // مساهمة شخصية (نقد أو احترام)

    // Experience UI properties (bind to MainPage bars)
    public double LevelProgress
    {
        get => _levelProgress;
        set { _levelProgress = value; }
    }

    public string XPText
    {
        get => _xpText;
        set { _xpText = value; }
    }

    // Nobility UI properties (bind to MainPage bars)
    public double NobilityProgress
    {
        get => _nobilityProgress;
        set { _nobilityProgress = value; }
    }

    public string NobilityText
    {
        get => _nobilityText;
        set { _nobilityText = value; }
    }

    // ✅ CRITICAL FIX: Override CurrentXP setter to trigger UI updates
    public new long CurrentXP
    {
        get => MainStatesObject.CurrentExperience;
        set
        {
            MainStatesObject.CurrentExperience = value;
        }
    }
    public int Courage
    {
        get => MainStatesObject.CourageCurrent;
        set => MainStatesObject.CourageCurrent = value;
    }
    public int MaxCourage
    {
        get => MainStatesObject.CourageMax;
        set => MainStatesObject.CourageMax = value;
    }
    private int _crystalCount = 0;
    public int CrystalCount
    {
        get => _crystalCount;
        set { _crystalCount = value; OnPropertyChanged(); }
    }


    private long _personalLoyalty = 0;
    public long PersonalLoyalty
    {
        get => _personalLoyalty;
        set { _personalLoyalty = value; OnPropertyChanged(); }
    }
    // Progress

    public int MaxXP { get; set; } = 100;

    // General Stats
    public int CrimeAttempts { get; set; } = 0;
    public int Shovels { get; set; } = 0;
    public int HospitalVisits { get; set; } = 0;
    public int JailTimes { get; set; } = 0;
    public int Flights { get; set; } = 0;
    public int HerbsUsed { get; set; } = 0;
    public int ItemsFound { get; set; } = 0;

    // Skills
    public Skill Greatness { get; set; } = new Skill("عظمة", 0, 0, 0);
    public Skill KillingDifficulty { get; set; } = new Skill("صعوبة القتل", 0, 0, 0);
    public Skill FastGhost { get; set; } = new Skill("شبح سريع", 0, 0, 0);
    public Skill LightMovement { get; set; } = new Skill("خفيف الحركة", 0, 0, 0);

    // 🏫 School-related stat bonuses
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

    // 🏢 Estate Info (BACKWARD COMPATIBILITY - DO NOT REMOVE)
    public string EstateType { get; set; } = "لا يوجد";
    public string EstateOwner { get; set; } = "لا يوجد";
    public int EstateHours { get; set; } = 0;
    public int EstateUpgrades { get; set; } = 0;
    public int EstateWorkers { get; set; } = 0;

    public string ImageResource { get; set; } = "default_avatar.png";
    public string PrisonRemainingTime => PrisonService.GetRemainingTime(CrimeObject.PrisonReleaseTime);
    public string HospitalRemainingTime => HospitalService.GetRemainingTime(CrimeObject.HospitalReleaseTime);
    // 🏫 School System
    public SchoolObject School { get; set; } = new SchoolObject();

    // 💪 Gym System (5 lessons - Running Gym REMOVED)
    public GymObject Gym { get; set; } = new GymObject();

    // 💼 Stock System
    public StockObject StockObject { get; set; } = new StockObject();

    // 🏛️ Museum System
    public MuseumObject Museum { get; set; } = new MuseumObject();

    // 🏠 Estate System
    public EstateObject Estate { get; set; } = new EstateObject();
    public WorkObject WorkObject { get; set; } = new WorkObject();

    // 🔑 MULTIPLE ESTATES SUPPORT (MUST BE INITIALIZED!)
    public List<EstateObject> Estates { get; set; } = new List<EstateObject>();

    // Primary residence tracking (default to "عشة" = ID 0)
    public int PrimaryResidenceEstateId { get; set; } = 0;

    // ✅ CRITICAL FIX: Track PRIMARY RESIDENCE by INSTANCE ID (not type ID)
    public string PrimaryResidenceEstateInstanceId { get; set; } = string.Empty;

    // Crime system integration
    public CrimeObject CrimeObject { get; set; }
    // ⚔️ Combat System
    public CombatObject Combat { get; set; } = new CombatObject();

    // ✅ ADD THIS SINGLE LINE (DOES NOT BREAK ANYTHING)
    public ArmingObject ArmingObject { get; set; } = new ArmingObject();

    // In PlayerAccount.cs, add:
    public GangObject GangObject { get; set; } = null;

    // Inside GangObject.cs
    public List<string> MilitiaMemberIds { get; set; } = new List<string>();
    public string MembersIdsJoinedMilitia { get; set; } = string.Empty;


    // ✅ CONSTRUCTOR: Initialize default estate ("عشة") + gym system + stock items
    public PlayerAccount()
    {
        // Initialize estates with "عشة" (ID 0) as default home
        Estates = new List<EstateObject>
    {
        new EstateObject
        {
            Id = 0, // "عشة" = ID 0
            InstanceId = Guid.NewGuid().ToString().Substring(0, 8), // ✅ MUST SET HERE!
            EstateOwnerId = PlayerId,
            IsUsed = true,
            LastTaxPaidTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            FixedModifications = new List<bool> { true, false, false }
        }
    };

        // Set primary residence to "عشة"
        PrimaryResidenceEstateInstanceId = Estates[0].InstanceId;
        PrimaryResidenceEstateId = 0;

        Estate = Estates[0];
        EstateType = "عشة";
        EstateOwner = "انت";

        // ✅ INITIALIZE STOCK WITH SAMPLE ITEMS (CORRECT SYNTAX)
        StockObject = new StockObject();

        // Initialize crime system
        CrimeObject = new CrimeObject
        {
            Courage = 100,
            MaxCourage = 100,
            LastCourageRechargeTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        };
        /*        CrimeObject.SetProgress(0, 0, 100); // first task always succeeds
        */

        // ✅ ADD THIS LINE IN CONSTRUCTOR (DOES NOT BREAK ANYTHING)
        ArmingObject = new ArmingObject();
        AvatarPath = Preferences.Get("AvatarPath", "player_avatar.png");
    }

    // ⚡ ENERGY REGENERATION METHOD
    public void RegenerateEnergy()
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var elapsedSeconds = (now - LastEnergyRechargeTime) / 1000;
        var regenerated = elapsedSeconds / 30; // 1 energy per 30 seconds

        if (regenerated > 0)
        {
            Energy = Math.Min(MaxEnergy, Energy + (int)regenerated);
            LastEnergyRechargeTime = now;
        }
    }

}

// Skill class for acquired skills
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