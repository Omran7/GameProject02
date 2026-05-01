namespace GameProject02.Models;

public class BuildingInfo
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ImageResource { get; set; } = string.Empty;
    public double X { get; set; }
    public double Y { get; set; }
    public int ZIndex { get; set; }
    public int Width { get; set; } = 200;
    public int Height { get; set; } = 150;
    public string NavigationTarget { get; set; } = string.Empty;
    public bool IsClickable { get; set; } = true;
    public string Description { get; set; } = string.Empty;

}