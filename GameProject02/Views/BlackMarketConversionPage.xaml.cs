using GameProject02.Models;
using GameProject02.Services;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GameProject02.Views;

public partial class BlackMarketConversionPage : ContentPage
{
    private readonly string _category;
    private PlayerAccount _player;
    private BlackMarketRecipe _selectedRecipe;

    public BlackMarketConversionPage(string category)
    {
        InitializeComponent();
        _category = category;
        CategoryTitle.Text = _category switch
        {
            "Diamonds" => "💎 تحويل الألماس",
            "Checks" => "💳 تحويل الشيكات",
            "Tools" => "🛠️ تحويل الأدوات",
            "Food" => "🍽️ تحويل الطعام",
            _ => "التحويل"
        };
        LoadData();
    }

    private void LoadData()
    {
        _player = AccountService.GetCurrentPlayer();
        RecipesList.ItemsSource = BlackMarketService.GetRecipesByCategory(_category);
    }

    private void OnRecipeSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is BlackMarketRecipe recipe)
        {
            _selectedRecipe = recipe;
            ConvertPanel.IsVisible = true;
            SelectedRecipeName.Text = recipe.Name;
            QuantitySlider.Minimum = recipe.MinQuantity;
            QuantitySlider.Maximum = recipe.MaxQuantity;
            QuantitySlider.Value = recipe.MinQuantity;
            QuantityEntry.Text = recipe.MinQuantity.ToString();
            UpdateStatus();
            RecipesList.SelectedItem = null;
        }
    }

    private void OnQuantityChanged(object sender, ValueChangedEventArgs e)
    {
        if (QuantityEntry.Text != ((int)e.NewValue).ToString())
            QuantityEntry.Text = ((int)e.NewValue).ToString();
        UpdateStatus();
    }

    private void OnQuantityTextChanged(object sender, TextChangedEventArgs e)
    {
        if (_selectedRecipe == null) return;
        if (int.TryParse(e.NewTextValue, out int qty))
        {
            qty = Math.Max(_selectedRecipe.MinQuantity, Math.Min(_selectedRecipe.MaxQuantity, qty));
            if (QuantitySlider.Value != qty) QuantitySlider.Value = qty;
            UpdateStatus();
        }
    }

    private void UpdateStatus()
    {
        if (_selectedRecipe == null || _player == null) return;
        int qty = (int)QuantitySlider.Value;
        var (canConvert, msg) = BlackMarketService.CanConvert(_player, _selectedRecipe, qty);
        ConvertButton.IsEnabled = canConvert;
        StatusLabel.Text = canConvert ? "✅ جاهز" : $"❌ {msg}";
    }

    private async void OnConvertClicked(object sender, EventArgs e)
    {
        if (_selectedRecipe == null) return;
        int qty = (int)QuantitySlider.Value;
        ConvertButton.IsEnabled = false; ConvertButton.Text = "جاري...";

        var (success, message) = BlackMarketService.ExecuteConversion(_player, _selectedRecipe, qty);
        ConvertButton.Text = "تحويل"; ConvertButton.IsEnabled = true;

        await DisplayAlert(success ? "✅ نجاح" : "❌ فشل", message, "موافق");
        if (success) UpdateStatus();
    }

    private async void OnBackClicked(object sender, EventArgs e) => await Navigation.PopAsync();
    private async void OnHomeClicked(object sender, EventArgs e) => await Navigation.PopToRootAsync();
}