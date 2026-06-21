using GameProject02.Helpers;
using GameProject02.Models;
using GameProject02.Services;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace GameProject02.Views;

public partial class NewsPage : ContentPage
{
    private System.Timers.Timer _clockTimer;
    private List<NewsItem> _playerAds = new();
    private List<NewsItem> _systemAds = new();
    private NewsItem _latestBounty;
    private string _activeTab = "ads";

    public NewsPage()
    {
        InitializeComponent();
        StartClock();

        // ✅ Subscribe to refresh messages from AdminAnnouncementPage
        MessagingCenter.Subscribe<AdminAnnouncementPage>(this, "RefreshNews", async (_) =>
        {
            Debug.WriteLine("[NEWS] RefreshNews message received!");
            await LoadAllData(forceRefresh: true);
        });
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        Debug.WriteLine("[NEWS] OnAppearing - loading data");
        await LoadAllData();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _clockTimer?.Stop();
        _clockTimer?.Dispose();
        MessagingCenter.Unsubscribe<AdminAnnouncementPage>(this, "RefreshNews");
    }

    // ── Live Clock ───────────────────────────────────────────
    private void StartClock()
    {
        _clockTimer = new System.Timers.Timer(1000);
        _clockTimer.Elapsed += (s, e) =>
            MainThread.BeginInvokeOnMainThread(UpdateClockUI);
        _clockTimer.AutoReset = true;
        _clockTimer.Enabled = true;
        UpdateClockUI();
    }

    private void UpdateClockUI()
    {
        var now = DateTime.Now;
        ServerTimeLabel.Text = now.ToString("HH:mm:ss");
        ServerDateLabel.Text = now.ToString("dd-MM-yyyy");
        ServerDayLabel.Text = now.ToString("dddd").ToUpper();
    }

    // ── Load Data ─────────────────────────────────────────────
    private async Task LoadAllData(bool forceRefresh = false)
    {
        try
        {
            Debug.WriteLine($"[NEWS] Loading data (forceRefresh={forceRefresh})...");

            // Always fetch fresh data
            _playerAds = await NewsService.GetPlayerAdsAsync();
            _systemAds = await NewsService.GetSystemAdsAsync();
            _latestBounty = await NewsService.GetLatestBountyAsync();

            Debug.WriteLine($"[NEWS] Player ads: {_playerAds.Count}, System ads: {_systemAds.Count}, Bounty: {_latestBounty != null}");

            UpdateFeaturedSections();
            ApplyActiveTab();

            // Force UI refresh if needed
            NewsList.ItemsSource = null;
            NewsList.ItemsSource = GetItemsForCurrentTab();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[NEWS] Load error: {ex.Message}");
        }
    }

    // ── Get items for the current tab ────────────────────────
    private List<NewsItem> GetItemsForCurrentTab()
    {
        return _activeTab switch
        {
            "ads" => _playerAds.Concat(_systemAds)
                .OrderByDescending(n => n.Timestamp)
                .ToList(),
            "system" => _systemAds,
            "rank" => new List<NewsItem>(),
            "jobs" => new List<NewsItem>(),
            "guard" => new List<NewsItem>(),
            _ => new List<NewsItem>()
        };
    }

    // ── Featured Sections ────────────────────────────────────
    private void UpdateFeaturedSections()
    {
        var lastAd = _playerAds.OrderByDescending(a => a.Timestamp).FirstOrDefault();
        if (lastAd != null)
        {
            LastAdAuthor.Text = lastAd.Author;
            LastAdContent.Text = lastAd.Content;
        }
        else
        {
            LastAdAuthor.Text = "—";
            LastAdContent.Text = "No ads yet";
        }

        if (_latestBounty != null)
        {
            BountyName.Text = _latestBounty.BountyPlayerName;
            BountyLevel.Text = $"Lv.{_latestBounty.BountyLevel}";
            BountyCost.Text = $"{_latestBounty.BountyCost:N0} ذهب";
            BountyPlace.Text = $"📍 {_latestBounty.BountyPlace}";
            BountyDesc.Text = _latestBounty.BountyDescription;
        }
        else
        {
            BountyName.Text = "—";
            BountyLevel.Text = "";
            BountyCost.Text = "";
            BountyPlace.Text = "";
            BountyDesc.Text = "No bounties available";
        }
    }

    // ── Tab Switching ────────────────────────────────────────
    private void ApplyActiveTab()
    {
        UpdateTabColors();
        NewsList.ItemsSource = GetItemsForCurrentTab();
    }

    private void UpdateTabColors()
    {
        RankTab.BackgroundColor = _activeTab == "rank"
            ? Color.FromArgb("#3498db") : Color.FromArgb("#2c3e50");
        JobsTab.BackgroundColor = _activeTab == "jobs"
            ? Color.FromArgb("#3498db") : Color.FromArgb("#2c3e50");
        AdsTab.BackgroundColor = _activeTab == "ads"
            ? Color.FromArgb("#3498db") : Color.FromArgb("#2c3e50");
        GuardTab.BackgroundColor = _activeTab == "guard"
            ? Color.FromArgb("#3498db") : Color.FromArgb("#2c3e50");
    }

    private void OnRankTabClicked(object sender, EventArgs e) => SetTab("rank");
    private void OnJobsTabClicked(object sender, EventArgs e) => SetTab("jobs");
    private void OnAdsTabClicked(object sender, EventArgs e) => SetTab("ads");
    private void OnGuardTabClicked(object sender, EventArgs e) => SetTab("guard");

    private void SetTab(string tab)
    {
        _activeTab = tab;
        ApplyActiveTab();
    }

    // ── Publish Ad ───────────────────────────────────────────
    private async void OnPublishAdClicked(object sender, EventArgs e)
    {
        var player = AccountService.GetCurrentPlayer();
        if (player == null) return;

        // ✅ Check if banned from news
        if (await BanHelper.CheckAndShowBanAlert(player, "news"))
            return;

        if (player.Gold < 20000)
        {
            await DisplayAlert("أموال غير كافية", "تحتاج 20,000 ذهب للنشر", "موافق");
            return;
        }

        string message = await DisplayPromptAsync(
            "نشر إعلان", "أدخل رسالتك (حد أقصى 50 حرفاً):",
            "نشر", "إلغاء", maxLength: 50, keyboard: Keyboard.Text);

        if (string.IsNullOrWhiteSpace(message)) return;

        var (success, resultMessage) = await NewsService.PublishAdAsync(player, message);
        await DisplayAlert(success ? "تم النشر" : "خطأ", resultMessage, "موافق");

        if (success)
        {
            _playerAds.Insert(0, new NewsItem
            {
                Author = player.Username,
                Content = message,
                Date = DateTime.Now,
                Type = NewsType.PlayerAd,
                Icon = "📢"
            });
            ApplyActiveTab();
            UpdateFeaturedSections();
        }
    }
    // ── Navigation ───────────────────────────────────────────
    private async void OnBackClicked(object sender, EventArgs e) =>
        await Navigation.PopAsync();

    private async void OnRefreshClicked(object sender, EventArgs e)
    {
        await LoadAllData(forceRefresh: true);
        await DisplayAlert("تم", "تم تحديث البيانات", "موافق");
    }
}