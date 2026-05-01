namespace GameProject02.Models;

public class MapTile
{
    public string Id { get; set; } = string.Empty;
    public string ImageName { get; set; } = "tile_default";
    public double X { get; set; }
    public double Y { get; set; }
    public int ZIndex { get; set; }
    public string LayerName { get; set; } = "floor"; // floor, roads, buildings
    public bool FlipX { get; set; } = false;
}