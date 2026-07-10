using AnumoDriver.Structures.INPUT;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace AnumoDriver.Designs.UserControls;

/// <summary>
/// Interaction logic for InputAction_RollMouseWheel_Uctrl.xaml
/// </summary>
public partial class InputAction_RollMouseWheel_Uctrl : UserControl
{
    private InputAction_RollMouseWheel _inputAction;
    private string _prevMouseScrollDelta;

    public InputAction_RollMouseWheel_Uctrl()
    {
        InitializeComponent();
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        _inputAction = DataContext as InputAction_RollMouseWheel;

        MouseScrollDeltaTextBox.Text = _inputAction.Delta.ToString();
    }

    private void MouseScrollDeltaTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (int.TryParse(MouseScrollDeltaTextBox.Text, out int result))
        {
            _inputAction.Delta = result;
            _prevMouseScrollDelta = MouseScrollDeltaTextBox.Text;
        }
        else
            MouseScrollDeltaTextBox.Text = _prevMouseScrollDelta;
    }
}
