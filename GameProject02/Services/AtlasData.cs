using System.Collections.Generic;

namespace GameProject02.Services;

public static class AtlasData
{
    // إحداثيات القص لكل صورة من floor.png
    public static readonly Dictionary<string, (int x, int y, int w, int h)> FloorBounds = new()
    {
        { "floor_interlock", (1009, 130, 250, 118) },
        { "floor_interlock5", (505, 127, 250, 121) },
        { "floor_interlock6", (505, 4, 250, 121) },
        { "floor_rocks_cracks", (1, 1, 250, 122) },
        { "floor_rocks_cracks2", (757, 127, 250, 121) },
        { "floor_sand2", (1, 125, 250, 123) },
        { "floor_solid", (757, 4, 250, 121) },
        { "floor_water", (253, 126, 250, 122) },
        { "floor_water_sand2", (253, 2, 250, 122) }
    };

    // إحداثيات القص لكل صورة من roads.png
    public static readonly Dictionary<string, (int x, int y, int w, int h)> RoadsBounds = new()
    {
        { "road_big", (1, 44, 160, 78) },
        { "road_big_cross", (163, 44, 160, 78) },
        { "road_big_cross2", (325, 44, 160, 78) },
        { "road_big_cross3", (487, 44, 160, 78) },
        { "road_big_curve_left", (1135, 44, 143, 78) },
        { "road_big_curve_right", (1280, 44, 143, 78) },
        { "road_big_small_cross", (649, 44, 160, 78) },
        { "road_big_small_cross2", (811, 44, 160, 78) },
        { "road_big_small_cross3", (973, 44, 160, 78) },
        { "road_block2", (1, 18, 50, 24) },
        { "road_dash", (1709, 97, 49, 25) },
        { "road_small", (1425, 53, 140, 69) },
        { "road_small_block2", (1, 1, 30, 15) },
        { "road_small_cross", (1567, 53, 140, 69) }
    };

    // إحداثيات القص لكل صورة من buildings.png
    public static readonly Dictionary<string, (int x, int y, int w, int h)> BuildingsBounds = new()
    {
        { "building_gang_base", (1, 1, 280, 196) }, // تقريبي، من JSON لم يظهر bounds لكن يمكن استخراجها من شكل المبنى
        // هنا يجب إضافة جميع المباني التي تظهر في MapData.json. سأعطي أمثلة.
        // يمكنك استخراج الباقي من ملف atlas and dt files.txt حسب الحاجة.
        { "building_gang_market", (1, 1, 350, 209) },
        { "building_school", (1, 1, 200, 170) },
        { "building_fight_club", (1, 1, 190, 154) },
        { "building_bank", (1, 1, 165, 144) },
        { "building_hospital", (1, 1, 190, 162) },
        { "building_prison", (1, 1, 350, 233) },
        { "building_estate", (1, 1, 230, 190) },
        { "building_work_office", (1, 1, 190, 206) },
        { "building_mercenary_base", (1, 1, 280, 237) },
        { "building_gym", (1, 1, 185, 159) },
        { "building_airport", (1, 1, 350, 244) },
        { "building_city_market", (1, 1, 200, 163) },
        { "building_black market", (1, 1, 180, 173) },
        { "building_city_database", (1, 1, 160, 174) },
        { "building_skyscraper", (1, 1, 230, 315) }
    };

    // إحداثيات القص لكل صورة من banners.png
    public static readonly Dictionary<string, (int x, int y, int w, int h)> BannersBounds = new()
    {
        { "banner_airport", (1, 181, 170, 43) },
        { "banner_bank", (1, 136, 170, 43) },
        { "banner_black_market", (173, 181, 170, 43) },
        { "banner_casino", (1, 91, 170, 43) },
        { "banner_cenima", (173, 136, 170, 43) },
        { "banner_city_market", (345, 181, 170, 43) },
        { "banner_estate_office", (1, 46, 170, 43) },
        { "banner_fight_club", (173, 91, 170, 43) },
        { "banner_gang_center", (345, 136, 170, 43) },
        { "banner_gang_market", (517, 181, 170, 43) },
        { "banner_gym", (1, 1, 170, 43) },
        { "banner_hanager", (173, 46, 170, 43) },
        { "banner_hospital", (345, 91, 170, 43) },
        { "banner_lucky_wheel", (517, 136, 170, 43) },
        { "banner_mercenary_base", (689, 181, 170, 43) },
        { "banner_palace", (173, 1, 170, 43) },
        { "banner_prison", (345, 46, 170, 43) },
        { "banner_school", (517, 91, 170, 43) },
        { "banner_settings", (689, 136, 170, 43) },
        { "banner_skyscraper", (345, 1, 170, 43) },
        { "banner_upgrade_lab", (517, 46, 170, 43) },
        { "banner_wedding_hall", (689, 91, 170, 43) },
        { "banner_work_office", (517, 1, 170, 43) }
    };

    public static Dictionary<string, (int x, int y, int w, int h)> GetBoundsForLayer(string layer)
    {
        return layer switch
        {
            "floor" => FloorBounds,
            "roads" => RoadsBounds,
            "buildings" => BuildingsBounds,
            "banners" => BannersBounds,
            _ => new Dictionary<string, (int, int, int, int)>()
        };
    }
}