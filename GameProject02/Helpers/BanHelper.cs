using GameProject02.Models;
using GameProject02.Services;
using Microsoft.Maui.Controls;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace GameProject02.Helpers;

public static class BanHelper
{
    public static async Task<bool> CheckAndShowBanAlert(PlayerAccount player, string featureName)
    {
        if (player == null) return true;

        bool isBanned = featureName switch
        {
            "chat" => player.IsBannedFromChat,
            "profile" => player.IsBannedFromChangeProfilePic,
            "news" => player.IsBannedFromNews,
            "messages" => player.IsBannedFromPrivateMessages,
            _ => false
        };

        if (isBanned)
        {
            string message = featureName switch
            {
                "chat" => "⛔ تم حظرك من الدردشة من قبل الإدارة.\nلا يمكنك إرسال رسائل في الدردشة العامة.",
                "profile" => "⛔ تم حظرك من تغيير الصورة الشخصية من قبل الإدارة.",
                "news" => "⛔ تم حظرك من نشر الإعلانات والأخبار من قبل الإدارة.",
                "messages" => "⛔ تم حظرك من الرسائل الخاصة من قبل الإدارة.",
                _ => "⛔ تم حظرك من هذه الميزة من قبل الإدارة."
            };
            await ShowAlert(message, "🚫 ممنوع");
            return true;
        }
        return false;
    }

    public static async Task ShowBansOnLogin(PlayerAccount player)
    {
        // ✅ Only show if logged in and player matches
        if (!AccountService.IsLoggedIn() || AccountService.CurrentPlayer?.PlayerId != player?.PlayerId)
            return;

        // ✅ Allow UI to settle (page transition completes)
        await Task.Delay(300);

        var activeBans = new List<string>();
        if (player.IsBannedFromChat) activeBans.Add("• الدردشة العامة");
        if (player.IsBannedFromChangeProfilePic) activeBans.Add("• تغيير الصورة الشخصية");
        if (player.IsBannedFromNews) activeBans.Add("• نشر الإعلانات والأخبار");
        if (player.IsBannedFromPrivateMessages) activeBans.Add("• الرسائل الخاصة");

        if (activeBans.Count > 0)
        {
            string message = "⚠️ تم حظر حسابك من الميزات التالية:\n\n" +
                             string.Join("\n", activeBans) +
                             "\n\nيمكنك استخدام باقي ميزات اللعبة بشكل طبيعي.";
            await ShowAlert(message, "🚫 تنبيه حظر");
        }
    }

    // Helper to show alert on the main thread
    private static async Task ShowAlert(string message, string title)
    {
        try
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                var page = Application.Current?.MainPage;
                if (page == null) return;
                await page.DisplayAlert(title, message, "موافق");
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[BAN] ShowAlert error: {ex.Message}");
        }
    }
}