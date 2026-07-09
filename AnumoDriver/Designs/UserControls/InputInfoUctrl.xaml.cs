using AnumoDriver.Structures.INPUT;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace AnumoDriver.Designs.UserControls;

/// <summary>
/// Interaction logic for InputUctrl.xaml
/// </summary>
public partial class InputInfoUctrl : UserControl
{
    private Input? _input;

    public ObservableCollection<string> SignalTypes { get; private set; }
    public ObservableCollection<string> ActionTypes { get; private set; }

    // ContentControl references for dynamic content
    private ContentControl? _signalContentControl;
    private ContentControl? _actionContentControl;

    public InputInfoUctrl()
    {
        DataContextChanged += (s, e) => OnDataContextChanged();
        _input = DataContext as Input;

        // Initialize enum collections
        SignalTypes = new ObservableCollection<string>(
            Enum.GetNames(typeof(InputSignal.Type))
        );

        ActionTypes = new ObservableCollection<string>(
           Enum.GetNames(typeof(InputAction.Type))
        );

        InitializeComponent();

        // Get ContentControl references after InitializeComponent
        _signalContentControl = FindName("SignalContentControl") as ContentControl;
        _actionContentControl = FindName("ActionContentControl") as ContentControl;

        // Load initial controls if DataContext is already set
        if (_input != null)
        {
            LoadSignalControl();
            LoadActionControl();
        }
    }

    /// <summary>
    /// Called when DataContext changes - captures the Input object and subscribes to changes
    /// </summary>
    private void OnDataContextChanged()
    {
        if (_input is INotifyPropertyChanged oldInput)
            oldInput.PropertyChanged -= Input_PropertyChanged;

        _input = DataContext as Input;

        if (_input is INotifyPropertyChanged newInput)
            newInput.PropertyChanged += Input_PropertyChanged;

        // Load initial controls
        LoadSignalControl();
        LoadActionControl();
    }

    /// <summary>
    /// Handles when Input properties change (e.g., SignalType or ActionType changes)
    /// </summary>
    private void Input_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Input.SignalType))
            LoadSignalControl();
        else if (e.PropertyName == nameof(Input.ActionType))
            LoadActionControl();
    }

    /// <summary>
    /// Dynamically loads the appropriate signal UserControl based on SignalType
    /// </summary>
    private void LoadSignalControl()
    {
        if (_input?.InputSignal == null)
            return;

        var signalControl = GetSignalControl(_input.InputSignal);
        if (signalControl != null && _signalContentControl != null)
        {
            _signalContentControl.Content = signalControl;
        }
    }

    /// <summary>
    /// Dynamically loads the appropriate action UserControl based on ActionType
    /// </summary>
    private void LoadActionControl()
    {
        if (_input?.InputAction == null)
            return;

        var actionControl = GetActionControl(_input.InputAction);
        if (actionControl != null && _actionContentControl != null)
        {
            _actionContentControl.Content = actionControl;
        }
    }

    /// <summary>
    /// Maps InputSignal objects to their corresponding UserControl instances
    /// </summary>
    private UserControl? GetSignalControl(InputSignal inputSignal)
    {
        return inputSignal switch
        {
            InputSignal_Key signalKey => new InputSignal_Key_Uctrl { DataContext = signalKey },
            InputSignal_Rotary signalRotary => new InputSignal_Rotary_Uctrl { DataContext = signalRotary },
            _ => null
        };
    }

    /// <summary>
    /// Maps InputAction objects to their corresponding UserControl instances
    /// </summary>
    private UserControl? GetActionControl(InputAction inputAction)
    {
        return inputAction switch
        {
            InputAction_SimulateKeyboardKeys actionKeys => new InputAction_SimulateKeyboardKeys_Uctrl { DataContext = actionKeys },
            InputAction_RollMouseWheel actionWheel => new InputAction_RollMouseWheel_Uctrl { DataContext = actionWheel },
            InputAction_ChangeLayer actionLayer => new InputAction_ChangeLayer_Uctrl { DataContext = actionLayer },
            InputAction_WriteText actionText => new InputAction_WriteText_Uctrl { DataContext = actionText },
            InputAction_ClickMouseButton actionClick => new InputAction_ClickMouseButton_Uctrl { DataContext = actionClick },
            _ => null
        };
    }

    private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
    }
}

