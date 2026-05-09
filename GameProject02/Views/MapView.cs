using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;
using GameProject02.Models;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Linq;

namespace GameProject02.Views;

// ═══════════════════════════════════════════════════════════════════════════
//  COORDINATE SYSTEM (HyperLap2D / libGDX)
// ═══════════════════════════════════════════════════════════════════════════
//
//  img.x / img.y    = BOTTOM-LEFT corner of the sprite in parent/world space
//  img.originX/Y    = pivot measured FROM the sprite's bottom-left corner
//                     (equals width/2 and height/2 when pivot is at centre)
//
//  Sprite world bounding box:
//    left   = img.x
//    right  = img.x + bmp.Width          ← actual PNG width
//    bottom = img.y
//    top    = img.y + bmp.Height         ← actual PNG height
//
//  In Skia (Y-down) after translating canvas to the pivot point:
//    dst = SKRect( -originX,            ← left   (originX px left of pivot)
//                  originY - bmpHeight, ← top    (bmpHeight-originY px above pivot)
//                  bmpWidth - originX,  ← right  (bmpWidth-originX px right of pivot)
//                  originY )            ← bottom (originY px below pivot)
//
//  Special case — pivot at exact centre (originX=W/2, originY=H/2):
//    dst = SKRect(-W/2, -H/2, W/2, H/2)   ← same as old (-origin, +origin)
// ═══════════════════════════════════════════════════════════════════════════

public class MapView : ContentView
{
    // ── Bitmap cache: key = raw imageName from .dt  ──────────────────────────
    private readonly Dictionary<string, SKBitmap> _imageCache = new();
    private readonly SKCanvasView _canvasView;
    private CompositeItemVO _sceneRoot;
    private bool _isMapLoaded = false;

    // ── Camera ───────────────────────────────────────────────────────────────
    private float _camX = 1170f;
    private float _camY = -1155f;
    private float _zoom = 2.5f;
    private float _startX, _startY;
    private bool _wasPanning = false;

    // ── Inertia ───────────────────────────────────────────────────────────────
    private float _velocityX, _velocityY;
    private float _lastTotalX, _lastTotalY;
    private System.Timers.Timer _inertiaTimer;

    // ── Camera bounds ────────────────────────────────────────────────────────
    private const float CAM_X_MIN = 0f;
    private const float CAM_X_MAX = 2500f;
    private const float CAM_Y_MIN = -1900f;
    private const float CAM_Y_MAX = -265f;

    // ── Visible world rect for culling (updated each frame) ──────────────────
    private float _visMinX, _visMaxX, _visMinY, _visMaxY;

    public event EventHandler<string> BuildingTapped;

    // ════════════════════════════════════════════════════════════════════════
    //  CONSTRUCTOR
    // ════════════════════════════════════════════════════════════════════════

    public MapView()
    {
        _canvasView = new SKCanvasView();
        _canvasView.PaintSurface += OnPaint;
        Content = _canvasView;

        // ── Pan gesture ──────────────────────────────────────────────────────
        var pan = new PanGestureRecognizer();
        pan.PanUpdated += (s, e) =>
        {
            if (e.StatusType == GestureStatus.Started)
            {
                StopInertia();
                _startX = _camX;
                _startY = _camY;
                _wasPanning = false;
                _velocityX = _velocityY = 0f;
                _lastTotalX = _lastTotalY = 0f;
            }

            if (e.StatusType == GestureStatus.Running)
            {
                if (Math.Abs(e.TotalX) > 8 || Math.Abs(e.TotalY) > 8)
                    _wasPanning = true;

                _velocityX = (float)(e.TotalX - _lastTotalX);
                _velocityY = (float)(e.TotalY - _lastTotalY);
                _lastTotalX = (float)e.TotalX;
                _lastTotalY = (float)e.TotalY;

                _camX = _startX - (float)e.TotalX / _zoom;
                _camY = _startY + (float)e.TotalY / _zoom;
                ClampCamera();
                _canvasView.InvalidateSurface();
            }

            if (e.StatusType == GestureStatus.Completed ||
                e.StatusType == GestureStatus.Canceled)
            {
                StartInertia();
                Task.Delay(200).ContinueWith(_ => _wasPanning = false);
            }
        };
        GestureRecognizers.Add(pan);

        // ── Tap gesture ──────────────────────────────────────────────────────
        var tap = new TapGestureRecognizer();
        tap.Tapped += OnTap;
        GestureRecognizers.Add(tap);

        InitializeMap();
    }

    // ════════════════════════════════════════════════════════════════════════
    //  CAMERA HELPERS
    // ════════════════════════════════════════════════════════════════════════

    private void ClampCamera()
    {
        float halfW = (_canvasView.CanvasSize.Width > 0
            ? _canvasView.CanvasSize.Width : 1080f) / 2f / _zoom;
        float halfH = (_canvasView.CanvasSize.Height > 0
            ? _canvasView.CanvasSize.Height : 1920f) / 2f / _zoom;

        _camX = Math.Clamp(_camX, CAM_X_MIN + halfW, CAM_X_MAX - halfW);
        _camY = Math.Clamp(_camY, CAM_Y_MIN + halfH, CAM_Y_MAX - halfH);
    }

    private void StartInertia()
    {
        if (Math.Abs(_velocityX) < 1f && Math.Abs(_velocityY) < 1f) return;

        const float friction = 0.99f;  // احتكاك — قلّله للتوقف أسرع
        const float minSpeed = 0.08f;
        const int intervalMs = 16;    // ~60 fps

        _inertiaTimer = new System.Timers.Timer(intervalMs);
        _inertiaTimer.Elapsed += (_, _) =>
        {
            _velocityX *= friction;
            _velocityY *= friction;

            if (Math.Abs(_velocityX) < minSpeed && Math.Abs(_velocityY) < minSpeed)
            {
                StopInertia();
                return;
            }

            _camX -= _velocityX / _zoom;
            _camY += _velocityY / _zoom;
            ClampCamera();
            MainThread.BeginInvokeOnMainThread(_canvasView.InvalidateSurface);
        };
        _inertiaTimer.AutoReset = true;
        _inertiaTimer.Start();
    }

    private void StopInertia()
    {
        _inertiaTimer?.Stop();
        _inertiaTimer?.Dispose();
        _inertiaTimer = null;
    }

    // ════════════════════════════════════════════════════════════════════════
    //  TAP → HIT TEST
    // ════════════════════════════════════════════════════════════════════════

    private void OnTap(object sender, TappedEventArgs e)
    {
        if (_wasPanning || !_isMapLoaded || _sceneRoot == null) return;

        var pt = e.GetPosition(_canvasView);
        if (pt == null) return;

        float canvasW = _canvasView.CanvasSize.Width;
        float canvasH = _canvasView.CanvasSize.Height;
        if (canvasW <= 0 || canvasH <= 0) return;

        // DIPs → physical pixels
        float density = canvasW / (float)_canvasView.Width;
        float pxX = (float)pt.Value.X * density;
        float pxY = (float)pt.Value.Y * density;

        // Screen → world (Y-up)
        float wx = (pxX - canvasW / 2f) / _zoom + _camX;
        float wy = _camY - (pxY - canvasH / 2f) / _zoom;

        Debug.WriteLine($"[TAP] screen=({pxX:F0},{pxY:F0})  world=({wx:F0},{wy:F0})");

        string hit = HitTestImages(_sceneRoot.content, 0f, 0f, wx, wy);
        if (!string.IsNullOrEmpty(hit))
        {
            Debug.WriteLine($"[HIT] {hit}");
            BuildingTapped?.Invoke(this, hit);
        }
    }

    /// <summary>
    /// Hit-test all images in the content tree.
    /// parentX/Y = accumulated world bottom-left of the current composite.
    ///
    /// Supports two scene layouts:
    ///   1. Composite-based: buildings live inside CompositeItemVO with an itemIdentifier.
    ///   2. Flat scene (current MainScene.dt): all sprites are direct SimpleImageVO items
    ///      — the imageName is used to look up the building ID via GetBuildingId().
    /// </summary>
    private string HitTestImages(ContentVO content, float parentX, float parentY,
                                  float wx, float wy)
    {
        if (content == null) return null;

        // ── 1. Test nested composites (composite-based layout) ────────────────
        if (content.items != null)
        {
            // Highest zIndex = drawn last = on top → test first
            var sorted = content.items
                .OrderByDescending(c => c.zIndex)
                .ToList();

            foreach (var civo in sorted)
            {
                float cblX = parentX + civo.x;
                float cblY = parentY + civo.y;
                bool isNamed = !string.IsNullOrEmpty(civo.itemIdentifier);

                if (civo.content?.images != null)
                {
                    foreach (var img in civo.content.images)
                    {
                        if (!_imageCache.TryGetValue(img.imageName, out var bmp)) continue;

                        float imgLeft = cblX + img.x;
                        float imgBottom = cblY + img.y;
                        float imgRight = imgLeft + bmp.Width;
                        float imgTop = imgBottom + bmp.Height;

                        if (wx >= imgLeft && wx <= imgRight &&
                            wy >= imgBottom && wy <= imgTop)
                        {
                            if (isNamed) return civo.itemIdentifier;
                        }
                    }
                }

                string inner = HitTestImages(civo.content, cblX, cblY, wx, wy);
                if (inner != null)
                    return isNamed ? civo.itemIdentifier : inner;
            }
        }

        // ── 2. Test root-level building images (flat scene layout) ────────────
        //
        // In a flat scene, all sprites (floor, roads, buildings) live directly
        // in content.images. We only want to hit "real" named buildings — not
        // floor tiles, roads, fake buildings, trees, or decorations.
        // GetBuildingId() returns null for everything that is not clickable.
        if (content.images != null)
        {
            // Highest zIndex = drawn on top → test first
            var buildingImgs = content.images
                .Where(img => img.layerName == "buildings")
                .OrderByDescending(img => img.zIndex);

            foreach (var img in buildingImgs)
            {
                string buildingId = GetBuildingId(img.imageName);
                if (string.IsNullOrEmpty(buildingId)) continue;

                if (!_imageCache.TryGetValue(img.imageName, out var bmp)) continue;

                float imgLeft = parentX + img.x;
                float imgBottom = parentY + img.y;
                float imgRight = imgLeft + bmp.Width;
                float imgTop = imgBottom + bmp.Height;

                if (wx >= imgLeft && wx <= imgRight &&
                    wy >= imgBottom && wy <= imgTop)
                {
                    Debug.WriteLine($"[HitTest] hit '{buildingId}' at world({wx:F0},{wy:F0})  bounds([{imgLeft:F0},{imgRight:F0}]×[{imgBottom:F0},{imgTop:F0}])");
                    return buildingId;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Maps a HyperLap2D image name to the building identifier used by CityMapPage.
    /// Returns null for decorations, fake buildings, trees, roads, and floor tiles
    /// — these should never trigger navigation.
    /// </summary>
    private static string GetBuildingId(string imageName)
    {
        return imageName?.TrimStart('@') switch
        {
            "building_casino" => "Casino",
            "building_airport" => "Airport",
            "building_lucky_wheel" => "LuckyWheel",
            "building_cinema" => "Cinema",
            "building_black market" => "BlackMarket",
            "building_bank" => "Bank",
            "building_estate" => "Estate",
            "building_city_market" => "CityMarket",
            "building_city_database" => "CityDatabase",
            "building_fight_club" => "FightClub",
            "building_gang_base" => "GangBase",
            "building_gym" => "Gym",
            "building_gang_market" => "GangMarket",
            "building_mercenary_base" => "MercenaryBase",
            "building_prison" => "Prison",
            "building_skyscraper" => "Skyscraper",
            "building_upgrade_lab" => "UpgradeLab",
            "building_work_office" => "WorkOffice",
            "building_hospital" => "Hospital",
            "building_school" => "School",
            // ── Not clickable ───────────────────────────────────────────────
            // fake buildings, war banners, decorations → return null
            _ => null
        };
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
                // ── Load scene JSON ──────────────────────────────────────────
                string json = "";
                foreach (var file in new[] { "MainScene.dt"})
                {
                    try { json = await LoadText(file); break; }
                    catch { /* try next */ }
                }

                if (string.IsNullOrEmpty(json))
                {
                    Debug.WriteLine("[MapView] ERROR: scene file not found");
                    return;
                }

                var data = JsonConvert.DeserializeObject<SceneRoot>(json);
                if (data?.composite?.content == null) return;

                // ── Collect all unique image names ───────────────────────────
                var imageNames = new HashSet<string>();
                CollectImageNames(data.composite.content, imageNames);
                Debug.WriteLine($"[MapView] {imageNames.Count} unique assets to load");

                // ── Load bitmaps ─────────────────────────────────────────────
                // FIX #3: expanded variants list — handles spaces in names
                // e.g. "@building_black market" → "building_black_market.png"
                var folders = new[] { "", "floor/", "roads/", "banners/", "buildings/", "others/" };
                var exts = new[] { ".png", ".PNG", ".jpg", ".jpeg" };

                foreach (var raw in imageNames)
                {
                    string name = raw.Trim();
                    if (_imageCache.ContainsKey(name)) continue;

                    string bare = name.TrimStart('@');
                    string bareUnderscore = bare.Replace(" ", "_");
                    string bareSpace = bare.Replace("_", " ");

                    // All variants we'll try (with and without @, case variants,
                    // space/underscore swapped)
                    var variants = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                    {
                        name,                              // @building_black market
                        bare,                              // building_black market
                        "@" + bare,                        // @building_black market
                        bareUnderscore,                    // building_black_market  ✅ NEW
                        "@" + bareUnderscore,              // @building_black_market ✅ NEW
                        bareSpace,                         // building black market
                        name.ToLower(),
                        bare.ToLower(),
                        bareUnderscore.ToLower(),          // ✅ NEW lowercase variant
                        bareSpace.ToLower(),
                    };

                    bool found = false;
                    foreach (var folder in folders)
                    {
                        if (found) break;
                        foreach (var variant in variants)
                        {
                            if (found) break;
                            foreach (var ext in exts)
                            {
                                if (await TryLoadBitmap($"{folder}{variant}", name, ext))
                                {
                                    found = true;
                                    break;
                                }
                            }
                        }
                    }

                    if (!found)
                        Debug.WriteLine($"[MapView] MISSING: {name}");
                }

                _sceneRoot = data.composite;
                Debug.WriteLine($"[MapView] Loaded — {_imageCache.Count}/{imageNames.Count} images");
                _isMapLoaded = true;
                MainThread.BeginInvokeOnMainThread(_canvasView.InvalidateSurface);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MapView] CRITICAL: {ex.Message}");
            }
        });
    }

    private void CollectImageNames(ContentVO content, HashSet<string> names)
    {
        if (content == null) return;
        content.images?.ForEach(img => { if (!string.IsNullOrEmpty(img.imageName)) names.Add(img.imageName); });
        content.items?.ForEach(civo => CollectImageNames(civo.content, names));
    }

    private async Task<bool> TryLoadBitmap(string file, string cacheKey, string ext)
    {
        try
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync($"Assets/images/maincity/{file}{ext}");
            var bmp = SKBitmap.Decode(stream);
            if (bmp == null) return false;
            _imageCache[cacheKey] = bmp;
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

        float W = e.Info.Width;
        float H = e.Info.Height;

        // Compute visible world rectangle for culling
        float halfW = W / 2f / _zoom;
        float halfH = H / 2f / _zoom;
        _visMinX = _camX - halfW;
        _visMaxX = _camX + halfW;
        _visMinY = _camY - halfH;
        _visMaxY = _camY + halfH;

        // World → screen transform
        //   screen_x = (world_x − camX) × zoom + W/2
        //   screen_y = −(world_y − camY) × zoom + H/2   ← Y-flip
        canvas.Translate(W / 2f, H / 2f);
        canvas.Scale(_zoom, _zoom);
        canvas.Translate(-_camX, _camY);

        RenderContent(canvas, _sceneRoot.content, 0f, 0f);
    }

    private void RenderContent(SKCanvas canvas, ContentVO content,
                               float parentX, float parentY)
    {
        if (content == null) return;

        // Merge images and composites into one draw list sorted by zIndex
        var drawList = new List<(int z, object obj)>();
        content.images?.ForEach(img => drawList.Add((img.zIndex, img)));
        content.items?.ForEach(civo => drawList.Add((civo.zIndex, civo)));
        drawList.Sort((a, b) => a.z.CompareTo(b.z));

        foreach (var (_, obj) in drawList)
        {
            if (obj is SimpleImageVO sivo)
            {
                // ── Per-sprite culling using actual bitmap size ────────────
                if (!_imageCache.TryGetValue(sivo.imageName, out var bmp)) continue;

                float spriteLeft = parentX + sivo.x;
                float spriteBottom = parentY + sivo.y;
                float spriteRight = spriteLeft + bmp.Width;
                float spriteTop = spriteBottom + bmp.Height;

                if (spriteRight < _visMinX || spriteLeft > _visMaxX ||
                    spriteTop < _visMinY || spriteBottom > _visMaxY)
                    continue;   // off-screen — skip
                // ─────────────────────────────────────────────────────────

                DrawSprite(canvas, sivo, bmp);
            }
            else if (obj is CompositeItemVO civo)
            {
                // Translate canvas to composite's bottom-left (Y-flipped for Skia)
                canvas.Save();
                canvas.Translate(civo.x, -civo.y);
                RenderContent(canvas, civo.content,
                              parentX + civo.x, parentY + civo.y);
                canvas.Restore();
            }
        }
    }

    // ════════════════════════════════════════════════════════════════════════
    //  DRAW SPRITE  (the core fix)
    // ════════════════════════════════════════════════════════════════════════
    //
    //  HyperLap2D places a sprite with its BOTTOM-LEFT at (img.x, img.y)
    //  and draws it at the PNG's actual pixel size (bmp.Width × bmp.Height).
    //  The originX/Y is ONLY a rotation/scale pivot — it does NOT define size.
    //
    //  We translate the Skia canvas to the PIVOT POINT, then draw a rect
    //  that is positioned such that:
    //    • left   edge is originX pixels LEFT  of the pivot
    //    • bottom edge is originY pixels BELOW the pivot   (in Y-up world)
    //    • right  edge is (bmpWidth  − originX) pixels RIGHT of the pivot
    //    • top    edge is (bmpHeight − originY) pixels ABOVE the pivot
    //
    //  In Skia coordinates (Y-down):
    //    SKRect( left           = −originX,
    //            top (Skia-up)  = −(bmpHeight − originY)  = originY − bmpHeight,
    //            right          = bmpWidth − originX,
    //            bottom (Skia)  = originY )
    // ════════════════════════════════════════════════════════════════════════

    private void DrawSprite(SKCanvas canvas, SimpleImageVO img, SKBitmap bmp)
    {
        float bw = bmp.Width;
        float bh = bmp.Height;
        float ox = img.originX;
        float oy = img.originY;

        canvas.Save();

        // Move canvas origin to the sprite's pivot point in world space
        //   pivot world X = img.x + originX
        //   pivot world Y = img.y + originY  →  Skia: −(img.y + originY)
        canvas.Translate(img.x + ox, -(img.y + oy));

        // Horizontal / vertical flip around the pivot
        if (img.flipX || img.flipY)
            canvas.Scale(img.flipX ? -1f : 1f, img.flipY ? -1f : 1f);

        // ── FIX #1: use actual PNG dimensions, not 2*origin ──────────────────
        var dst = new SKRect(
            left: -ox,       // originX px left  of pivot
            top: oy - bh,   // (bh−oy) px above pivot  (Skia Y-down: negative = up)
            right: bw - ox,   // (bw−ox) px right of pivot
            bottom: oy         // originY px below pivot
        );

        // ── FIX #2: close sub-pixel gaps between floor / road tiles ──────────
        // At non-integer zoom levels, Skia's antialiasing creates hairline seams.
        // Expanding each tile by half a screen-pixel in world units closes them.
        if (img.layerName == "floor" || img.layerName == "roads")
        {
            float px = 0.6f / _zoom;    // half screen-pixel in world units
            dst = new SKRect(dst.Left - px, dst.Top - px,
                             dst.Right + px, dst.Bottom + px);
        }
        // ─────────────────────────────────────────────────────────────────────

        canvas.DrawBitmap(bmp, dst);
        canvas.Restore();
    }
}