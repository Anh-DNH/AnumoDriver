using AnumoDriver.Structures.INPUT;
using AnumoDriver.Utilities;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AnumoDriver.Designs.UserControls;

/// <summary>
/// Interaction logic for InputUctrl.xaml
/// </summary>
public partial class InputInfoUctrl : UserControl
{
    private InputDataEditorWindow _iDataEditorW;

    public event Action<object, RoutedEventArgs> OnRemove;
    public event Action<object, RoutedEventArgs> OnMoveUp;
    public event Action<object, RoutedEventArgs> OnMoveDown;
    public event Action<InputDataEditorWindow> OpenInputDataEditor;
    public event Action<InputDataEditorWindow> CloseInputDataEditor;

    public InputInfoUctrl()
    {
        InitializeComponent();
    }

    private void EditInputButton_Click(object sender, RoutedEventArgs e)
    {
        if (_iDataEditorW == null)
        {
            _iDataEditorW = new()
            {
                DataContext = DataContext
            };
            _iDataEditorW.Closed += (e, i) =>
            {
                CloseInputDataEditor.Invoke(_iDataEditorW);
                _iDataEditorW = null;
            };
            _iDataEditorW.Show();
            OpenInputDataEditor.Invoke(_iDataEditorW);
        }
        else
        {
            _iDataEditorW.Focus();
        }
    }

    private void RemoveInputButton_Click(object sender, RoutedEventArgs e) =>
        OnRemove?.Invoke(sender, e);

    private void MoveInputUpButton_Click(object sender, RoutedEventArgs e) =>
        OnMoveUp?.Invoke(sender, e);

    private void MoveInputDownButton_Click(object sender, RoutedEventArgs e) =>
        OnMoveDown?.Invoke(sender, e);
}

