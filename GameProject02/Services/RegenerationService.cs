using GameProject02.Models;
using Microsoft.Maui.Storage;
using System;
using System.Timers;

namespace GameProject02.Services;

/// <summary>
/// يعمل أوف لاين وأون لاين:
///
/// أون لاين  → Timer يُضيف نقطة كل فترة محددة
/// أوف لاين  → عند الفتح يحسب كم وقت مضى ويُضيف النقاط المتراكمة دفعة واحدة
///
/// القواعد:
///   الطاقة   → +1 كل 2 دقيقة  (حد 100)
///   الشجاعة  → +1 كل 2 دقيقة  (حد 100)
///   الشهامة  → +1 كل 5 دقائق  (حد 100)
///   الصحة    → +نسبة% كل 2 دقيقة (حد 500)
/// </summary>
public static class RegenerationService
{
    // ══════════════════════════════════════════════════════════════════
    //  إعدادات التجديد — غيّر هذه الأرقام فقط
    // ══════════════════════════════════════════════════════════════════

    // الطاقة
    private const int EnergyPerTick = 1;           // نقاط لكل دورة
    private const double EnergyIntervalMin = 2.0;         // كل كم دقيقة

    // الشجاعة
    private const int CouragePerTick = 1;
    private const double CourageIntervalMin = 2.0;

    // الشهامة
    private const int NobilityPerTick = 1;
    private const double NobilityIntervalMin = 5.0;

    // الصحة
    private const double HealthPercentPerTick = 0.05;      // 5% من الحد الأقصى كل دورة
    private const double HealthIntervalMin = 2.0;

    // ══════════════════════════════════════════════════════════════════
    //  مفاتيح حفظ الوقت في Preferences (للأوف لاين)
    // ══════════════════════════════════════════════════════════════════

    private const string KeyEnergy = "regen_last_energy";
    private const string KeyCourage = "regen_last_courage";
    private const string KeyNobility = "regen_last_nobility";
    private const string KeyHealth = "regen_last_health";

    // ══════════════════════════════════════════════════════════════════

    private static System.Timers.Timer _timer;
    private static PlayerAccount _player;

    // ════════════════════════════════════════════════════════════════════
    //  تشغيل الخدمة — استدعيه مرة واحدة عند تسجيل الدخول
    // ════════════════════════════════════════════════════════════════════
    public static void Start(PlayerAccount player)
    {
        _player = player;

        // ── خطوة 1: احسب النقاط المتراكمة أوف لاين ──────────────────────
        ApplyOfflineRegen();

        // ── خطوة 2: شغّل التايمر للأون لاين (كل دقيقة) ──────────────────
        Stop(); // أوقف أي تايمر قديم أولاً

        _timer = new System.Timers.Timer(60_000); // كل دقيقة
        _timer.Elapsed += OnTick;
        _timer.AutoReset = true;
        _timer.Start();

        System.Diagnostics.Debug.WriteLine("[RegenService] Started");
    }

    // ════════════════════════════════════════════════════════════════════
    //  إيقاف الخدمة — استدعيه عند تسجيل الخروج
    // ════════════════════════════════════════════════════════════════════
    public static void Stop()
    {
        if (_timer == null) return;
        _timer.Stop();
        _timer.Dispose();
        _timer = null;
        System.Diagnostics.Debug.WriteLine("[RegenService] Stopped");
    }

    // ════════════════════════════════════════════════════════════════════
    //  حساب النقاط المتراكمة أوف لاين
    //  يُستدعى مرة واحدة عند بدء التشغيل
    // ════════════════════════════════════════════════════════════════════
    private static void ApplyOfflineRegen()
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        // ── طاقة ──────────────────────────────────────────────────────────
        long lastEnergy = Preferences.Get(KeyEnergy, now);
        int energyTicks = CalcTicks(now, lastEnergy, EnergyIntervalMin);
        if (energyTicks > 0)
        {
            _player.Energy = Math.Min(_player.MaxEnergy,
                                      _player.Energy + energyTicks * EnergyPerTick);
            System.Diagnostics.Debug.WriteLine($"[Regen Offline] Energy +{energyTicks * EnergyPerTick}");
        }
        Preferences.Set(KeyEnergy, now);

        // ── شجاعة ─────────────────────────────────────────────────────────
        long lastCourage = Preferences.Get(KeyCourage, now);
        int courageTicks = CalcTicks(now, lastCourage, CourageIntervalMin);
        if (courageTicks > 0)
        {
            _player.Courage = Math.Min(_player.MaxCourage,
                                       _player.Courage + courageTicks * CouragePerTick);
            System.Diagnostics.Debug.WriteLine($"[Regen Offline] Courage +{courageTicks * CouragePerTick}");
        }
        Preferences.Set(KeyCourage, now);

        // ── شهامة ─────────────────────────────────────────────────────────
        long lastNobility = Preferences.Get(KeyNobility, now);
        int nobilityTicks = CalcTicks(now, lastNobility, NobilityIntervalMin);
        if (nobilityTicks > 0)
        {
            _player.NobilityCurrent = Math.Min(100,
                                               _player.NobilityCurrent + nobilityTicks * NobilityPerTick);
            System.Diagnostics.Debug.WriteLine($"[Regen Offline] Nobility +{nobilityTicks * NobilityPerTick}");
        }
        Preferences.Set(KeyNobility, now);

        // ── صحة ───────────────────────────────────────────────────────────
        long lastHealth = Preferences.Get(KeyHealth, now);
        int healthTicks = CalcTicks(now, lastHealth, HealthIntervalMin);
        if (healthTicks > 0)
        {
            int healthGain = (int)(_player.MaxHealth * HealthPercentPerTick * healthTicks);
            _player.Health = Math.Min(_player.MaxHealth, _player.Health + healthGain);
            System.Diagnostics.Debug.WriteLine($"[Regen Offline] Health +{healthGain}");
        }
        Preferences.Set(KeyHealth, now);
    }

    // ════════════════════════════════════════════════════════════════════
    //  دورة التايمر — كل دقيقة
    // ════════════════════════════════════════════════════════════════════
    private static void OnTick(object sender, ElapsedEventArgs e)
    {
        if (_player == null) return;

        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        // ── طاقة ──────────────────────────────────────────────────────────
        long lastEnergy = Preferences.Get(KeyEnergy, now);
        if (CalcTicks(now, lastEnergy, EnergyIntervalMin) >= 1)
        {
            if (_player.Energy < _player.MaxEnergy)
            {
                _player.Energy = Math.Min(_player.MaxEnergy, _player.Energy + EnergyPerTick);
                System.Diagnostics.Debug.WriteLine($"[Regen] Energy → {_player.Energy}/{_player.MaxEnergy}");
            }
            Preferences.Set(KeyEnergy, now);
        }

        // ── شجاعة ─────────────────────────────────────────────────────────
        long lastCourage = Preferences.Get(KeyCourage, now);
        if (CalcTicks(now, lastCourage, CourageIntervalMin) >= 1)
        {
            if (_player.Courage < _player.MaxCourage)
            {
                _player.Courage = Math.Min(_player.MaxCourage, _player.Courage + CouragePerTick);
                System.Diagnostics.Debug.WriteLine($"[Regen] Courage → {_player.Courage}/{_player.MaxCourage}");
            }
            Preferences.Set(KeyCourage, now);
        }

        // ── شهامة ─────────────────────────────────────────────────────────
        long lastNobility = Preferences.Get(KeyNobility, now);
        if (CalcTicks(now, lastNobility, NobilityIntervalMin) >= 1)
        {
            if (_player.NobilityCurrent < 100)
            {
                _player.NobilityCurrent = Math.Min(100, _player.NobilityCurrent + NobilityPerTick);
                System.Diagnostics.Debug.WriteLine($"[Regen] Nobility → {_player.NobilityCurrent}/100");
            }
            Preferences.Set(KeyNobility, now);
        }

        // ── صحة ───────────────────────────────────────────────────────────
        long lastHealth = Preferences.Get(KeyHealth, now);
        if (CalcTicks(now, lastHealth, HealthIntervalMin) >= 1)
        {
            if (_player.Health < _player.MaxHealth)
            {
                int gain = (int)(_player.MaxHealth * HealthPercentPerTick);
                _player.Health = Math.Min(_player.MaxHealth, _player.Health + gain);
                System.Diagnostics.Debug.WriteLine($"[Regen] Health → {_player.Health}/{_player.MaxHealth}");
            }
            Preferences.Set(KeyHealth, now);
        }
    }

    // ════════════════════════════════════════════════════════════════════
    //  حساب عدد الدورات المنقضية بين وقتين
    // ════════════════════════════════════════════════════════════════════
    private static int CalcTicks(long nowSec, long lastSec, double intervalMin)
    {
        double elapsedMin = (nowSec - lastSec) / 60.0;
        return (int)(elapsedMin / intervalMin);
    }
}