using GameProject02.Models;
using System.Collections.Generic;

namespace GameProject02.Services;

public static class CityMapService
{
    public static List<BuildingInfo> GetMainCityBuildings()
    {
        return new List<BuildingInfo>
        {
            // Main buildings from atlas data
            new BuildingInfo {
                Id = "gang_base", Name = "مقر العصابة",
                ImageResource = "building_gang_base",
                X = 1610, Y = -1517, ZIndex = 552,
                Width = 200, Height = 150,
                NavigationTarget = "GangBase",
                IsClickable = true
            },
            new BuildingInfo {
                Id = "gang_market", Name = "متجر العصابات",
                ImageResource = "building_gang_market",
                X = 1010, Y = -1747, ZIndex = 553,
                Width = 200, Height = 150,
                NavigationTarget = "BlackMarket",
                IsClickable = true
            },
            new BuildingInfo {
                Id = "school", Name = "المدرسة",
                ImageResource = "building_school",
                X = 1138, Y = -1028, ZIndex = 554,
                Width = 200, Height = 170,
                NavigationTarget = "School",
                IsClickable = true
            },
            new BuildingInfo {
                Id = "fight_club", Name = "نادي القتال",
                ImageResource = "building_fight_club",
                X = 893, Y = -1150, ZIndex = 555,
                Width = 200, Height = 140,
                NavigationTarget = "FightClub",
                IsClickable = true
            },
            new BuildingInfo {
                Id = "bank", Name = "البنك",
                ImageResource = "building_bank",
                X = 818, Y = -811, ZIndex = 557,
                Width = 170, Height = 150,
                NavigationTarget = "Bank",
                IsClickable = true
            },
            new BuildingInfo {
                Id = "casino", Name = "الكازينو",
                ImageResource = "building_casino",
                X = 122, Y = -766, ZIndex = 560,
                Width = 260, Height = 210,
                NavigationTarget = "Casino",
                IsClickable = true
            },
            new BuildingInfo {
                Id = "hospital", Name = "المستشفى",
                ImageResource = "building_hospital",
                X = 1130, Y = -1266, ZIndex = 1252,
                Width = 200, Height = 165,
                NavigationTarget = "Hospital",
                IsClickable = true
            },
            new BuildingInfo {
                Id = "prison", Name = "السجن",
                ImageResource = "building_prison",
                X = 1629, Y = -582, ZIndex = 688,
                Width = 350, Height = 230,
                NavigationTarget = "Prison",
                IsClickable = true
            },
            new BuildingInfo {
                Id = "estate", Name = "العقارات",
                ImageResource = "building_estate",
                X = 316, Y = -1419, ZIndex = 1245,
                Width = 220, Height = 190,
                NavigationTarget = "Estate",
                IsClickable = true
            },
            new BuildingInfo {
                Id = "work_office", Name = "مكتب العمل",
                ImageResource = "building_work_office",
                X = 1321, Y = -1605, ZIndex = 1256,
                Width = 200, Height = 210,
                NavigationTarget = "WorkOffice",
                IsClickable = true
            },
            new BuildingInfo {
                Id = "mercenary_base", Name = "قاعدة المرتزقة",
                ImageResource = "building_mercenary_base",
                X = 1735, Y = -292, ZIndex = 710,
                Width = 300, Height = 230,
                NavigationTarget = "Crime",
                IsClickable = true
            },
            new BuildingInfo {
                Id = "gym", Name = "صالة التدريب",
                ImageResource = "building_gym",
                X = 1160, Y = -1830, ZIndex = 1035,
                Width = 200, Height = 150,
                NavigationTarget = "Gym",
                IsClickable = true
            },
            new BuildingInfo {
                Id = "airport", Name = "المطار",
                ImageResource = "building_airport",
                X = 1867, Y = -322, ZIndex = 645,
                Width = 200, Height = 150,
                NavigationTarget = "Airport",
                IsClickable = true
            },
            new BuildingInfo {
                Id = "city_market", Name = "سوق المدينة",
                ImageResource = "building_city_market",
                X = 823, Y = -1668, ZIndex = 1033,
                Width = 200, Height = 170,
                NavigationTarget = "Market",
                IsClickable = true
            },
            new BuildingInfo {
                Id = "black_market", Name = "السوق السوداء",
                ImageResource = "building_black market",
                X = 789, Y = -1650, ZIndex = 1034,
                Width = 200, Height = 150,
                NavigationTarget = "BlackMarket",
                IsClickable = true
            },
            new BuildingInfo {
                Id = "city_database", Name = "قاعدة البيانات",
                ImageResource = "building_city_database",
                X = 635, Y = -1255, ZIndex = 1250,
                Width = 200, Height = 170,
                NavigationTarget = "Profile",
                IsClickable = true
            },
            new BuildingInfo {
                Id = "skyscraper", Name = "ناطحة السحاب",
                ImageResource = "building_skyscraper",
                X = 2197, Y = -1496, ZIndex = 1259,
                Width = 240, Height = 320,
                NavigationTarget = "",
                IsClickable = false
            }
        };
    }
}