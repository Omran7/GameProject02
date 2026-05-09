using GameProject02.Models;
using Microsoft.Maui.Storage;
using System;
using System.Timers;

namespace GameProject02.Services;

public static class RegenerationService
{
    private const int EnergyPerTick = 1;
    private const double EnergyIntervalMin = 2.0;
    private static bool _energyFullNotified = false;

    private const int CouragePerTick = 1;
    private const double CourageIntervalMin = 2.0;
    private static bool _courageFullNotified = false;

    private const int NobilityPerTick = 1;
    private const double NobilityIntervalMin = 5.0;
    private static bool _nobilityFullNotified = false;

    private const double HealthPercentPerTick = 0.05;
    private const double HealthIntervalMin = 2.0;

    private const string KeyEnergy = "regen_last_energy";
    private const string KeyCourage = "regen_last_courage";
    private const string KeyNobility = "regen_last_nobility";
    private const string KeyHealth = "regen_last_health";

    private static System.Timers.Timer _timer;
    private static PlayerAccount _player;

    public static void Start(PlayerAccount player)
    {
        _player = player;
        _energyFullNotified = false;
        _courageFullNotified = false;
        _nobilityFullNotified = false;

        ApplyOfflineRegen();
        Stop();

        _timer = new System.Timers.Timer(60_000);
        _timer.Elapsed += OnTick;
        _timer.AutoReset = true;
        _timer.Start();
    }

    public static void Stop()
    {
        if (_timer == null) return;
        _timer.Stop();
        _timer.Dispose();
        _timer = null;
    }

    private static void ApplyOfflineRegen()
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        // Energy
        long lastEnergy = Preferences.Get(KeyEnergy, now);
        int energyTicks = CalcTicks(now, lastEnergy, EnergyIntervalMin);
        if (energyTicks > 0)
        {
            int old = _player.Energy;
            _player.Energy = Math.Min(_player.MaxEnergy, _player.Energy + energyTicks * EnergyPerTick);
            if (old < _player.MaxEnergy && _player.Energy >= _player.MaxEnergy && !_energyFullNotified)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                    NotificationService.AddGameNotification("⚡ الطاقة ممتلئة!", "طاقتك تجددت بالكامل. ابدأ جريمة جديدة!", GameNotificationPriority.Normal, "⚡", "CrimePage"));
                _energyFullNotified = true;
            }
        }
        Preferences.Set(KeyEnergy, now);

        // Courage
        long lastCourage = Preferences.Get(KeyCourage, now);
        int courageTicks = CalcTicks(now, lastCourage, CourageIntervalMin);
        if (courageTicks > 0)
        {
            int old = _player.Courage;
            _player.Courage = Math.Min(_player.MaxCourage, _player.Courage + courageTicks * CouragePerTick);
            if (old < _player.MaxCourage && _player.Courage >= _player.MaxCourage && !_courageFullNotified)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                    NotificationService.AddGameNotification("🔥 الشجاعة ممتلئة!", "شجاعتك تجددت بالكامل. نفذ جريمة جديدة!", GameNotificationPriority.Normal, "🔥", "CrimePage"));
                _courageFullNotified = true;
            }
        }
        Preferences.Set(KeyCourage, now);

        // Nobility
        long lastNobility = Preferences.Get(KeyNobility, now);
        int nobilityTicks = CalcTicks(now, lastNobility, NobilityIntervalMin);
        if (nobilityTicks > 0)
        {
            int old = _player.NobilityCurrent;
            _player.NobilityCurrent = Math.Min(100, _player.NobilityCurrent + nobilityTicks * NobilityPerTick);
            if (old < 100 && _player.NobilityCurrent >= 100 && !_nobilityFullNotified)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                    NotificationService.AddGameNotification("🎖️ الشهامة ممتلئة!", "شهامتك تجددت بالكامل. استخدمها في الجرائم!", GameNotificationPriority.Normal, "🎖️", "CrimePage"));
                _nobilityFullNotified = true;
            }
        }
        Preferences.Set(KeyNobility, now);

        // Health
        long lastHealth = Preferences.Get(KeyHealth, now);
        int healthTicks = CalcTicks(now, lastHealth, HealthIntervalMin);
        if (healthTicks > 0)
        {
            int gain = (int)(_player.MaxHealth * HealthPercentPerTick * healthTicks);
            _player.Health = Math.Min(_player.MaxHealth, _player.Health + gain);
        }
        Preferences.Set(KeyHealth, now);
    }

    private static void OnTick(object sender, ElapsedEventArgs e)
    {
        if (_player == null) return;
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        // Energy
        long lastEnergy = Preferences.Get(KeyEnergy, now);
        if (CalcTicks(now, lastEnergy, EnergyIntervalMin) >= 1 && _player.Energy < _player.MaxEnergy)
        {
            int old = _player.Energy;
            _player.Energy = Math.Min(_player.MaxEnergy, _player.Energy + EnergyPerTick);
            if (old < _player.MaxEnergy && _player.Energy >= _player.MaxEnergy && !_energyFullNotified)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                    NotificationService.AddGameNotification("⚡ الطاقة ممتلئة!", "طاقتك تجددت بالكامل. ابدأ جريمة جديدة!", GameNotificationPriority.Normal, "⚡", "CrimePage"));
                _energyFullNotified = true;
            }
            Preferences.Set(KeyEnergy, now);
        }

        // Courage
        long lastCourage = Preferences.Get(KeyCourage, now);
        if (CalcTicks(now, lastCourage, CourageIntervalMin) >= 1 && _player.Courage < _player.MaxCourage)
        {
            int old = _player.Courage;
            _player.Courage = Math.Min(_player.MaxCourage, _player.Courage + CouragePerTick);
            if (old < _player.MaxCourage && _player.Courage >= _player.MaxCourage && !_courageFullNotified)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                    NotificationService.AddGameNotification("🔥 الشجاعة ممتلئة!", "شجاعتك تجددت بالكامل. نفذ جريمة جديدة!", GameNotificationPriority.Normal, "🔥", "CrimePage"));
                _courageFullNotified = true;
            }
            Preferences.Set(KeyCourage, now);
        }

        // Nobility
        long lastNobility = Preferences.Get(KeyNobility, now);
        if (CalcTicks(now, lastNobility, NobilityIntervalMin) >= 1 && _player.NobilityCurrent < 100)
        {
            int old = _player.NobilityCurrent;
            _player.NobilityCurrent = Math.Min(100, _player.NobilityCurrent + NobilityPerTick);
            if (old < 100 && _player.NobilityCurrent >= 100 && !_nobilityFullNotified)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                    NotificationService.AddGameNotification("🎖️ الشهامة ممتلئة!", "شهامتك تجددت بالكامل. استخدمها في الجرائم!", GameNotificationPriority.Normal, "🎖️", "CrimePage"));
                _nobilityFullNotified = true;
            }
            Preferences.Set(KeyNobility, now);
        }

        // Health
        long lastHealth = Preferences.Get(KeyHealth, now);
        if (CalcTicks(now, lastHealth, HealthIntervalMin) >= 1 && _player.Health < _player.MaxHealth)
        {
            int gain = (int)(_player.MaxHealth * HealthPercentPerTick);
            _player.Health = Math.Min(_player.MaxHealth, _player.Health + gain);
            Preferences.Set(KeyHealth, now);
        }
    }

    private static int CalcTicks(long nowSec, long lastSec, double intervalMin) =>
        (int)((nowSec - lastSec) / 60.0 / intervalMin);
}