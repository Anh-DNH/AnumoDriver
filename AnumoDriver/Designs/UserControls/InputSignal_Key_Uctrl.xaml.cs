using AnumoDriver.Structures.INPUT;
using AnumoDriver.Utilities;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace AnumoDriver.Designs.UserControls;

/// <summary>
/// Interaction logic for InputSignal_Key_Uctrl.xaml
/// </summary>
public partial class InputSignal_Key_Uctrl : UserControl
{
    private InputSignal_Key _inputSignal;
    private string _prevKeyCode;
    private string _prevClickCount;

    private ObservableCollection<string> _keyTypes = [];

    public InputSignal_Key_Uctrl()
    {
        InitializeComponent();
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        _inputSignal = DataContext as InputSignal_Key;
        if (_inputSignal == null)
            return;

        // Initialize enum collections
        var enumNames = Enum.GetNames(typeof(InputSignal_Key.PressType));
        foreach (var enumName in enumNames)
        {
            _keyTypes.Add(EnumToStringConverter.Convert(enumName));
        }
        KeyPressTypeComboBox.ItemsSource = _keyTypes;
        KeyPressTypeComboBox.SelectedIndex = (int)_inputSignal.KeyPressType;

        KeyCodeTextBox.Text = _inputSignal.Key.ToString();
        ClickCountTextBox.Text = _inputSignal.ClickCount.ToString();

        ShowWhenTypeEqualMultiple.Visibility =
            _inputSignal.KeyPressType == InputSignal_Key.PressType.Multiple ? Visibility.Visible : Visibility.Hidden;
    }

    private void KeyPressTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _inputSignal.KeyPressType = (InputSignal_Key.PressType)KeyPressTypeComboBox.SelectedIndex;

        ShowWhenTypeEqualMultiple.Visibility =
            _inputSignal.KeyPressType == InputSignal_Key.PressType.Multiple ? Visibility.Visible : Visibility.Hidden;
    }

    private void KeyCodeTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (int.TryParse(KeyCodeTextBox.Text, out int result))
        {
            _inputSignal.Key = result;
            _prevKeyCode = KeyCodeTextBox.Text;
        }
        else
            KeyCodeTextBox.Text = _prevKeyCode;
    }

    private void ClickCountTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (int.TryParse(ClickCountTextBox.Text, out int result))
        {
            _inputSignal.ClickCount = result;
            _prevClickCount = ClickCountTextBox.Text;
        }
        else
            ClickCountTextBox.Text = _prevClickCount;
    }
}
