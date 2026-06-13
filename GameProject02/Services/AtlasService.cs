using GameProject02.Models;
using System.Collections.Generic;

namespace GameProject02.Services;

public static class AtlasService
{
    // Dictionary mapping [Layer][SpriteName] to [X, Y, Width, Height]
    // Width and Height are crucial for correct collision and origin math
    public static readonly Dictionary<string, Dictionary<string, int[]>> Atlas = new()
    {
        ["buildings"] = new() {
            { "building_gang_base", new[] {0, 0, 280, 196} },
            { "building_gang_market", new[] {0, 0, 350, 209} },
            { "building_school", new[] {0, 0, 200, 170} },
            { "building_fight_club", new[] {0, 0, 190, 154} },
            { "building_bank", new[] {0, 0, 165, 144} },
            { "building_hospital", new[] {0, 0, 190, 162} },
            { "building_prison", new[] {0, 0, 350, 233} },
            { "building_estate", new[] {0, 0, 230, 190} },
            { "building_work_office", new[] {0, 0, 190, 206} },
            { "building_mercenary_base", new[] {0, 0, 280, 237} },
            { "building_gym", new[] {0, 0, 185, 159} },
            { "building_airport", new[] {0, 0, 350, 244} },
            { "building_skyscraper", new[] {0, 0, 230, 315} },
            { "building_war_banner", new[] {0, 0, 76, 120} },
            { "building_lucky_wheel", new[] {0, 0, 49, 130} }
        },
        ["floor"] = new() {
            { "floor_rocks_cracks2", new[] {757, 127, 250, 121} },
            { "floor_solid", new[] {757, 4, 250, 121} }
        },
        ["roads"] = new() {
            { "road_big", new[] {1, 44, 160, 78} }
        }
    };

    public static List<MapItem> GetMainScene()
    {
        return new List<MapItem>
    {
        // Coordinates and Names matched to your phone photos
        new MapItem { DisplayName = "كازينو", Id = "Casino", X = 122, Y = -766, Ox = 130, Oy = 112, ImageName = "building_casino", Layer = "buildings", ZIndex = 500, Clickable = true },
        new MapItem { DisplayName = "هايبر ماركت", Id = "Hyper", X = 1000, Y = -1500, Ox = 175, Oy = 120, ImageName = "building_airport", Layer = "buildings", ZIndex = 501, Clickable = true },
        new MapItem { DisplayName = "سجن", Id = "Prison", X = 1629, Y = -582, Ox = 175, Oy = 116.5, ImageName = "building_prison", Layer = "buildings", ZIndex = 502, Clickable = true },
        new MapItem { DisplayName = "مقر العصابة", Id = "Base", X = 1610, Y = -1517, Ox = 140, Oy = 98, ImageName = "building_gang_base", Layer = "buildings", ZIndex = 503, Clickable = true }
    };
    }
}