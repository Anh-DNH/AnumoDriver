using AnumoDriver.Structures.INPUT;
using AnumoDriver.Utilities;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Controls;

namespace AnumoDriver.Designs.UserControls;

/// <summary>
/// Interaction logic for InputAction_SimulateKeyboardKeys_Uctrl.xaml
/// </summary>
public partial class InputAction_SimulateKeyboardKeys_Uctrl : UserControl
{
    private InputAction_SimulateKeyboardKeys _inputAction;
    private List<string> _keyNames;

    private class KeyData
    {
        public List<string> KeyNames { get; set; }
        public int ComboBoxSelectedIndex { get; set; }
        public int Index { get; set; }
        public VirtualKeyCode Key { get; set; }
    }
    private ObservableCollection<KeyData> _keyDataList;

    public InputAction_SimulateKeyboardKeys_Uctrl()
    {
        InitializeComponent();
    }

    private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        _inputAction = DataContext as InputAction_SimulateKeyboardKeys;
        _keyNames = [..Enum.GetNames(typeof(VirtualKeyCode))];

        _keyDataList = [];
        for (int i = 0; i < _inputAction.Keys.Count; i++)
        {
            _keyDataList.Add(new KeyData
            {
                KeyNames = _keyNames,
                ComboBoxSelectedIndex = _keyNames.IndexOf(_inputAction.Keys[i].ToString()),
                Index = i
            });
        }
        VirtualKeyItemsControl.ItemsSource = _keyDataList;
    }

    private void KeyNameComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is not ComboBox cbox)
            return;

        KeyData keyData = (KeyData)cbox.DataContext;

        var newKey = Enum.Parse<VirtualKeyCode>(_keyNames[keyData.ComboBoxSelectedIndex]);
        _inputAction.Keys[keyData.Index] = newKey;
    }

    private void KeyNameButton_MoveUp_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        if (sender is not Button bton)
            return;

        KeyData keyData = (KeyData)bton.DataContext;

        if (keyData.Index == 0)
            return;

        var keyData2 = _keyDataList[keyData.Index - 1];
        _keyDataList[keyData.Index - 1] = keyData;
        _keyDataList[keyData.Index] = keyData2;

        (_inputAction.Keys[keyData.Index], _inputAction.Keys[keyData2.Index]) = (_inputAction.Keys[keyData2.Index], _inputAction.Keys[keyData.Index]);
        
        (keyData.Index, keyData2.Index) = (keyData2.Index, keyData.Index);
    }

    private void KeyNameButton_MoveDown_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        if (sender is not Button bton)
            return;

        KeyData keyData = (KeyData)bton.DataContext;

        if (keyData.Index == _keyDataList.Count - 1)
            return;

        var keyData2 = _keyDataList[keyData.Index + 1];
        _keyDataList[keyData.Index + 1] = keyData;
        _keyDataList[keyData.Index] = keyData2;

        (_inputAction.Keys[keyData.Index], _inputAction.Keys[keyData2.Index]) = (_inputAction.Keys[keyData2.Index], _inputAction.Keys[keyData.Index]);

        (keyData.Index, keyData2.Index) = (keyData2.Index, keyData.Index);
    }

    private void KeyNameButton_Remove_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        if (sender is not Button bton)
            return;

        KeyData keyData = (KeyData)bton.DataContext;
        
        _keyDataList.RemoveAt(keyData.Index);
        for (var i = keyData.Index; i < _keyDataList.Count; i++)
            _keyDataList[i].Index = i;

        _inputAction.Keys.RemoveAt(keyData.Index);
    }

    private void AddButton_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        _inputAction.Keys.Add(VirtualKeyCode.RETURN);

        _keyDataList.Add(new KeyData
        {
            KeyNames = _keyNames,
            ComboBoxSelectedIndex = _keyNames.IndexOf(_inputAction.Keys[_keyDataList.Count].ToString()),
            Index = _keyDataList.Count
        });

    }

    private void ClearButton_Click(object sender, System.Windows.RoutedEventArgs e)
    {

    }
}
