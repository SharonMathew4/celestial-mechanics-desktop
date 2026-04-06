using System.Windows;
using System.Windows.Controls;
using CelestialMechanics.Physics.Types;

namespace CelestialMechanics.Desktop.Views.Panels;

public partial class BodyTypePalette : UserControl
{
    public BodyTypePalette()
    {
        InitializeComponent();
        AddHandler(Button.ClickEvent, new RoutedEventHandler(OnButtonClick));
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
                var vm = Window.GetWindow(this)?.DataContext as dynamic;
                if (vm != null)
                {
                    vm.SelectBodyType(bodyType);
                }
            }
        }
    }
}
