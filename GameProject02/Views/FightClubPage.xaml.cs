using GameProject02.Models;
using GameProject02.Services;
using Microsoft.Maui.Controls;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace GameProject02.Views;

public partial class FightClubPage : ContentPage, INotifyPropertyChanged
{
    private PlayerAccount _player;
    private ObservableCollection<FightClubPlayer> _players;

    public ObservableCollection<FightClubPlayer> Players
    {
        get => _players;
        set { _players = value; OnPropertyChanged(); }
    }

    public FightClubPage()
    {
        InitializeComponent();
        BindingContext = this;
        LoadPlayers();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadPlayers();
    }

    private async void LoadPlayers()
    {
        _player = AccountService.GetCurrentPlayer();
        if (_player == null) return;

        _player.CrimeObject.CheckConfinementStatus();
        if (_player.CrimeObject.IsInPrison || _player.CrimeObject.IsInHospital)
        {
            await DisplayAlert("غير مسموح", "لا يمكنك الوصول إلى نادي القتال أثناء وجودك في السجن أو المستشفى!", "موافق");
            await Navigation.PopAsync(false);
            return;
        }

        var opponents = await FightClubService.GetEligibleOpponentsAsync(_player);
        Players = new ObservableCollection<FightClubPlayer>(opponents);
    }

    private async void OnFightClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is FightClubPlayer opponent)
        {
            await btn.ScaleTo(0.92, 100, Easing.CubicOut);
            await Task.Delay(100);
            await btn.ScaleTo(1.0, 100, Easing.CubicIn);

            // Check if player is in prison/hospital
            _player.CrimeObject.CheckConfinementStatus();
            if (_player.CrimeObject.IsInPrison || _player.CrimeObject.IsInHospital)
            {
                await DisplayAlert("⚠️ غير مسموح", "لا يمكنك القتال أثناء وجودك في السجن أو المستشفى!", "موافق");
                return;
            }

            // Navigate to fight screen
            await Navigation.PushAsync(new FightPage(_player, opponent), false);
        }
    }

    private async void OnBackClicked(object sender, EventArgs e) => await Navigation.PopAsync(false);
    private async void OnHomeClicked(object sender, EventArgs e) => await Navigation.PopToRootAsync(false);
    private async void OnRefreshClicked(object sender, EventArgs e)
    {
        LoadPlayers();
        await DisplayAlert("تحديث", "تم تحديث قائمة اللاعبين", "موافق");
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
