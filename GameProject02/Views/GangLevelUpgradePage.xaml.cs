using GameProject02.Models;
using GameProject02.Services;
using Microsoft.Maui.Controls;

namespace GameProject02.Views;

public partial class GangLevelUpgradePage : ContentPage
{
    private PlayerAccount _player;
    private GangObject _gang;

    public GangLevelUpgradePage()
    {
        InitializeComponent();
        _player = AccountService.GetCurrentPlayer();
        _gang = _player?.GangObject;
        if (_gang == null) Navigation.PopAsync(false);
        else LoadData();
    }

    private void LoadData()
    {
        CurrentLevelLabel.Text = $"المستوى الحالي: {_gang.Level}";
        NextLevelLabel.Text = $"المستوى التالي: {_gang.Level + 1}";
        int requiredMembers = _gang.Level * 5;
        long requiredRespect = _gang.Level * 1000;
        long requiredCash = _gang.Level * 5000;
        MembersReqLabel.Text = $"الأعضاء المطلوبين: {requiredMembers} (لديك {_gang.MembersWithPositions.Count})";
        RespectReqLabel.Text = $"الاحترام المطلوب: {requiredRespect} (لديك {_gang.AvailableRespect})";
        CashReqLabel.Text = $"النقد المطلوب: {requiredCash} (لديك {_gang.GangCash})";
    }

    private async void OnUpgradeClicked(object sender, EventArgs e)
    {
        var result = GangService.UpgradeGangLevel(_gang, _player);
        if (result.success)
        {
            await DisplayAlert("✅ نجاح", result.message, "موافق");
            await Navigation.PopAsync(false);
        }
        else
        {
            StatusLabel.Text = $"❌ {result.message}";
        }
    }
}