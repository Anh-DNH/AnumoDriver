using AnumoDriver.Structures.INPUT;
using System.Windows.Controls;

namespace AnumoDriver.Designs.UserControls;

/// <summary>
/// Interaction logic for InputAction_ChangeLayer_Uctrl.xaml
/// </summary>
public partial class InputAction_ChangeLayer_Uctrl : UserControl
{
    private InputAction_ChangeLayer _inputAction;
    private string _prevLayerIndex;

    public InputAction_ChangeLayer_Uctrl()
    {
        InitializeComponent();
    }

    private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        _inputAction = DataContext as InputAction_ChangeLayer;

        LayerIndexTextBox.Text = _inputAction.LayerId.ToString();
    }

    private void LayerIndexTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (int.TryParse(LayerIndexTextBox.Text, out int result))
        {
            _inputAction.LayerId = result;
            _prevLayerIndex = LayerIndexTextBox.Text;
        }
        else
            LayerIndexTextBox.Text = _prevLayerIndex;
    }
}
