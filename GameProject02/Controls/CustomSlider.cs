using Microsoft.Maui.Controls;

namespace GameProject02.Controls;

public class CustomSlider : Slider
{
    // خاصية صورة المسار الأدنى (الممتلئ)
    public static readonly BindableProperty MinimumTrackImageProperty =
        BindableProperty.Create(nameof(MinimumTrackImage), typeof(ImageSource), typeof(CustomSlider));

    public ImageSource MinimumTrackImage
    {
        get => (ImageSource)GetValue(MinimumTrackImageProperty);
        set => SetValue(MinimumTrackImageProperty, value);
    }

    // خاصية صورة المسار الأعلى (الفارغ)
    public static readonly BindableProperty MaximumTrackImageProperty =
        BindableProperty.Create(nameof(MaximumTrackImage), typeof(ImageSource), typeof(CustomSlider));

    public ImageSource MaximumTrackImage
    {
        get => (ImageSource)GetValue(MaximumTrackImageProperty);
        set => SetValue(MaximumTrackImageProperty, value);
    }

    // خاصية صورة المقبض (Thumb)
    public static readonly BindableProperty ThumbImageSourceProperty =
        BindableProperty.Create(nameof(ThumbImageSource), typeof(ImageSource), typeof(CustomSlider));

    public ImageSource ThumbImageSource
    {
        get => (ImageSource)GetValue(ThumbImageSourceProperty);
        set => SetValue(ThumbImageSourceProperty, value);
    }

    // خاصية حجم المقبض
    public static readonly BindableProperty ThumbSizeProperty =
        BindableProperty.Create(nameof(ThumbSize), typeof(double), typeof(CustomSlider), 40.0);

    public double ThumbSize
    {
        get => (double)GetValue(ThumbSizeProperty);
        set => SetValue(ThumbSizeProperty, value);
    }
}