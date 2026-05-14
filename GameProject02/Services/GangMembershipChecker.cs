using System;
using System.Threading.Tasks;
using GameProject02.Models;
using GameProject02.Services;
using Microsoft.Maui.Controls;
using System.Diagnostics;

namespace GameProject02.Helpers
{
    public static class GangMembershipChecker
    {
        private static System.Timers.Timer _timer;
        private static string _lastGangId;

        public static void StartPolling(int intervalSeconds = 10)
        {
            if (_timer != null) return;
            _timer = new System.Timers.Timer(intervalSeconds * 1000);
            _timer.Elapsed += async (s, e) => await CheckMembership();
            _timer.Start();
        }

        private static async Task CheckMembership()
        {
            try
            {
                var player = AccountService.CurrentPlayer;
                if (player == null)
                {
                    Debug.WriteLine("[CheckMembership] CurrentPlayer is null, skipping");
                    return;
                }

                if (string.IsNullOrEmpty(player.PlayerId))
                {
                    Debug.WriteLine("[CheckMembership] PlayerId is null or empty");
                    return;
                }

                var freshPlayer = await FirebaseService.LoadPlayerAsync(player.PlayerId);
                if (freshPlayer == null)
                {
                    Debug.WriteLine("[CheckMembership] LoadPlayerAsync returned null");
                    return;
                }

                // Ensure GangId is not null
                string freshGangId = freshPlayer.GangId ?? "";
                string currentGangId = player.GangId ?? "";

                // Reload gang if needed
                if (!string.IsNullOrEmpty(freshGangId) && freshPlayer.GangObject == null)
                {
                    try
                    {
                        freshPlayer.GangObject = await GangDatabaseService.GetGangAsync(freshGangId);
                        if (freshPlayer.GangObject == null)
                        {
                            Debug.WriteLine($"[CheckMembership] Gang not found for Id: {freshGangId}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[CheckMembership] GetGangAsync error: {ex.Message}");
                    }
                }

                // Check if gang status changed
                bool gangChanged = freshGangId != currentGangId;
                if (gangChanged)
                {
                    // Update the CurrentPlayer reference
                    if (freshPlayer != null)
                    {
                        AccountService.CurrentPlayer = freshPlayer;
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            try
                            {
                                // ✅ FIXED: use null instead of 'this' (static context)
                                MessagingCenter.Send<object, string>(new object(), "GangStatusChanged", freshGangId);
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine($"[CheckMembership] MessagingCenter error: {ex.Message}");
                            }
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CheckMembership] Unhandled exception: {ex}");
            }
        }

        public static void StopPolling()
        {
            _timer?.Stop();
            _timer?.Dispose();
            _timer = null;
        }
    }
}