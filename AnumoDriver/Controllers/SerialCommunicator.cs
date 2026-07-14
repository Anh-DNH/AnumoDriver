using System.ComponentModel;
using System.Diagnostics;
using System.IO.Ports;
using System.Runtime.CompilerServices;
using System.Text;

namespace AnumoDriver.Controllers;

public class SerialCommunicator : INotifyPropertyChanged
{
    public SerialPort ActivePort { get; private set; }

    public string DeviceName
    {
        get => _deviceName;
        private set => SetProperty(ref _deviceName, value);
    }
    private string _deviceName;

    public enum ConnectionStatus
    {
        Disconnected,
        Connecting,
        Connected
    }
    public ConnectionStatus ConnectStatus { get; private set; } = ConnectionStatus.Disconnected;

    public event Action<byte[]> OnReceiveInputData;

    private byte[] _connectKeys = [0, 0, 0];

    private const byte COMMUTYPE_OHAYO = 1;
    //private const byte COMMUTYPE_ADIOS = 2;
    private const byte COMMUTYPE_NAME = 2;
    private const byte COMMUTYPE_INPUT = 3;
    private const byte COMMUTYPE_MAX = 4;


    public string[] GetAvailablePorts() => SerialPort.GetPortNames();

    public async Task ConnectToPort(string portName)
    {
        //  --  --  SETUP   --  --  //
        ConnectStatus = ConnectionStatus.Connecting;
        var canceller = new CancellationTokenSource();

        var checkPort = new SerialPort
        {
            PortName = portName,
            BaudRate = 115200,

            WriteTimeout = 1000,

            RtsEnable = true,
            DtrEnable = true,

            NewLine = "\n",
        };

        checkPort.DataReceived += async (sender, e) =>
        {
            if (sender is not SerialPort port)
                return;

            if (ConnectStatus == ConnectionStatus.Connected)
                ReceivedDataHandler(port);
            else
                ConnectionHandler(port, canceller);
        };

        //  --  --  START CONNECTION    --  --  //
        try
        {
            //Try to open the port and connect asynchronously to avoid blocking the UI thread
            await Task.Run(
                () => {
                    try
                    {
                        Trace.WriteLine($"Attempt connecting to {checkPort.PortName}");
                        checkPort.Open();

                        Random r = new();
                        _connectKeys = [
                            (byte)r.Next(COMMUTYPE_MAX, 256),
                            (byte)r.Next(256),
                            (byte)r.Next(256),
                        ];
                        checkPort.Write(_connectKeys, 0, 3);

                        Trace.WriteLine(
                            $"PC->{checkPort.PortName}: {
                                Convert.ToString(_connectKeys[0], 2).PadLeft(8, '0')
                            }, {
                                Convert.ToString(_connectKeys[1], 2).PadLeft(8, '0')
                            }, {
                                Convert.ToString(_connectKeys[2], 2).PadLeft(8, '0')
                            }"
                        );
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine($"Exception occur while trying to connect to {portName}:\t-> {ex.Message}");
                        checkPort.Close();
                        ConnectStatus = ConnectionStatus.Disconnected;
                        canceller.Cancel();
                    }
                }
            );

            //Wait for 1 seconds or cancellation.
            await Task.Delay(2000, canceller.Token);

            //Beyond this line is the code that will handle timed out process.

            if (ConnectStatus != ConnectionStatus.Connected)
            {
                Trace.WriteLine($"Connection to {portName} timed out.");
                checkPort.Close();
                ConnectStatus = ConnectionStatus.Disconnected;
            }
        }
        catch (TaskCanceledException) //What to do when the task is cancelled.
        {
            Trace.WriteLine($"{portName} cancelled task!");
            if (ConnectStatus != ConnectionStatus.Connected)
                ConnectStatus = ConnectionStatus.Disconnected;
        }

        Trace.WriteLine($"FINISH CONNECTION TASK. CONNECTION STATUS: {ConnectStatus}");
    }

    void ConnectionHandler(SerialPort port, CancellationTokenSource canceller)
    {
        if (ConnectStatus != ConnectionStatus.Connecting)
            return;

        byte[] keys = new byte[3];

        port.Read(keys, 0, 3);
        Trace.WriteLine(
            $"{port.PortName}->PC: {
                Convert.ToString(keys[0], 2).PadLeft(8, '0')
            }, {
                Convert.ToString(keys[1], 2).PadLeft(8, '0')
            }, {
                Convert.ToString(keys[2], 2).PadLeft(8, '0')
            }"
        );

        var result = keys[0] == (_connectKeys[0] ^ 255);
        result = keys[1] == (_connectKeys[1] ^ 255) && result;
        result = keys[2] == (_connectKeys[2] ^ 255) && result;

        if (result)
        {
            port.Write([COMMUTYPE_OHAYO], 0, 1);
            ActivePort = port;
            ConnectStatus = ConnectionStatus.Connected;

            Trace.WriteLine($"SUCCESSFULLY CONNECTED TO DEVICE ON PORT {port.PortName}");
        }
        else
        {
            Trace.WriteLine($"FAILED TO CONNECT TO DEVICE ON PORT {port.PortName}. KEY IS MISMATCH");
            ConnectStatus = ConnectionStatus.Disconnected;
        }

        canceller.Cancel();
    }

    void ReceivedDataHandler(SerialPort port)
    {
        if (!port.IsOpen)
            return;

        //Trace.Write($"Received data from port {port.PortName}. ");

        var byteLen = port.BytesToRead;
        byte[] data = new byte[byteLen];
        port.Read(data, 0, byteLen);

        //if (data[0] == COMMUTYPE_MSG) //Communication message
        //{
        //    Trace.WriteLine($"Type: MESSAGE");
        //    Trace.Write($"{port.PortName}->PC: ");

        //    string chars = Encoding.UTF8.GetString(
        //        data[1..data.Length]
        //    );
        //    Trace.WriteLine(chars);
        //}
        //else 
        if (data[0] == COMMUTYPE_NAME) //Get name
        {
            DeviceName = Encoding.UTF8.GetString(data[1..data.Length]);
            //Trace.WriteLine($"Type: NAME");
            //Trace.WriteLine(DeviceName);
        }
        else
        if (data[0] == COMMUTYPE_INPUT) // Usually input data
        {
            //Trace.WriteLine($"Type: INPUT");
            OnReceiveInputData.Invoke(data[1..data.Length]);
        }
        //else
        //{
        //    Trace.WriteLine($"Type: UNKNOWN");
        //    Trace.WriteLine(data);
        //}
    }



    //public bool ReceiveInputData(out byte[] data)
    //{
    //    data = [];
    //    if (_receivedInputData.Count != 0)
    //    {
    //        data = _receivedInputData[0];
    //        _receivedInputData.RemoveAt(0);
    //        return true;
    //    }
    //    return false;
    //}

    public void Disconnect()
    {
        if (ConnectStatus != ConnectionStatus.Connected)
            return;
        if (ActivePort is null)
            return;
        if (!ActivePort.IsOpen)
            return;

        ActivePort.Close();
        ActivePort.Dispose();
        ActivePort = null;
        ConnectStatus = ConnectionStatus.Disconnected;
    }

    #region INotifyPropertyChanged stuff
    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
    #endregion
}