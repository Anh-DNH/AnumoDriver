using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace AnumoDriver.Controllers;

public class AppFocusHandler
{
    public int AppId { get; set; }
    public string AppName { get; private set; }
    public ImageSource AppIcon { get; private set; }


    [DllImport("user32.dll")]
    static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll", SetLastError = true)]
    static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    [DllImport("user32.dll")]
    static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    static extern IntPtr LoadIcon(IntPtr hInstance, IntPtr lpIconName);

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

    /// <summary>
    /// Get the name of the currently focused app
    /// </summary>
    public void GetFocusName()
    {
        try
        {
            IntPtr foregroundWindow = GetForegroundWindow();
            if (foregroundWindow != IntPtr.Zero && GetWindowThreadProcessId(foregroundWindow, out uint processId) != 0)
            {
                AppId = (int)processId;
                using (Process process = Process.GetProcessById((int)processId))
                {
                    AppName = process.ProcessName;
                }
            }
            else
            {
                AppName = "Unknown";
            }

            GetSmallWindowIcon(foregroundWindow);
        }
        catch (Exception)
        {
            AppName = "Error reading app";
        }
    }

    /// <summary>
    /// Get the icon of the focusing app
    /// </summary>
    /// <param name="hWnd"></param>
    void GetSmallWindowIcon(IntPtr hWnd)
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
}
