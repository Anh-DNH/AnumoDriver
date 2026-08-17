using AnumoDriver.Controllers;
using AnumoDriver.Designs;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace AnumoDriver;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    //Services
    private readonly SerialCommunicator _communicator;
    private readonly InputController _inputController;
    private readonly AppFocusHandler _appFocusHandler;
    private readonly GlobalTick _globalTick;
    private readonly InternalSave _interalSave;


    private InputEditorWindow _iEditorWindow;

    private bool _isWindowPinned = false;

    public MainWindow(
        SerialCommunicator connector,
        InputController inputManager,
        AppFocusHandler appMonitor,
        GlobalTick globalTick,
        InternalSave internalSave
    )
    {
        _communicator = connector;
        _inputController = inputManager;
        _appFocusHandler = appMonitor;
        _globalTick = globalTick;
        _interalSave = internalSave;

        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        Trace.WriteLine("Starting up the app!");

        var ports = _communicator.GetAvailablePorts();
        PortComboBox.ItemsSource = ports;
        if (ports.Length > 0 && !string.IsNullOrEmpty(_interalSave.CurrentSerialGate))
        {
            PortComboBox.SelectedItem = _interalSave.CurrentSerialGate;
            ConnectButton_Click(sender, e);
        }
        else
            PortComboBox.SelectedIndex = 0;

        _communicator.OnReceiveInputData += GetIncomingInputData;
        _appFocusHandler.OnFocusingOnAnApp += ChangingFocusingAppData;
        ChangingFocusingAppData();

        _globalTick.OnTick += _inputController.Loop;

        Left = _interalSave.MainWindowPosX;
        Top = _interalSave.MainWindowPosY;

        _isWindowPinned = _interalSave.MainWindowPinned;
        PinToggleButton.IsChecked = _isWindowPinned;

        Topmost = _interalSave.MainWindowTopmost;
        TopMostToggleButton.IsChecked = Topmost;

    }

    private void GetIncomingInputData(byte[] inputData) =>
        _inputController.GetInput(inputData);

    private void ChangingFocusingAppData()
    {
        _inputController.ChangeAppLayer(_appFocusHandler.AppName);

        var binding = new Binding("InputLayerIndex")
        {
            Mode = BindingMode.OneWay,
            Source = _inputController.CurrentAppLayer
        };
        BindingOperations.SetBinding(LayerNumberText, TextBlock.TextProperty, binding);

        AppNameText.Text = _inputController.CurrentAppLayer.Name;
        AppIcon.Source = _inputController.CurrentAppLayer.IconImage;
    }

    #region Buttons

    private async void ConnectButton_Click(object sender, RoutedEventArgs e)
    {
        // If currently disconnected, start a connection
        if (_communicator.ConnectStatus == SerialCommunicator.ConnectionStatus.Disconnected)
        {
            if (PortComboBox.SelectedItem is not string selectedPort)
            {
                MessageBox.Show("Please select a port first.", "No Port Selected");
                return;
            }

            ConnectButton.IsEnabled = false;
            ConnectButton.Content = "Connecting...";

            await _communicator.ConnectToPort(selectedPort);

            if (_communicator.ConnectStatus == SerialCommunicator.ConnectionStatus.Connected)
            {
                ConnectButton.Content = "Disconnect";
                ConnectButton.IsEnabled = true;
                DeviceNameText.Text = _communicator.DeviceName ?? "Unknown device";
                //MessageBox.Show("Successfully connected to device on port: " + selectedPort, "SUCCESS");
            }
            else
            {
                ConnectButton.Content = "Connect";
                ConnectButton.IsEnabled = true;
                MessageBox.Show("Failed to connect to device on port: " + selectedPort, "FAILED");
            }
        }
        else if (_communicator.ConnectStatus == SerialCommunicator.ConnectionStatus.Connected)
        {
            // Currently connected - disconnect
            _communicator.Disconnect();
            ConnectButton.Content = "Connect";
            ConnectButton.IsEnabled = true;
            DeviceNameText.Text = "No device";
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        _communicator.Disconnect();

        _iEditorWindow?.Close();

        Close();
    }
    private void SetupButton_Click(object sender, RoutedEventArgs e)
    {
        if (_iEditorWindow == null)
        {
            _iEditorWindow = new(_inputController);
            _iEditorWindow.Closed += (e, i) =>
            {
                _iEditorWindow = null;
            };
            _iEditorWindow.Show();
        }
        else
        {
            _iEditorWindow.Focus();
        }
    }

    private void PinToggleButton_Click(object sender, RoutedEventArgs e)
    {
        _isWindowPinned = !_isWindowPinned;
        PinToggleButton.IsChecked = _isWindowPinned;
        _interalSave.MainWindowPinned = _isWindowPinned;
    }

    private void TopMostToggleButton_Click(object sender, RoutedEventArgs e)
    {
        Topmost = !Topmost;
        TopMostToggleButton.IsChecked = Topmost;
        _interalSave.MainWindowTopmost = Topmost;
    }

    #endregion

    /// <summary>
    /// Drag and move window anywhere on screen
    /// </summary>
    private void Background_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (!_isWindowPinned)
            DragMove();
        _interalSave.MainWindowPosX = (int)Left;
        _interalSave.MainWindowPosY = (int)Top;
    }

    /// <summary>
    /// Refresh port list everytime we open combo box to select
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void PortComboBox_DropDownOpened(object sender, EventArgs e) =>
        PortComboBox.ItemsSource = _communicator.GetAvailablePorts();

    protected override void OnClosed(EventArgs e)
    {
        //_dispatcherTimer?.Stop();
        _communicator.Disconnect();
        _appFocusHandler.Dispose();

        base.OnClosed(e);
    }
}
