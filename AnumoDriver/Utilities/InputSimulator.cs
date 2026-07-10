using AnumoDriver.Structures.INPUT;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Controls;
using static System.Runtime.CompilerServices.RuntimeHelpers;

namespace AnumoDriver.Utilities;

/// <summary>
/// Handles Win32 API keyboard input simulation for key press, release, and hold operations.
/// </summary>
public class InputSimulator : IDisposable
{
    private const int INPUT_MOUSE = 0;
    private const int INPUT_KEYBOARD = 1;

    private const uint KEYEVENTF_KEYDOWN = 0x0000;
    private const uint KEYEVENTF_KEYEXT = 0x0001;
    private const uint KEYEVENTF_KEYUP = 0x0002;
    private const uint KEYEVENTF_UNICODE = 0x0004;

    private const uint MOUSEF_MOVE = 0x0001;
    private const uint MOUSEF_LEFTDW = 0x0002;
    private const uint MOUSEF_LEFTUP = 0x0004;
    private const uint MOUSEF_RIGHTDW = 0x0008;
    private const uint MOUSEF_RIGHTUP = 0x0010;
    private const uint MOUSEF_MIDDLEDW = 0x0020;
    private const uint MOUSEF_MIDDLEUP = 0x0040;
    private const uint MOUSEF_XDW = 0x0080;
    private const uint MOUSEF_XUP = 0x0100;
    private const uint MOUSEF_VWHEEL = 0x0800;

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern void keybd_event(byte keyCode, byte scan, uint flag, int extraInfo);
    
    //[DllImport("user32.dll", SetLastError = true)]
    //private static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);



    [StructLayout(LayoutKind.Sequential)]
    private struct INPUT
    {
        public uint type;
        public InputUnion data;
    }


    [StructLayout(LayoutKind.Explicit)]
    private struct InputUnion
    {
        [FieldOffset(0)]
        public MOUSEINPUT mouse;

        [FieldOffset(0)]
        public KEYBDINPUT keyboard;

        [FieldOffset(0)]
        public HARDWAREINPUT hardware;
    }

    /// <summary>
    /// The KEYBDINPUT structure contains information about a simulated keyboard event.  (see: http://msdn.microsoft.com/en-us/library/ms646271(VS.85).aspx)
    /// Declared in Winuser.h, include Windows.h
    /// </summary>
    /// <remarks>
    /// Windows 2000/XP: INPUT_KEYBOARD supports nonkeyboard-input methodssuch as handwriting recognition or voice recognitionas if it were text input by using the KEYEVENTF_UNICODE flag. If KEYEVENTF_UNICODE is specified, SendInput sends a WM_KEYDOWN or WM_KEYUP message to the foreground thread's message queue with wParam equal to VK_PACKET. Once GetMessage or PeekMessage obtains this message, passing the message to TranslateMessage posts a WM_CHAR message with the Unicode character originally specified by wScan. This Unicode character will automatically be converted to the appropriate ANSI value if it is posted to an ANSI window.
    /// Windows 2000/XP: Set the KEYEVENTF_SCANCODE flag to define keyboard input in terms of the scan code. This is useful to simulate a physical keystroke regardless of which keyboard is currently being used. The virtual key value of a key may alter depending on the current keyboard layout or what other keys were pressed, but the scan code will always be the same.
    /// </remarks>
    private struct KEYBDINPUT
    {
        /// <summary>
        /// Specifies a virtual-key code. The code must be a value in the range 1 to 254. The Winuser.h header file provides macro definitions (VK_*) for each value. If the dwFlags member specifies KEYEVENTF_UNICODE, wVk must be 0. 
        /// </summary>
        public ushort KeyCode;

        /// <summary>
        /// Specifies a hardware scan code for the key. If dwFlags specifies KEYEVENTF_UNICODE, wScan specifies a Unicode character which is to be sent to the foreground application. 
        /// </summary>
        public ushort Scan;

        /// <summary>
        /// Specifies various aspects of a keystroke. This member can be certain combinations of the following values.
        /// KEYEVENTF_EXTENDEDKEY - If specified, the scan code was preceded by a prefix byte that has the value 0xE0 (224).
        /// KEYEVENTF_KEYUP - If specified, the key is being released. If not specified, the key is being pressed.
        /// KEYEVENTF_SCANCODE - If specified, wScan identifies the key and wVk is ignored. 
        /// KEYEVENTF_UNICODE - Windows 2000/XP: If specified, the system synthesizes a VK_PACKET keystroke. The wVk parameter must be zero. This flag can only be combined with the KEYEVENTF_KEYUP flag. For more information, see the Remarks section. 
        /// </summary>
        public uint Flags;

        /// <summary>
        /// Time stamp for the event, in milliseconds. If this parameter is zero, the system will provide its own time stamp. 
        /// </summary>
        public uint Time;

        /// <summary>
        /// Specifies an additional value associated with the keystroke. Use the GetMessageExtraInfo function to obtain this information. 
        /// </summary>
        public IntPtr ExtraInfo;
    }

    /// <summary>
    /// The MOUSEINPUT structure contains information about a simulated mouse event. (see: http://msdn.microsoft.com/en-us/library/ms646273(VS.85).aspx)
    /// Declared in Winuser.h, include Windows.h
    /// </summary>
    /// <remarks>
    /// If the mouse has moved, indicated by MOUSEEVENTF_MOVE, dx and dy specify information about that movement. The information is specified as absolute or relative integer values. 
    /// If MOUSEEVENTF_ABSOLUTE value is specified, dx and dy contain normalized absolute coordinates between 0 and 65,535. The event procedure maps these coordinates onto the display surface. Coordinate (0,0) maps onto the upper-left corner of the display surface; coordinate (65535,65535) maps onto the lower-right corner. In a multimonitor system, the coordinates map to the primary monitor. 
    /// Windows 2000/XP: If MOUSEEVENTF_VIRTUALDESK is specified, the coordinates map to the entire virtual desktop.
    /// If the MOUSEEVENTF_ABSOLUTE value is not specified, dx and dy specify movement relative to the previous mouse event (the last reported position). Positive values mean the mouse moved right (or down); negative values mean the mouse moved left (or up). 
    /// Relative mouse motion is subject to the effects of the mouse speed and the two-mouse threshold values. A user sets these three values with the Pointer Speed slider of the Control Panel's Mouse Properties sheet. You can obtain and set these values using the SystemParametersInfo function. 
    /// The system applies two tests to the specified relative mouse movement. If the specified distance along either the x or y axis is greater than the first mouse threshold value, and the mouse speed is not zero, the system doubles the distance. If the specified distance along either the x or y axis is greater than the second mouse threshold value, and the mouse speed is equal to two, the system doubles the distance that resulted from applying the first threshold test. It is thus possible for the system to multiply specified relative mouse movement along the x or y axis by up to four times.
    /// </remarks>
    private struct MOUSEINPUT
    {
        /// <summary>
        /// Specifies the absolute position of the mouse, or the amount of motion since the last mouse event was generated, depending on the value of the dwFlags member. Absolute data is specified as the x coordinate of the mouse; relative data is specified as the number of pixels moved. 
        /// </summary>
        public int X;

        /// <summary>
        /// Specifies the absolute position of the mouse, or the amount of motion since the last mouse event was generated, depending on the value of the dwFlags member. Absolute data is specified as the y coordinate of the mouse; relative data is specified as the number of pixels moved. 
        /// </summary>
        public int Y;

        /// <summary>
        /// If dwFlags contains MOUSEEVENTF_WHEEL, then mouseData specifies the amount of wheel movement. A positive value indicates that the wheel was rotated forward, away from the user; a negative value indicates that the wheel was rotated backward, toward the user. One wheel click is defined as WHEEL_DELTA, which is 120. 
        /// Windows Vista: If dwFlags contains MOUSEEVENTF_HWHEEL, then dwData specifies the amount of wheel movement. A positive value indicates that the wheel was rotated to the right; a negative value indicates that the wheel was rotated to the left. One wheel click is defined as WHEEL_DELTA, which is 120.
        /// Windows 2000/XP: IfdwFlags does not contain MOUSEEVENTF_WHEEL, MOUSEEVENTF_XDOWN, or MOUSEEVENTF_XUP, then mouseData should be zero. 
        /// If dwFlags contains MOUSEEVENTF_XDOWN or MOUSEEVENTF_XUP, then mouseData specifies which X buttons were pressed or released. This value may be any combination of the following flags. 
        /// </summary>
        public int MouseData;

        /// <summary>
        /// A set of bit flags that specify various aspects of mouse motion and button clicks. The bits in this member can be any reasonable combination of the following values. 
        /// The bit flags that specify mouse button status are set to indicate changes in status, not ongoing conditions. For example, if the left mouse button is pressed and held down, MOUSEEVENTF_LEFTDOWN is set when the left button is first pressed, but not for subsequent motions. Similarly, MOUSEEVENTF_LEFTUP is set only when the button is first released. 
        /// You cannot specify both the MOUSEEVENTF_WHEEL flag and either MOUSEEVENTF_XDOWN or MOUSEEVENTF_XUP flags simultaneously in the dwFlags parameter, because they both require use of the mouseData field. 
        /// </summary>
        public uint Flags;

        /// <summary>
        /// Time stamp for the event, in milliseconds. If this parameter is 0, the system will provide its own time stamp. 
        /// </summary>
        public uint Time;

        /// <summary>
        /// Specifies an additional value associated with the mouse event. An application calls GetMessageExtraInfo to obtain this extra information. 
        /// </summary>
        public IntPtr ExtraInfo;
    }

    /// <summary>
    /// The HARDWAREINPUT structure contains information about a simulated message generated by an input device other than a keyboard or mouse.
    /// (see: http://msdn.microsoft.com/en-us/library/ms646269(VS.85).aspx)
    /// Declared in Winuser.h, include Windows.h
    /// </summary>
    /// Author: michaelnoonan
    internal struct HARDWAREINPUT
    {
        /// <summary>
        /// Value specifying the message generated by the input hardware. 
        /// </summary>
        public uint Msg;

        /// <summary>
        /// Specifies the low-order word of the lParam parameter for uMsg. 
        /// </summary>
        public ushort ParamL;

        /// <summary>
        /// Specifies the high-order word of the lParam parameter for uMsg. 
        /// </summary>
        public ushort ParamH;
    }

    private readonly List<VirtualKeyCode> _heldKeys = new();
    private readonly object _lockObject = new();

    /// <summary>
    /// Simulates pressing a key.
    /// </summary>
    public void KeyDown(VirtualKeyCode virtualKeyCode)
    {
        //var input = new INPUT
        //{
        //    type = INPUT_KEYBOARD,
        //    data = new InputUnion
        //    {
        //        keyboard =
        //        new KEYBDINPUT
        //        {
        //            KeyCode = (ushort)virtualKeyCode,
        //            Scan = 0,
        //            Flags = KEYEVENTF_KEYDOWN,
        //            Time = 0,
        //            ExtraInfo = IntPtr.Zero
        //        }
        //    }
        //};

        //_ = SendInput(1, [input], Marshal.SizeOf<INPUT>());

        keybd_event((byte)virtualKeyCode, 0, KEYEVENTF_KEYDOWN, 0);
    }

    /// <summary>
    /// Simulates releasing a key.
    /// </summary>
    public void KeyUp(VirtualKeyCode virtualKeyCode)
    {
        //var input = new INPUT
        //{
        //    type = INPUT_KEYBOARD,
        //    data = new InputUnion
        //    {
        //        keyboard = new KEYBDINPUT
        //        {
        //            KeyCode = (ushort)virtualKeyCode,
        //            Scan = 0,
        //            Flags = KEYEVENTF_KEYUP,
        //            Time = 0,
        //            ExtraInfo = IntPtr.Zero
        //        }
        //    }
        //};

        //_ = SendInput(1, [input], Marshal.SizeOf<INPUT>());

        keybd_event((byte)virtualKeyCode, 0, KEYEVENTF_KEYUP, 0);
    }

    /// <summary>
    /// Simulates pressing a mouse button.
    /// </summary>
    public void MouseDown(int buttonId)
    {
        var flag = buttonId switch
        {
            1 => MOUSEF_LEFTDW,
            2 => MOUSEF_RIGHTDW,
            3 => MOUSEF_MIDDLEDW,
            4 => MOUSEF_XDW,
            5 => MOUSEF_XDW,
            _ => throw new Exception($"Button ID is invalid: {buttonId}")
        };

        buttonId = buttonId > 2 ? buttonId - 2 : 0;

        var input = new INPUT
        {
            type = INPUT_MOUSE,
            data = new InputUnion
            {
                mouse = new MOUSEINPUT
                {
                    Flags = flag,
                    MouseData = buttonId,
                    Time = 0,
                    ExtraInfo = IntPtr.Zero
                }
            }
        };

        _ = SendInput(1, [input], Marshal.SizeOf<INPUT>());
    }

    /// <summary>
    /// Simulates releasing a mouse button.
    /// </summary>
    public void MouseUp(int buttonId)
    {
        var flag = buttonId switch
        {
            1 => MOUSEF_LEFTUP,
            2 => MOUSEF_RIGHTUP,
            3 => MOUSEF_MIDDLEUP,
            4 => MOUSEF_XUP,
            5 => MOUSEF_XUP,
            _ => throw new Exception($"Button ID is invalid: {buttonId}")
        };

        buttonId = buttonId > 2 ? buttonId - 2 : 0;

        var input = new INPUT
        {
            type = INPUT_MOUSE,
            data = new InputUnion
            {
                mouse = new MOUSEINPUT
                {
                    Flags = flag,
                    MouseData = buttonId,
                    Time = 0,
                    ExtraInfo = IntPtr.Zero
                }
            }
        };

        _ = SendInput(1, [input], Marshal.SizeOf<INPUT>());
    }

    /// <summary>
    /// Simulates scrolling a mouse wheel.
    /// </summary>
    public void MouseScroll(int delta)
    {
        var input = new INPUT
        {
            type = INPUT_MOUSE,
            data = new InputUnion
            {
                mouse = new MOUSEINPUT
                {
                    Flags = MOUSEF_VWHEEL,
                    MouseData = delta,
                    Time = 0,
                    ExtraInfo = IntPtr.Zero
                }
            }
        };

        _ = SendInput(1, [input], Marshal.SizeOf<INPUT>());
    }

    /// <summary>
    /// Simulates writing a line of text.
    /// </summary>
    public void WriteText(string text)
    {
        foreach (char c in text.ToCharArray())
        {
            keybd_event(0, (byte)c, KEYEVENTF_UNICODE, 0);
            keybd_event(0, (byte)c, KEYEVENTF_UNICODE | KEYEVENTF_KEYUP, 0);
        }
    }

    ///// <summary>
    ///// Simulates a complete key press (down then up).
    ///// </summary>
    //public void KeyPress(ushort virtualKeyCode, int delayMilliseconds = 10)
    //{
    //    try
    //    {
    //        KeyDown(virtualKeyCode);
    //        Thread.Sleep(delayMilliseconds);
    //        KeyUp(virtualKeyCode);

    //        Trace.WriteLine($"KeyPress: {virtualKeyCode}");
    //    }
    //    catch (Exception ex)
    //    {
    //        Trace.WriteLine($"Error in KeyPress: {ex.Message}");
    //    }
    //}

    ///// <summary>
    ///// Simulates multiple keys being pressed simultaneously.
    ///// </summary>
    //public void MultiKeyPress(ushort[] virtualKeyCodes, int delayMilliseconds = 10)
    //{
    //    try
    //    {
    //        // Press all keys
    //        foreach (var keyCode in virtualKeyCodes)
    //        {
    //            KeyDown(keyCode);
    //        }

    //        Thread.Sleep(delayMilliseconds);

    //        // Release all keys in reverse order
    //        for (int i = virtualKeyCodes.Length - 1; i >= 0; i--)
    //        {
    //            KeyUp(virtualKeyCodes[i]);
    //        }

    //        Trace.WriteLine($"MultiKeyPress: {string.Join(", ", virtualKeyCodes)}");
    //    }
    //    catch (Exception ex)
    //    {
    //        Trace.WriteLine($"Error in MultiKeyPress: {ex.Message}");
    //    }
    //}

    ///// <summary>
    ///// Releases all currently held keys.
    ///// </summary>
    //public void ReleaseAllKeys()
    //{
    //    lock (_lockObject)
    //    {
    //        foreach (var keyCode in _heldKeys)
    //        {
    //            KeyUp(keyCode);
    //        }
    //        _heldKeys.Clear();
    //    }

    //    Trace.WriteLine("Released all keys");
    //}

    /// <summary>
    /// Gets the list of currently held keys.
    /// </summary>
    public IReadOnlyList<VirtualKeyCode> GetHeldKeys()
    {
        lock (_lockObject)
        {
            return _heldKeys.AsReadOnly();
        }
    }

    public void Dispose()
    {
        //ReleaseAllKeys();
    }
}