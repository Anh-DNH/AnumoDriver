using System.Diagnostics;
using System.IO.Ports;
using System.Windows.Interop;

namespace AnumoDriver.Controllers;

public class SerialCommunicator : IDisposable
{
    public SerialPort ActivePort { get; private set; }
    public string DeviceName { get; private set; }
    //public string PortName => ActivePort.PortName;

    private List<byte[]> _receivedInputData = [];

    public enum ConnectionStatus
    {
        Disconnected,
        Connecting,
        GetName,
        Connected
    }
    public ConnectionStatus ConnectStatus { get; private set; } = ConnectionStatus.Disconnected;

    private byte[] connectKeys = [0, 0, 0];

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
                ReceivedDataHanlder(port);
            else
                ConnectionHandler(port, canceller);
        };

        //  --  --  START CONNECTION    --  --  //
        try
        {
            //Try to open the port and connect asynchronously to avoid blocking the UI thread
            await Task.Run(
                () =>
                {
                    try
                    {
                        Trace.WriteLine($"Attempt connecting to {checkPort.PortName}");
                        checkPort.Open();

                        Random r = new();
                        connectKeys = [
                            (byte)r.Next(256),
                            (byte)r.Next(256),
                            (byte)r.Next(256),
                        ];
                        checkPort.Write(connectKeys, 0, 3);

                        Trace.WriteLine(
                            $"PC->{checkPort.PortName}: {
                                Convert.ToString(connectKeys[0], 2)
                            }, {
                                Convert.ToString(connectKeys[1], 2)
                            }, {
                                Convert.ToString(connectKeys[2], 2)
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
            await Task.Delay(1000, canceller.Token);

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
        if (ConnectStatus == ConnectionStatus.Connecting)
        {
            if (port.BytesToRead == 3)
            {
                byte[] keys = new byte[3];

                port.Read(keys, 0, 3);
                Trace.WriteLine(
                    $"{port.PortName}->PC: {Convert.ToString(keys[0], 2)}, {Convert.ToString(keys[1], 2)}, {Convert.ToString(keys[2], 2)}"
                );

                var result = keys[0] == (connectKeys[0] ^ 255);
                result = keys[1] == (connectKeys[1] ^ 255) && result;
                result = keys[2] == (connectKeys[2] ^ 255) && result;

                if (result)
                {
                    port.Write("ANUMO.OKAY");
                    Trace.WriteLine($"SUCCESSFULLY CONNECTED TO DEVICE ON PORT {port.PortName}");
                    ActivePort = port;
                    ConnectStatus = ConnectionStatus.GetName;
                }
                else
                {
                    Trace.WriteLine($"FAILED TO DEVICE ON PORT {port.PortName}. KEY IS MISMATCH");
                    ConnectStatus = ConnectionStatus.Disconnected;
                }
            }
        }
        else if (ConnectStatus == ConnectionStatus.GetName)
        {
            DeviceName = port.ReadLine();
            ConnectStatus = ConnectionStatus.Connected;
            canceller.Cancel();
        }
    }

    void ReceivedDataHanlder(SerialPort port)
    {
        try
        {
            var byteLen = port.BytesToRead;
            byte[] keys = new byte[byteLen];
            port.Read(keys, 0, byteLen);

            if (byteLen > 6)    // Usually communication message
            {
                Trace.Write($"{port.PortName}->PC: ");

                string chars = System.Text.Encoding.UTF8.GetString(keys);
                Trace.WriteLine(chars);
            }
            else                // Usually input data
            {
                ////For debug
                //Trace.Write($"{port.PortName}->PC: keys = [ ");
                //for (var i = 0; i < keys.Length; i++)
                //{
                //    Trace.Write(Convert.ToString(keys[i], 2));

                //    if (i == keys.Length - 1)
                //        Trace.Write(" ");
                //    else
                //        Trace.Write(", ");
                //}
                //Trace.WriteLine($"];");

                //Safe data to buffer for later get
                _receivedInputData.Add(keys);
            }
        }
        catch (Exception ex)
        {
            Trace.WriteLine(ex.ToString());
        }
    }


    public bool ReceiveInputData(out byte[] data)
    {
        data = [];
        if (_receivedInputData.Count != 0)
        {
            data = _receivedInputData[0];
            _receivedInputData.RemoveAt(0);
            return true;
        }
        return false;
    }

    public void Disconnect()
    {
        if (ConnectStatus != ConnectionStatus.Connected)
            return;
        if (ActivePort is null)
            return;
        if (!ActivePort.IsOpen)
            return;

        ActivePort.Write("ANUMO.ADIOS");
        Trace.WriteLine($"PC->{ActivePort.PortName}: ANUMO.ADIOS");
        ActivePort.Close();
        ActivePort.Dispose();
        ActivePort = null;
        ConnectStatus = ConnectionStatus.Disconnected;

    }

    public void Dispose()
    {
    }
}