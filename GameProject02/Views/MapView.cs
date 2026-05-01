using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;
using GameProject02.Models;
using Newtonsoft.Json;
using System.Diagnostics;

namespace GameProject02.Views;

// ═══════════════════════════════════════════════════════════════════════════
//  ROOT CAUSE OF THE COORDINATE BUG (confirmed by reading MainScene.dt)
// ═══════════════════════════════════════════════════════════════════════════
//
//  In HyperLap2D / libGDX:
//    composite.x / composite.y  = BOTTOM-LEFT corner of the composite in world
//    image.x     / image.y      = BOTTOM-LEFT corner of the image in composite-local space
//    image.originX / originY    = rotation pivot measured FROM the image's bottom-left
//                                 For ALL items in this scene: origin = exact centre
//                                 i.e.  originX = width/2,  originY = height/2
//
//  WRONG (previous code):
//    canvas.Translate(img.x, -img.y)          ← lands at image BOTTOM-LEFT
//    dst = (-originX, -originY, originX, originY)  ← draws centred there
//    → image centre ends up at the bottom-left instead of the true centre
//    → every building is offset by (−originX, −originY) from its correct position
//
//  RIGHT (this code):
//    canvas.Translate(img.x + img.originX, -(img.y + img.originY))
//                                             ← lands at image CENTRE (pivot)
//    dst = (-originX, -originY, originX, originY)  ← draws centred there  ✓
//    → image centre is at world (composite.x + img.x + originX,
//                                composite.y + img.y + originY)             ✓
//
//  Verified numbers (from the actual .dt file):
//    Casino  composite BL=(122,−766)  origin=(130,112)
//    WRONG centre → (122,−766)   CORRECT centre → (252,−654)
//    Casino image covers world X[122,382] Y[−766,−542]  ✓
// ═══════════════════════════════════════════════════════════════════════════

public class MapView : ContentView
{
    private readonly SKCanvasView _canvasView;
    private readonly Dictionary<string, SKBitmap> _imageCache = new();
    private CompositeItemVO _sceneRoot;
    private bool _isMapLoaded = false;

    // ── Camera ───────────────────────────────────────────────────────────────
    // Buildings span roughly  X: 95–2250,  Y: –1876 to –432
    // Centre ≈ (1170, –1155) — good initial camera
    private float _camX = 1170f;
    private float _camY = -1155f;
    private float _zoom = 1.5f;    // zoom level — increase to make map bigger, decrease to see more
    private float _startX, _startY;
    private bool _wasPanning = false;   // suppress tap that fires at end of a pan

    public event EventHandler<string> BuildingTapped;

    public MapView()
    {
        _canvasView = new SKCanvasView();
        _canvasView.PaintSurface += OnPaint;
        Content = _canvasView;

        var pan = new PanGestureRecognizer();
        pan.PanUpdated += (s, e) =>
        {
            if (e.StatusType == GestureStatus.Started)
            {
                _startX = _camX; _startY = _camY;
                _wasPanning = false;
            }
            if (e.StatusType == GestureStatus.Running)
            {
                if (Math.Abs(e.TotalX) > 8 || Math.Abs(e.TotalY) > 8)
                    _wasPanning = true;

                _camX = _startX - (float)e.TotalX / _zoom;
                _camY = _startY + (float)e.TotalY / _zoom;
                _canvasView.InvalidateSurface();
            }
            if (e.StatusType == GestureStatus.Completed ||
                e.StatusType == GestureStatus.Canceled)   // ✅ fixed spelling
            {
                Task.Delay(200).ContinueWith(_ => _wasPanning = false);
            }
        };
        GestureRecognizers.Add(pan);

        var tap = new TapGestureRecognizer();
        tap.Tapped += OnTap;
        GestureRecognizers.Add(tap);

        InitializeMap();
    }

    // ════════════════════════════════════════════════════════════════════════
    //  TAP → HIT TEST
    // ════════════════════════════════════════════════════════════════════════

    private void OnTap(object sender, TappedEventArgs e)
    {
        // Ignore the ghost tap that fires at the end of a pan gesture
        if (_wasPanning) return;
        if (!_isMapLoaded || _sceneRoot == null) return;

        var pt = e.GetPosition(_canvasView);
        if (pt == null) return;

        // ── CRITICAL: e.GetPosition returns DIPs, but OnPaint uses PHYSICAL PIXELS.
        // ── We must convert the touch position to pixels to match the canvas transform.
        // ── _canvasView.CanvasSize is always in physical pixels.
        float canvasW = _canvasView.CanvasSize.Width;
        float canvasH = _canvasView.CanvasSize.Height;

        if (canvasW <= 0 || canvasH <= 0) return;

        // Scale factor: how many physical pixels per DIP
        float density = canvasW / (float)_canvasView.Width;

        float pxX = (float)pt.Value.X * density;
        float pxY = (float)pt.Value.Y * density;

        // Invert the canvas transform (which is in physical pixels):
        //   cx = (wx - camX)*zoom + canvasW/2  →  wx = (cx - canvasW/2)/zoom + camX
        //   cy = -(wy - camY)*zoom + canvasH/2 →  wy = camY - (cy - canvasH/2)/zoom
        float wx = (pxX - canvasW / 2f) / _zoom + _camX;
        float wy = _camY - (pxY - canvasH / 2f) / _zoom;

        Debug.WriteLine($"[MapView TAP] screen=({pxX:F0},{pxY:F0}) world=({wx:F0},{wy:F0})");

        string hit = HitTestContent(_sceneRoot.content, 0f, 0f, wx, wy);
        if (!string.IsNullOrEmpty(hit))
        {
            Debug.WriteLine($"[MapView HIT] {hit}");
            BuildingTapped?.Invoke(this, hit);
        }
    }

    /// <summary>
    /// All coordinates here are world Y-up.
    /// parentX/Y = accumulated world position of the current composite's bottom-left.
    /// </summary>
    private string HitTestContent(ContentVO content, float parentX, float parentY, float wx, float wy)
    {
        if (content == null) return null;

        if (content.items != null)
        {
            // Reverse order: highest zIndex drawn last = on top = hit first
            for (int i = content.items.Count - 1; i >= 0; i--)
            {
                var civo = content.items[i];
                float cblX = parentX + civo.x;
                float cblY = parentY + civo.y;
                bool isNamedBuilding = !string.IsNullOrEmpty(civo.itemIdentifier);

                // Check each image inside this composite
                if (civo.content?.images != null)
                {
                    foreach (var img in civo.content.images)
                    {
                        float iblX = cblX + img.x;
                        float iblY = cblY + img.y;
                        float logW = img.originX * 2f;
                        float logH = img.originY * 2f;

                        if (wx >= iblX && wx <= iblX + logW &&
                            wy >= iblY && wy <= iblY + logH)
                        {
                            // Only return a hit for named buildings — ignore unnamed composites
                            if (isNamedBuilding) return civo.itemIdentifier;
                        }
                    }
                }

                // Recurse into nested composites
                string inner = HitTestContent(civo.content, cblX, cblY, wx, wy);
                if (inner != null)
                    return isNamedBuilding ? civo.itemIdentifier : inner;
            }
        }

        // Do NOT test content.images here.
        // Top-level images are floor tiles, roads, and decorations — not clickable.
        return null;
    }

    // ════════════════════════════════════════════════════════════════════════
    //  MAP LOADING
    // ════════════════════════════════════════════════════════════════════════

    private void InitializeMap()
    {
        Task.Run(async () =>
        {
            try
            {
                string json = "";
                foreach (var name in new[] { "MainScene.dt", "MainScene.json" })
                {
                    try { json = await LoadText(name); break; }
                    catch { /* try next */ }
                }
                if (string.IsNullOrEmpty(json))
                {
                    Debug.WriteLine("[MapView] ERROR: scene file not found");
                    return;
                }

                var data = JsonConvert.DeserializeObject<SceneRoot>(json);
                if (data?.composite?.content == null) return;

                var imageNames = new HashSet<string>();
                CollectImageNames(data.composite.content, imageNames);
                Debug.WriteLine($"[MapView] {imageNames.Count} unique assets");

                var folders = new[] { "", "floor/", "roads/", "banners/", "buildings/", "others/" };
                var exts = new[] { ".png", ".PNG", ".jpg", ".jpeg" };

                foreach (var raw in imageNames)
                {
                    string name = raw.Trim();
                    if (_imageCache.ContainsKey(name)) continue;

                    var variants = new HashSet<string>
                    {
                        name,
                        name.TrimStart('@'),
                        "@" + name.TrimStart('@'),
                        name.Replace(" ", "_"),
                        name.Replace("_", " "),
                        name.ToLower(),
                        name.TrimStart('@').ToLower(),
                        name.TrimStart('@').Replace("_", " ").ToLower()
                    };

                    bool found = false;
                    foreach (var folder in folders)
                    {
                        foreach (var variant in variants)
                        {
                            foreach (var ext in exts)
                            {
                                if (await TryLoad($"{folder}{variant}", name, ext))
                                { found = true; break; }
                            }
                            if (found) break;
                        }
                        if (found) break;
                    }

                    if (!found) Debug.WriteLine($"[MapView] MISSING: {name}");
                }

                _sceneRoot = data.composite;
                Debug.WriteLine($"[MapView] Done — {_imageCache.Count} images loaded");
                _isMapLoaded = true;
                MainThread.BeginInvokeOnMainThread(_canvasView.InvalidateSurface);
            }
            catch (Exception ex) { Debug.WriteLine($"[MapView] CRITICAL: {ex.Message}"); }
        });
    }

    private void CollectImageNames(ContentVO content, HashSet<string> names)
    {
        if (content == null) return;
        content.images?.ForEach(img => names.Add(img.imageName));
        content.items?.ForEach(civo => CollectImageNames(civo.content, names));
    }

    private async Task<bool> TryLoad(string file, string key, string ext)
    {
        try
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync($"Assets/images/{file}{ext}");
            var bmp = SKBitmap.Decode(stream);
            if (bmp == null) return false;
            _imageCache[key] = bmp;
            return true;
        }
        catch { return false; }
    }

    private async Task<string> LoadText(string path)
    {
        using var s = await FileSystem.OpenAppPackageFileAsync(path);
        using var r = new StreamReader(s);
        return await r.ReadToEndAsync();
    }

    // ════════════════════════════════════════════════════════════════════════
    //  RENDERING
    // ════════════════════════════════════════════════════════════════════════

    private void OnPaint(object sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear(new SKColor(18, 18, 20));
        if (!_isMapLoaded || _sceneRoot == null) return;

        // World → screen:
        //   cx = (wx - camX)*zoom + W/2
        //   cy = -(wy - camY)*zoom + H/2   ← Y-flip (world Y-up → Skia Y-down)
        canvas.Translate(e.Info.Width / 2f, e.Info.Height / 2f);
        canvas.Scale(_zoom, _zoom);
        canvas.Translate(-_camX, _camY);  // +camY because sprites draw at −y

        RenderContent(canvas, _sceneRoot.content);
    }

    private void RenderContent(SKCanvas canvas, ContentVO content)
    {
        if (content == null) return;

        var drawList = new List<(int z, object obj)>();
        content.images?.ForEach(img => drawList.Add((img.zIndex, img)));
        content.items?.ForEach(civo => drawList.Add((civo.zIndex, civo)));
        drawList.Sort((a, b) => a.z.CompareTo(b.z));

        foreach (var (_, obj) in drawList)
        {
            if (obj is SimpleImageVO sivo)
            {
                DrawSprite(canvas, sivo);
            }
            else if (obj is CompositeItemVO civo)
            {
                canvas.Save();

                // Move canvas to composite's BOTTOM-LEFT corner (Y-flip: world +y → Skia −y)
                canvas.Translate(civo.x, -civo.y);

                RenderContent(canvas, civo.content);

                canvas.Restore();
            }
        }
    }

    /// <summary>
    /// Draw a single sprite image.
    ///
    /// In HyperLap2D:
    ///   (img.x, img.y)   = image BOTTOM-LEFT corner in parent local space
    ///   (originX, originY) = rotation pivot from image bottom-left = centre of image
    ///
    /// So image CENTRE in world = (parentBL.x + img.x + originX,
    ///                             parentBL.y + img.y + originY)
    ///
    /// We translate the canvas to that CENTRE and draw a rect
    ///   (−originX, −originY, +originX, +originY)
    /// which is the image centred at (0,0) in the local space.
    ///
    /// DrawBitmap stretches the bitmap to fill the rect, so the image renders
    /// at exactly (2*originX) × (2*originY) world units regardless of the
    /// actual pixel dimensions of the PNG file.
    /// </summary>
    private void DrawSprite(SKCanvas canvas, SimpleImageVO img)
    {
        if (string.IsNullOrEmpty(img.imageName) ||
            !_imageCache.TryGetValue(img.imageName, out var bmp)) return;

        canvas.Save();

        // ── THE FIX ──────────────────────────────────────────────────────────
        // Translate to the image's CENTRE (pivot), not to its bottom-left.
        //   world centre X = img.x + originX
        //   world centre Y = img.y + originY   → Skia: -(img.y + originY)
        canvas.Translate(img.x + img.originX, -(img.y + img.originY));
        // ─────────────────────────────────────────────────────────────────────

        // Horizontal/vertical flip around the centre
        if (img.flipX || img.flipY)
            canvas.Scale(img.flipX ? -1f : 1f, img.flipY ? -1f : 1f);

        // Rect centred at pivot: logical size = 2*originX × 2*originY world units
        // DrawBitmap stretches bmp to fill → correct size regardless of file resolution
        var dst = new SKRect(-img.originX, -img.originY, img.originX, img.originY);
        canvas.DrawBitmap(bmp, dst);
        canvas.Restore();
    }

    /// <summary>
}