using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace GameProject02.Helpers;
using Path = System.IO.Path;
public enum PopupOperationType { Confirm, Sell, Move, Remove, Rent, Upgrade, Alert }

public static class PopupService
{
    // نخزن النوافذ المفتوحة في Stack للتعامل مع التداخل
    private static Stack<PopupContext> _popupStack = new();
    public enum AvatarPopupResult { Change, Remove, Cancel }
    private class PopupContext
    {
        public View Overlay { get; set; } = null!;
        public View PopupContainer { get; set; } = null!;
        public TaskCompletionSource<bool>? Tcs { get; set; }
        public bool IsSelection { get; set; }
        public object? SelectionTcs { get; set; }
        public Border? SelectionBorder { get; set; }
        public BoxView? SelectionOverlay { get; set; }
    }

    private static ContentPage? GetActivePage()
    {
        var page = Application.Current?.MainPage;
        if (page is NavigationPage nav) return nav.CurrentPage as ContentPage;
        return page as ContentPage;
    }

    private static (string positive, string negative) GetButtonTexts(PopupOperationType type) => type switch
    {
        PopupOperationType.Sell => ("نعم", "لا"),
        PopupOperationType.Move => ("نعم", "لا"),
        PopupOperationType.Remove => ("نعم", "لا"),
        PopupOperationType.Rent => ("نعم", "لا"),
        PopupOperationType.Upgrade => ("نعم", "لا"),
        PopupOperationType.Alert => ("نعم", ""),
        _ => ("نعم", "لا")
    };

    // =====================================================================
    // نوافذ التأكيد (مع دعم عدم إغلاق النافذة السفلية)
    // =====================================================================
    public static async Task<bool> ShowConfirmAsync(
        string title,
        string message,
        PopupOperationType operationType = PopupOperationType.Confirm,
        Color? positiveColor = null,
        Color? negativeColor = null,
        string? positiveImage = null,
        string? negativeImage = null,
        Func<Task>? onPositive = null,
        Func<Task>? onNegative = null,
        string? overridePositiveText = null,
        string? overrideNegativeText = null,
        View? customContent = null,
        bool keepUnderlying = false)
    {
        var tcs = new TaskCompletionSource<bool>();
        var activePage = GetActivePage();
        if (activePage == null) { tcs.TrySetResult(false); return false; }

        Grid? rootGrid = activePage.Content as Grid;
        View? originalContent = null;
        if (rootGrid == null)
        {
            originalContent = activePage.Content;
            rootGrid = new Grid { BackgroundColor = Colors.Transparent };
            rootGrid.Add(originalContent);
            activePage.Content = rootGrid;
        }

        try
        {
            double screenWidth = DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Density;
            double popupWidth = Math.Min(screenWidth * 0.85, 375);
            double topHeight = Math.Max(40, popupWidth * 0.12);
            double bottomHeight = Math.Max(35, popupWidth * 0.10);

            var overlay = new Grid
            {
                BackgroundColor = Color.FromArgb("#80000000"),
                IsVisible = false,
                Opacity = 0,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill
            };
            overlay.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(() => { }) });

            var popupContainer = new Grid
            {
                WidthRequest = popupWidth,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                BackgroundColor = Colors.Transparent,
                RowDefinitions = new RowDefinitionCollection
                {
                    new RowDefinition { Height = topHeight },
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = bottomHeight }
                }
            };

            var topImage = new Image { Source = "popup_top", Aspect = Aspect.Fill };
            Grid.SetRow(topImage, 0); popupContainer.Add(topImage);
            var middleImage = new Image { Source = "popup_middle", Aspect = Aspect.Fill };
            Grid.SetRow(middleImage, 1); popupContainer.Add(middleImage);
            var bottomImage = new Image { Source = "popup_bottom", Aspect = Aspect.Fill };
            Grid.SetRow(bottomImage, 2); popupContainer.Add(bottomImage);

            var contentGrid = new Grid { RowDefinitions = popupContainer.RowDefinitions };

            var titleLabel = new Label
            {
                Text = title,
                Style = (Style)Application.Current.Resources["PopupTitle"]
            };
            Grid.SetRow(titleLabel, 0);
            contentGrid.Add(titleLabel);

            var middleContentStack = new VerticalStackLayout
            {
                Spacing = 16,
                HorizontalOptions = LayoutOptions.Fill,
                WidthRequest = popupWidth - 40,
                VerticalOptions = LayoutOptions.Center,
                Padding = new Thickness(20, 10, 20, 0)
            };

            if (customContent != null)
            {
                middleContentStack.Children.Add(customContent);
            }
            else
            {
                var messageLabel = new Label
                {
                    Text = message,
                    Style = (Style)Application.Current.Resources["PopupMessage"]
                };
                middleContentStack.Children.Add(messageLabel);

                var buttonsRow = new HorizontalStackLayout
                {
                    Spacing = 20,
                    HorizontalOptions = LayoutOptions.Center,
                    Margin = new Thickness(0, 15, 0, 0)
                };

                var (defaultPositive, defaultNegative) = GetButtonTexts(operationType);
                string positiveText = overridePositiveText ?? defaultPositive;
                string negativeText = overrideNegativeText ?? defaultNegative;

                var confirmBtn = CreateButton(positiveText, positiveColor ?? Color.FromArgb("#000000"), positiveImage ?? "button_background.png", popupWidth * 0.30);
                var cancelBtn = CreateButton(negativeText, negativeColor ?? Color.FromArgb("#000000"), negativeImage ?? "button_background_no.png", popupWidth * 0.30);

                if (operationType != PopupOperationType.Alert && !string.IsNullOrEmpty(negativeText))
                {
                    buttonsRow.Children.Add(cancelBtn);
                }
                buttonsRow.Children.Insert(0, confirmBtn);
                middleContentStack.Children.Add(buttonsRow);
            }

            Grid.SetRow(middleContentStack, 1);
            contentGrid.Add(middleContentStack);
            popupContainer.Add(contentGrid);
            Grid.SetRowSpan(contentGrid, 3);

            overlay.Add(popupContainer);
            Grid.SetRow(overlay, 0); Grid.SetColumn(overlay, 0);
            Grid.SetRowSpan(overlay, rootGrid.RowDefinitions.Count > 0 ? rootGrid.RowDefinitions.Count : 1);
            Grid.SetColumnSpan(overlay, rootGrid.ColumnDefinitions.Count > 0 ? rootGrid.ColumnDefinitions.Count : 1);
            rootGrid.Add(overlay);

            // حفظ السياق في المكدس
            var context = new PopupContext
            {
                Overlay = overlay,
                PopupContainer = popupContainer,
                Tcs = tcs,
                IsSelection = false
            };
            _popupStack.Push(context);

            popupContainer.Scale = 0.95;
            popupContainer.Opacity = 0;
            overlay.IsVisible = true;
            await Task.WhenAll(
                overlay.FadeTo(1, 150, Easing.Linear),
                popupContainer.FadeTo(1, 200, Easing.SinOut),
                popupContainer.ScaleTo(1, 200, Easing.SinOut)
            );

            if (customContent == null)
            {
                var confirmBtn = (middleContentStack.Children[1] as HorizontalStackLayout)?.Children[0] as Border;
                var cancelBtn = (middleContentStack.Children[1] as HorizontalStackLayout)?.Children.Count > 1 ? (middleContentStack.Children[1] as HorizontalStackLayout)?.Children[1] as Border : null;

                if (confirmBtn != null)
                {
                    confirmBtn.GestureRecognizers.Add(new TapGestureRecognizer
                    {
                        Command = new Command(async () =>
                        {
                            await AnimateButton(confirmBtn);
                            await CloseTopPopup(context, rootGrid, true);
                            onPositive?.Invoke();
                        })
                    });
                }

                if (cancelBtn != null && operationType != PopupOperationType.Alert)
                {
                    cancelBtn.GestureRecognizers.Add(new TapGestureRecognizer
                    {
                        Command = new Command(async () =>
                        {
                            await AnimateButton(cancelBtn);
                            await CloseTopPopup(context, rootGrid, false);
                            onNegative?.Invoke();
                        })
                    });
                }
            }
            else
            {
                overlay.GestureRecognizers.Clear();
                overlay.GestureRecognizers.Add(new TapGestureRecognizer
                {
                    Command = new Command(async () =>
                    {
                        await CloseTopPopup(context, rootGrid, false);
                    })
                });
            }

            return await tcs.Task;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[PopupService Error] {ex.Message}");
            tcs.TrySetResult(false);
            return false;
        }
        finally
        {
            if (!keepUnderlying && originalContent != null && activePage.Content is Grid tempGrid && tempGrid.Children.Contains(originalContent))
                activePage.Content = originalContent;
        }
    }

    private static async Task CloseTopPopup(PopupContext context, Grid rootGrid, bool result)
    {
        try
        {
            await Task.WhenAll(
                context.Overlay.FadeTo(0, 150, Easing.Linear),
                context.PopupContainer.FadeTo(0, 200, Easing.SinIn),
                context.PopupContainer.ScaleTo(0.95, 200, Easing.SinIn)
            );
        }
        catch { }
        finally
        {
            SafeClose(context.Overlay, rootGrid);
            SafeClose(context.PopupContainer, rootGrid);
            _popupStack.Pop();
            context.Tcs?.TrySetResult(result);
        }
    }

    public static async Task ShowAlertAsync(string title, string message, Color? buttonColor = null, string? buttonImage = null)
    {
        await ShowConfirmAsync(title, message, PopupOperationType.Alert, buttonColor, null, buttonImage, null, null, null);
    }

    // =====================================================================
    // نوافذ الاختيار (Selection Popups)
    // =====================================================================
    public static async Task<T?> ShowSelectionPopupWithCustomView<T>(
        string title,
        IEnumerable<T> items,
        Func<T, View> createItemView,
        string cancelButtonText = "✘ إلغاء")
    {
        return await ShowSelectionPopupInternal(title, items, createItemView, null, cancelButtonText);
    }

    public static async Task<T?> ShowSelectionPopupWithCustomView<T>(
        string title,
        IEnumerable<T> items,
        Func<T, View> createItemView,
        Func<T, Task>? onItemSelected,
        string cancelButtonText = "✘ إلغاء")
    {
        return await ShowSelectionPopupInternal(title, items, createItemView, onItemSelected, cancelButtonText);
    }

    private static async Task<T?> ShowSelectionPopupInternal<T>(
        string title,
        IEnumerable<T> items,
        Func<T, View> createItemView,
        Func<T, Task>? onItemSelected,
        string cancelButtonText)
    {
        var tcs = new TaskCompletionSource<T?>();
        var activePage = GetActivePage();
        if (activePage == null) { tcs.TrySetResult(default); return default; }

        Grid? rootGrid = activePage.Content as Grid;
        View? originalContent = null;
        if (rootGrid == null)
        {
            originalContent = activePage.Content;
            rootGrid = new Grid { BackgroundColor = Colors.Transparent };
            rootGrid.Add(originalContent);
            activePage.Content = rootGrid;
        }

        try
        {
            double sw = EstateUIConstants.ScreenWidth;
            double sh = EstateUIConstants.ScreenHeight;
            double popupWidth = Math.Min(sw * 0.85, 375);
            double maxPopupHeight = Math.Min(sh * 0.55, 425);
            double topHeight = Math.Max(40, popupWidth * 0.12);
            double bottomHeight = Math.Max(35, popupWidth * 0.10);
            double scrollMaxHeight = maxPopupHeight - topHeight - bottomHeight - 60;

            var overlay = new BoxView
            {
                Style = (Style)Application.Current.Resources["PopupOverlay"],
                IsVisible = false,
                Opacity = 0
            };
            // منع النقر على الخلفية من التأثير على الصفحة
            overlay.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(() => { }) });

            var popupBorder = new Border
            {
                Style = (Style)Application.Current.Resources["PopupContainer"],
                WidthRequest = popupWidth,
                MaximumHeightRequest = maxPopupHeight,
                IsVisible = false,
                Opacity = 0,
                Scale = 0.8
            };

            var popupContainer = new Grid
            {
                RowDefinitions = new RowDefinitionCollection
                {
                    new RowDefinition { Height = topHeight },
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = bottomHeight }
                }
            };

            // Top
            var topGrid = new Grid();
            topGrid.Add(new Image { Source = "popup_top", Aspect = Aspect.Fill });
            topGrid.Add(new Label
            {
                Text = title,
                Style = (Style)Application.Current.Resources["PopupTitle"]
            });
            popupContainer.Add(topGrid, 0, 0);

            // Middle
            var middleGrid = new Grid
            {
                RowDefinitions = new RowDefinitionCollection
                {
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto }
                }
            };
            var middleBg = new Image { Source = "popup_middle", Aspect = Aspect.Fill };
            Grid.SetRowSpan(middleBg, 2);
            middleGrid.Add(middleBg);

            var itemsStack = new VerticalStackLayout { Spacing = 0 };
            foreach (var item in items)
            {
                var itemView = createItemView(item);

                if (onItemSelected == null)
                {
                    var tap = new TapGestureRecognizer();
                    tap.Tapped += async (s, e) =>
                    {
                        await CloseSelectionPopup(popupBorder, overlay, rootGrid, tcs, item);
                    };
                    itemView.GestureRecognizers.Add(tap);
                }

                itemsStack.Children.Add(itemView);
            }

            var scrollView = new ScrollView
            {
                Content = itemsStack,
                Padding = new Thickness(30, 5, 30, 0),
                MaximumHeightRequest = scrollMaxHeight,
                VerticalScrollBarVisibility = ScrollBarVisibility.Never
            };
            middleGrid.Add(scrollView, 0, 0);

            // Cancel Button
            var cancelButton = new Border
            {
                Style = (Style)Application.Current.Resources["PopupCancelButton"],
                Margin = new Thickness(0, 2, 0, 0)
            };
            var cancelGrid = new Grid();
            cancelGrid.Add(new Image { Source = "button_background_no.png", Aspect = Aspect.Fill });
            cancelGrid.Add(new Label
            {
                Text = cancelButtonText,
                Style = (Style)Application.Current.Resources["PopupButtonText"]
            });
            cancelButton.Content = cancelGrid;
            cancelButton.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(async () =>
                {
                    await AnimateButton(cancelButton);
                    await CloseSelectionPopup(popupBorder, overlay, rootGrid, tcs, default);
                })
            });
            middleGrid.Add(cancelButton, 0, 1);

            popupContainer.Add(middleGrid, 0, 1);

            // Bottom
            var bottomGrid = new Grid();
            bottomGrid.Add(new Image { Source = "popup_bottom", Aspect = Aspect.Fill });
            popupContainer.Add(bottomGrid, 0, 2);

            popupBorder.Content = popupContainer;

            rootGrid.Add(overlay);
            Grid.SetRowSpan(overlay, rootGrid.RowDefinitions.Count > 0 ? rootGrid.RowDefinitions.Count : 1);
            Grid.SetColumnSpan(overlay, rootGrid.ColumnDefinitions.Count > 0 ? rootGrid.ColumnDefinitions.Count : 1);
            rootGrid.Add(popupBorder);
            Grid.SetRowSpan(popupBorder, rootGrid.RowDefinitions.Count > 0 ? rootGrid.RowDefinitions.Count : 1);
            Grid.SetColumnSpan(popupBorder, rootGrid.ColumnDefinitions.Count > 0 ? rootGrid.ColumnDefinitions.Count : 1);

            // حفظ في المكدس
            var context = new PopupContext
            {
                Overlay = overlay,
                PopupContainer = popupBorder,
                Tcs = null,
                IsSelection = true,
                SelectionTcs = tcs,
                SelectionBorder = popupBorder,
                SelectionOverlay = overlay
            };
            _popupStack.Push(context);

            overlay.IsVisible = true;
            popupBorder.IsVisible = true;
            await Task.WhenAll(
                overlay.FadeTo(0.7, 200),
                popupBorder.FadeTo(1, 200),
                popupBorder.ScaleTo(1, 200, Easing.CubicOut)
            );

            return await tcs.Task;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[PopupService Selection Error] {ex.Message}");
            tcs.TrySetResult(default);
            return default;
        }
        finally
        {
            if (originalContent != null && activePage.Content is Grid tempGrid && tempGrid.Children.Contains(originalContent))
                activePage.Content = originalContent;
        }
    }

    public static async Task CompleteSelection<T>(T result)
    {
        if (_popupStack.Count > 0)
        {
            var context = _popupStack.Peek();
            if (context.IsSelection && context.SelectionTcs is TaskCompletionSource<T> tcs &&
                context.SelectionBorder != null && context.SelectionOverlay != null &&
                context.SelectionBorder.Parent is Grid rootGrid)
            {
                await CloseSelectionPopup(context.SelectionBorder, context.SelectionOverlay, rootGrid, tcs, result);
                _popupStack.Pop();
            }
        }
    }

    private static async Task CloseSelectionPopup<T>(Border popupBorder, BoxView overlay, Grid rootGrid, TaskCompletionSource<T?> tcs, T? result)
    {
        try
        {
            await Task.WhenAll(
                popupBorder.FadeTo(0, 200),
                popupBorder.ScaleTo(0.8, 200, Easing.CubicIn),
                overlay.FadeTo(0, 200)
            );
        }
        catch { }
        finally
        {
            SafeClose(overlay, rootGrid);
            SafeClose(popupBorder, rootGrid);
            tcs.TrySetResult(result);
        }
    }

    // =====================================================================
    // دوال مساعدة
    // =====================================================================
    private static void SafeClose(View overlay, Grid? rootGrid)
    {
        try
        {
            if (rootGrid != null && rootGrid.Children.Contains(overlay))
                rootGrid.Children.Remove(overlay);
            else if (overlay.Parent is Grid parentGrid)
                parentGrid.Children.Remove(overlay);
        }
        catch { }
    }

    private static Border CreateButton(string text, Color color, string imageSource, double width)
    {
        var border = new Border
        {
            Stroke = Colors.Transparent,
            BackgroundColor = Colors.Transparent,
            HeightRequest = 45,
            WidthRequest = width,
            Padding = 0
        };
        var grid = new Grid();
        grid.Add(new Image { Source = imageSource, Aspect = Aspect.Fill });
        grid.Add(new Label
        {
            Text = text,
            Style = (Style)Application.Current.Resources["PopupButtonText"],
            TextColor = color
        });
        border.Content = grid;
        return border;
    }

    private static async Task AnimateButton(View button)
    {
        try
        {
            await button.ScaleTo(0.95, 80, Easing.CubicOut);
            await button.ScaleTo(1, 80, Easing.CubicIn);
        }
        catch { }
    }

    public static async Task CloseAllPopups()
    {
        // نجمع كل النوافذ المفتوحة حالياً
        var popupsToClose = _popupStack.ToArray();
        _popupStack.Clear();

        foreach (var context in popupsToClose)
        {
            try
            {
                if (context.IsSelection && context.SelectionTcs != null)
                {
                    var tcs = context.SelectionTcs as dynamic;
                    tcs?.TrySetResult(null);
                }
                else
                {
                    context.Tcs?.TrySetResult(false);
                }

                // إغلاق بصري
                await Task.WhenAll(
                    context.Overlay.FadeTo(0, 150, Easing.Linear),
                    context.PopupContainer.FadeTo(0, 150, Easing.Linear),
                    context.PopupContainer.ScaleTo(0.8, 150, Easing.CubicIn)
                );

                // إزالة من الواجهة
                if (context.Overlay.Parent is Grid parentGrid)
                {
                    parentGrid.Children.Remove(context.Overlay);
                    parentGrid.Children.Remove(context.PopupContainer);
                }
            }
            catch { }
        }
    }
    // =====================================================================
    // نافذة تغيير صورة اللاعب
    // =====================================================================
    public static async Task<AvatarPopupResult> ShowAvatarPopupAsync(string currentAvatarPath)
    {
        var tcs = new TaskCompletionSource<AvatarPopupResult>();
        var activePage = GetActivePage();
        if (activePage == null)
        {
            tcs.TrySetResult(AvatarPopupResult.Cancel);
            return AvatarPopupResult.Cancel;
        }

        Grid? rootGrid = activePage.Content as Grid;
        View? originalContent = null;
        if (rootGrid == null)
        {
            originalContent = activePage.Content;
            rootGrid = new Grid { BackgroundColor = Colors.Transparent };
            rootGrid.Add(originalContent);
            activePage.Content = rootGrid;
        }

        try
        {
            double screenWidth = DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Density;
            double popupWidth = Math.Min(screenWidth * 0.85, 375);
            double topHeight = Math.Max(40, popupWidth * 0.12);
            double bottomHeight = Math.Max(35, popupWidth * 0.10);

            // الخلفية المعتمة
            var overlay = new Grid
            {
                BackgroundColor = Color.FromArgb("#80000000"),
                IsVisible = false,
                Opacity = 0,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill
            };
            overlay.GestureRecognizers.Add(
                new TapGestureRecognizer { Command = new Command(() => { }) });

            // حاوية البوب
            var popupContainer = new Grid
            {
                WidthRequest = popupWidth,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                BackgroundColor = Colors.Transparent,
                RowDefinitions = new RowDefinitionCollection
                {
                    new RowDefinition { Height = topHeight },
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = bottomHeight }
                }
            };

            // خلفيات البوب
            var topImg = new Image { Source = "popup_top", Aspect = Aspect.Fill };
            var middleImg = new Image { Source = "popup_middle", Aspect = Aspect.Fill };
            var bottomImg = new Image { Source = "popup_bottom", Aspect = Aspect.Fill };
            Grid.SetRow(topImg, 0); popupContainer.Add(topImg);
            Grid.SetRow(middleImg, 1); popupContainer.Add(middleImg);
            Grid.SetRow(bottomImg, 2); popupContainer.Add(bottomImg);

            // طبقة المحتوى
            var contentGrid = new Grid { RowDefinitions = popupContainer.RowDefinitions };

            // العنوان
            var titleLabel = new Label
            {
                Text = "تغيير الصورة",
                Style = (Style)Application.Current.Resources["PopupTitle"]
            };
            Grid.SetRow(titleLabel, 0);
            contentGrid.Add(titleLabel);

            // الجزء الأوسط
            var middleStack = new VerticalStackLayout
            {
                Spacing = 20,
                HorizontalOptions = LayoutOptions.Center,
                Padding = new Thickness(20, 15, 20, 10)
            };

            // صورة اللاعب داخل الكنار
            double avatarContainerSz = popupWidth * 0.30;
            double avatarInnerSz = avatarContainerSz * 0.85;

            var avatarGrid = new Grid
            {
                WidthRequest = avatarContainerSz,
                HeightRequest = avatarContainerSz,
                HorizontalOptions = LayoutOptions.Center
            };

            avatarGrid.Add(new Image
            {
                Source = "avatar_frame.png",
                Aspect = Aspect.Fill,
                WidthRequest = avatarContainerSz,
                HeightRequest = avatarContainerSz,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            });

            ImageSource avatarSource;
            if (!string.IsNullOrEmpty(currentAvatarPath)
                && Path.IsPathRooted(currentAvatarPath)
                && File.Exists(currentAvatarPath))
                avatarSource = ImageSource.FromFile(currentAvatarPath);
            else
                avatarSource = "avatar_player.png";

            avatarGrid.Add(new Frame
            {
                WidthRequest = avatarInnerSz,
                HeightRequest = avatarInnerSz,
                CornerRadius = 0,
                Padding = 0,
                HasShadow = false,
                IsClippedToBounds = true,
                BackgroundColor = Colors.Transparent,
                BorderColor = Colors.Transparent,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                Content = new Image
                {
                    Source = avatarSource,
                    Aspect = Aspect.AspectFill,
                    WidthRequest = avatarInnerSz,
                    HeightRequest = avatarInnerSz
                }
            });

            middleStack.Children.Add(avatarGrid);

            // الأزرار الثلاثة
            double btnWidth = popupWidth * 0.26;

            var buttonsRow = new HorizontalStackLayout
            {
                Spacing = 12,
                HorizontalOptions = LayoutOptions.Center
            };

            var removeBtn = CreateButton("ازالة", Color.FromArgb("#cc0000"), "button_background_no.png", btnWidth);
            var cancelBtn = CreateButton("الغاء", Color.FromArgb("#888888"), "button_background_no.png", btnWidth);
            var changeBtn = CreateButton("تغيير", Color.FromArgb("#000000"), "button_background.png", btnWidth);

            removeBtn.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(async () =>
                {
                    await AnimateButton(removeBtn);
                    await CloseAvatarPopup(overlay, popupContainer, rootGrid);
                    tcs.TrySetResult(AvatarPopupResult.Remove);
                })
            });

            cancelBtn.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(async () =>
                {
                    await AnimateButton(cancelBtn);
                    await CloseAvatarPopup(overlay, popupContainer, rootGrid);
                    tcs.TrySetResult(AvatarPopupResult.Cancel);
                })
            });

            changeBtn.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(async () =>
                {
                    await AnimateButton(changeBtn);
                    await CloseAvatarPopup(overlay, popupContainer, rootGrid);
                    tcs.TrySetResult(AvatarPopupResult.Change);
                })
            });

            buttonsRow.Children.Add(removeBtn);
            buttonsRow.Children.Add(cancelBtn);
            buttonsRow.Children.Add(changeBtn);
            middleStack.Children.Add(buttonsRow);

            Grid.SetRow(middleStack, 1);
            contentGrid.Add(middleStack);

            popupContainer.Add(contentGrid);
            Grid.SetRowSpan(contentGrid, 3);

            overlay.Add(popupContainer);
            Grid.SetRowSpan(overlay,
                rootGrid.RowDefinitions.Count > 0 ? rootGrid.RowDefinitions.Count : 1);
            Grid.SetColumnSpan(overlay,
                rootGrid.ColumnDefinitions.Count > 0 ? rootGrid.ColumnDefinitions.Count : 1);
            rootGrid.Add(overlay);

            popupContainer.Scale = 0.95;
            popupContainer.Opacity = 0;
            overlay.IsVisible = true;

            await Task.WhenAll(
                overlay.FadeTo(1, 150, Easing.Linear),
                popupContainer.FadeTo(1, 200, Easing.SinOut),
                popupContainer.ScaleTo(1, 200, Easing.SinOut)
            );

            return await tcs.Task;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AvatarPopup Error] {ex.Message}");
            tcs.TrySetResult(AvatarPopupResult.Cancel);
            return AvatarPopupResult.Cancel;
        }
        finally
        {
            if (originalContent != null
                && activePage.Content is Grid tempGrid
                && tempGrid.Children.Contains(originalContent))
                activePage.Content = originalContent;
        }
    }

    private static async Task CloseAvatarPopup(View overlay, View popupContainer, Grid rootGrid)
    {
        try
        {
            await Task.WhenAll(
                overlay.FadeTo(0, 150, Easing.Linear),
                popupContainer.FadeTo(0, 200, Easing.SinIn),
                popupContainer.ScaleTo(0.95, 200, Easing.SinIn)
            );
        }
        catch { }
        finally
        {
            SafeClose(overlay, rootGrid);
        }
    }
}