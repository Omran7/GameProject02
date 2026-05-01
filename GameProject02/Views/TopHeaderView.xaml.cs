using GameProject02.Models;
using GameProject02.Services;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using System;
using System.Timers;

namespace GameProject02.Views;

public partial class TopHeaderView : ContentView
{
    private System.Timers.Timer _refreshTimer;
    private PlayerAccount _player;

    public TopHeaderView()
    {
        InitializeComponent();
        // Subscribe to avatar changes
        PlayerAccount.AvatarChanged += OnAvatarChanged;
        StartRefreshTimer();
    }

    private void StartRefreshTimer()
    {
        _refreshTimer = new System.Timers.Timer(3000);
        _refreshTimer.Elapsed += (s, e) => MainThread.BeginInvokeOnMainThread(UpdateStats);
        _refreshTimer.AutoReset = true;
        _refreshTimer.Enabled = true;

        UpdateStats();
    }

    private void UpdateStats()
    {
        var player = AccountService.GetCurrentPlayer();
        if (player == null) return;

        // 1. Basic Info
        PlayerNameLabel.Text = player.Username?.ToUpper() ?? "PLAYER";
        LevelLabel.Text = $"LVL {player.Level}";

        // 2. Gold and Diamonds
        GoldLabel.Text = player.Gold.ToString("N0");
        DiamondButton.Text = $"💎 {player.Diamonds}";

        // 3. Status Bars Logic

        // Courage Bar
        if (player.MaxCourage > 0)
            CourageBar.Progress = (double)player.Courage / player.MaxCourage;

        // Health Bar
        if (player.MaxHealth > 0)
            HPBar.Progress = (double)player.Health / player.MaxHealth;

        // Energy Bar
        if (player.MaxEnergy > 0)
            EnergyBar.Progress = (double)player.Energy / player.MaxEnergy;

        // Nobility Bar (Based on Max 100 from your PlayerAccount model)
        // If NobilityCurrent is 100, bar is full.
        NobilityBar.Progress = (double)player.NobilityCurrent / 100.0;

        // XP Bar
        if (player.MaxXP > 0)
            XPBar.Progress = (double)player.CurrentXP / player.MaxXP;

    }

    private void OnAvatarChanged(string newPath)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            AvatarImage.Source = ImageSource.FromFile(newPath);
        });
    }

    ~TopHeaderView()
    {
        PlayerAccount.AvatarChanged -= OnAvatarChanged;
    }
}