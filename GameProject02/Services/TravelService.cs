using GameProject02.Models;
using System;
using System.Collections.Generic;

namespace GameProject02.Services
{
    public static class TravelService
    {
        // Destination map – copied from the old game's switch/case logic
        private static readonly Dictionary<int, List<AirportDestination>> _destinations = new()
        {
            { 0, new List<AirportDestination> {  // main city
                new() { CityId = 2, CityName = "دبي", TravelCostGold = 300, TravelTimeSeconds = 675 },
                new() { CityId = 1, CityName = "غزة", TravelCostGold = 100, TravelTimeSeconds = 225 },
                new() { CityId = 3, CityName = "الرياض", TravelCostGold = 400, TravelTimeSeconds = 900 },
                new() { CityId = 4, CityName = "صنعاء", TravelCostGold = 500, TravelTimeSeconds = 1125 },
                new() { CityId = 5, CityName = "القاهرة", TravelCostGold = 1000, TravelTimeSeconds = 2250 },
            }},
            { 1, new List<AirportDestination> {
                new() { CityId = 0, CityName = "المدينة الرئيسية", TravelCostGold = 100, TravelTimeSeconds = 225 },
                new() { CityId = 4, CityName = "صنعاء", TravelCostGold = 600, TravelTimeSeconds = 1350 },
                new() { CityId = 2, CityName = "دبي", TravelCostGold = 400, TravelTimeSeconds = 900 },
                new() { CityId = 3, CityName = "الرياض", TravelCostGold = 400, TravelTimeSeconds = 900 },
            }},
            { 2, new List<AirportDestination> {
                new() { CityId = 6, CityName = "الدوحة", TravelCostGold = 400, TravelTimeSeconds = 900 },
                new() { CityId = 1, CityName = "غزة", TravelCostGold = 400, TravelTimeSeconds = 900 },
                new() { CityId = 0, CityName = "المدينة الرئيسية", TravelCostGold = 300, TravelTimeSeconds = 675 },
                new() { CityId = 7, CityName = "طهران", TravelCostGold = 800, TravelTimeSeconds = 1800 },
                new() { CityId = 8, CityName = "إدلب", TravelCostGold = 1000, TravelTimeSeconds = 2250 },
            }},
            { 3, new List<AirportDestination> {
                new() { CityId = 0, CityName = "المدينة الرئيسية", TravelCostGold = 400, TravelTimeSeconds = 900 },
                new() { CityId = 4, CityName = "صنعاء", TravelCostGold = 800, TravelTimeSeconds = 1800 },
                new() { CityId = 1, CityName = "غزة", TravelCostGold = 400, TravelTimeSeconds = 900 },
                new() { CityId = 7, CityName = "طهران", TravelCostGold = 800, TravelTimeSeconds = 1800 },
            }},
            { 4, new List<AirportDestination> {
                new() { CityId = 1, CityName = "غزة", TravelCostGold = 600, TravelTimeSeconds = 1350 },
                new() { CityId = 3, CityName = "الرياض", TravelCostGold = 800, TravelTimeSeconds = 1800 },
                new() { CityId = 0, CityName = "المدينة الرئيسية", TravelCostGold = 500, TravelTimeSeconds = 1125 },
            }},
            { 5, new List<AirportDestination> {
                new() { CityId = 9, CityName = "طرابلس", TravelCostGold = 200, TravelTimeSeconds = 450 },
                new() { CityId = 10, CityName = "الرباط", TravelCostGold = 600, TravelTimeSeconds = 1350 },
                new() { CityId = 8, CityName = "إدلب", TravelCostGold = 100, TravelTimeSeconds = 225 },
                new() { CityId = 0, CityName = "المدينة الرئيسية", TravelCostGold = 1000, TravelTimeSeconds = 2250 },
            }},
            { 6, new List<AirportDestination> {
                new() { CityId = 7, CityName = "طهران", TravelCostGold = 700, TravelTimeSeconds = 1575 },
                new() { CityId = 10, CityName = "الرباط", TravelCostGold = 700, TravelTimeSeconds = 1575 },
                new() { CityId = 2, CityName = "دبي", TravelCostGold = 400, TravelTimeSeconds = 900 },
            }},
            { 7, new List<AirportDestination> {
                new() { CityId = 6, CityName = "الدوحة", TravelCostGold = 700, TravelTimeSeconds = 1575 },
                new() { CityId = 9, CityName = "طرابلس", TravelCostGold = 300, TravelTimeSeconds = 675 },
                new() { CityId = 2, CityName = "دبي", TravelCostGold = 800, TravelTimeSeconds = 1800 },
                new() { CityId = 3, CityName = "الرياض", TravelCostGold = 800, TravelTimeSeconds = 1800 },
            }},
            { 8, new List<AirportDestination> {
                new() { CityId = 2, CityName = "دبي", TravelCostGold = 1000, TravelTimeSeconds = 2250 },
                new() { CityId = 5, CityName = "القاهرة", TravelCostGold = 100, TravelTimeSeconds = 225 },
                new() { CityId = 9, CityName = "طرابلس", TravelCostGold = 200, TravelTimeSeconds = 450 },
                new() { CityId = 10, CityName = "الرباط", TravelCostGold = 300, TravelTimeSeconds = 675 },
            }},
            { 9, new List<AirportDestination> {
                new() { CityId = 7, CityName = "طهران", TravelCostGold = 300, TravelTimeSeconds = 675 },
                new() { CityId = 8, CityName = "إدلب", TravelCostGold = 200, TravelTimeSeconds = 450 },
                new() { CityId = 5, CityName = "القاهرة", TravelCostGold = 200, TravelTimeSeconds = 450 },
                new() { CityId = 10, CityName = "الرباط", TravelCostGold = 200, TravelTimeSeconds = 450 },
            }},
            { 10, new List<AirportDestination> {
                new() { CityId = 8, CityName = "إدلب", TravelCostGold = 300, TravelTimeSeconds = 675 },
                new() { CityId = 5, CityName = "القاهرة", TravelCostGold = 600, TravelTimeSeconds = 1350 },
                new() { CityId = 9, CityName = "طرابلس", TravelCostGold = 200, TravelTimeSeconds = 450 },
                new() { CityId = 6, CityName = "الدوحة", TravelCostGold = 700, TravelTimeSeconds = 1575 },
            }}
        };

        public static List<AirportDestination> GetDestinationsForCurrentCity()
        {
            var player = AccountService.GetCurrentPlayer();
            if (player == null) return new List<AirportDestination>();

            // Convert city string to int (temporary mapping – adjust based on your actual city property)
            int currentCityId = player.City switch
            {
                "مدينة العصابات" => 0,
                "Gaza" => 1,
                "Dubai" => 2,
                "Riyadh" => 3,
                "Sanaa" => 4,
                "Cairo" => 5,
                "Doha" => 6,
                "Tehran" => 7,
                "Idlib" => 8,
                "Tripoli" => 9,
                "Rabat" => 10,
                _ => 0
            };

            return _destinations.TryGetValue(currentCityId, out var list)
                ? list : new List<AirportDestination>();
        }

        public static (bool success, string message) Travel(PlayerAccount player, AirportDestination dest)
        {
            if (player.Gold < dest.TravelCostGold)
                return (false, $"تحتاج {dest.TravelCostGold} ذهب للسفر.");

            if (player.CrimeObject.IsInPrison || player.CrimeObject.IsInHospital)
                return (false, "لا يمكنك السفر الآن.");

            player.Gold -= dest.TravelCostGold;
            player.City = dest.CityName;   // update city
            player.Flights++;              // track flights

            // Save player
            _ = FirebaseService.SavePlayerAsync(player);

            // Notification
            NotificationService.AddGameNotification(
                "✈️ سفر",
                $"سافرت إلى {dest.CityName}.\nتكلفة الرحلة: {dest.TravelCostGold} ذهب",
                GameNotificationPriority.Normal, "✈️", "MainPage"
            );

            return (true, $"سافرت إلى {dest.CityName}!");
        }

        public static (bool success, string message) StartTravel(PlayerAccount player, AirportDestination dest)
        {
            if (player.Gold < dest.TravelCostGold)
                return (false, $"تحتاج {dest.TravelCostGold} ذهب للسفر.");
            if (player.CrimeObject.IsInPrison || player.CrimeObject.IsInHospital || player.CrimeObject.IsInPlane)
                return (false, "لا يمكنك السفر الآن.");

            player.Gold -= dest.TravelCostGold;
            player.Flights++;

            // Enter plane state
            player.CrimeObject.IsInPlane = true;
            player.CrimeObject.FlightReleaseTime =
                DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + dest.TravelTimeSeconds * 1000L;
            player.City = dest.CityName;   // destination city (will be active after landing)

            _ = FirebaseService.SavePlayerAsync(player);

            NotificationService.AddGameNotification(
                "✈️ سفر",
                $"أنت في طريقك إلى {dest.CityName}",
                GameNotificationPriority.Normal, "✈️", "PlanePage"
            );

            return (true, "استمتع برحلتك!");
        }
    }
}