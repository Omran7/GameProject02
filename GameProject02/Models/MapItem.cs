using Newtonsoft.Json;
using System.Collections.Generic;

namespace GameProject02.Models;

// ═══════════════════════════════════════════════════════════════════════════
//  Scene data models — match the HyperLap2D .dt JSON structure exactly
// ═══════════════════════════════════════════════════════════════════════════

/// <summary>Root wrapper: { "sceneName": "...", "composite": { ... } }</summary>
public class SceneRoot
{
    public CompositeItemVO composite { get; set; }
}

/// <summary>
/// Holds the two possible child arrays.
/// HyperLap2D uses the full class name as the JSON key.
/// </summary>
public class ContentVO
{
    [JsonProperty("games.rednblack.editor.renderer.data.CompositeItemVO")]
    public List<CompositeItemVO> items { get; set; } = new();

    [JsonProperty("games.rednblack.editor.renderer.data.SimpleImageVO")]
    public List<SimpleImageVO> images { get; set; } = new();
}

/// <summary>
/// A named group of sprites (building, prop group, …).
///   x, y        = BOTTOM-LEFT corner of the group in parent/world space
///   zIndex      = draw order (lower = drawn first / behind)
///   itemIdentifier = name shown in editor (e.g. "Casino", "Airport")
/// </summary>
public class CompositeItemVO
{
    public string itemIdentifier { get; set; } = "";
    public string layerName { get; set; } = "";
    public float x { get; set; }
    public float y { get; set; }
    public int zIndex { get; set; }
    public float scaleX { get; set; } = 1f;
    public float scaleY { get; set; } = 1f;
    public float rotation { get; set; } = 0f;
    public ContentVO content { get; set; }
}

/// <summary>
/// A single sprite (floor tile, road piece, building image, banner, …).
///   x, y         = BOTTOM-LEFT corner of the sprite in parent/world space
///   originX/Y    = pivot point measured FROM the bottom-left corner
///                  (= width/2, height/2 when the pivot is at the sprite centre)
///   zIndex       = draw order
///   flipX/flipY  = mirror the image around its pivot
/// </summary>
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
    public float scaleX { get; set; } = 1f;
    public float scaleY { get; set; } = 1f;
    public float rotation { get; set; } = 0f;
}

// ═══════════════════════════════════════════════════════════════════════════
//  MapItem — lightweight DTO used by the game logic layer (not for rendering)
// ═══════════════════════════════════════════════════════════════════════════

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
    public string LayerName { get; set; } = "";
}

// ═══════════════════════════════════════════════════════════════════════════
//  MapTile — used by the UI tile grid (not for SkiaSharp rendering)
// ═══════════════════════════════════════════════════════════════════════════

public class MapTile
{
    public string Id { get; set; } = string.Empty;
    public string ImageName { get; set; } = "tile_default";
    public double X { get; set; }
    public double Y { get; set; }
    public int ZIndex { get; set; }
    public string LayerName { get; set; } = "floor";
    public bool FlipX { get; set; } = false;
}