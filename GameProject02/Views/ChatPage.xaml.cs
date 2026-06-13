using GameProject02.Models;
using GameProject02.Services;
using Microsoft.Maui.Controls;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace GameProject02.Views
{
    public partial class ChatPage : ContentPage
    {
        public ObservableCollection<ChatMessage> Messages { get; } = new();
        private string _currentChannel = "worldwide";
        private bool _isPolling = true;

        public ChatPage()
        {
            InitializeComponent();
            BindingContext = this;
            UpdateChannelTabs();
            _ = StartPollingAsync();
        }

        private void UpdateChannelTabs()
        {
            var channels = ChatService.GetAvailableChannels();

            // 🔧 FIX: use the correct prefixes for the new channel format
            GangTab.IsVisible = channels.Any(c => c.StartsWith("gang:"));
            PlaceTab.IsVisible = channels.Any(c => c.StartsWith("place:"));
        }

        private async Task StartPollingAsync()
        {
            while (_isPolling)
            {
                await LoadMessagesAsync();
                await Task.Delay(3000);
            }
        }

        private async Task LoadMessagesAsync()
        {
            var messages = await ChatService.GetMessagesAsync(_currentChannel);
            Messages.Clear();
            foreach (var msg in messages)
                Messages.Add(msg);

            if (Messages.Count > 0)
                MessagesList.ScrollTo(Messages.Last(), position: ScrollToPosition.End);
        }

        private async void OnSendClicked(object sender, EventArgs e)
        {
            string content = MessageEntry.Text?.Trim();
            if (string.IsNullOrWhiteSpace(content)) return;

            MessageEntry.Text = "";
            bool sent = await ChatService.SendMessageAsync(_currentChannel, content);
            if (sent)
                await LoadMessagesAsync();
            else
                await DisplayAlert("خطأ", "فشل إرسال الرسالة", "موافق");
        }

        private async void OnWorldwideTabClicked(object sender, EventArgs e)
        {
            _currentChannel = "worldwide";
            UpdateTabColors();
            await LoadMessagesAsync();
        }

        private async void OnGangTabClicked(object sender, EventArgs e)
        {
            var gangChannel = ChatService.GetGangChannel();
            if (gangChannel != null)
            {
                _currentChannel = gangChannel;
                UpdateTabColors();
                await LoadMessagesAsync();
            }
        }

        private async void OnPlaceTabClicked(object sender, EventArgs e)
        {
            var placeChannel = ChatService.GetPlaceChannel();
            if (placeChannel != null)
            {
                _currentChannel = placeChannel;
                UpdateTabColors();
                await LoadMessagesAsync();
            }
        }

        private void UpdateTabColors()
        {
            WorldwideTab.BackgroundColor = _currentChannel == "worldwide"
                ? Color.FromArgb("#3498db") : Color.FromArgb("#2c3e50");

            GangTab.BackgroundColor = _currentChannel.StartsWith("gang:")
                ? Color.FromArgb("#3498db") : Color.FromArgb("#2c3e50");

            PlaceTab.BackgroundColor = _currentChannel.StartsWith("place:")
                ? Color.FromArgb("#3498db") : Color.FromArgb("#2c3e50");
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _isPolling = false;
        }
    }
}