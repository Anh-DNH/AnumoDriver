using AnumoDriver.Structures.INPUT;
using AnumoDriver.Utilities;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace AnumoDriver.Designs.UserControls;

/// <summary>
/// Interaction logic for InputSignal_Rotary_Ctrl.xaml
/// </summary>
public partial class InputSignal_Rotary_Uctrl : UserControl
{
    private InputSignal_Rotary _inputSignal;
    private ObservableCollection<string> _rotateSide = [];
    private string _prevRotaryIndex;

    public InputSignal_Rotary_Uctrl()
    {
        InitializeComponent();
    }

    private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        _inputSignal = DataContext as InputSignal_Rotary;

        EnumToStringConverter converter = new();
        foreach (string rotateSide in Enum.GetNames(typeof(InputSignal_Rotary.RollType)))
            _rotateSide.Add((string)converter.Convert(rotateSide, null, null, null));

        RotateSideComboBox.ItemsSource = _rotateSide;
        RotateSideComboBox.SelectedIndex = (int)_inputSignal.RotaryRollType;

        RotaryIndexTextBox.Text = _inputSignal.RotaryIndex.ToString();
    }

    private void RotaryIndexTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (int.TryParse(RotaryIndexTextBox.Text, out int result))
        {
            _inputSignal.RotaryIndex = result;
            _prevRotaryIndex = RotaryIndexTextBox.Text;
        }
        else
            RotaryIndexTextBox.Text = _prevRotaryIndex;
    }

    private void RotateSideComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _inputSignal.RotaryRollType = (InputSignal_Rotary.RollType)RotateSideComboBox.SelectedIndex;
    }
}
