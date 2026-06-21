using GameProject02.Helpers;
using GameProject02.Models;
using GameProject02.Services;
using Microsoft.Maui.Controls;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace GameProject02.Views
{
    public partial class PrivateChatPage : ContentPage
    {
        private string _conversationId;
        private string _otherPlayerId;
        private string _otherPlayerName;
        private ObservableCollection<PrivateChatMessage> _messages = new();
        private bool _isPolling = true;
        private long _lastTimestampSeen = 0;

        public string OtherPlayerName => _otherPlayerName;
        public ObservableCollection<PrivateChatMessage> Messages => _messages;

        public PrivateChatPage(string conversationId, string otherPlayerId, string otherPlayerName)
        {
            InitializeComponent();
            _conversationId = conversationId;
            _otherPlayerId = otherPlayerId;
            _otherPlayerName = otherPlayerName;

            BindingContext = this;
            MessagesList.ItemsSource = _messages;

            _ = LoadMessagesAsync();
            _ = StartPollingAsync();
        }

        private async Task StartPollingAsync()
        {
            while (_isPolling)
            {
                await Task.Delay(2000);
                await RefreshNewMessagesAsync();
            }
        }

        private async Task LoadMessagesAsync()
        {
            try
            {
                var msgs = await PrivateChatService.GetMessagesAsync(_conversationId);
                _messages.Clear();
                _lastTimestampSeen = 0;
                foreach (var m in msgs)
                {
                    _messages.Add(m);
                    if (m.Timestamp > _lastTimestampSeen)
                        _lastTimestampSeen = m.Timestamp;
                }
                if (_messages.Count > 0)
                    ScrollToBottom();

                // Mark conversation as read
                var player = AccountService.GetCurrentPlayer();
                if (player != null)
                {
                    await PrivateChatService.MarkConversationAsReadAsync(_conversationId, player.PlayerId);
                    // Notify list page to refresh unread counts
                    MessagingCenter.Send(this, "RefreshConversations");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[PrivateChat] Load error: {ex.Message}");
            }
        }
        private async Task RefreshNewMessagesAsync()
        {
            try
            {
                var msgs = await PrivateChatService.GetMessagesAsync(_conversationId);
                var newMessages = msgs.Where(m => m.Timestamp > _lastTimestampSeen).ToList();
                if (newMessages.Any())
                {
                    foreach (var m in newMessages)
                    {
                        _messages.Add(m);
                        if (m.Timestamp > _lastTimestampSeen)
                            _lastTimestampSeen = m.Timestamp;
                    }
                    ScrollToBottom();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[PrivateChat] Refresh error: {ex.Message}");
            }
        }

        private void ScrollToBottom()
        {
            if (_messages.Count == 0) return;
            MainThread.BeginInvokeOnMainThread(() =>
            {
                MessagesList.ScrollTo(_messages.Last(), position: ScrollToPosition.End, animate: false);
            });
        }

        private async void OnSendClicked(object sender, EventArgs e)
        {
            string content = MessageEntry.Text?.Trim();
            if (string.IsNullOrEmpty(content)) return;

            var player = AccountService.GetCurrentPlayer();
            if (player == null) return;

            // ✅ Check if banned from private messages
            if (await BanHelper.CheckAndShowBanAlert(player, "messages"))
            {
                MessageEntry.Text = "";
                return;
            }

            MessageEntry.Text = "";

            long optimisticTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var optimisticMsg = new PrivateChatMessage
            {
                SenderId = player.PlayerId,
                Content = content,
                Timestamp = optimisticTimestamp
            };
            _messages.Add(optimisticMsg);
            ScrollToBottom();
            if (optimisticTimestamp > _lastTimestampSeen)
                _lastTimestampSeen = optimisticTimestamp;

            bool sent = await PrivateChatService.SendPrivateMessageAsync(_conversationId, player.PlayerId, content);
            if (sent)
            {
                MessagingCenter.Send(this, "RefreshConversations");
            }
            else
            {
                _messages.Remove(optimisticMsg);
                _lastTimestampSeen = _messages.Any() ? _messages.Max(m => m.Timestamp) : 0;
                await DisplayAlert("خطأ", "فشل إرسال الرسالة.", "موافق");
            }
        }
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _isPolling = false;
        }
    }
}