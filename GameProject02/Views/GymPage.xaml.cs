using GameProject02.Models;
using GameProject02.Services;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using System;

namespace GameProject02.Views;

public partial class GymPage : ContentPage
{
    private PlayerAccount _player;
    private bool _updatingUI = false; // Prevent recursive updates

    public GymPage()
    {
        InitializeComponent();

        // Slider → Entry synchronization
        StrengthSlider.ValueChanged += OnSliderChanged;
        DefenseSlider.ValueChanged += OnSliderChanged;
        SpeedSlider.ValueChanged += OnSliderChanged;
        DexteritySlider.ValueChanged += OnSliderChanged;

        // Entry → Slider synchronization (with validation)
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

        // Regenerate energy before displaying
        _player.RegenerateEnergy();

        // Update UI
        EnergyLabel.Text = $"الطاقة: {_player.Energy}/{_player.MaxEnergy}";
        StrengthStatLabel.Text = $"{_player.Strength:N1}";
        DefenseStatLabel.Text = $"{_player.Defense:N1}";
        SpeedStatLabel.Text = $"{_player.Speed:N1}";
        DexterityStatLabel.Text = $"{_player.Dexterity:N1}";

        // ✅ INITIALIZE SLIDERS WITH DYNAMIC CONSTRAINTS
        _updatingUI = true;

        // Set initial max values (all sliders can use full energy initially)
        StrengthSlider.Maximum = _player.Energy;
        DefenseSlider.Maximum = _player.Energy;
        SpeedSlider.Maximum = _player.Energy;
        DexteritySlider.Maximum = _player.Energy;

        // Set initial values to 0
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
        UpdateSliderConstraints(); // Apply initial constraints
        BuildLessonsUI();
    }

    // ✅ DYNAMIC SLIDER CONSTRAINTS (KEY FEATURE)
    private void UpdateSliderConstraints()
    {
        if (_player == null || _updatingUI) return;

        _updatingUI = true;

        // Get current values
        int strength = (int)StrengthSlider.Value;
        int defense = (int)DefenseSlider.Value;
        int speed = (int)SpeedSlider.Value;
        int dexterity = (int)DexteritySlider.Value;

        // ✅ CRITICAL LOGIC: Each slider's max = total energy - sum of OTHER sliders
        // This ensures the total can NEVER exceed available energy
        int totalEnergy = _player.Energy;

        StrengthSlider.Maximum = Math.Max(0, totalEnergy - (defense + speed + dexterity));
        DefenseSlider.Maximum = Math.Max(0, totalEnergy - (strength + speed + dexterity));
        SpeedSlider.Maximum = Math.Max(0, totalEnergy - (strength + defense + dexterity));
        DexteritySlider.Maximum = Math.Max(0, totalEnergy - (strength + defense + speed));

        // Update max labels
        StrengthMaxLabel.Text = ((int)StrengthSlider.Maximum).ToString();
        DefenseMaxLabel.Text = ((int)DefenseSlider.Maximum).ToString();
        SpeedMaxLabel.Text = ((int)SpeedSlider.Maximum).ToString();
        DexterityMaxLabel.Text = ((int)DexteritySlider.Maximum).ToString();

        // Ensure current values don't exceed new maxima (edge case handling)
        StrengthSlider.Value = Math.Min(StrengthSlider.Value, StrengthSlider.Maximum);
        DefenseSlider.Value = Math.Min(DefenseSlider.Value, DefenseSlider.Maximum);
        SpeedSlider.Value = Math.Min(SpeedSlider.Value, SpeedSlider.Maximum);
        DexteritySlider.Value = Math.Min(DexteritySlider.Value, DexteritySlider.Maximum);

        _updatingUI = false;

        UpdateTotalEnergyDisplay();
    }

    // Slider → Entry synchronization + CONSTRAINT UPDATE
    private void OnSliderChanged(object sender, ValueChangedEventArgs e)
    {
        if (_updatingUI || _player == null) return;

        _updatingUI = true;

        // Update corresponding entry text
        if (sender == StrengthSlider)
            StrengthEntry.Text = ((int)e.NewValue).ToString();
        else if (sender == DefenseSlider)
            DefenseEntry.Text = ((int)e.NewValue).ToString();
        else if (sender == SpeedSlider)
            SpeedEntry.Text = ((int)e.NewValue).ToString();
        else if (sender == DexteritySlider)
            DexterityEntry.Text = ((int)e.NewValue).ToString();

        _updatingUI = false;

        // ✅ UPDATE CONSTRAINTS AFTER ANY SLIDER CHANGE
        UpdateSliderConstraints();
    }

    // Entry → Slider synchronization (with validation) + CONSTRAINT UPDATE
    private void OnEntryTextChanged(object sender, TextChangedEventArgs e)
    {
        if (_updatingUI || _player == null) return;

        _updatingUI = true;

        // Parse and validate input
        int value = 0;
        if (int.TryParse(((Entry)sender).Text, out int parsed))
        {
            // Get current max for this slider BEFORE updating
            double currentMax = 0;
            if (sender == StrengthEntry) currentMax = StrengthSlider.Maximum;
            else if (sender == DefenseEntry) currentMax = DefenseSlider.Maximum;
            else if (sender == SpeedEntry) currentMax = SpeedSlider.Maximum;
            else if (sender == DexterityEntry) currentMax = DexteritySlider.Maximum;

            // Clamp to valid range [0, currentMax]
            value = Math.Max(0, Math.Min(parsed, (int)currentMax));
        }

        // Update corresponding slider
        if (sender == StrengthEntry)
            StrengthSlider.Value = value;
        else if (sender == DefenseEntry)
            DefenseSlider.Value = value;
        else if (sender == SpeedEntry)
            SpeedSlider.Value = value;
        else if (sender == DexterityEntry)
            DexteritySlider.Value = value;

        // Update entry text to clamped value
        ((Entry)sender).Text = value.ToString();

        _updatingUI = false;

        // ✅ UPDATE CONSTRAINTS AFTER ANY ENTRY CHANGE
        UpdateSliderConstraints();
    }

    private void UpdateTotalEnergyDisplay()
    {
        if (_player == null) return;

        // Parse entry values safely
        int strength = int.TryParse(StrengthEntry.Text, out int s) ? s : 0;
        int defense = int.TryParse(DefenseEntry.Text, out int d) ? d : 0;
        int speed = int.TryParse(SpeedEntry.Text, out int sp) ? sp : 0;
        int dexterity = int.TryParse(DexterityEntry.Text, out int dex) ? dex : 0;

        int total = strength + defense + speed + dexterity;
        int remaining = _player.Energy - total;

        // Update display
        TotalEnergyLabel.Text = $"المجموع: {total} / {_player.Energy} (متبقي: {Math.Max(0, remaining)})";

        // Enable/disable train button based on having allocated energy
        TrainButton.IsEnabled = (total > 0 && total <= _player.Energy);

        if (total == 0)
        {
            TotalEnergyWarning.Text = "⚠️ يجب تخصيص طاقة للتدريب على الأقل لإحصائية واحدة!";
            TotalEnergyWarning.IsVisible = true;
        }
        else if (total > _player.Energy)
        {
            // This should never happen with constraints, but just in case
            TotalEnergyWarning.Text = $"⚠️ المجموع يتجاوز طاقتك ({_player.Energy})!";
            TotalEnergyWarning.IsVisible = true;
        }
        else
        {
            TotalEnergyWarning.IsVisible = false;
        }

        // Update stat gain previews with current lesson multipliers
        StrengthValueLabel.Text = $"+{Math.Round(strength * 0.5 * _player.Gym.GetStrengthMultiplier())}";
        DefenseValueLabel.Text = $"+{Math.Round(defense * 0.5 * _player.Gym.GetDefenseMultiplier())}";
        SpeedValueLabel.Text = $"+{Math.Round(speed * 0.5 * _player.Gym.GetSpeedMultiplier())}";
        DexterityValueLabel.Text = $"+{Math.Round(dexterity * 0.5 * _player.Gym.GetDexterityMultiplier())}";
    }

    private void BuildLessonsUI()
    {
        LessonsContainer.Children.Clear();

        // ✅ ONLY 5 LESSONS (Running Gym REMOVED)
        for (int i = 0; i < 5; i++) // Changed from 6 to 5
        {
            var lesson = GymObject.GetLessonInfo(i);
            var isUnlocked = _player.Gym.LessonUnlocked[i];
            var progress = i < _player.Gym.LessonProgress.Count ? _player.Gym.LessonProgress[i] : 0;
            var isSelected = _player.Gym.SelectedLesson == i;

            // Create lesson card
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

            // Lesson icon
            grid.Add(new Label
            {
                Text = lesson.Icon,
                FontSize = 28,
                TextColor = isUnlocked ? Color.FromArgb("#ff6b6b") : Color.FromArgb("#555555"),
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center
            }, 0, 0);

            // Lesson name and description
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

            // Progress bar for all lessons (including Basic)
            var progressStack = new StackLayout { Orientation = StackOrientation.Horizontal, Spacing = 5 };
            progressStack.Children.Add(new Label
            {
                Text = $"{progress}%",
                TextColor = Color.FromArgb("#45b7d1"),
                FontSize = 12
            });

            var progressBar = new ProgressBar
            {
                Progress = progress / 100.0,
                ProgressColor = Color.FromArgb("#45b7d1"),
                HeightRequest = 6,
                WidthRequest = 100
            };

            progressStack.Children.Add(progressBar);
            nameStack.Children.Add(progressStack);

            grid.Add(nameStack, 1, 0);

            // Action button
            var actionButton = new Button
            {
                Text = isUnlocked ? "اختر" : (i == 0 ? "متوفر" : $"يحتاج إكمال الجيم الأساسي"),
                TextColor = isUnlocked || i == 0 ? Colors.White : Color.FromArgb("#aaaaaa"),
                BackgroundColor = (isUnlocked || i == 0) ? Color.FromArgb("#2c3e50") : Color.FromArgb("#1a1a1a"),
                CornerRadius = 8,
                FontSize = 12,
                HeightRequest = 45,
                WidthRequest = 85,
                HorizontalOptions = LayoutOptions.End,
                IsEnabled = isUnlocked || i == 0 // Basic Gym always selectable
            };

            int lessonIndex = i;
            actionButton.Clicked += (s, e) =>
            {
                if (isUnlocked || lessonIndex == 0) // Allow selecting Basic Gym even if not "unlocked" (it's default)
                {
                    _player.Gym.SelectedLesson = lessonIndex;
                    BuildLessonsUI(); // Refresh UI to show selection
                    UpdateSliderConstraints(); // Update constraints with new multipliers
                }
            };

            grid.Add(actionButton, 2, 0);

            lessonCard.Content = grid;
            LessonsContainer.Children.Add(lessonCard);

            // Add tap gesture to select lesson
            var tapGesture = new TapGestureRecognizer();
            tapGesture.Tapped += (s, e) =>
            {
                if (isUnlocked || lessonIndex == 0)
                {
                    _player.Gym.SelectedLesson = lessonIndex;
                    BuildLessonsUI();
                    UpdateSliderConstraints();
                }
            };
            lessonCard.GestureRecognizers.Add(tapGesture);
        }
    }
    private async void OnTrainClicked(object sender, EventArgs e)
    {
        if (_player == null) return;

        // Parse energy values safely
        int strength = int.TryParse(StrengthEntry.Text, out int s) ? s : 0;
        int defense = int.TryParse(DefenseEntry.Text, out int d) ? d : 0;
        int speed = int.TryParse(SpeedEntry.Text, out int sp) ? sp : 0;
        int dexterity = int.TryParse(DexterityEntry.Text, out int dex) ? dex : 0;

        int totalEnergy = strength + defense + speed + dexterity;

        // Validate (should be impossible to fail with constraints, but double-check)
        if (totalEnergy == 0)
        {
            await DisplayAlert("⚠️ تنبيه", "يجب تخصيص طاقة للتدريب على الأقل لإحصائية واحدة!", "موافق");
            return;
        }

        if (totalEnergy > _player.Energy)
        {
            await DisplayAlert("❌ طاقة منخفضة",
                $"تحتاج {totalEnergy} طاقة للتدريب، لكنك تملك فقط {_player.Energy} طاقة",
                "موافق");
            return;
        }

        // Train! Pass ENERGY ALLOCATION ARRAY for all 4 stats
        var result = _player.Gym.Train(_player, new int[] { strength, defense, speed, dexterity });

        if (result.success)
        {
            // Update UI
            EnergyLabel.Text = $"الطاقة: {_player.Energy}/{_player.MaxEnergy}";
            StrengthStatLabel.Text = $"{_player.Strength:N1}";
            DefenseStatLabel.Text = $"{_player.Defense:N1}";
            SpeedStatLabel.Text = $"{_player.Speed:N1}";
            DexterityStatLabel.Text = $"{_player.Dexterity:N1}";

            // Reset sliders/entries to 0 after training
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

            // Re-apply constraints with new energy level
            UpdateSliderConstraints();
            BuildLessonsUI(); // Refresh progress bars

            await DisplayAlert("✅ تم التدريب!", result.message, "موافق");
        }
        else
        {
            await DisplayAlert("❌ فشل التدريب", result.message, "موافق");
        }
    }

    private async void OnProfileClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new ProfilePage());
    }

    private async void OnHomeClicked(object sender, EventArgs e)
    {
        await Navigation.PopToRootAsync();
    }
}