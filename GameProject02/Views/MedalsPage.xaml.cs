using GameProject02.Models;
using GameProject02.Services;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.Linq;

namespace GameProject02.Views;

public partial class MedalsPage : ContentPage
{
    private PlayerAccount _player;
    public ObservableCollection<MedalViewModel> Medals { get; set; } = new();

    public MedalsPage()
    {
        InitializeComponent();
        BindingContext = this;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _player = AccountService.GetCurrentPlayer();
        LoadMedals();
    }

    private void LoadMedals()
    {
        Medals.Clear();

        // Ensure MedalDatabase.AllMedals is not empty
        var allMedals = MedalDatabase.AllMedals;
        System.Diagnostics.Debug.WriteLine($"Total medals in DB: {allMedals.Count}");

        foreach (var medal in allMedals)
        {
            bool isEarned = _player.EarnedMedalIds?.Contains(medal.Id) ?? false;
            Medals.Add(new MedalViewModel
            {
                Id = medal.Id,
                Name = medal.Name,
                Description = medal.Description,
                MeritsReward = medal.MeritsReward,
                IsEarned = isEarned,
                ImageSource = isEarned ? "medal_earned.png" : "medal_locked.png"
            });
        }

        System.Diagnostics.Debug.WriteLine($"Medals loaded: {Medals.Count}");
    }
}

public class MedalViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public int MeritsReward { get; set; }
    public bool IsEarned { get; set; }
    public string ImageSource { get; set; } = "medal_locked.png";
}