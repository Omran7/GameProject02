using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;
using GameProject02.Models;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Linq;

namespace GameProject02.Views;

public class MapView : ContentView
{
    // ══════════════════════════════════════════════════════════════════════════
    //  SPLINE TABLE
    // ══════════════════════════════════════════════════════════════════════════
    private static readonly float[] SplinePos = BuildSplineTable();

    private static float[] BuildSplineTable()
    {
        var tbl = new float[101];
        for (int i = 0; i < 100; i++)
        {
            float alpha = i / 100f;
            float lo = 0f, hi = 1f, t = 0f;
            for (int n = 0; n < 64; n++)
            {
                t = (lo + hi) * 0.5f;
                float tc = 1f - t;
                float tx = (t * 0.35f + tc * 0.175f) * 3f * t * tc + t * t * t;
                if (MathF.Abs(tx - alpha) < 1e-6f) break;
                if (tx > alpha) hi = t; else lo = t;
            }
            float tc2 = 1f - t;
            tbl[i] = (0.5f * tc2 + t) * 3f * t * tc2 + t * t * t;
        }
        tbl[100] = 1f;
        return tbl;
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  PHYSICS CONSTANTS
    // ══════════════════════════════════════════════════════════════════════════
    private const float DECEL_RATE = 2.35783f;
    private const float FRICTION = 0.015f;

    // ══════════════════════════════════════════════════════════════════════════
    //  ZOOM CONSTANTS
    //  ─────────────────────────────────────────────────────────────────────────
    //  الجهاز المرجعي: 1080px عرض، density=3 (360 DIP عرض)
    //  إذا بدت الخريطة صحيحة على جهاز مختلف غيّر REF_DIP_WIDTH و REF_DENSITY
    // ══════════════════════════════════════════════════════════════════════════
    private const float REFERENCE_ZOOM = 2.5f;  // ← الـ zoom الصحيح على الجهاز المرجعي
    private const float REF_DIP_WIDTH = 360f;  // ← عرض الجهاز المرجعي بالـ DIP
    private const float REF_DENSITY = 3.0f;  // ← كثافة الجهاز المرجعي
    private const float ZOOM_MIN = 1.0f;  // ← أقل zoom مسموح
    private const float ZOOM_MAX = 6.0f;  // ← أكبر zoom مسموح

    // ══════════════════════════════════════════════════════════════════════════
    //  FIELDS
    // ══════════════════════════════════════════════════════════════════════════
    private readonly Dictionary<string, SKBitmap> _imageCache = new();
    private readonly SKCanvasView _canvasView;
    private CompositeItemVO _sceneRoot;
    private bool _isMapLoaded = false;

    // ── Camera ────────────────────────────────────────────────────────────────
    private float _camX;
    private float _camY;
    private float _zoom;   // تُحسب في InitializeCamera()
    private bool _wasPanning = false;

    // ── Density ───────────────────────────────────────────────────────────────
    private float Density => (float)DeviceDisplay.MainDisplayInfo.Density;

    // ── Pan velocity ──────────────────────────────────────────────────────────
    private float _smoothVX, _smoothVY;
    private float _velX, _velY;
    private float _prevTotalX, _prevTotalY;
    private long _prevTimeMs;

    // ── Fling state ───────────────────────────────────────────────────────────
    private float _flingStartX, _flingStartY;
    private float _flingDX, _flingDY;
    private long _flingStartMs;
    private int _flingDurMs;
    private bool _xBlocked, _yBlocked;

    private System.Timers.Timer _inertiaTimer;

    // ── Camera bounds ─────────────────────────────────────────────────────────
    private const float CAM_X_MIN = 0f;
    private const float CAM_X_MAX = 2500f;
    private const float CAM_Y_MIN = -1900f;
    private const float CAM_Y_MAX = -265f;

    private float _visMinX, _visMaxX, _visMinY, _visMaxY;

    public event EventHandler<string> BuildingTapped;

    // ══════════════════════════════════════════════════════════════════════════
    //  CONSTRUCTOR
    // ══════════════════════════════════════════════════════════════════════════
    public MapView()
    {
        InitializeCamera();

        _canvasView = new SKCanvasView();
        _canvasView.PaintSurface += OnPaint;
        Content = _canvasView;

        var pan = new PanGestureRecognizer();
        pan.PanUpdated += OnPan;
        GestureRecognizers.Add(pan);

        var tap = new TapGestureRecognizer();
        tap.Tapped += OnTap;
        GestureRecognizers.Add(tap);

        InitializeMap();
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  ✅ حساب الـ zoom المناسب لكل جهاز
    //  ─────────────────────────────────────────────────────────────────────────
    //  المعادلة تضمن أن الخريطة تبدو بنفس الحجم البصري على جميع الأجهزة:
    //
    //  zoom = REFERENCE_ZOOM
    //       × (density / REF_DENSITY)    ← يعوّض الكثافة المختلفة
    //       × (dipWidth / REF_DIP_WIDTH) ← يعوّض حجم الشاشة المختلف
    //
    //  مثال (المرجع: 360 DIP, density=3, zoom=2.5):
    //    هاتف 360 DIP, density=2 → zoom = 2.5 × (2/3) × (360/360) = 1.67
    //    هاتف 480 DIP, density=3 → zoom = 2.5 × (3/3) × (480/360) = 3.33
    //    تابلت 800 DIP, density=2 → zoom = 2.5 × (2/3) × (800/360) = 3.70
    // ══════════════════════════════════════════════════════════════════════════
    private void InitializeCamera()
    {
        var info = DeviceDisplay.MainDisplayInfo;
        float physW = (float)info.Width;
        float density = (float)info.Density;
        float dipWidth = physW / density;

        _zoom = REFERENCE_ZOOM
              * (density / REF_DENSITY)
              * (dipWidth / REF_DIP_WIDTH);

        _zoom = Math.Clamp(_zoom, ZOOM_MIN, ZOOM_MAX);

        _camX = 1170f;
        _camY = -1155f;
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  PAN GESTURE
    // ══════════════════════════════════════════════════════════════════════════
    private void OnPan(object sender, PanUpdatedEventArgs e)
    {
        float d = Density;

        switch (e.StatusType)
        {
            case GestureStatus.Started:
                StopInertia();
                _wasPanning = false;
                _smoothVX = _smoothVY = 0f;
                _velX = _velY = 0f;
                _prevTotalX = _prevTotalY = 0f;
                _prevTimeMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                break;

            case GestureStatus.Running:
                float dxPx = ((float)e.TotalX - _prevTotalX) * d;
                float dyPx = ((float)e.TotalY - _prevTotalY) * d;
                _prevTotalX = (float)e.TotalX;
                _prevTotalY = (float)e.TotalY;

                _camX -= dxPx / _zoom;
                _camY += dyPx / _zoom;
                ClampCamera();

                long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                long dt = Math.Max(1, now - _prevTimeMs);
                _prevTimeMs = now;

                float nvX = dxPx * 16f / dt;
                float nvY = dyPx * 16f / dt;
                _smoothVX = 0.7f * nvX + 0.3f * _smoothVX;
                _smoothVY = 0.7f * nvY + 0.3f * _smoothVY;

                if (MathF.Abs(dxPx) > 2 || MathF.Abs(dyPx) > 2)
                    _wasPanning = true;

                _canvasView.InvalidateSurface();
                break;

            case GestureStatus.Completed:
            case GestureStatus.Canceled:
                _velX = _smoothVX;
                _velY = _smoothVY;
                StartFling();
                Task.Delay(150).ContinueWith(_ => _wasPanning = false);
                break;
        }
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  FLING PHYSICS
    // ══════════════════════════════════════════════════════════════════════════
    private void StartFling()
    {
        float speed = MathF.Sqrt(_velX * _velX + _velY * _velY);
        if (speed < 1f) return;

        float d = Density;
        float ppi = d * 160f * 386.0878f * 0.84f;
        float vPxSec = speed * 62.5f;
        float ratio = vPxSec * 0.35f / (FRICTION * ppi);
        if (ratio <= 0f) return;

        float l = MathF.Log(ratio);
        int dur = Math.Max(50, (int)(MathF.Exp(l / (DECEL_RATE - 1f)) * 1000f));

        float distPx = (float)(Math.Exp((double)(DECEL_RATE / (DECEL_RATE - 1f)) * l)
                                * FRICTION * ppi);

        float dirX = _velX / speed;
        float dirY = _velY / speed;

        _flingStartX = _camX;
        _flingStartY = _camY;
        _flingDX = -(dirX * distPx) / _zoom;
        _flingDY = (dirY * distPx) / _zoom;
        _flingStartMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        _flingDurMs = dur;
        _xBlocked = false;
        _yBlocked = false;

        Debug.WriteLine($"[Fling] v={vPxSec:F0}px/s  dist={distPx:F0}px  dur={dur}ms");
        StartFlingTimer();
    }

    private void StartFlingTimer()
    {
        StopInertia();
        _inertiaTimer = new System.Timers.Timer(16);
        _inertiaTimer.Elapsed += OnFlingTick;
        _inertiaTimer.AutoReset = true;
        _inertiaTimer.Start();
    }

    private void OnFlingTick(object? s, System.Timers.ElapsedEventArgs _)
    {
        long elapsed = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - _flingStartMs;

        float pos;
        if (elapsed >= _flingDurMs)
        {
            pos = 1f;
        }
        else
        {
            float t = (float)elapsed / _flingDurMs;
            int idx = Math.Clamp((int)(t * 100f), 0, 99);
            float frc = t * 100f - idx;
            pos = SplinePos[idx] + (SplinePos[idx + 1] - SplinePos[idx]) * frc;
        }

        float targetX = _xBlocked ? _camX : _flingStartX + _flingDX * pos;
        float targetY = _yBlocked ? _camY : _flingStartY + _flingDY * pos;

        float halfW = (_canvasView.CanvasSize.Width > 0
                        ? _canvasView.CanvasSize.Width : 1080f) / 2f / _zoom;
        float halfH = (_canvasView.CanvasSize.Height > 0
                        ? _canvasView.CanvasSize.Height : 1920f) / 2f / _zoom;

        float clampedX = Math.Clamp(targetX, CAM_X_MIN + halfW, CAM_X_MAX - halfW);
        float clampedY = Math.Clamp(targetY, CAM_Y_MIN + halfH, CAM_Y_MAX - halfH);

        if (!_xBlocked && MathF.Abs(clampedX - targetX) > 0.1f) _xBlocked = true;
        if (!_yBlocked && MathF.Abs(clampedY - targetY) > 0.1f) _yBlocked = true;

        _camX = clampedX;
        _camY = clampedY;

        MainThread.BeginInvokeOnMainThread(_canvasView.InvalidateSurface);

        if (elapsed >= _flingDurMs || (_xBlocked && _yBlocked))
            StopInertia();
    }

    private void StopInertia()
    {
        _inertiaTimer?.Stop();
        _inertiaTimer?.Dispose();
        _inertiaTimer = null;
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  CAMERA HELPERS
    // ══════════════════════════════════════════════════════════════════════════
    private void ClampCamera()
    {
        float halfW = (_canvasView.CanvasSize.Width > 0
                        ? _canvasView.CanvasSize.Width : 1080f) / 2f / _zoom;
        float halfH = (_canvasView.CanvasSize.Height > 0
                        ? _canvasView.CanvasSize.Height : 1920f) / 2f / _zoom;
        _camX = Math.Clamp(_camX, CAM_X_MIN + halfW, CAM_X_MAX - halfW);
        _camY = Math.Clamp(_camY, CAM_Y_MIN + halfH, CAM_Y_MAX - halfH);
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  TAP → HIT TEST
    // ══════════════════════════════════════════════════════════════════════════
    private void OnTap(object sender, TappedEventArgs e)
    {
        if (_wasPanning || !_isMapLoaded || _sceneRoot == null) return;

        var pt = e.GetPosition(_canvasView);
        if (pt == null) return;

        float cW = _canvasView.CanvasSize.Width;
        float cH = _canvasView.CanvasSize.Height;
        if (cW <= 0 || cH <= 0) return;

        float den = cW / (float)_canvasView.Width;
        float pxX = (float)pt.Value.X * den;
        float pxY = (float)pt.Value.Y * den;

        float wx = (pxX - cW / 2f) / _zoom + _camX;
        float wy = _camY - (pxY - cH / 2f) / _zoom;

        string hit = HitTestImages(_sceneRoot.content, 0f, 0f, wx, wy);
        if (!string.IsNullOrEmpty(hit))
        {
            Debug.WriteLine($"[HIT] {hit}");
            BuildingTapped?.Invoke(this, hit);
        }
    }

    private string HitTestImages(ContentVO content, float pX, float pY, float wx, float wy)
    {
        if (content == null) return null;

        if (content.items != null)
            foreach (var civo in content.items.OrderByDescending(c => c.zIndex))
            {
                float cx = pX + civo.x;
                float cy = pY + civo.y;
                bool named = !string.IsNullOrEmpty(civo.itemIdentifier);

                if (civo.content?.images != null)
                    foreach (var img in civo.content.images)
                    {
                        if (!_imageCache.TryGetValue(img.imageName, out var bmp)) continue;
                        if (wx >= cx + img.x && wx <= cx + img.x + bmp.Width &&
                            wy >= cy + img.y && wy <= cy + img.y + bmp.Height)
                            if (named) return civo.itemIdentifier;
                    }

                string inner = HitTestImages(civo.content, cx, cy, wx, wy);
                if (inner != null) return named ? civo.itemIdentifier : inner;
            }

        if (content.images != null)
            foreach (var img in content.images
                         .Where(i => i.layerName == "buildings")
                         .OrderByDescending(i => i.zIndex))
            {
                string bid = GetBuildingId(img.imageName);
                if (bid == null) continue;
                if (!_imageCache.TryGetValue(img.imageName, out var bmp)) continue;
                if (wx >= pX + img.x && wx <= pX + img.x + bmp.Width &&
                    wy >= pY + img.y && wy <= pY + img.y + bmp.Height)
                    return bid;
            }

        return null;
    }

    private static string GetBuildingId(string name) =>
        name?.TrimStart('@') switch
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
            _ => null
        };

    // ══════════════════════════════════════════════════════════════════════════
    //  MAP LOADING
    // ══════════════════════════════════════════════════════════════════════════
    private void InitializeMap()
    {
        Task.Run(async () =>
        {
            try
            {
                string json = "";
                try { json = await LoadText("MainScene.dt"); } catch { }
                if (string.IsNullOrEmpty(json)) return;

                var data = JsonConvert.DeserializeObject<SceneRoot>(json);
                if (data?.composite?.content == null) return;

                var names = new HashSet<string>();
                CollectImageNames(data.composite.content, names);

                var folders = new[] { "", "floor/", "roads/", "banners/", "buildings/", "others/" };
                var exts = new[] { ".png", ".PNG", ".jpg", ".jpeg" };

                foreach (var raw in names)
                {
                    string key = raw.Trim();
                    if (_imageCache.ContainsKey(key)) continue;
                    string bare = key.TrimStart('@');

                    var variants = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                    {
                        key, bare, "@" + bare,
                        bare.Replace(" ", "_"), "@" + bare.Replace(" ", "_"),
                        bare.ToLower(), bare.Replace(" ", "_").ToLower(),
                    };

                    bool found = false;
                    foreach (var f in folders)
                    {
                        if (found) break;
                        foreach (var v in variants)
                        {
                            if (found) break;
                            foreach (var x in exts)
                                if (await TryLoad($"{f}{v}", key, x)) { found = true; break; }
                        }
                    }
                }

                _sceneRoot = data.composite;
                _isMapLoaded = true;
                MainThread.BeginInvokeOnMainThread(_canvasView.InvalidateSurface);
            }
            catch (Exception ex) { Debug.WriteLine($"[MapView] {ex.Message}"); }
        });
    }

    private void CollectImageNames(ContentVO c, HashSet<string> s)
    {
        if (c == null) return;
        c.images?.ForEach(i => { if (!string.IsNullOrEmpty(i.imageName)) s.Add(i.imageName); });
        c.items?.ForEach(ci => CollectImageNames(ci.content, s));
    }

    private async Task<bool> TryLoad(string file, string key, string ext)
    {
        try
        {
            using var s = await FileSystem.OpenAppPackageFileAsync(
                                $"Assets/images/maincity/{file}{ext}");
            var bmp = SKBitmap.Decode(s);
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

    // ══════════════════════════════════════════════════════════════════════════
    //  RENDERING
    // ══════════════════════════════════════════════════════════════════════════
    private void OnPaint(object sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear(new SKColor(18, 18, 20));
        if (!_isMapLoaded || _sceneRoot == null) return;

        float W = e.Info.Width;
        float H = e.Info.Height;

        _visMinX = _camX - W / 2f / _zoom;
        _visMaxX = _camX + W / 2f / _zoom;
        _visMinY = _camY - H / 2f / _zoom;
        _visMaxY = _camY + H / 2f / _zoom;

        canvas.Translate(W / 2f, H / 2f);
        canvas.Scale(_zoom, _zoom);
        canvas.Translate(-_camX, _camY);

        RenderContent(canvas, _sceneRoot.content, 0f, 0f);
    }

    private void RenderContent(SKCanvas canvas, ContentVO content, float pX, float pY)
    {
        if (content == null) return;

        var list = new List<(int z, object o)>();
        content.images?.ForEach(i => list.Add((i.zIndex, i)));
        content.items?.ForEach(c => list.Add((c.zIndex, c)));
        list.Sort((a, b) => a.z.CompareTo(b.z));

        foreach (var (_, obj) in list)
        {
            if (obj is SimpleImageVO sivo)
            {
                if (!_imageCache.TryGetValue(sivo.imageName, out var bmp)) continue;
                float sl = pX + sivo.x, sr = sl + bmp.Width;
                float sb = pY + sivo.y, st = sb + bmp.Height;
                if (sr < _visMinX || sl > _visMaxX || st < _visMinY || sb > _visMaxY) continue;
                DrawSprite(canvas, sivo, bmp);
            }
            else if (obj is CompositeItemVO civo)
            {
                canvas.Save();
                canvas.Translate(civo.x, -civo.y);
                RenderContent(canvas, civo.content, pX + civo.x, pY + civo.y);
                canvas.Restore();
            }
        }
    }

    private void DrawSprite(SKCanvas canvas, SimpleImageVO img, SKBitmap bmp)
    {
        float ox = img.originX;
        float oy = img.originY;
        float bw = bmp.Width;
        float bh = bmp.Height;

        canvas.Save();
        canvas.Translate(img.x + ox, -(img.y + oy));

        if (img.flipX || img.flipY)
            canvas.Scale(img.flipX ? -1f : 1f, img.flipY ? -1f : 1f);

        var dst = new SKRect(-ox, oy - bh, bw - ox, oy);

        if (img.layerName == "floor" || img.layerName == "roads")
        {
            float px = 0.6f / _zoom;
            dst = new SKRect(dst.Left - px, dst.Top - px, dst.Right + px, dst.Bottom + px);
        }

        canvas.DrawBitmap(bmp, dst);
        canvas.Restore();
    }
}