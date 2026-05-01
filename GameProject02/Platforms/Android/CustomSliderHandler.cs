using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Widget;
using GameProject02.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using System;
using System.Threading.Tasks;

namespace GameProject02.Platforms.Android;

public class CustomSliderHandler : SliderHandler
{
    private CustomTrackDrawable? _trackDrawable;
    private ImageSource? _lastThumbSource;
    private bool _isUpdatingFromCode;

    public static new IPropertyMapper<CustomSlider, CustomSliderHandler> PropertyMapper =>
        new PropertyMapper<CustomSlider, CustomSliderHandler>(SliderHandler.Mapper)
        {
            [nameof(CustomSlider.Value)] = MapValue,
            [nameof(CustomSlider.MinimumTrackImage)] = (h, v) => h.UpdateTrackImagesAsync(),
            [nameof(CustomSlider.MaximumTrackImage)] = (h, v) => h.UpdateTrackImagesAsync(),
            [nameof(CustomSlider.ThumbSize)] = MapThumbSize
        };

    public CustomSliderHandler() : base(PropertyMapper)
    {
    }

    private static void MapValue(CustomSliderHandler handler, CustomSlider slider)
    {
        if (handler.PlatformView == null)
            return;

        int max = Math.Max(1, (int)Math.Round(slider.Maximum - slider.Minimum));
        int progress = (int)Math.Round(slider.Value - slider.Minimum);
        progress = Math.Clamp(progress, 0, max);

        if (handler.PlatformView.Progress != progress)
        {
            handler._isUpdatingFromCode = true;
            handler.PlatformView.Progress = progress;
            handler.UpdateTrackProgress();
            handler._isUpdatingFromCode = false;
        }
    }

    private static async void MapThumbSize(CustomSliderHandler handler, CustomSlider slider)
    {
        await handler.UpdateThumbSizeAsync(slider);
    }

    protected override void ConnectHandler(SeekBar platformView)
    {
        base.ConnectHandler(platformView);

        if (platformView.LayoutDirection == global::Android.Views.LayoutDirection.Rtl)
        {
            platformView.ScaleX = -1;
        }

        platformView.SplitTrack = false;
        platformView.ThumbOffset = 0;
        platformView.SetPadding(35, 0, 35, 0);

        platformView.ProgressChanged += OnProgressChanged;
        platformView.Touch += OnSeekBarTouch;

        _ = UpdateTrackImagesAsync();
    }

    private async Task UpdateThumbSizeAsync(CustomSlider slider)
    {
        if (_lastThumbSource == null)
            _lastThumbSource = slider.ThumbImageSource;

        if (_lastThumbSource == null || PlatformView == null)
            return;

        try
        {
            var context = PlatformView.Context;
            var thumbBitmap = await LoadBitmapAsync(context, _lastThumbSource);
            if (thumbBitmap == null)
                return;

            int size = Math.Max((int)slider.ThumbSize, 25);
            var bmp = Bitmap.CreateScaledBitmap(thumbBitmap, size, size, true);

            Bitmap finalBitmap = bmp;

            if (PlatformView.LayoutDirection == global::Android.Views.LayoutDirection.Rtl)
            {
                var matrix = new Matrix();
                matrix.PreScale(-1, 1);
                finalBitmap = Bitmap.CreateBitmap(
                    bmp,
                    0,
                    0,
                    bmp.Width,
                    bmp.Height,
                    matrix,
                    true);
            }

            var drawable = new BitmapDrawable(context.Resources, finalBitmap);
            drawable.SetBounds(0, 0, size, size);

            PlatformView.SetThumb(drawable);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CustomSliderHandler] Thumb update error: {ex.Message}");
        }
    }

    private void OnSeekBarTouch(object? sender, global::Android.Views.View.TouchEventArgs e)
    {
        var seekBar = sender as SeekBar;
        if (seekBar == null || VirtualView is not CustomSlider slider)
            return;

        // مهم:
        // نتدخل فقط عند أول ضغطة لكي يقفز السلايدر للمكان المضغوط.
        // بعد ذلك نترك Android يتعامل مع السحب بشكل طبيعي لتكون الحركة ناعمة.
        if (e.Event.Action == global::Android.Views.MotionEventActions.Down)
        {
            JumpToTouchPosition(seekBar, slider, e.Event);
            // لا نستهلك الحدث بالكامل، حتى يكمل SeekBar السحب بسلاسة
            e.Handled = false;
            return;
        }

        e.Handled = false;
    }

    private void JumpToTouchPosition(SeekBar seekBar, CustomSlider slider, global::Android.Views.MotionEvent motionEvent)
    {
        float x = motionEvent.GetX();

        float usableWidth = seekBar.Width - seekBar.PaddingLeft - seekBar.PaddingRight;
        if (usableWidth <= 0)
            return;

        float percent = (x - seekBar.PaddingLeft) / usableWidth;
        percent = Math.Clamp(percent, 0f, 1f);

        if (seekBar.LayoutDirection == global::Android.Views.LayoutDirection.Rtl)
        {
            percent = 1f - percent;
        }

        int max = Math.Max(1, seekBar.Max);
        int newProgress = (int)Math.Round(percent * max);
        newProgress = Math.Clamp(newProgress, 0, max);

        _isUpdatingFromCode = true;
        seekBar.Progress = newProgress;
        slider.Value = slider.Minimum + newProgress;
        UpdateTrackProgress();
        _isUpdatingFromCode = false;
    }

    private void OnProgressChanged(object? sender, SeekBar.ProgressChangedEventArgs e)
    {
        if (VirtualView is not CustomSlider slider)
            return;

        if (!_isUpdatingFromCode)
        {
            double newValue = slider.Minimum + e.Progress;

            if (Math.Abs(slider.Value - newValue) > 0.001)
            {
                slider.Value = newValue;
            }
        }

        UpdateTrackProgress();
    }

    private void UpdateTrackProgress()
    {
        if (_trackDrawable == null || PlatformView == null)
            return;

        if (PlatformView.Max <= 0)
            return;

        float progress = PlatformView.Progress / (float)PlatformView.Max;

        if (Math.Abs(_trackDrawable.Progress - progress) > 0.0001f)
        {
            _trackDrawable.Progress = progress;
            PlatformView.Invalidate();
        }
    }

    private async Task UpdateTrackImagesAsync()
    {
        if (VirtualView is not CustomSlider slider || PlatformView == null)
            return;

        try
        {
            var context = PlatformView.Context;

            Bitmap? minBitmap = null;
            Bitmap? maxBitmap = null;

            if (slider.MinimumTrackImage != null)
                minBitmap = await LoadBitmapAsync(context, slider.MinimumTrackImage);

            if (slider.MaximumTrackImage != null)
                maxBitmap = await LoadBitmapAsync(context, slider.MaximumTrackImage);

            if (slider.ThumbImageSource != null)
            {
                _lastThumbSource = slider.ThumbImageSource;
                await UpdateThumbSizeAsync(slider);
            }

            PlatformView.Min = 0;
            PlatformView.Max = Math.Max(1, (int)Math.Round(slider.Maximum - slider.Minimum));
            PlatformView.Progress = (int)Math.Round(
                Math.Clamp(slider.Value - slider.Minimum, 0, PlatformView.Max));

            _trackDrawable = new CustomTrackDrawable(minBitmap, maxBitmap);
            PlatformView.ProgressDrawable = _trackDrawable;

            UpdateTrackProgress();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CustomSliderHandler] Error: {ex.Message}");
        }
    }

    private async Task<Bitmap?> LoadBitmapAsync(Context context, ImageSource source)
    {
        try
        {
            var provider = MauiContext?.Services?.GetRequiredService<IImageSourceServiceProvider>();
            if (provider == null)
                return null;

            var service = provider.GetRequiredImageSourceService(source);
            var result = await service.GetDrawableAsync(source, context);

            return (result?.Value as BitmapDrawable)?.Bitmap;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CustomSliderHandler] Load failed: {ex.Message}");
            return null;
        }
    }

    protected override void DisconnectHandler(SeekBar platformView)
    {
        platformView.Touch -= OnSeekBarTouch;
        platformView.ProgressChanged -= OnProgressChanged;
        base.DisconnectHandler(platformView);
    }

    private class CustomTrackDrawable : Drawable
    {
        private readonly Bitmap? _progressBitmap;
        private readonly Bitmap? _backgroundBitmap;
        private float _progress;

        public float Progress
        {
            get => _progress;
            set => _progress = Math.Clamp(value, 0f, 1f);
        }

        public CustomTrackDrawable(Bitmap? progressBitmap, Bitmap? backgroundBitmap)
        {
            _progressBitmap = progressBitmap;
            _backgroundBitmap = backgroundBitmap;
        }

        public override void Draw(Canvas canvas)
        {
            var b = Bounds;

            int width = b.Right - b.Left;
            int height = b.Bottom - b.Top;

            if (width <= 0 || height <= 0)
                return;

            if (_backgroundBitmap != null)
            {
                var bgSrc = new global::Android.Graphics.Rect(
                    0, 0, _backgroundBitmap.Width, _backgroundBitmap.Height);

                var bgDst = new global::Android.Graphics.Rect(
                    b.Left, b.Top, b.Right, b.Bottom);

                canvas.DrawBitmap(_backgroundBitmap, bgSrc, bgDst, null);
            }

            if (_progressBitmap != null && _progress > 0f)
            {
                int progressWidth = (int)(width * _progress);

                int insetX = 7;
                int insetY = 7;

                if (progressWidth > insetX * 2 && height > insetY * 2)
                {
                    var progressSrc = new global::Android.Graphics.Rect(
                        0, 0, _progressBitmap.Width, _progressBitmap.Height);

                    var progressDst = new global::Android.Graphics.Rect(
                        b.Left + insetX,
                        b.Top + insetY,
                        b.Left + progressWidth - insetX,
                        b.Bottom - insetY);

                    canvas.DrawBitmap(_progressBitmap, progressSrc, progressDst, null);
                }
            }
        }

        public override void SetAlpha(int alpha) { }

        public override void SetColorFilter(ColorFilter? colorFilter) { }

        public override int Opacity => (int)Format.Translucent;
    }
}