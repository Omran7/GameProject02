using GameProject02.Helpers;
using GameProject02.Models;
using GameProject02.Services;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using System;
using System.Threading.Tasks;

namespace GameProject02.Views;

public partial class WorkDetailsPage : ContentPage
{
    private readonly int _categoryId;
    private readonly string _categoryName;
    private PlayerAccount _player;
    private bool _hasJobInThisCategory;
    private bool _hasJobInOtherCategory;
    private string _otherCategoryName;
    private bool _isCheckingWork;
    private bool _isTimerRunning;

    private Label _titleLabel;
    private VerticalStackLayout _inactiveContent;
    private VerticalStackLayout _activeContent;
    private Label _daysWorkedValueLabel;
    private Label _timerValueLabel;
    private Border _startButton;
    private Border _cancelButton;
    private Border _collectButton;
    private Grid _buttonsGrid;
    private VerticalStackLayout _activeButtonsStack;

    private Label _nextJobNameLabel;
    private Label _reqDaysNumberLabel;
    private Border _promoteButton;

    public WorkDetailsPage(int categoryId, string categoryName)
    {
        InitializeComponent();
        _categoryId = categoryId;
        _categoryName = categoryName;
        LoadData();
        StartWorkCheckTimer();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _player = AccountService.GetCurrentPlayer();
        LoadData();
        SetupFooter();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _isCheckingWork = false;
        _isTimerRunning = false;
    }

    private void SetupFooter()
    {
        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = GridLength.Auto }
            },
            Padding = new Thickness(25, 0)
        };

        var backButton = PageFooter.CreateFooterButton(
            text: "رجوع",
            tappedHandler: OnBackClicked,
            buttonImageSource: "footer_button_back.png"
        );

        grid.Add(backButton, 1, 0);
        PageFooter.SetContent(grid);
    }

    private void LoadData()
    {
        try
        {
            _player = AccountService.GetCurrentPlayer();
            if (_player == null) return;

            long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            long startTime = _player.WorkObject.JobStartTimeMilli;
            bool hasValidStartTime = startTime > 0 && startTime <= now;

            _hasJobInThisCategory = (_player.WorkObject.WorkType == _categoryId &&
                                     _player.WorkObject.JobLevel >= 0 &&
                                     hasValidStartTime);

            _hasJobInOtherCategory = (!_hasJobInThisCategory &&
                                      _player.WorkObject.WorkType >= 0 &&
                                      _player.WorkObject.JobLevel >= 0 &&
                                      hasValidStartTime);

            if (_hasJobInOtherCategory)
            {
                var otherCategory = WorkOfficeService.GetCategoryById(_player.WorkObject.WorkType);
                _otherCategoryName = otherCategory?.Name ?? "أخرى";
            }

            BuildMainCardContent();
            BuildNextJobCardContent();
            NextJobCard.IsVisible = _hasJobInThisCategory;
        }
        catch (Exception ex)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
                await DisplayAlert("خطأ في تحميل البيانات", ex.Message, "موافق"));
        }
    }

    private void BuildMainCardContent()
    {
        try
        {
            var baseJob = WorkOfficeService.GetJob(_categoryId, 0);
            if (baseJob == null) return;

            object currentJobObj = null;
            if (_hasJobInThisCategory)
                currentJobObj = WorkOfficeService.GetJob(_categoryId, _player.WorkObject.JobLevel);

            string jobName = GetPropertyValue(currentJobObj, "Name") ?? GetPropertyValue(baseJob, "Name") ?? "";
            int salary = GetIntProperty(currentJobObj, "SalaryGold") ?? GetIntProperty(baseJob, "SalaryGold") ?? 0;
            int exp = GetIntProperty(currentJobObj, "ExperienceReward") ?? GetIntProperty(baseJob, "ExperienceReward") ?? 0;
            int reqStr = GetIntProperty(baseJob, "RequiredStrength") ?? 0;
            int reqDef = GetIntProperty(baseJob, "RequiredDefense") ?? 0;
            int reqSpd = GetIntProperty(baseJob, "RequiredSpeed") ?? 0;
            int reqDex = GetIntProperty(baseJob, "RequiredDexterity") ?? 0;

            var mainStack = new VerticalStackLayout
            {
                Spacing = EstateUIConstants.StackSpacing * 1.2,
                Padding = new Thickness(EstateUIConstants.CardContentPadding)
            };

            _titleLabel = new Label
            {
                Text = jobName,
                Style = (Style)Application.Current.Resources["CardTitle"],
                FontSize = EstateUIConstants.FontSizeMedium,
                HorizontalOptions = LayoutOptions.Center,
                HorizontalTextAlignment = TextAlignment.Center
            };
            mainStack.Children.Add(_titleLabel);

            _inactiveContent = new VerticalStackLayout { Spacing = EstateUIConstants.StackSpacing };

            var rewardsRow = new HorizontalStackLayout
            {
                Spacing = EstateUIConstants.ColumnSpacing,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Center,
                Margin = new Thickness(0, 5, 0, 0)
            };
            rewardsRow.Children.Add(new Label
            {
                Text = "🏆 جوائز العمل : ",
                Style = (Style)Application.Current.Resources["CardDescription"],
                FontSize = EstateUIConstants.FontSizeSmall,
                TextColor = Colors.Goldenrod
            });
            rewardsRow.Children.Add(new Label
            {
                Text = $"({salary:N0}) ذهب",
                TextColor = Colors.WhiteSmoke,
                FontFamily = "Cairo-Black",
                FontSize = EstateUIConstants.FontSizeSmall
            });
            rewardsRow.Children.Add(new Label
            {
                Text = $"({exp}) خبرة",
                TextColor = Colors.WhiteSmoke,
                FontFamily = "Cairo-Black",
                FontSize = EstateUIConstants.FontSizeSmall
            });
            _inactiveContent.Children.Add(rewardsRow);

            var reqRow = new HorizontalStackLayout
            {
                Spacing = EstateUIConstants.ColumnSpacing,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Center,
                Margin = new Thickness(0, 5, 0, 5)
            };
            reqRow.Children.Add(new Label
            {
                Text = "📋 متطلبات العمل : ",
                Style = (Style)Application.Current.Resources["CardDescription"],
                FontSize = EstateUIConstants.FontSizeSmall,
                TextColor = Colors.Goldenrod
            });

            double iconSize = EstateUIConstants.ImageSize * 0.3;
            AddIconWithValue(reqRow, "strength_icon.png", reqStr, iconSize);
            AddIconWithValue(reqRow, "defense_icon.png", reqDef, iconSize);
            AddIconWithValue(reqRow, "speed_icon.png", reqSpd, iconSize);
            AddIconWithValue(reqRow, "dexterity_icon.png", reqDex, iconSize);

            _inactiveContent.Children.Add(reqRow);
            mainStack.Children.Add(_inactiveContent);

            _activeContent = new VerticalStackLayout { Spacing = EstateUIConstants.StackSpacing, IsVisible = false };

            var activeRewardsRow = new HorizontalStackLayout
            {
                Spacing = EstateUIConstants.ColumnSpacing,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Center,
                Margin = new Thickness(0, 5, 0, 0)
            };
            activeRewardsRow.Children.Add(new Label
            {
                Text = "🏆 جوائز العمل : ",
                Style = (Style)Application.Current.Resources["CardDescription"],
                FontSize = EstateUIConstants.FontSizeSmall,
                TextColor = Colors.Goldenrod
            });
            activeRewardsRow.Children.Add(new Label
            {
                Text = $"({salary:N0}) ذهب",
                TextColor = Colors.WhiteSmoke,
                FontFamily = "Cairo-Black",
                FontSize = EstateUIConstants.FontSizeSmall
            });
            activeRewardsRow.Children.Add(new Label
            {
                Text = $"({exp}) خبرة",
                TextColor = Colors.WhiteSmoke,
                FontFamily = "Cairo-Black",
                FontSize = EstateUIConstants.FontSizeSmall
            });
            _activeContent.Children.Add(activeRewardsRow);

            var daysRow = new HorizontalStackLayout
            {
                Spacing = EstateUIConstants.ColumnSpacing,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Center,
                Margin = new Thickness(0, 5, 0, 5)
            };
            daysRow.Children.Add(new Label
            {
                Text = "📅 أيام العمل : ",
                Style = (Style)Application.Current.Resources["CardDescription"],
                FontSize = EstateUIConstants.FontSizeSmall,
                TextColor = Colors.Goldenrod
            });
            _daysWorkedValueLabel = new Label
            {
                Text = "0.0 يوم",
                TextColor = Colors.WhiteSmoke,
                FontFamily = "Cairo-Black",
                FontSize = EstateUIConstants.FontSizeSmall,
                FontAttributes = FontAttributes.Bold
            };
            daysRow.Children.Add(_daysWorkedValueLabel);
            _activeContent.Children.Add(daysRow);

            var timerRow = new HorizontalStackLayout
            {
                Spacing = 4,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Center,
                Margin = new Thickness(0, 0, 0, 5)
            };
            timerRow.Children.Add(new Label
            {
                Text = "⏳ القبض بعد : ",
                Style = (Style)Application.Current.Resources["CardDescription"],
                FontSize = EstateUIConstants.FontSizeSmall,
                TextColor = Colors.Goldenrod
            });
            _timerValueLabel = new Label
            {
                Text = "",
                TextColor = Colors.WhiteSmoke,
                FontFamily = "Cairo-Black",
                FontSize = EstateUIConstants.FontSizeSmall,
                FontAttributes = FontAttributes.Bold
            };
            timerRow.Children.Add(_timerValueLabel);
            _activeContent.Children.Add(timerRow);

            var activeGrid = new Grid
            {
                ColumnDefinitions = new ColumnDefinitionCollection
                {
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Auto }
                },
                ColumnSpacing = 0
            };
            activeGrid.Add(_activeContent, 0, 0);

            _activeButtonsStack = new VerticalStackLayout
            {
                Spacing = 8,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                IsVisible = false
            };
            _cancelButton = CreateImageButton("أستقالة", "button_background_no.png", async () => await OnCancelJobClicked());
            _collectButton = CreateImageButton("استلام", "button_background.png", async () => await OnWorkClicked());
            _activeButtonsStack.Children.Add(_collectButton);
            _activeButtonsStack.Children.Add(_cancelButton);
            activeGrid.Add(_activeButtonsStack, 1, 0);

            mainStack.Children.Add(activeGrid);

            _buttonsGrid = new Grid
            {
                ColumnSpacing = EstateUIConstants.ButtonSpacing,
                HorizontalOptions = LayoutOptions.Center,
                Margin = new Thickness(0, 5, 0, 5)
            };

            if (_hasJobInOtherCategory)
            {
                _startButton = CreateImageButton("مقفول", "card_background.png", async () => { });
                _startButton.IsEnabled = false;
                _startButton.Opacity = 1.0;
                var grid = _startButton.Content as Grid;
                if (grid?.Children.Count > 1 && grid.Children[1] is Label lbl)
                    lbl.TextColor = Colors.DarkRed;
            }
            else
            {
                _startButton = CreateImageButton("بدء العمل", "button_background.png", async () => await OnStartJobClicked());
            }
            _buttonsGrid.Children.Add(_startButton);
            Grid.SetColumn(_startButton, 0);
            mainStack.Children.Add(_buttonsGrid);

            if (_hasJobInThisCategory)
            {
                _inactiveContent.IsVisible = false;
                _activeContent.IsVisible = true;
                _activeButtonsStack.IsVisible = true;
                _startButton.IsVisible = false;
                UpdateActiveWorkDisplay();
                StartSecondTimer();
            }

            var rootGrid = MainCard.Content as Grid;
            if (rootGrid != null)
            {
                rootGrid.Children.Clear();
                rootGrid.Children.Add(new Image { Source = "card_background.png", Aspect = Aspect.Fill, Opacity = 1 });
                rootGrid.Children.Add(mainStack);
            }
        }
        catch (Exception ex)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
                await DisplayAlert("خطأ في بناء البطاقة", ex.Message, "موافق"));
        }
    }

    private void BuildNextJobCardContent()
    {
        try
        {
            if (!_hasJobInThisCategory)
            {
                NextJobCard.IsVisible = false;
                return;
            }

            var nextJob = WorkOfficeService.GetJob(_categoryId, _player.WorkObject.JobLevel + 1);
            if (nextJob == null)
            {
                NextJobCard.IsVisible = false;
                return;
            }
            NextJobCard.IsVisible = true;

            var mainStack = new VerticalStackLayout
            {
                Spacing = EstateUIConstants.StackSpacing * 1.2,
                Padding = new Thickness(EstateUIConstants.CardContentPadding)
            };

            _nextJobNameLabel = new Label
            {
                Text = nextJob.Name,
                Style = (Style)Application.Current.Resources["CardTitle"],
                FontSize = EstateUIConstants.FontSizeMedium,
                HorizontalOptions = LayoutOptions.Center,
                HorizontalTextAlignment = TextAlignment.Center
            };
            mainStack.Children.Add(_nextJobNameLabel);

            var reqRow = new HorizontalStackLayout
            {
                Spacing = EstateUIConstants.ColumnSpacing,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Center,
                Margin = new Thickness(0, 5, 0, 0)
            };
            reqRow.Children.Add(new Label
            {
                Text = "📋 متطلبات الترقية : ",
                Style = (Style)Application.Current.Resources["CardDescription"],
                FontSize = EstateUIConstants.FontSizeSmall,
                TextColor = Colors.Goldenrod
            });

            double iconSize = EstateUIConstants.ImageSize * 0.3;
            AddIconWithValueConditional(reqRow, "strength_icon.png", nextJob.RequiredStrength, _player.Strength, iconSize);
            AddIconWithValueConditional(reqRow, "defense_icon.png", nextJob.RequiredDefense, _player.Defense, iconSize);
            AddIconWithValueConditional(reqRow, "speed_icon.png", nextJob.RequiredSpeed, _player.Speed, iconSize);
            AddIconWithValueConditional(reqRow, "dexterity_icon.png", nextJob.RequiredDexterity, _player.Dexterity, iconSize);

            mainStack.Children.Add(reqRow);

            double daysWorked = 0;
            if (_player.WorkObject.JobStartTimeMilli > 0)
                daysWorked = (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - _player.WorkObject.JobStartTimeMilli) / (86400.0 * 1000.0);
            bool daysMet = daysWorked >= nextJob.RequiredDaysWorked;

            var daysReqRow = new HorizontalStackLayout
            {
                Spacing = 4,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Center,
                Margin = new Thickness(0, 5, 0, 5)
            };
            daysReqRow.Children.Add(new Label
            {
                Text = "📅 عدد الأيام المطلوب : ",
                Style = (Style)Application.Current.Resources["CardDescription"],
                FontSize = EstateUIConstants.FontSizeSmall,
                TextColor = Colors.Goldenrod
            });
            _reqDaysNumberLabel = new Label
            {
                Text = nextJob.RequiredDaysWorked.ToString(),
                TextColor = daysMet ? Colors.WhiteSmoke : Colors.Red,
                FontFamily = "Cairo-Black",
                FontSize = EstateUIConstants.FontSizeSmall,
                FontAttributes = FontAttributes.Bold
            };
            daysReqRow.Children.Add(_reqDaysNumberLabel);
            daysReqRow.Children.Add(new Label
            {
                Text = " يوم",
                Style = (Style)Application.Current.Resources["CardDescription"],
                FontSize = EstateUIConstants.FontSizeSmall,
                TextColor = Colors.Goldenrod
            });
            mainStack.Children.Add(daysReqRow);

            bool canPromote = WorkOfficeService.CanPromote(_player, _categoryId, _player.WorkObject.JobLevel);
            _promoteButton = CreateImageButton(
                canPromote ? "ترقية" : "ترقية",
                canPromote ? "button_background.png" : "card_background.png",
                async () => await OnPromoteClicked()
            );
            _promoteButton.IsEnabled = canPromote;
            _promoteButton.Opacity = canPromote ? 1 : 0.7;
            if (!canPromote)
            {
                var grid = _promoteButton.Content as Grid;
                if (grid?.Children.Count > 1 && grid.Children[1] is Label lbl)
                    lbl.TextColor = Colors.DarkRed;
            }
            mainStack.Children.Add(_promoteButton);

            var rootGrid = NextJobCard.Content as Grid;
            if (rootGrid == null)
            {
                rootGrid = new Grid();
                NextJobCard.Content = rootGrid;
            }
            rootGrid.Children.Clear();
            rootGrid.Children.Add(new Image { Source = "card_background.png", Aspect = Aspect.Fill, Opacity = 1 });
            rootGrid.Children.Add(mainStack);
        }
        catch (Exception ex)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
                await DisplayAlert("خطأ في بطاقة الترقية", ex.Message, "موافق"));
        }
    }

    private void AddIconWithValueConditional(HorizontalStackLayout parent, string iconName, int required, int current, double iconSize)
    {
        bool met = current >= required;
        var container = new HorizontalStackLayout { Spacing = 4, VerticalOptions = LayoutOptions.Center };
        container.Children.Add(new Image { Source = iconName, WidthRequest = iconSize, HeightRequest = iconSize, Aspect = Aspect.AspectFit });
        container.Children.Add(new Label
        {
            Text = required.ToString(),
            TextColor = met ? Colors.WhiteSmoke : Colors.Red,
            FontFamily = "Cairo-Black",
            FontSize = EstateUIConstants.FontSizeSmall,
            FontAttributes = FontAttributes.Bold,
            VerticalOptions = LayoutOptions.Center
        });
        parent.Children.Add(container);
    }

    private void AddIconWithValue(HorizontalStackLayout parent, string iconName, int value, double iconSize)
    {
        var container = new HorizontalStackLayout { Spacing = 4, VerticalOptions = LayoutOptions.Center };
        container.Children.Add(new Image { Source = iconName, WidthRequest = iconSize, HeightRequest = iconSize, Aspect = Aspect.AspectFit });
        container.Children.Add(new Label
        {
            Text = value.ToString(),
            TextColor = Colors.WhiteSmoke,
            FontFamily = "Cairo-Black",
            FontSize = EstateUIConstants.FontSizeSmall,
            FontAttributes = FontAttributes.Bold,
            VerticalOptions = LayoutOptions.Center
        });
        parent.Children.Add(container);
    }

    private void SwitchToActiveMode()
    {
        _inactiveContent.IsVisible = false;
        _activeContent.IsVisible = true;
        _startButton.IsVisible = false;
        _activeButtonsStack.IsVisible = true;
    }

    private void SwitchToInactiveMode()
    {
        _inactiveContent.IsVisible = true;
        _activeContent.IsVisible = false;
        _startButton.IsVisible = true;
        _activeButtonsStack.IsVisible = false;
    }

    private Border CreateImageButton(string text, string imageSource, Func<Task> action)
    {
        var border = new Border
        {
            Stroke = Colors.Transparent,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = EstateUIConstants.ButtonCornerRadius },
            WidthRequest = EstateUIConstants.ButtonWidth,
            HeightRequest = EstateUIConstants.ButtonHeight,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            BackgroundColor = Colors.Transparent
        };
        var grid = new Grid();
        grid.Add(new Image { Source = imageSource, Aspect = Aspect.Fill });
        grid.Add(new Label
        {
            Text = text,
            TextColor = EstateUIConstants.TextDark,
            FontFamily = "Cairo-Black",
            FontSize = EstateUIConstants.FontSizeButton,
            FontAttributes = FontAttributes.Bold,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        });
        border.Content = grid;
        var tap = new TapGestureRecognizer();
        tap.Tapped += async (s, e) => { await AnimateBorder(border); await action(); };
        border.GestureRecognizers.Add(tap);
        return border;
    }

    private async Task AnimateBorder(Border border)
    {
        try
        {
            await border.ScaleTo(EstateUIConstants.AnimationPressScale, EstateUIConstants.AnimationPressDuration, Easing.CubicIn);
            await border.ScaleTo(1, EstateUIConstants.AnimationPressDuration, Easing.CubicOut);
        }
        catch { }
    }

    private void UpdateActiveWorkDisplay()
    {
        if (!_hasJobInThisCategory) return;
        double daysWorked = 0;
        if (_player.WorkObject.JobStartTimeMilli > 0)
            daysWorked = (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - _player.WorkObject.JobStartTimeMilli) / (86400.0 * 1000.0);
        if (_daysWorkedValueLabel != null)
            _daysWorkedValueLabel.Text = $"{daysWorked:F0} يوم";
        UpdateTimerDisplay();
        UpdateNextJobCard();
    }

    private void UpdateTimerDisplay()
    {
        if (!_hasJobInThisCategory) return;
        bool canCollect = WorkOfficeService.CanCollectSalary(_player);
        var grid = _collectButton?.Content as Grid;
        var img = grid?.Children[0] as Image;
        var lbl = grid?.Children.Count > 1 ? grid.Children[1] as Label : null;

        if (canCollect)
        {
            if (_timerValueLabel != null)
            {
                _timerValueLabel.Text = "يمكنك استلام الراتب الآن!";
                _timerValueLabel.TextColor = Colors.WhiteSmoke;
            }
            if (_collectButton != null)
            {
                _collectButton.IsEnabled = true;
                _collectButton.Opacity = 1;
            }
            if (img != null) img.Source = "button_background.png";
            if (lbl != null) lbl.TextColor = EstateUIConstants.TextDark;
        }
        else
        {
            var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var startTime = _player.WorkObject.JobStartTimeMilli;
            var timePassed = now - startTime;
            var cooldown = 24 * 60 * 60 * 1000;
            var timeRemaining = cooldown - (timePassed % cooldown);
            if (timeRemaining < 0) timeRemaining = 0;
            var hours = (int)(timeRemaining / (60 * 60 * 1000));
            var minutes = (int)((timeRemaining % (60 * 60 * 1000)) / (60 * 1000));
            var seconds = (int)((timeRemaining % (60 * 1000)) / 1000);
            if (_timerValueLabel != null)
            {
                _timerValueLabel.Text = $"{hours:D2}:{minutes:D2}:{seconds:D2}";
                _timerValueLabel.TextColor = Colors.WhiteSmoke;
            }
            if (_collectButton != null)
            {
                _collectButton.IsEnabled = false;
                _collectButton.Opacity = 1;
            }
            if (img != null) img.Source = "card_background.png";
            if (lbl != null) lbl.TextColor = Colors.DarkRed;
        }
    }

    private void StartSecondTimer()
    {
        _isTimerRunning = true;
        Device.StartTimer(TimeSpan.FromSeconds(1), () =>
        {
            if (!_isTimerRunning) return false;
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (_hasJobInThisCategory && _activeContent != null && _activeContent.IsVisible)
                    UpdateActiveWorkDisplay();
            });
            return _isTimerRunning;
        });
    }

    private async void StartWorkCheckTimer()
    {
        _isCheckingWork = true;
        while (_isCheckingWork)
        {
            await Task.Delay(60000);
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (_hasJobInThisCategory)
                    UpdateNextJobCard();
            });
        }
    }

    private async Task OnStartJobClicked()
    {
        try
        {
            if (_player == null)
            {
                await DisplayAlert("خطأ", "بيانات اللاعب غير متوفرة", "موافق");
                return;
            }
            if (_titleLabel == null || string.IsNullOrEmpty(_titleLabel.Text))
            {
                await DisplayAlert("خطأ", "الرجاء الانتظار حتى تحميل البيانات", "موافق");
                return;
            }

            bool confirm = await PopupService.ShowConfirmAsync(
                title: "بدء العمل",
                message: $"هل تريد بدء العمل كـ {_titleLabel.Text}؟\nستحصل على الراتب بعد 24 ساعة من بدء العمل.",
                operationType: PopupOperationType.Confirm
            );
            if (!confirm) return;

            var result = WorkOfficeService.StartJob(_player, _categoryId);
            if (result.success)
            {
                _hasJobInThisCategory = true;
                _hasJobInOtherCategory = false;
                SwitchToActiveMode();

                BuildMainCardContent();
                BuildNextJobCardContent();
                NextJobCard.IsVisible = true;

                if (_collectButton != null)
                {
                    _collectButton.IsEnabled = false;
                    _collectButton.Opacity = 1.0;
                    var grid = _collectButton.Content as Grid;
                    if (grid != null)
                    {
                        if (grid.Children[0] is Image img) img.Source = "card_background.png";
                        if (grid.Children.Count > 1 && grid.Children[1] is Label lbl) lbl.TextColor = Colors.DarkRed;
                    }
                }

                UpdateActiveWorkDisplay();
                StartSecondTimer();
                await ToastService.Show(result.message, ToastType.Success);
            }
            else
            {
                await ToastService.Show(result.message, ToastType.Error);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("خطأ غير متوقع", ex.Message, "موافق");
        }
    }

    private async Task OnCancelJobClicked()
    {
        bool confirm = await PopupService.ShowConfirmAsync(
            title: "تأكيد الاستقالة",
            message: "هل انت متأكد أنك تريد الأستقالة! من عملك الحالي؟",
            operationType: PopupOperationType.Confirm
        );
        if (!confirm) return;

        _player.WorkObject.WorkType = -1;
        _player.WorkObject.JobLevel = 0;
        _player.WorkObject.JobStartTimeMilli = 0;

        _hasJobInThisCategory = false;
        _hasJobInOtherCategory = false;

        SwitchToInactiveMode();
        _isTimerRunning = false;
        BuildMainCardContent();
        NextJobCard.IsVisible = false;

        await ToastService.Show("تمت الأستقالة من العمل", ToastType.Success);
    }

    private async Task OnWorkClicked()
    {
        var result = WorkOfficeService.CollectSalary(_player);
        if (result.success)
        {
            await ToastService.Show(result.message, ToastType.Success);
            UpdateActiveWorkDisplay();
        }
        else
        {
            await ToastService.Show(result.message, ToastType.Error);
        }
    }

    private async Task OnPromoteClicked()
    {
        if (!_hasJobInThisCategory)
        {
            await ToastService.Show("ليس لديك وظيفة في هذا القسم", ToastType.Error);
            return;
        }

        var result = WorkOfficeService.Promote(_player, _categoryId, _player.WorkObject.JobLevel);
        if (result.success)
        {
            await ToastService.Show(result.message, ToastType.Success);
            LoadData();
            if (_hasJobInThisCategory)
            {
                SwitchToActiveMode();
                UpdateActiveWorkDisplay();
                StartSecondTimer();
            }
            NextJobCard.IsVisible = _hasJobInThisCategory;
        }
        else
        {
            await ToastService.Show(result.message, ToastType.Error);
        }
    }

    private void UpdateNextJobCard()
    {
        try
        {
            if (!_hasJobInThisCategory)
            {
                NextJobCard.IsVisible = false;
                return;
            }

            var nextJob = WorkOfficeService.GetJob(_categoryId, _player.WorkObject.JobLevel + 1);
            if (nextJob == null)
            {
                NextJobCard.IsVisible = false;
                return;
            }
            NextJobCard.IsVisible = true;

            bool canPromote = WorkOfficeService.CanPromote(_player, _categoryId, _player.WorkObject.JobLevel);
            if (_promoteButton != null)
            {
                _promoteButton.IsEnabled = canPromote;
                _promoteButton.Opacity = canPromote ? 1 : 0.7;

                var grid = _promoteButton.Content as Grid;
                if (grid != null)
                {
                    if (grid.Children[0] is Image img)
                        img.Source = canPromote ? "button_background.png" : "card_background.png";
                    if (grid.Children.Count > 1 && grid.Children[1] is Label lbl)
                    {
                        lbl.Text = canPromote ? "ترقية" : "ترقية";
                        lbl.TextColor = canPromote ? EstateUIConstants.TextDark : Colors.DarkRed;
                    }
                }
            }

            if (_reqDaysNumberLabel != null)
            {
                double daysWorked = 0;
                if (_player.WorkObject.JobStartTimeMilli > 0)
                    daysWorked = (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - _player.WorkObject.JobStartTimeMilli) / (86400.0 * 1000.0);
                bool daysMet = daysWorked >= nextJob.RequiredDaysWorked;
                _reqDaysNumberLabel.Text = nextJob.RequiredDaysWorked.ToString();
                _reqDaysNumberLabel.TextColor = daysMet ? Colors.WhiteSmoke : Colors.Red;
            }
        }
        catch (Exception ex)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
                await DisplayAlert("خطأ في تحديث بطاقة الترقية", ex.Message, "موافق"));
        }
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        _isCheckingWork = false;
        _isTimerRunning = false;
        if (sender is Border border) await AnimateBorder(border);
        await Navigation.PopAsync(false);
    }

    private string GetPropertyValue(object obj, string propName)
        => obj?.GetType().GetProperty(propName)?.GetValue(obj)?.ToString();

    private int? GetIntProperty(object obj, string propName)
    {
        var val = obj?.GetType().GetProperty(propName)?.GetValue(obj);
        if (val is int i) return i;
        if (val is long l) return (int)l;
        return null;
    }
}