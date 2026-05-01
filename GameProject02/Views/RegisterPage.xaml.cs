using GameProject02.Services;
using Microsoft.Maui.Controls;

namespace GameProject02.Views;

public partial class RegisterPage : ContentPage
{
    public RegisterPage()
    {
        InitializeComponent();
    }

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        var username = UsernameEntry.Text?.Trim();
        var password = PasswordEntry.Text?.Trim();
        var confirmPassword = ConfirmPasswordEntry.Text?.Trim();

        // Validate inputs
        if (string.IsNullOrWhiteSpace(username) || username.Length < 3 || username.Length > 15)
        {
            ShowError("Username must be 3-15 characters");
            return;
        }

        if (!System.Text.RegularExpressions.Regex.IsMatch(username, "^[a-zA-Z0-9_]*$"))
        {
            ShowError("Username can only contain letters, numbers, and underscores");
            return;
        }

        if (string.IsNullOrWhiteSpace(password) || password.Length < 4 || password.Length > 20)
        {
            ShowError("Password must be 4-20 characters");
            return;
        }

        if (password != confirmPassword)
        {
            ShowError("Passwords don't match");
            return;
        }

        // Create account
        if (AccountService.RegisterAccount(username, password))
        {
            var player = AccountService.GetCurrentPlayer();
            await DisplayAlert("Success!", $"Account created successfully!\n\nYour Player ID: {player.PlayerId}\nYour Username: {player.Username}", "OK");

            // Navigate back to login page
            await Navigation.PopAsync();
        }
        else
        {
            ShowError("Username already exists. Please choose another one.");
        }
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        // Navigate back to login page
        await Navigation.PopAsync();
    }

    private void ShowError(string message)
    {
        ErrorMessage.Text = message;
        ErrorMessage.IsVisible = true;
        ErrorMessage.Opacity = 0;
        ErrorMessage.FadeTo(1, 700, Easing.CubicInOut);

        // Auto-hide after 5 seconds
        _ = Task.Delay(5000).ContinueWith(_ =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (ErrorMessage.IsVisible)
                {
                    ErrorMessage.FadeTo(0, 700, Easing.CubicInOut)
                        .ContinueWith(_ => ErrorMessage.IsVisible = false);
                }
            });
        });
    }
}