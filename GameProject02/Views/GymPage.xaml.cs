using GameProject02.Models;
using GameProject02.Services;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using System;

namespace GameProject02.Views;

public partial class GymPage : ContentPage
{
    private PlayerAccount _player;
    private bool _updatingUI = false;

    public GymPage()
    {
        InitializeComponent();

        StrengthSlider.ValueChanged += OnSliderChanged;
        DefenseSlider.ValueChanged += OnSliderChanged;
        SpeedSlider.ValueChanged += OnSliderChanged;
        DexteritySlider.ValueChanged += OnSliderChanged;

        StrengthEntry.TextChanged += OnEntryTextChanged;
        DefenseEntry.TextChanged += OnEntryTextChanged;
        SpeedEntry.TextChanged += OnEntryTextChanged;
        DexterityEntry.TextChanged += OnEntryTextChanged;

        LoadPlayerData();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadPlayerData();
    }

    private void LoadPlayerData()
    {
        _player = AccountService.GetCurrentPlayer();
        if (_player == null) return;

        // ── RegenerateEnergy() حُذفت — RegenerationService يتولى التجديد تلقائياً ──

        EnergyLabel.Text = $"الطاقة: {_player.Energy}/{_player.MaxEnergy}";
        StrengthStatLabel.Text = $"{_player.Strength:N1}";
        DefenseStatLabel.Text = $"{_player.Defense:N1}";
        SpeedStatLabel.Text = $"{_player.Speed:N1}";
        DexterityStatLabel.Text = $"{_player.Dexterity:N1}";

        _updatingUI = true;

        StrengthSlider.Maximum = _player.Energy;
        DefenseSlider.Maximum = _player.Energy;
        SpeedSlider.Maximum = _player.Energy;
        DexteritySlider.Maximum = _player.Energy;

        StrengthSlider.Value = 0;
        DefenseSlider.Value = 0;
        SpeedSlider.Value = 0;
        DexteritySlider.Value = 0;

        StrengthEntry.Text = "0";
        DefenseEntry.Text = "0";
        SpeedEntry.Text = "0";
        DexterityEntry.Text = "0";

        _updatingUI = false;

        UpdateTotalEnergyDisplay();
        UpdateSliderConstraints();
        BuildLessonsUI();
    }

    private void UpdateSliderConstraints()
    {
        if (_player == null || _updatingUI) return;

        _updatingUI = true;

        int strength = (int)StrengthSlider.Value;
        int defense = (int)DefenseSlider.Value;
        int speed = (int)SpeedSlider.Value;
        int dexterity = (int)DexteritySlider.Value;
        int total = _player.Energy;

        StrengthSlider.Maximum = Math.Max(0, total - (defense + speed + dexterity));
        DefenseSlider.Maximum = Math.Max(0, total - (strength + speed + dexterity));
        SpeedSlider.Maximum = Math.Max(0, total - (strength + defense + dexterity));
        DexteritySlider.Maximum = Math.Max(0, total - (strength + defense + speed));

        StrengthMaxLabel.Text = ((int)StrengthSlider.Maximum).ToString();
        DefenseMaxLabel.Text = ((int)DefenseSlider.Maximum).ToString();
        SpeedMaxLabel.Text = ((int)SpeedSlider.Maximum).ToString();
        DexterityMaxLabel.Text = ((int)DexteritySlider.Maximum).ToString();

        StrengthSlider.Value = Math.Min(StrengthSlider.Value, StrengthSlider.Maximum);
        DefenseSlider.Value = Math.Min(DefenseSlider.Value, DefenseSlider.Maximum);
        SpeedSlider.Value = Math.Min(SpeedSlider.Value, SpeedSlider.Maximum);
        DexteritySlider.Value = Math.Min(DexteritySlider.Value, DexteritySlider.Maximum);

        _updatingUI = false;

        UpdateTotalEnergyDisplay();
    }

    private void OnSliderChanged(object sender, ValueChangedEventArgs e)
    {
        if (_updatingUI || _player == null) return;

        _updatingUI = true;

        if (sender == StrengthSlider) StrengthEntry.Text = ((int)e.NewValue).ToString();
        else if (sender == DefenseSlider) DefenseEntry.Text = ((int)e.NewValue).ToString();
        else if (sender == SpeedSlider) SpeedEntry.Text = ((int)e.NewValue).ToString();
        else if (sender == DexteritySlider) DexterityEntry.Text = ((int)e.NewValue).ToString();

        _updatingUI = false;

        UpdateSliderConstraints();
    }

    private void OnEntryTextChanged(object sender, TextChangedEventArgs e)
    {
        if (_updatingUI || _player == null) return;

        _updatingUI = true;

        int value = 0;
        if (int.TryParse(((Entry)sender).Text, out int parsed))
        {
            double currentMax = 0;
            if (sender == StrengthEntry) currentMax = StrengthSlider.Maximum;
            else if (sender == DefenseEntry) currentMax = DefenseSlider.Maximum;
            else if (sender == SpeedEntry) currentMax = SpeedSlider.Maximum;
            else if (sender == DexterityEntry) currentMax = DexteritySlider.Maximum;

            value = Math.Max(0, Math.Min(parsed, (int)currentMax));
        }

        if (sender == StrengthEntry) StrengthSlider.Value = value;
        else if (sender == DefenseEntry) DefenseSlider.Value = value;
        else if (sender == SpeedEntry) SpeedSlider.Value = value;
        else if (sender == DexterityEntry) DexteritySlider.Value = value;

        ((Entry)sender).Text = value.ToString();

        _updatingUI = false;

        UpdateSliderConstraints();
    }

    private void UpdateTotalEnergyDisplay()
    {
        if (_player == null) return;

        int strength = int.TryParse(StrengthEntry.Text, out int s) ? s : 0;
        int defense = int.TryParse(DefenseEntry.Text, out int d) ? d : 0;
        int speed = int.TryParse(SpeedEntry.Text, out int sp) ? sp : 0;
        int dexterity = int.TryParse(DexterityEntry.Text, out int dex) ? dex : 0;

        int total = strength + defense + speed + dexterity;
        int remaining = _player.Energy - total;

        TotalEnergyLabel.Text = $"المجموع: {total} / {_player.Energy} (متبقي: {Math.Max(0, remaining)})";
        TrainButton.IsEnabled = (total > 0 && total <= _player.Energy);

        if (total == 0)
        {
            TotalEnergyWarning.Text = "⚠️ يجب تخصيص طاقة للتدريب على الأقل لإحصائية واحدة!";
            TotalEnergyWarning.IsVisible = true;
        }
        else if (total > _player.Energy)
        {
            TotalEnergyWarning.Text = $"⚠️ المجموع يتجاوز طاقتك ({_player.Energy})!";
            TotalEnergyWarning.IsVisible = true;
        }
        else
        {
            TotalEnergyWarning.IsVisible = false;
        }

        StrengthValueLabel.Text = $"+{Math.Round(strength * 0.5 * _player.Gym.GetStrengthMultiplier())}";
        DefenseValueLabel.Text = $"+{Math.Round(defense * 0.5 * _player.Gym.GetDefenseMultiplier())}";
        SpeedValueLabel.Text = $"+{Math.Round(speed * 0.5 * _player.Gym.GetSpeedMultiplier())}";
        DexterityValueLabel.Text = $"+{Math.Round(dexterity * 0.5 * _player.Gym.GetDexterityMultiplier())}";
    }

    private void BuildLessonsUI()
    {
        LessonsContainer.Children.Clear();

        for (int i = 0; i < 5; i++)
        {
            var lesson = GymObject.GetLessonInfo(i);
            var isUnlocked = _player.Gym.LessonUnlocked[i];
            var progress = i < _player.Gym.LessonProgress.Count ? _player.Gym.LessonProgress[i] : 0;
            var isSelected = _player.Gym.SelectedLesson == i;

            var lessonCard = new Border
            {
                Stroke = Colors.Transparent,
                StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(12) },
                BackgroundColor = isSelected ? Color.FromArgb("#2a2a2a") : Color.FromArgb("#1a1a1a"),
                Padding = new Thickness(15),
                Margin = new Thickness(0, 0, 0, 10)
            };

            var grid = new Grid
            {
                ColumnDefinitions = new ColumnDefinitionCollection
                {
                    new ColumnDefinition { Width = new GridLength(60) },
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                    new ColumnDefinition { Width = new GridLength(90) }
                },
                RowSpacing = 5
            };

            grid.Add(new Label
            {
                Text = lesson.Icon,
                FontSize = 28,
                TextColor = isUnlocked ? Color.FromArgb("#ff6b6b") : Color.FromArgb("#555555"),
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center
            }, 0, 0);

            var nameStack = new StackLayout { Spacing = 3 };
            nameStack.Children.Add(new Label
            {
                Text = lesson.Name,
                TextColor = isUnlocked ? Colors.White : Color.FromArgb("#888888"),
                FontSize = 16,
                FontAttributes = FontAttributes.Bold
            });
            nameStack.Children.Add(new Label
            {
                Text = lesson.Description,
                TextColor = Color.FromArgb("#888888"),
                FontSize = 12
            });

            var progressStack = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                Spacing = 5
            };
            progressStack.Children.Add(new Label
            {
                Text = $"{progress}%",
                TextColor = Color.FromArgb("#45b7d1"),
                FontSize = 12
            });
            progressStack.Children.Add(new ProgressBar
            {
                Progress = progress / 100.0,
                ProgressColor = Color.FromArgb("#45b7d1"),
                HeightRequest = 6,
                WidthRequest = 100
            });

            nameStack.Children.Add(progressStack);
            grid.Add(nameStack, 1, 0);

            var actionButton = new Button
            {
                Text = isUnlocked ? "اختر" : (i == 0 ? "متوفر" : "يحتاج إكمال الجيم الأساسي"),
                TextColor = isUnlocked || i == 0 ? Colors.White : Color.FromArgb("#aaaaaa"),
                BackgroundColor = isUnlocked || i == 0 ? Color.FromArgb("#2c3e50") : Color.FromArgb("#1a1a1a"),
                CornerRadius = 8,
                FontSize = 12,
                HeightRequest = 45,
                WidthRequest = 85,
                HorizontalOptions = LayoutOptions.End,
                IsEnabled = isUnlocked || i == 0
            };

            int lessonIndex = i;
            actionButton.Clicked += (s, e) =>
            {
                if (isUnlocked || lessonIndex == 0)
                {
                    _player.Gym.SelectedLesson = lessonIndex;
                    BuildLessonsUI();
                    UpdateSliderConstraints();
                }
            };

            grid.Add(actionButton, 2, 0);
            lessonCard.Content = grid;
            LessonsContainer.Children.Add(lessonCard);

            var tap = new TapGestureRecognizer();
            tap.Tapped += (s, e) =>
            {
                if (isUnlocked || lessonIndex == 0)
                {
                    _player.Gym.SelectedLesson = lessonIndex;
                    BuildLessonsUI();
                    UpdateSliderConstraints();
                }
            };
            lessonCard.GestureRecognizers.Add(tap);
        }
    }

    private async void OnTrainClicked(object sender, EventArgs e)
    {
        if (_player == null) return;

        int strength = int.TryParse(StrengthEntry.Text, out int s) ? s : 0;
        int defense = int.TryParse(DefenseEntry.Text, out int d) ? d : 0;
        int speed = int.TryParse(SpeedEntry.Text, out int sp) ? sp : 0;
        int dexterity = int.TryParse(DexterityEntry.Text, out int dex) ? dex : 0;
        int total = strength + defense + speed + dexterity;

        if (total == 0)
        {
            await DisplayAlert("⚠️ تنبيه", "يجب تخصيص طاقة للتدريب على الأقل لإحصائية واحدة!", "موافق");
            return;
        }

        if (total > _player.Energy)
        {
            await DisplayAlert("❌ طاقة منخفضة",
                $"تحتاج {total} طاقة، لديك {_player.Energy} فقط", "موافق");
            return;
        }

        var result = _player.Gym.Train(_player, new int[] { strength, defense, speed, dexterity });

        if (result.success)
        {
            EnergyLabel.Text = $"الطاقة: {_player.Energy}/{_player.MaxEnergy}";
            StrengthStatLabel.Text = $"{_player.Strength:N1}";
            DefenseStatLabel.Text = $"{_player.Defense:N1}";
            SpeedStatLabel.Text = $"{_player.Speed:N1}";
            DexterityStatLabel.Text = $"{_player.Dexterity:N1}";

            _updatingUI = true;
            StrengthSlider.Value = 0;
            DefenseSlider.Value = 0;
            SpeedSlider.Value = 0;
            DexteritySlider.Value = 0;
            StrengthEntry.Text = "0";
            DefenseEntry.Text = "0";
            SpeedEntry.Text = "0";
            DexterityEntry.Text = "0";
            _updatingUI = false;

            UpdateSliderConstraints();
            BuildLessonsUI();

            await DisplayAlert("✅ تم التدريب!", result.message, "موافق");
        }
        else
        {
            await DisplayAlert("❌ فشل التدريب", result.message, "موافق");
        }
    }

    private async void OnProfileClicked(object sender, EventArgs e)
        => await Navigation.PushAsync(new ProfilePage(), false);

    private async void OnHomeClicked(object sender, EventArgs e)
        => await Navigation.PopToRootAsync(false);
}