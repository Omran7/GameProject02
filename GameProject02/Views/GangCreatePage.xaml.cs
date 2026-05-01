using GameProject02.Models;
using GameProject02.Services;
using Microsoft.Maui.Controls;
using System;

namespace GameProject02.Views;

public partial class GangCreatePage : ContentPage
{
    private PlayerAccount _player;

    public GangCreatePage()
    {
        InitializeComponent();
        _player = AccountService.GetCurrentPlayer();
    }

    private void OnTagTextChanged(object sender, TextChangedEventArgs e)
    {
        // Auto-format to uppercase letters only
        TagEntry.Text = new string(e.NewTextValue.ToUpper().Where(char.IsLetter).ToArray());
    }

    private async void OnCreateClicked(object sender, EventArgs e)
    {
        string name = NameEntry.Text?.Trim() ?? "";
        string tag = TagEntry.Text?.Trim() ?? "";

        // ✅ AUTHENTIC OLD GAME VALIDATION
        var (isValid, message) = GangService.ValidateGangCreation(_player, name, tag);
        if (!isValid)
        {
            StatusLabel.Text = $"❌ {message}";
            return;
        }

        CreateButton.IsEnabled = false;
        CreateButton.Text = "جاري الإنشاء...";

        try
        {
            // ✅ CREATE GANG & ASSIGN TO PLAYER
            var gang = GangService.CreateGang(_player, name, tag);

            StatusLabel.Text = "✅ تم إنشاء العصابة بنجاح!";
            await Task.Delay(800);

            // Navigate to gang profile
            await Navigation.PushAsync(new GangProfilePage());
        }
        catch (Exception ex)
        {
            StatusLabel.Text = $"❌ خطأ: {ex.Message}";
            CreateButton.IsEnabled = true;
            CreateButton.Text = "🏴󠁥󠁮󠁧󠁢󠁳󠁣󠁿 إنشاء العصابة";
        }
    }

    private async void OnBackClicked(object sender, EventArgs e) => await Navigation.PopAsync();
}