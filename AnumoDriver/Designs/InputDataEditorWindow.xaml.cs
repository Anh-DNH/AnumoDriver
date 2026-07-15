using AnumoDriver.Designs.UserControls;
using AnumoDriver.Structures.INPUT;
using AnumoDriver.Utilities;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AnumoDriver.Designs;

/// <summary>
/// Interaction logic for InputDataEditorWindow.xaml
/// </summary>
public partial class InputDataEditorWindow : Window
{
    private Input _input;

    public ObservableCollection<string> SignalTypes { get; private set; } = [];
    public ObservableCollection<string> ActionTypes { get; private set; } = [];

    public InputDataEditorWindow()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        _input = DataContext as Input;

        // Initialize enum collections
        var enumNames = Enum.GetNames(typeof(InputSignal.Type));
        foreach (var enumName in enumNames)
        {
            SignalTypes.Add(EnumToStringConverter.Convert(enumName));
        }
        enumNames = Enum.GetNames(typeof(InputAction.Type));
        foreach (var enumName in enumNames)
        {
            ActionTypes.Add(EnumToStringConverter.Convert(enumName));
        }

        InputSignalComboBox.ItemsSource = SignalTypes;
        InputActionComboBox.ItemsSource = ActionTypes;

        InputSignalComboBox.SelectedIndex = (int)_input.SignalType;
        InputActionComboBox.SelectedIndex = (int)_input.ActionType;

        InputSignalContentControl.Content = GetSignalControl(_input.InputSignal);
        InputActionContentControl.Content = GetActionControl(_input.InputAction);
    }

    private void InputSignalComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (InputSignalComboBox.SelectedIndex == (int)_input.SignalType)
            return;

        var result = MessageBox.Show("Changing signal type will delete all current data. Continue?", "Confirm change?", MessageBoxButton.YesNo);
        if (result == MessageBoxResult.No)
        {
            InputSignalComboBox.SelectedIndex = (int)_input.SignalType;
            return;
        }

        Trace.WriteLine("SIGNAL CHANGED! EVERYTHING DISCONNECTED! THE WORLD IS ABOUT TO COLLAPSE!!!");

        _input.SignalType = (InputSignal.Type)InputSignalComboBox.SelectedIndex;
        InputSignalContentControl.Content = GetSignalControl(_input.InputSignal);
    }

    private void InputActionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (InputActionComboBox.SelectedIndex == (int)_input.ActionType)
            return;

        var result = MessageBox.Show("Changing action type will delete all current data. Continue?", "Confirm change?", MessageBoxButton.YesNo);
        if (result == MessageBoxResult.No)
        {
            InputActionComboBox.SelectedIndex = (int)_input.ActionType;
            return;
        }

        Trace.WriteLine("SIGNAL CHANGED! EVERYTHING DISCONNECTED! THE WORLD IS ABOUT TO COLLAPSE!!!");

        _input.ActionType = (InputAction.Type)InputActionComboBox.SelectedIndex;
        InputActionContentControl.Content = GetActionControl(_input.InputAction);
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();

    /// <summary>
    /// Maps InputSignal objects to their corresponding UserControl instances
    /// </summary>
    private UserControl? GetSignalControl(InputSignal inputSignal) => inputSignal switch
    {
        InputSignal_Key signalKey => new InputSignal_Key_Uctrl { DataContext = signalKey },
        InputSignal_Rotary signalRotary => new InputSignal_Rotary_Uctrl { DataContext = signalRotary },
        _ => null
    };

    /// <summary>
    /// Maps InputAction objects to their corresponding UserControl instances
    /// </summary>
    private UserControl? GetActionControl(InputAction inputAction) => inputAction switch
    {
        InputAction_SimulateKeyboardKeys actionKeys => new InputAction_SimulateKeyboardKeys_Uctrl { DataContext = actionKeys },
        InputAction_ClickMouseButton actionClick => new InputAction_ClickMouseButton_Uctrl { DataContext = actionClick },
        InputAction_RollMouseWheel actionWheel => new InputAction_RollMouseWheel_Uctrl { DataContext = actionWheel },
        InputAction_ChangeLayer actionLayer => new InputAction_ChangeLayer_Uctrl { DataContext = actionLayer },
        InputAction_WriteText actionText => new InputAction_WriteText_Uctrl { DataContext = actionText },
        _ => null
    };

    /// <summary>
    /// Drag and move window anywhere on screen
    /// </summary>
    private void Background_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) =>
        DragMove();
}
