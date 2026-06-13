using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using GameProject02.Models;

namespace GameProject02.Services;

/*public class MapItem
{
    [JsonPropertyName("imageName")]
    public string ImageName { get; set; } = "";

    [JsonPropertyName("x")]
    public double X { get; set; }

    [JsonPropertyName("y")]
    public double Y { get; set; }

    [JsonPropertyName("originX")]
    public double OriginX { get; set; }

    [JsonPropertyName("originY")]
    public double OriginY { get; set; }

    [JsonPropertyName("zIndex")]
    public int ZIndex { get; set; }

    [JsonPropertyName("layerName")]
    public string LayerName { get; set; } = "";
}*/

public static class MapLoaderService
{
    public static async Task<List<MapItem>> LoadAtlasAsync()
    {
        var items = new List<MapItem>();
        try
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync("map_atlas.json");
            using var reader = new StreamReader(stream);
            var json = await reader.ReadToEndAsync();

            // Parse the JSON array
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var loadedItems = JsonSerializer.Deserialize<List<MapItem>>(json, options);

            if (loadedItems != null)
            {
                // Filter only items that have an image and a layer
                items = loadedItems.FindAll(i => !string.IsNullOrEmpty(i.ImageName) && !string.IsNullOrEmpty(i.LayerName));
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MAP ERROR] {ex.Message}");
        }
        return items;
    }
}