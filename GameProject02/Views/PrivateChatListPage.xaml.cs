using GameProject02.Models;
using GameProject02.Services;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace GameProject02.Views
{
    public partial class PrivateChatListPage : ContentPage
    {
        public ObservableCollection<ConversationModel> Conversations { get; set; } = new();

        public PrivateChatListPage()
        {
            InitializeComponent();
            ConversationsListView.ItemsSource = Conversations;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadConversationsAsync();
            MessagingCenter.Subscribe<PrivateChatPage>(this, "RefreshConversations", async (sender) =>
            {
                await LoadConversationsAsync();
            });
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            MessagingCenter.Unsubscribe<PrivateChatPage>(this, "RefreshConversations");
        }

        private async Task LoadConversationsAsync()
        {
            var player = AccountService.GetCurrentPlayer();
            if (player == null) return;

            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;

            try
            {
                var conversations = await PrivateChatService.GetMyConversationsAsync(player.PlayerId);
                Conversations.Clear();
                foreach (var conv in conversations)
                {
                    // Get other player's name if missing
                    if (string.IsNullOrEmpty(conv.OtherUserName))
                    {
                        var otherPlayer = await AccountService.GetPlayerByIdAsync(conv.OtherUserId);
                        conv.OtherUserName = otherPlayer?.Username ?? "لاعب غير معروف";
                    }
                    Conversations.Add(conv);
                }
            }
            catch (System.Exception ex)
            {
                await DisplayAlert("خطأ", $"فشل تحميل المحادثات: {ex.Message}", "موافق");
            }
            finally
            {
                LoadingIndicator.IsVisible = false;
                LoadingIndicator.IsRunning = false;
            }
        }

        private async void OnConversationSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is ConversationModel conv)
            {
                ((CollectionView)sender).SelectedItem = null;
                await Navigation.PushAsync(new PrivateChatPage(conv.ConversationId, conv.OtherUserId, conv.OtherUserName));
            }
        }

        private async void OnNewChatClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new PlayerSelectionPage());
        }
    }
}