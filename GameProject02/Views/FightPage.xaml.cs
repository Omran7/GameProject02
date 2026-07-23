using GameProject02.Models;
using GameProject02.Services;
using Microsoft.Maui.Controls;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace GameProject02.Views;

public partial class FightPage : ContentPage, INotifyPropertyChanged
{
    private PlayerAccount _player;
    private PlayerAccount _opponentPlayer;
    private FightClubPlayer _opponentDTO;
    private bool _fightActive = true;
    private bool _autoFightRunning = true;
    private bool _playerWon = false;
    private bool _isDraw = false;
    private Timer _redirectTimer;

    // Binding properties
    private string _yourName;
    private string _yourImage;
    private int _yourLevel;
    private int _yourHealth;
    private int _yourMaxHealth;
    private double _yourHealthPercent;

    private string _opponentName;
    private string _opponentImage;
    private int _opponentLevel;
    private int _opponentHealth;
    private int _opponentMaxHealth;
    private double _opponentHealthPercent;

    // ✅ POLICE ARREST TIMER (30 seconds after winning) - NON-DESTRUCTIVE ADDITION
    private Timer _policeTimer;
    private bool _actionTaken = false; // Prevent multiple actions

    public string YourName { get => _yourName; set { _yourName = value; OnPropertyChanged(); } }
    public string YourImage { get => _yourImage; set { _yourImage = value; OnPropertyChanged(); } }
    public int YourLevel { get => _yourLevel; set { _yourLevel = value; OnPropertyChanged(); } }
    public int YourHealth
    {
        get => _yourHealth;
        set { _yourHealth = value; OnPropertyChanged(); YourHealthPercent = (double)YourHealth / YourMaxHealth; }
    }
    public int YourMaxHealth { get => _yourMaxHealth; set { _yourMaxHealth = value; OnPropertyChanged(); } }
    public double YourHealthPercent { get => _yourHealthPercent; set { _yourHealthPercent = value; OnPropertyChanged(); } }

    public string OpponentName { get => _opponentName; set { _opponentName = value; OnPropertyChanged(); } }
    public string OpponentImage { get => _opponentImage; set { _opponentImage = value; OnPropertyChanged(); } }
    public int OpponentLevel { get => _opponentLevel; set { _opponentLevel = value; OnPropertyChanged(); } }
    public int OpponentHealth
    {
        get => _opponentHealth;
        set { _opponentHealth = value; OnPropertyChanged(); OpponentHealthPercent = (double)OpponentHealth / OpponentMaxHealth; }
    }
    public int OpponentMaxHealth { get => _opponentMaxHealth; set { _opponentMaxHealth = value; OnPropertyChanged(); } }
    public double OpponentHealthPercent { get => _opponentHealthPercent; set { _opponentHealthPercent = value; OnPropertyChanged(); } }

    public FightPage(PlayerAccount player, FightClubPlayer opponent)
    {
        InitializeComponent();
        _player = player;
        _opponentDTO = opponent;
        BindingContext = this;
        LoadFighters();
    }

    private async void LoadFighters()
    {
        if (_player == null || _opponentDTO == null)
        {
            await DisplayAlert("خطأ", "بيانات غير صالحة", "موافق");
            await Navigation.PopAsync(false);
            return;
        }

        // Get actual opponent PlayerAccount from global list
        _opponentPlayer = AccountService.GetAllPlayers().FirstOrDefault(p => p.PlayerId == _opponentDTO.PlayerId);
        if (_opponentPlayer == null)
        {
            await DisplayAlert("خطأ", "الخصم غير موجود", "موافق");
            await Navigation.PopAsync(false);
            return;
        }

        // Check player confinement
        _player.CrimeObject.CheckConfinementStatus();
        if (_player.CrimeObject.IsInPrison || _player.CrimeObject.IsInHospital)
        {
            await DisplayAlert("غير مسموح", "لا يمكنك القتال أثناء وجودك في السجن أو المستشفى!", "موافق");
            await Navigation.PopAsync(false);
            return;
        }

        // Check opponent confinement
        _opponentPlayer.CrimeObject.CheckConfinementStatus();
        if (_opponentPlayer.CrimeObject.IsInPrison || _opponentPlayer.CrimeObject.IsInHospital)
        {
            await DisplayAlert("غير متاح", "هذا اللاعب غير متاح للقتال حالياً", "موافق");
            await Navigation.PopAsync(false);
            return;
        }

        // Set UI data
        YourName = _player.Username;
        YourImage = _player.ImageResource;
        YourLevel = _player.Level;
        YourHealth = _player.Health;
        YourMaxHealth = _player.MaxHealth;

        OpponentName = _opponentPlayer.Username;
        OpponentImage = _opponentPlayer.ImageResource;
        OpponentLevel = _opponentPlayer.Level;
        OpponentHealth = _opponentPlayer.Health;
        OpponentMaxHealth = _opponentPlayer.MaxHealth;

        // ✅ CRITICAL FIX: Call UpdateArmingDisplay AFTER opponent initialization
        UpdateArmingDisplay();

        AddLog("⚔️ بدأت المعركة!");
        AddLog($"أنت: المستوى {YourLevel} - {YourHealth}/{YourMaxHealth} صحة");
        AddLog($"الخصم: المستوى {OpponentLevel} - {OpponentHealth}/{OpponentMaxHealth} صحة");

        // Start automatic fight simulation
        await AutoFight();
    }

    private async Task AutoFight()
    {
        int round = 1;
        const int maxRounds = 20;

        while (round <= maxRounds && _player.Health > 0 && _opponentPlayer.Health > 0)
        {
            AddLog($"\n🔁 الجولة {round}");

            // Player attacks first (ONLY DEALS DAMAGE - NO REWARDS)
            var playerResult = FightClubService.ExecutePlayerAction(_player, _opponentPlayer, 0); // 0 = Attack
            AddLog($"🔹 {_player.Username}: {playerResult.message}");
            UpdateHealthDisplay();

            if (_opponentPlayer.Health <= 0) break;

            // Opponent counter-attacks
            var oppResult = FightClubService.ExecuteOpponentAction(_opponentPlayer, _player);
            AddLog($"🔸 {_opponentPlayer.Username}: {oppResult.message}");
            UpdateHealthDisplay();

            if (_player.Health <= 0) break;

            round++;
            await Task.Delay(600);
        }

        // Determine outcome
        if (_player.Health <= 0 && _opponentPlayer.Health <= 0)
        {
            _isDraw = true;
            _playerWon = false;
            AddLog("\n⚖️ تعادل! كلاكما فقدتم الصحة.");
        }
        else if (_opponentPlayer.Health <= 0)
        {
            _playerWon = true;
            _isDraw = false;
            AddLog($"\n🎉 لقد هزمت {_opponentPlayer.Username}!");
        }
        else if (_player.Health <= 0)
        {
            _playerWon = false;
            _isDraw = false;
            AddLog($"\n💀 لقد هزمك {_opponentPlayer.Username}!");
        }
        else
        {
            _isDraw = true;
            _playerWon = false;
            AddLog($"\n⚖️ انتهت {maxRounds} جولة بدون هزيمة → تعادل!");
        }

        _autoFightRunning = false;
        TurnLabel.Text = "انتهت المعركة";

        // ✅ POST-FIGHT ACTIONS (AUTHENTIC OLD GAME BEHAVIOR)
        if (_playerWon)
        {
            // ✅ SEND OPPONENT TO HOSPITAL (CONSEQUENCE - NOT REWARD)
            FightClubService.SendToHospital(_opponentPlayer, _player);
            AddLog($"🏥 تم نقل {_opponentPlayer.Username} إلى المستشفى");

            // ✅ ENABLE POST-FIGHT ACTION BUTTONS
            StealButton.IsEnabled = true;
            DisabilityButton.IsEnabled = true;

            // ✅ START 30-SECOND POLICE TIMER
            StartPoliceTimer();

            // ✅ NOTE: XP REWARD WILL BE GIVEN WHEN PLAYER CLICKS ESCAPE/LEAVE
            AddLog("🎉 لقد هزمت الخصم! اضغط 'هروب' أو 'مغادرة' للحصول على الخبرة.");
        }
        else if (_isDraw)
        {
            StealButton.IsEnabled = false;
            DisabilityButton.IsEnabled = false;
            AddLog("المعركة انتهت بالتعادل. يمكنك المغادرة.");
            StartRedirectTimer(true);
        }
        else // Player lost - ✅ STAY 30 SECONDS TO VIEW RESULTS
        {
            // ✅ SEND PLAYER TO HOSPITAL (REAL CONFINEMENT)
            FightClubService.SendToHospital(_player, _opponentPlayer);
            AddLog($"🏥 تم نقلك إلى المستشفى لمدة {GetHospitalMinutes(_player)} دقائق.");

            // ✅ DISABLE ALL ACTION BUTTONS (EXCEPT LEAVE/BACK)
            StealButton.IsEnabled = false;
            DisabilityButton.IsEnabled = false;
            // Note: LeaveButton (هروب) remains ENABLED for immediate exit

            AddLog("لقد خسرت المعركة. انتظر 30 ثانية لعرض النتائج، أو اضغط 'هروب' للخروج فوراً.");

            // ✅ START 30-SECOND TIMER TO AUTO-REDIRECT TO HOSPITAL
            StartLoserRedirectTimer();
        }
    }

    // ✅ 30-SECOND TIMER FOR LOSERS (VIEW RESULTS BEFORE HOSPITAL)
    private void StartLoserRedirectTimer()
    {
        _redirectTimer?.Dispose();

        AddLog("⏳ جاري عرض نتائج المعركة... (30 ثانية)");

        _redirectTimer = new Timer(_ =>
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                _redirectTimer?.Dispose();
                _redirectTimer = null;

                if (!_playerWon && !_isDraw) // Only if player lost
                {
                    AddLog("⏭️ تم الانتقال تلقائياً إلى المستشفى...");
                    await RedirectToHospitalAfterFight();
                }
            });
        }, null, 30000, Timeout.Infinite); // 30 seconds
    }
    // ✅ HELPER: GET HOSPITAL DURATION IN MINUTES (FOR LOG MESSAGES)
    private int GetHospitalMinutes(PlayerAccount player)
    {
        if (!player.CrimeObject.IsInHospital) return 0;
        long remainingMs = player.CrimeObject.HospitalReleaseTime - DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        return Math.Max(1, (int)TimeSpan.FromMilliseconds(remainingMs).TotalMinutes);
    }

    // ✅ AUTO-REDIRECT TO HOSPITAL AFTER FIGHT (LOSERS ONLY)
    private async Task RedirectToHospitalAfterFight()
    {
        await Navigation.PopAsync(false); // Close fight page

        _player.CrimeObject.CheckConfinementStatus();

        if (_player.CrimeObject.IsInHospital)
        {
            await Application.Current.MainPage.Navigation.PushModalAsync(new HospitalPage());
        }
        else
        {
            await Application.Current.MainPage.Navigation.PopToRootAsync(false);
        }
    }
    // ✅ START 30-SECOND POLICE TIMER (AUTHENTIC ARREST MECHANIC) - NON-DESTRUCTIVE ADDITION
    // ✅ START 30-SECOND POLICE TIMER (AUTHENTIC ARREST MECHANIC FOR WINNERS)
    private void StartPoliceTimer()
    {
        _actionTaken = false;
        _policeTimer?.Dispose();

        AddLog("⏰ ⚠️ الشرطة قادمة! اختر إجراء واحد خلال 30 ثانية أو سيتم اعتقالك!");

        _policeTimer = new Timer(_ =>
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                if (!_actionTaken && _playerWon)
                {
                    // ✅ POLICE ARREST: 2 MINUTES IN PRISON FOR STAYING TOO LONG
                    _player.CrimeObject.IsInPrison = true;
                    _player.CrimeObject.PrisonReleaseTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                        + (long)TimeSpan.FromMinutes(2).TotalMilliseconds;
                    _player.CrimeObject.PrisonReason = "بقيت في مكان الشجار بعد الفوز";
                    _player.CrimeObject.TotalPrisonVisits++;
                    _player.CrimeObject.PrisonBailAmount = 5000;

                    AddLog("🚨 تم اعتقالك! بقيت في مكان الشجار لمدة طويلة");
                    AddLog("🔒 تم سجنك لمدة دقيقتين");

                    // Redirect to prison after 1 second
                    await Task.Delay(1000);
                    await Navigation.PopAsync(false);
                    await Task.Delay(300);
                    _player.CrimeObject.CheckConfinementStatus();
                    if (_player.CrimeObject.IsInPrison)
                        await Application.Current.MainPage.Navigation.PushModalAsync(new PrisonPage());
                }
            });
        }, null, 30000, Timeout.Infinite); // 30 seconds
    }    // ✅ REDIRECT TO PRISON AFTER ARREST - NON-DESTRUCTIVE ADDITION
    private async Task RedirectToPrison()
    {
        await Navigation.PopAsync(false); // Close fight page

        // Check confinement status
        _player.CrimeObject.CheckConfinementStatus();

        if (_player.CrimeObject.IsInPrison)
        {
            await Application.Current.MainPage.Navigation.PushModalAsync(new PrisonPage());
        }
        else
        {
            await Application.Current.MainPage.Navigation.PopToRootAsync(false);
        }
    }

    private void StartRedirectTimer(bool isDraw)
    {
        _redirectTimer = new Timer(_ =>
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                _redirectTimer?.Dispose();
                _redirectTimer = null;
                if (isDraw)
                    await RedirectToMainPage();
                else
                    await RedirectToHospital();
            });
        }, null, 30000, Timeout.Infinite); // 30 seconds
    }

    private async Task RedirectToMainPage()
    {
        await Navigation.PopAsync(false); // Close fight page
        await Application.Current.MainPage.Navigation.PopToRootAsync(false);
    }

    private async Task RedirectToHospital()
    {
        await Navigation.PopAsync(false); // Close fight page
        // Check again in case player already freed
        _player.CrimeObject.CheckConfinementStatus();
        if (_player.CrimeObject.IsInHospital)
            await Application.Current.MainPage.Navigation.PushModalAsync(new HospitalPage());
    }

    private void UpdateHealthDisplay()
    {
        YourHealth = _player.Health;
        OpponentHealth = _opponentPlayer.Health;
    }

    // ✅ STEAL BUTTON: 5% OF OPPONENT'S GOLD (MIN 100)
    // ✅ STEAL BUTTON: 5% GOLD + 15 NOBILITY LOSS (AUTHENTIC OLD GAME)
    private async void OnStealClicked(object sender, EventArgs e)
    {
        if (!_playerWon || _actionTaken || !_fightActive) return;

        _actionTaken = true;
        StealButton.IsEnabled = false;
        DisabilityButton.IsEnabled = false;

        // ✅ STEAL 5% OF OPPONENT'S GOLD
        int stealAmount = Math.Max(100, (int)(_opponentPlayer.Gold * 0.05));
        _player.Gold += stealAmount;
        _opponentPlayer.Gold -= stealAmount;

        // ✅ APPLY NOBILITY LOSS (15 POINTS - FROM fight_club old 4.txt)
        int nobilityLoss = NobilityService.ApplyNobilityLoss(_player, "steal");
        AccountService.SavePlayer(_player); // Persist nobility change

        AddLog($"💰 سرقت {stealAmount:N0} ذهب وفقدت {nobilityLoss} نقاط شهامة!");
        await DisplayAlert("سرقة", $"سرقت {stealAmount:N0} ذهب!\n❌ فقدت {nobilityLoss} نقاط شهامة", "موافق");

        await Task.Delay(1000);
        await Navigation.PopToRootAsync(false);
    }

    // ✅ DISABILITY BUTTON: 30-MIN HOSPITAL + 25 NOBILITY LOSS
    private async void OnDisabilityClicked(object sender, EventArgs e)
    {
        if (!_playerWon || _actionTaken || !_fightActive) return;

        _actionTaken = true;
        StealButton.IsEnabled = false;
        DisabilityButton.IsEnabled = false;

        // ✅ SEND OPPONENT TO HOSPITAL FOR 30 MINUTES
        _opponentPlayer.CrimeObject.IsInHospital = true;
        _opponentPlayer.CrimeObject.HospitalReason = $"أُصيب بعاهة من {_player.Username} بعد المعركة";
        _opponentPlayer.CrimeObject.TotalHospitalVisits++;
        _opponentPlayer.CrimeObject.HospitalReleaseTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            + (long)TimeSpan.FromMinutes(30).TotalMilliseconds;
        _opponentPlayer.Health = Math.Max(1, _opponentPlayer.Health / 2);

        // ✅ APPLY NOBILITY LOSS (25 POINTS - FROM fight_club old 4.txt)
        int nobilityLoss = NobilityService.ApplyNobilityLoss(_player, "cripple");
        AccountService.SavePlayer(_player); // Persist nobility change

        AddLog($"🩸 أصبت {_opponentPlayer.Username} بعاهة! تم نقله للمستشفى لمدة 30 دقيقة");
        AddLog($"⚠️ فقدت {nobilityLoss} نقاط شهامة!");
        await DisplayAlert("عاهة", $"تم إرسال {_opponentPlayer.Username} للمستشفى لمدة 30 دقيقة\n❌ فقدت {nobilityLoss} نقاط شهامة", "موافق");

        await Task.Delay(1000);
        await Navigation.PopToRootAsync(false);
    }

    // ✅ LEAVE BUTTON: ESCAPE POLICE + 10 NOBILITY LOSS
    private async void OnLeaveClicked(object sender, EventArgs e)
    {
        _redirectTimer?.Dispose();
        _redirectTimer = null;

        if (_playerWon && !_actionTaken)
        {
            _actionTaken = true;
            StealButton.IsEnabled = false;
            DisabilityButton.IsEnabled = false;

            // ✅ GIVE XP REWARD FOR LEAVING AFTER WIN
            int xpReward = _opponentPlayer.Level * 50;
            _player.CurrentXP += xpReward;

            // Handle level‑up(s)
            while (_player.MainStatesObject.CanLevelUp())
            {
                _player.MainStatesObject.LevelUp();
                // ✅ Level‑up notification
                NotificationService.AddGameNotification(
                    $"🎉 المستوى {_player.Level}!",
                    $"تهانينا! وصلت للمستوى {_player.Level}\n+{_player.Level * 50} ذهب مكافأة",
                    GameNotificationPriority.High, "🏆", "ProfilePage"
                );
            }

            // ✅ APPLY NOBILITY LOSS FOR LEAVING EARLY (10 POINTS)
            int nobilityLoss = NobilityService.ApplyNobilityLoss(_player, "leave_early");
            AccountService.SavePlayer(_player);

            AddLog($"💰 حصلت على {xpReward} خبرة للمغادرة بعد الانتصار!");
            AddLog("🏃 هربت من مكان الشجار قبل وصول الشرطة!");
            AddLog($"⚠️ فقدت {nobilityLoss} نقاط شهامة!");
            await DisplayAlert("هروب ناجح", $"هربت قبل وصول الشرطة!\n❌ فقدت {nobilityLoss} نقاط شهامة\n حصلت على {xpReward} خبرة!", "موافق");
        }

        await Navigation.PopToRootAsync(false);
    }
    private void AddLog(string message)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            var logLabel = new Label
            {
                Text = message,
                TextColor = Color.FromArgb("#bdc3c7"),
                FontSize = 12,
                Margin = new Thickness(0, 2, 0, 2)
            };
            FightLogContainer.Children.Add(logLabel);
        });
    }

    // ✅ SAFE ARMING DISPLAY (WITH NULL CHECKS TO PREVENT CRASHES) - PRESERVED FROM YOUR CODE
    private void UpdateArmingDisplay()
    {
        System.Diagnostics.Debug.WriteLine($"[ARMING DEBUG] Player: {_player?.Username ?? "NULL"}, Opponent: {_opponentPlayer?.Username ?? "NULL"}");

        // ✅ YOUR WEAPON DISPLAY
        if (_player != null && _player.ArmingObject != null && !string.IsNullOrEmpty(_player.ArmingObject.WeaponId))
        {
            System.Diagnostics.Debug.WriteLine($"[ARMING DEBUG] Your weapon ID: {_player.ArmingObject.WeaponId}");

            if (_player.StockObject?.ItemsInStock?.TryGetValue(_player.ArmingObject.WeaponId, out var yourWeapon) == true && yourWeapon != null)
            {
                System.Diagnostics.Debug.WriteLine($"[ARMING DEBUG] Found weapon in stock: {yourWeapon.Name}, Damage: {yourWeapon.Damage}, Accuracy: {yourWeapon.Accuracy}");
                YourWeaponLabel.Text = yourWeapon.Name;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[ARMING DEBUG] WARNING: Weapon ID {_player.ArmingObject.WeaponId} NOT FOUND in stock!");
                YourWeaponLabel.Text = "مجهز (غير معروف)";
            }
        }
        else
        {
            YourWeaponLabel.Text = "غير مجهز";
            System.Diagnostics.Debug.WriteLine($"[ARMING DEBUG] No weapon equipped (WeaponId: '{_player?.ArmingObject?.WeaponId ?? "NULL"}')");
        }

        // ✅ YOUR ARMOR DISPLAY
        if (_player != null && _player.ArmingObject != null && !string.IsNullOrEmpty(_player.ArmingObject.ArmorId))
        {
            System.Diagnostics.Debug.WriteLine($"[ARMING DEBUG] Your armor ID: {_player.ArmingObject.ArmorId}");

            if (_player.StockObject?.ItemsInStock?.TryGetValue(_player.ArmingObject.ArmorId, out var yourArmor) == true && yourArmor != null)
            {
                System.Diagnostics.Debug.WriteLine($"[ARMING DEBUG] Found armor in stock: {yourArmor.Name}, Defense: {yourArmor.Defense}, Evasion: {yourArmor.Evasion}");
                YourArmorLabel.Text = yourArmor.Name;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[ARMING DEBUG] WARNING: Armor ID {_player.ArmingObject.ArmorId} NOT FOUND in stock!");
                YourArmorLabel.Text = "مجهز (غير معروف)";
            }
        }
        else
        {
            YourArmorLabel.Text = "غير مجهز";
            System.Diagnostics.Debug.WriteLine($"[ARMING DEBUG] No armor equipped (ArmorId: '{_player?.ArmingObject?.ArmorId ?? "NULL"}')");
        }

        // ✅ OPPONENT WEAPON DISPLAY
        if (_opponentPlayer != null && _opponentPlayer.ArmingObject != null && !string.IsNullOrEmpty(_opponentPlayer.ArmingObject.WeaponId))
        {
            System.Diagnostics.Debug.WriteLine($"[ARMING DEBUG] Opponent weapon ID: {_opponentPlayer.ArmingObject.WeaponId}");

            if (_opponentPlayer.StockObject?.ItemsInStock?.TryGetValue(_opponentPlayer.ArmingObject.WeaponId, out var oppWeapon) == true && oppWeapon != null)
            {
                System.Diagnostics.Debug.WriteLine($"[ARMING DEBUG] Found opponent weapon: {oppWeapon.Name}");
                OpponentWeaponLabel.Text = oppWeapon.Name;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[ARMING DEBUG] WARNING: Opponent weapon ID {_opponentPlayer.ArmingObject.WeaponId} NOT FOUND in stock!");
                OpponentWeaponLabel.Text = "مجهز (غير معروف)";
            }
        }
        else
        {
            OpponentWeaponLabel.Text = "غير مجهز";
            System.Diagnostics.Debug.WriteLine($"[ARMING DEBUG] No opponent weapon equipped (WeaponId: '{_opponentPlayer?.ArmingObject?.WeaponId ?? "NULL"}')");
        }

        // ✅ OPPONENT ARMOR DISPLAY
        if (_opponentPlayer != null && _opponentPlayer.ArmingObject != null && !string.IsNullOrEmpty(_opponentPlayer.ArmingObject.ArmorId))
        {
            System.Diagnostics.Debug.WriteLine($"[ARMING DEBUG] Opponent armor ID: {_opponentPlayer.ArmingObject.ArmorId}");

            if (_opponentPlayer.StockObject?.ItemsInStock?.TryGetValue(_opponentPlayer.ArmingObject.ArmorId, out var oppArmor) == true && oppArmor != null)
            {
                System.Diagnostics.Debug.WriteLine($"[ARMING DEBUG] Found opponent armor: {oppArmor.Name}");
                OpponentArmorLabel.Text = oppArmor.Name;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[ARMING DEBUG] WARNING: Opponent armor ID {_opponentPlayer.ArmingObject.ArmorId} NOT FOUND in stock!");
                OpponentArmorLabel.Text = "مجهز (غير معروف)";
            }
        }
        else
        {
            OpponentArmorLabel.Text = "غير مجهز";
            System.Diagnostics.Debug.WriteLine($"[ARMING DEBUG] No opponent armor equipped (ArmorId: '{_opponentPlayer?.ArmingObject?.ArmorId ?? "NULL"}')");
        }
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        if (_autoFightRunning)
        {
            await DisplayAlert("تنبيه", "لا يمكنك الخروج أثناء المعركة", "موافق");
            return;
        }

        // ✅ IF PLAYER LOST AND IN HOSPITAL, REDIRECT TO HOSPITAL (NOT JUST POP)
        if (!_playerWon && !_isDraw && _player.CrimeObject.IsInHospital)
        {
            _redirectTimer?.Dispose();
            _redirectTimer = null;
            await RedirectToHospitalAfterFight();
        }
        else
        {
            await Navigation.PopAsync(false);
        }
    }

    private async void OnHomeClicked(object sender, EventArgs e) => await Navigation.PopToRootAsync(false);
    private async void OnRefreshClicked(object sender, EventArgs e) { /* Not allowed during fight */ }

    public event PropertyChangedEventHandler PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}