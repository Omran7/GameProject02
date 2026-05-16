using GameProject02.Models;
using GameProject02.Services;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace GameProject02.Views
{
    public partial class PlayerSelectionPage : ContentPage
    {
        private ObservableCollection<PlayerAccount> _allPlayers = new();
        private ObservableCollection<PlayerAccount> _filteredPlayers = new();
        private string _currentUserId;

        public PlayerSelectionPage()
        {
            InitializeComponent();
            PlayersList.ItemsSource = _filteredPlayers;
            _ = LoadPlayers();
        }

        private async Task LoadPlayers()
        {
            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;
            try
            {
                var current = AccountService.GetCurrentPlayer();
                if (current == null) return;
                _currentUserId = current.PlayerId;

                var players = await AccountService.GetAllPlayersAsync();
                _allPlayers.Clear();
                foreach (var p in players)
                {
                    // استبعاد اللاعب الحالي من القائمة
                    if (p.PlayerId != _currentUserId)
                        _allPlayers.Add(p);
                }

                RefreshFilteredList(_allPlayers);
            }
            catch (Exception ex)
            {
                await DisplayAlert("خطأ", $"فشل تحميل اللاعبين: {ex.Message}", "موافق");
            }
            finally
            {
                LoadingIndicator.IsVisible = false;
                LoadingIndicator.IsRunning = false;
            }
        }

        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            var query = e.NewTextValue?.Trim().ToLower() ?? "";
            var filtered = string.IsNullOrEmpty(query)
                ? _allPlayers.ToList()
                : _allPlayers.Where(p => p.Username.ToLower().Contains(query)).ToList();

            RefreshFilteredList(filtered);
        }

        private void RefreshFilteredList(IEnumerable<PlayerAccount> players)
        {
            _filteredPlayers.Clear();
            foreach (var p in players)
                _filteredPlayers.Add(p);
        }

        private async void OnPlayerSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is PlayerAccount selected)
            {
                // إلغاء التحديد لتمكين الضغط مرة أخرى
                ((CollectionView)sender).SelectedItem = null;

                // ✅ الحل هنا: بدلاً من استدعاء GetOrCreateConversationAsync
                // نقوم بتركيب الـ ID محلياً وفتح صفحة المحادثة مباشرة
                string conversationId = PrivateChatService.GetConversationId(_currentUserId, selected.PlayerId);

                // الانتقال لصفحة المحادثة (تأكد من مطابقة اسم الصفحة لديك)
                // إذا كان اسم الصفحة PrivateChatRoomPage قم بتغييره هنا
                await Navigation.PushAsync(new PrivateChatPage(conversationId, selected.PlayerId, selected.Username));
            }
        }
    }
}