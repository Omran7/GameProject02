using GameProject02.Models;
using GameProject02.Services;
using GameProject02.Views;
using System.Timers;
using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel;

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
        CreateTestAccount();
        CreateTestAccount1();
        CreateTestAccount2();
        CreateTestAccount3();
        CreateTestAccount4();
        CreateTestAccount5();
        CreateTestAccount6();
        CreateTestAccount7();
        CreateTestAccount8();
        CreateTestAccount9();
        CreateTestAccount10();
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

    // ✅ UPDATE NOBILITY ON APP RESUME (HANDLES OFFLINE RECOVERY)
    protected override void OnResume()
    {
        base.OnResume();
        _confinementTimer?.Start();

        // ✅ CRITICAL: UPDATE NOBILITY AFTER OFFLINE TIME (AUTHENTIC OLD GAME)
        var player = AccountService.GetCurrentPlayer();
        if (player != null)
        {
            NobilityService.UpdateNobility(player);
            System.Diagnostics.Debug.WriteLine($"[APP RESUME] Nobility updated to {player.NobilityCurrent}/100");
        }

        CheckAndNavigateToPrisonIfNeeded();
        CheckAndNavigateToHospitalIfNeeded();
    }

    private void CreateTestAccount()
    {
        var existingAccount = AccountService.GetAllPlayers()
            .FirstOrDefault(p => p.Username.Equals("omran", StringComparison.OrdinalIgnoreCase));

        if (existingAccount != null)
        {
            AccountService.Login("omran", "omran");
            return;
        }

        bool success = AccountService.RegisterAccount("omran", "omran");
        if (success)
        {
            var player = AccountService.GetCurrentPlayer();
            if (player != null)
            {
                player.Gold = 999999999;
                player.Level = 10;
                
                AddTestEstates(player);
                AccountService.Login("omran", "omran");
            }
        }
    }

    private void CreateTestAccount1()
    {
        var existingAccount = AccountService.GetAllPlayers()
            .FirstOrDefault(p => p.Username.Equals("omran1", StringComparison.OrdinalIgnoreCase));

        if (existingAccount != null)
        {
            AccountService.Login("omran1", "omran");
            return;
        }

        bool success = AccountService.RegisterAccount("omran1", "omran");
        if (success)
        {
            var player = AccountService.GetCurrentPlayer();
            if (player != null)
            {
                player.Gold = 999999999;
                AddTestEstates(player);
                AccountService.Login("omran1", "omran");
            }
        }
    }
    private void CreateTestAccount2()
    {
        var existingAccount = AccountService.GetAllPlayers()
            .FirstOrDefault(p => p.Username.Equals("omran2", StringComparison.OrdinalIgnoreCase));

        if (existingAccount != null)
        {
            AccountService.Login("omran2", "omran");
            return;
        }

        bool success = AccountService.RegisterAccount("omran2", "omran");
        if (success)
        {
            var player = AccountService.GetCurrentPlayer();
            if (player != null)
            {
                player.Gold = 999999999;
                AddTestEstates(player);
                AccountService.Login("omran2", "omran");
            }
        }
    }
    private void CreateTestAccount3()
    {
        var existingAccount = AccountService.GetAllPlayers()
            .FirstOrDefault(p => p.Username.Equals("omran3", StringComparison.OrdinalIgnoreCase));

        if (existingAccount != null)
        {
            AccountService.Login("omran3", "omran");
            return;
        }

        bool success = AccountService.RegisterAccount("omran3", "omran");
        if (success)
        {
            var player = AccountService.GetCurrentPlayer();
            if (player != null)
            {
                player.Gold = 999999999;
                AddTestEstates(player);
                AccountService.Login("omran3", "omran");
            }
        }
    }
    private void CreateTestAccount4()
    {
        var existingAccount = AccountService.GetAllPlayers()
            .FirstOrDefault(p => p.Username.Equals("omran4", StringComparison.OrdinalIgnoreCase));

        if (existingAccount != null)
        {
            AccountService.Login("omran4", "omran");
            return;
        }

        bool success = AccountService.RegisterAccount("omran4", "omran");
        if (success)
        {
            var player = AccountService.GetCurrentPlayer();
            if (player != null)
            {
                player.Gold = 999999999;
                AddTestEstates(player);
                AccountService.Login("omran4", "omran");
            }
        }
    }
    private void CreateTestAccount5()
    {
        var existingAccount = AccountService.GetAllPlayers()
            .FirstOrDefault(p => p.Username.Equals("omran5", StringComparison.OrdinalIgnoreCase));

        if (existingAccount != null)
        {
            AccountService.Login("omran5", "omran");
            return;
        }

        bool success = AccountService.RegisterAccount("omran5", "omran");
        if (success)
        {
            var player = AccountService.GetCurrentPlayer();
            if (player != null)
            {
                player.Gold = 999999999;
                AddTestEstates(player);
                AccountService.Login("omran5", "omran");
            }
        }
    }
    private void CreateTestAccount6()
    {
        var existingAccount = AccountService.GetAllPlayers()
            .FirstOrDefault(p => p.Username.Equals("omran6", StringComparison.OrdinalIgnoreCase));

        if (existingAccount != null)
        {
            AccountService.Login("omran6", "omran");
            return;
        }

        bool success = AccountService.RegisterAccount("omran6", "omran");
        if (success)
        {
            var player = AccountService.GetCurrentPlayer();
            if (player != null)
            {
                player.Gold = 999999999;
                AddTestEstates(player);
                AccountService.Login("omran6", "omran");
            }
        }
    }
    private void CreateTestAccount7()
    {
        var existingAccount = AccountService.GetAllPlayers()
            .FirstOrDefault(p => p.Username.Equals("omran7", StringComparison.OrdinalIgnoreCase));

        if (existingAccount != null)
        {
            AccountService.Login("omran7", "omran");
            return;
        }

        bool success = AccountService.RegisterAccount("omran7", "omran");
        if (success)
        {
            var player = AccountService.GetCurrentPlayer();
            if (player != null)
            {
                player.Gold = 999999999;
                AddTestEstates(player);
                AccountService.Login("omran7", "omran");
            }
        }
    }
    private void CreateTestAccount8()
    {
        var existingAccount = AccountService.GetAllPlayers()
            .FirstOrDefault(p => p.Username.Equals("omran8", StringComparison.OrdinalIgnoreCase));

        if (existingAccount != null)
        {
            AccountService.Login("omran8", "omran");
            return;
        }

        bool success = AccountService.RegisterAccount("omran8", "omran");
        if (success)
        {
            var player = AccountService.GetCurrentPlayer();
            if (player != null)
            {
                player.Gold = 999999999;
                AddTestEstates(player);
                AccountService.Login("omran8", "omran");
            }
        }
    }
    private void CreateTestAccount9()
    {
        var existingAccount = AccountService.GetAllPlayers()
            .FirstOrDefault(p => p.Username.Equals("omran9", StringComparison.OrdinalIgnoreCase));

        if (existingAccount != null)
        {
            AccountService.Login("omran9", "omran");
            return;
        }

        bool success = AccountService.RegisterAccount("omran9", "omran");
        if (success)
        {
            var player = AccountService.GetCurrentPlayer();
            if (player != null)
            {
                player.Gold = 999999999;
                AddTestEstates(player);
                AccountService.Login("omran9", "omran");
            }
        }
    }
    private void CreateTestAccount10()
    {
        var existingAccount = AccountService.GetAllPlayers()
            .FirstOrDefault(p => p.Username.Equals("omran10", StringComparison.OrdinalIgnoreCase));

        if (existingAccount != null)
        {
            AccountService.Login("omran10", "omran");
            return;
        }

        bool success = AccountService.RegisterAccount("omran10", "omran");
        if (success)
        {
            var player = AccountService.GetCurrentPlayer();
            if (player != null)
            {
                player.Gold = 999999999;
                AddTestEstates(player);
                AccountService.Login("omran10", "omran");
            }
        }
    }

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
                    // ✅ إذا كانت الصفحة الحالية هي السجن بالفعل، لا تفعل شيئاً
                    if (navPage.CurrentPage is PrisonPage)
                        return;

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
                    // ✅ إذا كانت الصفحة الحالية هي المستشفى بالفعل، لا تفعل شيئاً
                    if (navPage.CurrentPage is HospitalPage)
                        return;

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
        CreateTestAccount();
        CreateTestAccount1();

        // ✅ UPDATE NOBILITY AFTER TEST ACCOUNT CREATION
        var player = AccountService.GetCurrentPlayer();
        if (player != null)
        {
            NobilityService.UpdateNobility(player);
        }
#endif
    }
}