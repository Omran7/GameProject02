using GameProject02.Models;
using GameProject02.Services;
using Microsoft.Maui.Controls;
using System.Timers;

namespace GameProject02.Views;

public partial class NewsPage : ContentPage
{
    private System.Timers.Timer _clockTimer;

    public NewsPage()
    {
        InitializeComponent();
        LoadNews();
        StartClock();
    }

    private void StartClock()
    {
        // Update clock every second like the old project
        _clockTimer = new System.Timers.Timer(1000);
        _clockTimer.Elapsed += (s, e) => MainThread.BeginInvokeOnMainThread(UpdateClockUI);
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

    private void LoadNews()
    {
        var news = NewsService.GetLatestNews();
        NewsList.ItemsSource = news;
    }

    private async void OnPublishAdClicked(object sender, EventArgs e)
    {
        // Check player gold like the old project (20,000 cost)
        var player = AccountService.GetCurrentPlayer();
        if (player == null) return;

        if (player.Gold < 20000)
        {
            await DisplayAlert("Low Funds", "You need 20,000 gold to broadcast a message city-wide.", "OK");
            return;
        }

        string message = await DisplayPromptAsync("City Broadcast",
            "Enter your message (Max 50 characters):",
            "Publish", "Cancel",
            maxLength: 50,
            keyboard: Keyboard.Text);

        if (!string.IsNullOrWhiteSpace(message))
        {
            // Deduct funds
            player.Gold -= 20000;

            // In a real app, this would push to Firebase. 
            // For now, we add it to our local list.
            var ad = new NewsItem
            {
                Author = player.Username ?? "Anonymous",
                Content = message,
                Date = DateTime.Now,
                Type = NewsType.PlayerAd,
                Icon = "👤"
            };

            var currentList = (List<NewsItem>)NewsList.ItemsSource;
            currentList.Insert(0, ad);
            NewsList.ItemsSource = null;
            NewsList.ItemsSource = currentList;

            await DisplayAlert("Success", "Your ad has been broadcast to the city!", "OK");
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _clockTimer?.Stop();
        _clockTimer?.Dispose();
    }
}
