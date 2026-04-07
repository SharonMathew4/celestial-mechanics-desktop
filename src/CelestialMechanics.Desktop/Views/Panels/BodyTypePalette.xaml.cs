using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using CelestialMechanics.Desktop.Models;
using CelestialMechanics.Desktop.ViewModels;
using CelestialMechanics.Physics.Types;

namespace CelestialMechanics.Desktop.Views.Panels;

public partial class BodyTypePalette : UserControl
{
    public BodyTypePalette()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        AddHandler(Button.ClickEvent, new RoutedEventHandler(OnButtonClick));
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Populate subtype dropdowns
        PopulateSubtypes(StarSubtypes, BodyType.Star);
        PopulateSubtypes(PlanetSubtypes, BodyType.Planet);
        PopulateSubtypes(GasGiantSubtypes, BodyType.GasGiant);
        PopulateSubtypes(RockySubtypes, BodyType.RockyPlanet);
        PopulateSubtypes(MoonSubtypes, BodyType.Moon);
        PopulateSubtypes(AsteroidSubtypes, BodyType.Asteroid);
        PopulateSubtypes(NeutronSubtypes, BodyType.NeutronStar);
        PopulateSubtypes(BlackHoleSubtypes, BodyType.BlackHole);
        PopulateSubtypes(CometSubtypes, BodyType.Comet);
    }

    private void PopulateSubtypes(ItemsControl itemsControl, BodyType category)
    {
        var subtypes = BodyCatalog.GetSubtypes(category);
        itemsControl.Items.Clear();

        foreach (var subtype in subtypes)
        {
            var btn = new Button
            {
                Content = CreateSubtypeContent(subtype),
                Tag = subtype,
                Style = FindResource("SubtypeCardStyle") as Style ?? CreateDefaultSubtypeStyle(),
                Margin = new Thickness(2),
                Padding = new Thickness(8, 4, 8, 4),
                MinWidth = 120,
            };
            btn.Click += OnSubtypeClick;
            itemsControl.Items.Add(btn);
        }
    }

    private static UIElement CreateSubtypeContent(BodySubtype subtype)
    {
        var panel = new StackPanel { Orientation = Orientation.Vertical };
        
        var nameBlock = new TextBlock
        {
            Text = $"{subtype.IconGlyph} {subtype.Name}",
            FontWeight = FontWeights.SemiBold,
            Foreground = Brushes.White,
            FontSize = 11,
        };
        
        var descBlock = new TextBlock
        {
            Text = subtype.Description,
            FontSize = 9,
            Foreground = new SolidColorBrush(Color.FromRgb(0xA0, 0xA5, 0xB5)),
            TextWrapping = TextWrapping.Wrap,
            MaxWidth = 180,
        };

        panel.Children.Add(nameBlock);
        panel.Children.Add(descBlock);
        return panel;
    }

    private static Style CreateDefaultSubtypeStyle()
    {
        var style = new Style(typeof(Button));
        style.Setters.Add(new Setter(BackgroundProperty, new SolidColorBrush(Color.FromRgb(0x1E, 0x25, 0x3A))));
        style.Setters.Add(new Setter(ForegroundProperty, Brushes.White));
        style.Setters.Add(new Setter(BorderBrushProperty, new SolidColorBrush(Color.FromRgb(0x2A, 0x30, 0x48))));
        style.Setters.Add(new Setter(BorderThicknessProperty, new Thickness(1)));
        style.Setters.Add(new Setter(CursorProperty, Cursors.Hand));
        return style;
    }

    private void OnSubtypeClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is BodySubtype subtype)
        {
            var vm = Window.GetWindow(this)?.DataContext as MainWindowViewModel;
            vm?.SelectSubtype(subtype);

            // Close all popups
            CloseAllPopups();
        }
    }

    private void CloseAllPopups()
    {
        StarToggle.IsChecked = false;
        PlanetToggle.IsChecked = false;
        GasGiantToggle.IsChecked = false;
        RockyToggle.IsChecked = false;
        MoonToggle.IsChecked = false;
        AsteroidToggle.IsChecked = false;
        NeutronToggle.IsChecked = false;
        BlackHoleToggle.IsChecked = false;
        CometToggle.IsChecked = false;
    }

    /// <summary>
    /// Route button clicks to the SelectBodyTypeCommand with the correct BodyType enum value
    /// parsed from each button's Tag property.
    /// </summary>
    private void OnButtonClick(object sender, RoutedEventArgs e)
    {
        if (e.OriginalSource is Button btn && btn.Tag is string typeName)
        {
            if (Enum.TryParse<BodyType>(typeName, out var bodyType))
            {
                var vm = Window.GetWindow(this)?.DataContext as MainWindowViewModel;
                vm?.SelectBodyType(bodyType);
            }
        }
    }
}
