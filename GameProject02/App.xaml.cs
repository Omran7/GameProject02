using GameProject02.Models;
using GameProject02.Services;
using GameProject02.Views;
using System.Timers;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel;
using System.Threading.Tasks;

namespace GameProject02;

public partial class App : Application
{
    private System.Timers.Timer _confinementTimer;
    public static int CurrentCourage { get; set; } = 100;

    public App()
    {
        InitializeComponent();
        MainPage = new NavigationPage(new LoginPage());

        StartConfinementTimer();

#if DEBUG
        CreateTestAccounts();
#endif
    }

    private void StartConfinementTimer()
    {
        _confinementTimer = new System.Timers.Timer(60000);
        _confinementTimer.Elapsed += OnConfinementTimerElapsed;
        _confinementTimer.Start();
    }

    private void OnConfinementTimerElapsed(object sender, ElapsedEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            var allPlayers = AccountService.GetAllPlayers();
            foreach (var player in allPlayers)
            {
                player.CrimeObject.CheckConfinementStatus();
            }

            var currentPlayer = AccountService.GetCurrentPlayer();
            if (currentPlayer != null)
            {
                if (currentPlayer.CrimeObject.IsInPrison)
                    CheckAndNavigateToPrisonIfNeeded();
                else if (currentPlayer.CrimeObject.IsInHospital)
                    CheckAndNavigateToHospitalIfNeeded();
            }
        });
    }

    protected override void OnSleep()
    {
        _confinementTimer?.Stop();
        base.OnSleep();
    }

    protected override void OnResume()
    {
        base.OnResume();
        _confinementTimer?.Start();

        var player = AccountService.GetCurrentPlayer();
        if (player != null)
        {
            NobilityService.UpdateNobility(player);
        }

        CheckAndNavigateToPrisonIfNeeded();
        CheckAndNavigateToHospitalIfNeeded();
    }

    // ── Test accounts (debug only) ──────────────────────────────────
#if DEBUG
    private async void CreateTestAccounts()
    {
        var testAccounts = new[]
        {
            ("1234", "1234"),
            ("omran1", "omran"),
            ("omran2", "omran"),
            ("omran3", "omran"),
            ("omran4", "omran"),
            ("omran5", "omran"),
            ("omran6", "omran"),
            ("omran7", "omran"),
            ("omran8", "omran"),
            ("omran9", "omran"),
            ("omran10", "omran"),
        };

        foreach (var (username, password) in testAccounts)
        {
            // 1) Does this username already exist in the cloud?
            string existingPlayerId = await AccountService.GetPlayerIdByUsernameAsync(username);
            if (existingPlayerId != null)
            {
                // Username is already taken → log in silently (don't register)
                bool loggedIn = await AccountService.LoginAsync(username, password);
                if (!loggedIn)
                {
                    System.Diagnostics.Debug.WriteLine($"[TEST] Could not log in existing account {username}");
                }
                continue;   // move on to the next test account
            }

            // 2) Username is free → register normally
            bool success = await AccountService.RegisterAccountAsync(username, password);
            if (success)
            {
                var player = AccountService.GetCurrentPlayer();
                player.Gold = 999999999;
                AddTestEstates(player);

                _ = FirebaseService.SavePlayerAsync(player);
                System.Diagnostics.Debug.WriteLine($"[TEST] Created & saved: {username}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[TEST] Registration failed for {username}");
            }
        }

        // Finally, log in as the main test account "1234"
        await AccountService.LoginAsync("1234", "1234");
        var mainPlayer = AccountService.GetCurrentPlayer();
        if (mainPlayer != null)
            NobilityService.UpdateNobility(mainPlayer);
    }
#endif

    private void AddTestEstates(PlayerAccount player)
    {
        player.Estates.Add(new EstateObject
        {
            Id = 1,
            EstateOwnerId = player.PlayerId,
            IsUsed = true,
            LastTaxPaidTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            FixedModifications = new List<bool> { true, false, false }
        });
        player.Estates.Add(new EstateObject
        {
            Id = 2,
            EstateOwnerId = player.PlayerId,
            IsUsed = true,
            LastTaxPaidTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            FixedModifications = new List<bool> { true, false, false }
        });
        player.Estates.Add(new EstateObject
        {
            Id = 6,
            EstateOwnerId = player.PlayerId,
            IsUsed = true,
            LastTaxPaidTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            FixedModifications = new List<bool> { true, false, false }
        });
    }

    // ── Prison / Hospital redirects ─────────────────────────────────
    private void CheckAndNavigateToPrisonIfNeeded()
    {
        var player = AccountService.GetCurrentPlayer();
        if (player == null || !player.CrimeObject.IsInPrison) return;

        player.CrimeObject.CheckConfinementStatus();
        if (player.CrimeObject.IsInPrison)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Task.Delay(500);
                if (MainPage is NavigationPage navPage)
                {
                    if (navPage.CurrentPage is PrisonPage) return;
                    if (navPage.CurrentPage is MainPage)
                    {
                        await navPage.Navigation.PushModalAsync(new PrisonPage());
                    }
                }
            });
        }
    }

    private void CheckAndNavigateToHospitalIfNeeded()
    {
        var player = AccountService.GetCurrentPlayer();
        if (player == null || !player.CrimeObject.IsInHospital) return;

        player.CrimeObject.CheckConfinementStatus();
        if (player.CrimeObject.IsInHospital)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Task.Delay(500);
                if (MainPage is NavigationPage navPage)
                {
                    if (navPage.CurrentPage is HospitalPage) return;
                    if (navPage.CurrentPage is MainPage)
                    {
                        await navPage.Navigation.PushModalAsync(new HospitalPage());
                    }
                }
            });
        }
    }

    protected override void OnStart()
    {
        base.OnStart();
        MainPage = new NavigationPage(new LoginPage());
        StartConfinementTimer();

#if DEBUG
        var player = AccountService.GetCurrentPlayer();
        if (player != null)
            NobilityService.UpdateNobility(player);
#endif
    }
}