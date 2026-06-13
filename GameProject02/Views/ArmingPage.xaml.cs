using GameProject02.Models;
using GameProject02.Services;
using Microsoft.Maui.Controls;
using System;
using System.Threading.Tasks;

namespace GameProject02.Views;

public partial class ArmingPage : ContentPage
{
    private PlayerAccount _player;

    public ArmingPage()
    {
        InitializeComponent();
        LoadPlayerData();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadPlayerData();
    }

    private void LoadPlayerData()
    {
        _player = AccountService.GetCurrentPlayer();
        if (_player == null) return;

        // Initialize ArmingObject if null (first time)
        _player.ArmingObject ??= new ArmingObject();

        // ✅ UPDATE WEAPON DISPLAY (GET FROM STOCK - NOT MARKET)
        if (!string.IsNullOrEmpty(_player.ArmingObject.WeaponId))
        {
            // ✅ CRITICAL FIX: Get weapon details FROM STOCK ITEM (not market service)
            if (_player.StockObject.ItemsInStock.TryGetValue(_player.ArmingObject.WeaponId, out var stockItem) && stockItem != null)
            {
                WeaponNameLabel.Text = stockItem.Name;
                WeaponStatsLabel.Text = $"الضرر: {stockItem.Damage} | الدقة: {stockItem.Accuracy}";
                RemoveWeaponButton.IsEnabled = true;
            }
            else
            {
                // Fallback (should never happen for equipped items)
                WeaponNameLabel.Text = "غير معروف";
                WeaponStatsLabel.Text = "الضرر: 0 | الدقة: 0";
                RemoveWeaponButton.IsEnabled = false;
            }
        }
        else
        {
            WeaponNameLabel.Text = "غير مجهز";
            WeaponStatsLabel.Text = "الضرر: 0 | الدقة: 0";
            RemoveWeaponButton.IsEnabled = false;
        }

        // ✅ UPDATE ARMOR DISPLAY (GET FROM STOCK - NOT MARKET)
        if (!string.IsNullOrEmpty(_player.ArmingObject.ArmorId))
        {
            if (_player.StockObject.ItemsInStock.TryGetValue(_player.ArmingObject.ArmorId, out var stockItem) && stockItem != null)
            {
                ArmorNameLabel.Text = stockItem.Name;
                ArmorStatsLabel.Text = $"الدفاع: {stockItem.Defense} | التهرب: {stockItem.Evasion}";
                RemoveArmorButton.IsEnabled = true;
            }
            else
            {
                ArmorNameLabel.Text = "غير معروف";
                ArmorStatsLabel.Text = "الدفاع: 0 | التهرب: 0";
                RemoveArmorButton.IsEnabled = false;
            }
        }
        else
        {
            ArmorNameLabel.Text = "غير مجهز";
            ArmorStatsLabel.Text = "الدفاع: 0 | التهرب: 0";
            RemoveArmorButton.IsEnabled = false;
        }

        // ✅ UPDATE SPECIAL EQUIPMENT DISPLAY
        if (!string.IsNullOrEmpty(_player.ArmingObject.SpecialEquipmentId))
        {
            if (_player.StockObject.ItemsInStock.TryGetValue(_player.ArmingObject.SpecialEquipmentId, out var stockItem) && stockItem != null)
            {
                SpecialNameLabel.Text = stockItem.Name;
            }
            else
            {
                SpecialNameLabel.Text = "غير معروف";
            }
            RemoveSpecialButton.IsEnabled = true;
        }
        else
        {
            SpecialNameLabel.Text = "غير مجهز";
            RemoveSpecialButton.IsEnabled = false;
        }

        // ✅ UPDATE BIOCHEMICAL DISPLAY
        if (!string.IsNullOrEmpty(_player.ArmingObject.BioChemicalId))
        {
            if (_player.StockObject.ItemsInStock.TryGetValue(_player.ArmingObject.BioChemicalId, out var stockItem) && stockItem != null)
            {
                BiochemicalNameLabel.Text = stockItem.Name;
            }
            else
            {
                BiochemicalNameLabel.Text = "غير معروف";
            }
            RemoveBiochemicalButton.IsEnabled = true;
        }
        else
        {
            BiochemicalNameLabel.Text = "غير مجهز";
            RemoveBiochemicalButton.IsEnabled = false;
        }
    }
    // ✅ CORRECTED: Navigate to STOCK SELECTION (not market)
    private async void OnSelectWeaponClicked(object sender, EventArgs e) =>
        await Navigation.PushAsync(new ArmingStockSelectionPage("weapon", 0));

    private async void OnSelectArmorClicked(object sender, EventArgs e) =>
        await Navigation.PushAsync(new ArmingStockSelectionPage("armor", 1));

    private async void OnSelectSpecialClicked(object sender, EventArgs e) =>
        await Navigation.PushAsync(new ArmingStockSelectionPage("special", 3));

    private async void OnSelectBiochemicalClicked(object sender, EventArgs e) =>
        await Navigation.PushAsync(new ArmingStockSelectionPage("biochemical", 4));

    private async void OnRemoveWeaponClicked(object sender, EventArgs e) => await RemoveItem(_player.ArmingObject.WeaponId, "السلاح");
    private async void OnRemoveArmorClicked(object sender, EventArgs e) => await RemoveItem(_player.ArmingObject.ArmorId, "الدرع");
    private async void OnRemoveSpecialClicked(object sender, EventArgs e) => await RemoveItem(_player.ArmingObject.SpecialEquipmentId, "المعدات الخاصة");
    private async void OnRemoveBiochemicalClicked(object sender, EventArgs e) => await RemoveItem(_player.ArmingObject.BioChemicalId, "الكيمياء الحيوية");

    private async Task RemoveItem(string itemId, string itemType)
    {
        if (string.IsNullOrEmpty(itemId)) return;

        bool confirm = await DisplayAlert("إزالة التجهيز", $"هل أنت متأكد من إزالة {itemType}؟", "نعم", "إلغاء");
        if (!confirm) return;

        var result = ArmingService.UnequipItem(_player, itemId);
        await DisplayAlert(result.success ? "✅ نجاح" : "❌ فشل", result.message, "موافق");
        if (result.success) LoadPlayerData();
    }

    private async void OnBackClicked(object sender, EventArgs e) => await Navigation.PopAsync();
    private async void OnHomeClicked(object sender, EventArgs e) => await Navigation.PopToRootAsync();
    private async void OnRefreshClicked(object sender, EventArgs e) => LoadPlayerData();
}