using AnumoDriver.Structures.INPUT;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace AnumoDriver.Designs.UserControls;

/// <summary>
/// Interaction logic for InputAction_ClickMouseButton_Uctrl.xaml
/// </summary>
public partial class InputAction_ClickMouseButton_Uctrl : UserControl
{
    private InputAction_ClickMouseButton _inputAction;

    private ObservableCollection<string> _buttonName { get; set; } = [
        "Left click",
        "Right click",
        "Middle click",
        "MOUSE4",
        "MOUSE5"
    ];

    public InputAction_ClickMouseButton_Uctrl()
    {
        InitializeComponent();
    }

    private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        _inputAction = DataContext as InputAction_ClickMouseButton;

        ButtonComboBox.ItemsSource = _buttonName;
        ButtonComboBox.SelectedIndex = _inputAction.ButtonId - 1;
    }

    private void ButtonComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _inputAction.ButtonId = ButtonComboBox.SelectedIndex + 1;
    }
}
