using AnumoDriver.Controllers;
using AnumoDriver.Designs;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

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

    //For getting app name & icons
    private DispatcherTimer _dispatcherTimer;
    private DispatcherTimer _dispatcherTimer2;

    private bool _isWindowPinned = false;
    //SerialPort serialPort;

    private int _currentFocusingAppId = 0;

    public MainWindow(SerialCommunicator connector, InputController inputManager, AppFocusHandler appMonitor)
    {
        _communicator = connector;
        _inputController = inputManager;
        _appFocusHandler = appMonitor;

        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        Trace.WriteLine("Starting up the app!");
        RefreshPortList();
        StartDispatcherTimer();
    }

    private void RefreshPortList()
    {
        var ports = _communicator.GetAvailablePorts();
        PortComboBox.ItemsSource = ports;
        if (ports.Length > 0)
            PortComboBox.SelectedIndex = 0;
    }

    /// <summary>
    /// Run a timer to get focusing app, input from controller board, etc.
    /// </summary>
    public void StartDispatcherTimer()
    {
        _dispatcherTimer = new DispatcherTimer();

        _dispatcherTimer.Tick += (s, e) =>
        {
            TickMonitorApp();
            TickInputData();
        };
        _dispatcherTimer.Start();

        //_dispatcherTimer2 = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(0.25) };
        //_dispatcherTimer2.Tick += (s, e) =>
        //{
        //    TickMonitorApp();
        //};
        //_dispatcherTimer2.Start();
    }

    public void TickMonitorApp()
    {
        _appFocusHandler.GetFocusName();

        if (_appFocusHandler.AppId == _currentFocusingAppId)
            return;

        if (_appFocusHandler.AppName != _inputController.CurrentAppLayer.Name)
        {
            _inputController.ChangeAppLayer(_appFocusHandler.AppName);

            AppNameText.Text = _inputController.CurrentAppLayer.Name;
            AppIcon.Source = _inputController.CurrentAppLayer.IconImage;
        }

        //AppNameText.Text = _appFocusHandler.AppName;
        //AppIcon.Source = _appFocusHandler.AppIcon;

        _currentFocusingAppId = _appFocusHandler.AppId;
    }

    public void TickInputData()
    {
        // Check for input data from serial communicator
        if (_communicator.ReceiveInputData(out byte[] data))
        {
            _inputController.GetInput(data);

            // Format bytes as 8-bit binary strings
            string displayText = string.Join(
                ", ",
                data.Select(b => Convert.ToString(b, 2).PadLeft(8, '0'))
            ); 

            InputDataTextBox.Text = $"[{DateTime.Now:HH:mm:ss.fff}] {displayText}";
        }

        _inputController.Loop();
    }


    /// <summary>
    /// Drag and move window anywhere on screen
    /// </summary>
    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (!_isWindowPinned)
        {
            DragMove();
        }
    }

    #region Buttons
    
    private void SetupButton_Click(object sender, RoutedEventArgs e)
    {
        InputEditorWindow iEditorWindow = new(_inputController);
        iEditorWindow.Show();
    }

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
                MessageBox.Show("Successfully connected to device on port: " + selectedPort, "SUCCESS");
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

    private void PinButton_Click(object sender, RoutedEventArgs e)
    {
        _isWindowPinned = !_isWindowPinned;

        if (_isWindowPinned)
        {
            PinButton.Background = new SolidColorBrush(Color.FromRgb(128, 128, 128));
            TitleBar.Cursor = Cursors.Arrow;
        }
        else
        {
            PinButton.Background = new SolidColorBrush(Colors.Transparent);
            TitleBar.Cursor = Cursors.SizeAll;
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        _communicator.Disconnect();
        Close();
    }

    #endregion

    protected override void OnClosed(EventArgs e)
    {
        _dispatcherTimer?.Stop();
        _communicator.Disconnect();
        _communicator?.Dispose();
    }
}