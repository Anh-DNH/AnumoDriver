using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AnumoDriver.Controllers;

public class AppFocusHandler : IDisposable, INotifyPropertyChanged
{
    public int AppId
    {
        get => _appId;
        private set => SetProperty(ref _appId, value);
    }
    private int _appId;

    public string AppName
    {
        get => _appName;
        private set => SetProperty(ref _appName, value);
    }
    private string _appName;

    public ImageSource AppIcon
    {
        get => _appIcon;
        private set => SetProperty(ref _appIcon, value);
    }
    private ImageSource _appIcon;

    public event Action OnFocusingOnAnApp;

    #region Win32 API stuff
    private const uint EVENT_SYSTEM_FOREGROUND = 0x0003;
    private const uint WINEVENT_OUTOFCONTEXT = 0;

    #region Event hooking stuff
    // Delegate must be kept alive as a class-level variable to prevent Garbage Collection
    private WinEventDelegate _delegate;
    private IntPtr _hook;

    // P/Invoke signatures
    delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

    [DllImport("user32.dll")]
    static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

    [DllImport("user32.dll")]
    static extern bool UnhookWinEvent(IntPtr hWinEventHook);
    #endregion

    [DllImport("user32.dll")]
    static extern IntPtr GetForegroundWindow();

    #region Icon stuff
    [DllImport("user32.dll")]
    static extern IntPtr LoadIcon(IntPtr hInstance, IntPtr lpIconName);

    [DllImport("user32.dll", SetLastError = true)]
    static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    [DllImport("user32.dll")]
    static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", EntryPoint = "GetClassLong")]
    static extern uint GetClassLong32(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", EntryPoint = "GetClassLongPtr")]
    static extern IntPtr GetClassLong64(IntPtr hWnd, int nIndex);

    static IntPtr GetClassLongPtr(IntPtr hWnd, int nIndex)
    {
        if (IntPtr.Size == 4)
            return new IntPtr((long)GetClassLong32(hWnd, nIndex));
        else
            return GetClassLong64(hWnd, nIndex);
    }
    #endregion
    #endregion

    public AppFocusHandler()
    {
        //Initialize the hook
        _delegate = new WinEventDelegate(WinEventProc);
        _hook = SetWinEventHook(
            EVENT_SYSTEM_FOREGROUND,
            EVENT_SYSTEM_FOREGROUND,
            IntPtr.Zero,
            _delegate,
            0, 0,
            WINEVENT_OUTOFCONTEXT
        );
    }

    private void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
    {
        // This runs whenever the foreground window changes
        GetFocusIdAndName(hwnd);
        GetSmallWindowIcon(hwnd);

        OnFocusingOnAnApp?.Invoke();

        // Log or handle the change
        //Trace.WriteLine($"Foreground window changed to: {AppName} (HWND: {hwnd})");
    }


    /// <summary>
    /// Get the name of the currently focused app, using app's ID
    /// </summary>
    private void GetFocusIdAndName(IntPtr hWnd)
    {
        if (hWnd != IntPtr.Zero && GetWindowThreadProcessId(hWnd, out uint processId) != 0)
        {
            AppId = (int)processId;

            using (Process process = Process.GetProcessById((int)processId))
            {
                AppName = process.ProcessName;
            }
        }
        else
            AppName = "Unknown";
    }

    /// <summary>
    /// Get the icon of the focusing app
    /// </summary>
    /// <param name="hWnd"></param>
    private void GetSmallWindowIcon(IntPtr hWnd)
    {
        try
        {
            uint WM_GETICON = 0x007f;
            IntPtr ICON_SMALL2 = new IntPtr(2);
            IntPtr IDI_APPLICATION = new IntPtr(0x7F00);
            int GCL_HICON = -14;

            IntPtr hIcon = default(IntPtr);

            hIcon = SendMessage(hWnd, WM_GETICON, ICON_SMALL2, IntPtr.Zero);

            if (hIcon == IntPtr.Zero)
                hIcon = GetClassLongPtr(hWnd, GCL_HICON);

            if (hIcon == IntPtr.Zero)
                hIcon = LoadIcon(IntPtr.Zero, IDI_APPLICATION);

            if (hIcon != IntPtr.Zero)
                AppIcon = IconToImageSource(Icon.FromHandle(hIcon));
        }
        catch (Exception)
        {
        }
    }

    /// <summary>
    /// Convert image from Icon to ImageSource
    /// </summary>
    /// <param name="icon"></param>
    /// <returns></returns>
    private ImageSource IconToImageSource(Icon icon)
    {
        return Imaging.CreateBitmapSourceFromHIcon(
            icon.Handle,
            new Int32Rect(0, 0, icon.Width, icon.Height),
            BitmapSizeOptions.FromEmptyOptions());
    }

    public void Dispose()
    {
        UnhookWinEvent(_hook);
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
