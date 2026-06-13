using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SkiaSharp;
using GameProject02.Models;

namespace GameProject02.Helpers
{
    public static class SpriteCropHelper
    {
        public static async Task<SKBitmap> CropSpriteAsync(SKBitmap sourceSheet, string spriteName, Dictionary<string, SpriteBounds> boundsDict)
        {
            return await Task.Run(() =>
            {
                try
                {
                    if (sourceSheet == null)
                    {
                        System.Diagnostics.Debug.WriteLine($"❌ Source bitmap is null for {spriteName}");
                        return null;
                    }
                    if (!boundsDict.TryGetValue(spriteName, out var bounds))
                    {
                        System.Diagnostics.Debug.WriteLine($"⚠️ لم يتم العثور على bounds للصورة: {spriteName}");
                        return null;
                    }
                    if (bounds.X < 0 || bounds.Y < 0 ||
                        bounds.X + bounds.Width > sourceSheet.Width ||
                        bounds.Y + bounds.Height > sourceSheet.Height)
                    {
                        System.Diagnostics.Debug.WriteLine($"❌ Bounds out of range for {spriteName}");
                        return null;
                    }
                    var cropped = new SKBitmap(bounds.Width, bounds.Height);
                    using (var canvas = new SKCanvas(cropped))
                    {
                        canvas.DrawBitmap(sourceSheet,
                            new SKRect(bounds.X, bounds.Y, bounds.X + bounds.Width, bounds.Y + bounds.Height),
                            new SKRect(0, 0, bounds.Width, bounds.Height));
                    }
                    System.Diagnostics.Debug.WriteLine($"✅ تم قص {spriteName} بنجاح");
                    return cropped;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"🔥 Exception cropping {spriteName}: {ex.Message}");
                    return null;
                }
            });
        }
    }
}