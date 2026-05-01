using System.Text.Json.Serialization;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace GameProject02.Models;

public class MapItem
{
    public string Id { get; set; } = "";
    public string ImageName { get; set; } = "";
    public string Layer { get; set; } = "floor";
    public string DisplayName { get; set; } = "";
    public double X { get; set; }
    public double Y { get; set; }
    public double Ox { get; set; }
    public double Oy { get; set; }
    public int ZIndex { get; set; }
    public bool FlipX { get; set; }
    public bool Clickable { get; set; }
    [JsonPropertyName("layerName")]
    public string LayerName { get; set; } = "";
}

public class SceneRoot
{
    public CompositeItemVO composite { get; set; }
}

public class ContentVO
{
    [JsonProperty("games.rednblack.editor.renderer.data.CompositeItemVO")]
    public List<CompositeItemVO> items { get; set; } = new();

    [JsonProperty("games.rednblack.editor.renderer.data.SimpleImageVO")]
    public List<SimpleImageVO> images { get; set; } = new();
}

public class CompositeItemVO
{
    public string itemIdentifier { get; set; } = "";
    public string layerName { get; set; } = "";
    public float x { get; set; }
    public float y { get; set; }
    public int zIndex { get; set; }

    // ── ADDED: HyperLap2D stores scale/rotation on every item ──────────────
    public float scaleX { get; set; } = 1f;
    public float scaleY { get; set; } = 1f;
    public float rotation { get; set; } = 0f;
    // ────────────────────────────────────────────────────────────────────────

    public ContentVO content { get; set; }
}

public class SimpleImageVO
{
    public string imageName { get; set; } = "";
    public string itemIdentifier { get; set; } = "";
    public string layerName { get; set; } = "";
    public float x { get; set; }
    public float y { get; set; }
    public float originX { get; set; }
    public float originY { get; set; }
    public int zIndex { get; set; }
    public bool flipX { get; set; } = false;
    public bool flipY { get; set; } = false;

    // ── ADDED: HyperLap2D stores scale/rotation on every item ──────────────
    public float scaleX { get; set; } = 1f;
    public float scaleY { get; set; } = 1f;
    public float rotation { get; set; } = 0f;
    // ────────────────────────────────────────────────────────────────────────
}