using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace External_Mitigations.CommonUtilities
{
    class NativeImport
    {
        public class WindowsHookAdditionals
        {
            public delegate IntPtr HookProc(int code, IntPtr wParam, IntPtr lParam);

            public enum HookType : int
            {
                WH_JOURNALRECORD = 0,
                WH_JOURNALPLAYBACK = 1,
                WH_KEYBOARD = 2,
                WH_GETMESSAGE = 3,
                WH_CALLWNDPROC = 4,
                WH_CBT = 5,
                WH_SYSMSGFILTER = 6,
                WH_MOUSE = 7,
                WH_HARDWARE = 8,
                WH_DEBUG = 9,
                WH_SHELL = 10,
                WH_FOREGROUNDIDLE = 11,
                WH_CALLWNDPROCRET = 12,
                WH_KEYBOARD_LL = 13,
                WH_MOUSE_LL = 14
            }
        }

        public const int SW_HIDE = 0;
        public const int SW_SHOW = 5;
        public const int MY_CODE_PAGE = 437;
        public const uint GENERIC_WRITE = 0x40000000;
        public const uint GENERIC_READ = 0x80000000;
        public const uint FILE_SHARE_WRITE = 0x2;
        public const uint OPEN_EXISTING = 0x3;

        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HTCAPTION = 0x2;

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool AllocConsole();

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        public static extern bool FreeConsole();

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr CreateFile(
        string lpFileName,
        uint dwDesiredAccess,
        uint dwShareMode,
        uint lpSecurityAttributes,
        uint dwCreationDisposition,
        uint dwFlagsAndAttributes,
        uint hTemplateFile);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

        [DllImport("kernel32.dll")]
        public static extern bool SetStdHandle(int nStdHandle, IntPtr hHandle);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SetWindowsHookEx(WindowsHookAdditionals.HookType hookType, WindowsHookAdditionals.HookProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll")]
        public static extern bool UnhookWindowsHookEx(IntPtr hInstance);

        [DllImport("user32.dll")]
        public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        public static string GetActiveWindowTitle()
        {
            const int nChars = 256;
            StringBuilder Buff = new StringBuilder(nChars);
            IntPtr handle = GetForegroundWindow();

            if (GetWindowText(handle, Buff, nChars) > 0)
            {
                return Buff.ToString();
            }
            return null;
        }

        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Ansi)]
        public static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPStr)]string lpFileName);

        [DllImport("user32.dll", SetLastError = true)]
        public extern static uint GetRawInputDeviceInfo(IntPtr hDevice, uint uiCommand, IntPtr pData, ref uint pcbSize);
        [DllImport("User32.dll")]
        public extern static uint GetRawInputData(IntPtr hRawInput, uint uiCommand, IntPtr pData, ref uint pcbSize, uint cbSizeHeader);
        [DllImport("User32.dll")]
        public extern static bool RegisterRawInputDevices(NativeStructs.RAWINPUTDEVICE[] pRawInputDevice, uint uiNumDevices, uint cbSize);

        [DllImport("user32.dll")]
        public static extern bool SetWindowDisplayAffinity(IntPtr hwnd, Enums.DisplayAffinity affinity);

        public class NativeStructs
        {
            [StructLayout(LayoutKind.Sequential)]
            public class MSLLHOOKSTRUCT
            {
                public POINT pt;
                public int mouseData; 
                public int flags;
                public int time;
                public UIntPtr dwExtraInfo;
            }

            [StructLayout(LayoutKind.Sequential)]
            public class KBDLLHOOKSTRUCT
            {
                public uint vkCode;
                public uint scanCode;
                public int flags;
                public uint time;
                public UIntPtr dwExtraInfo;
            }

            [Flags]
            public enum KBDLLHOOKSTRUCTFlags : uint
            {
                LLKHF_EXTENDED = 0x01,
                LLKHF_INJECTED = 0x10,
                LLKHF_ALTDOWN = 0x20,
                LLKHF_UP = 0x80,
            }

            [StructLayout(LayoutKind.Sequential)]
            public class CWPSTRUCT
            {
                public IntPtr lparam;
                public IntPtr wparam;
                public int message;
                public IntPtr hwnd;
            }

            [StructLayout(LayoutKind.Sequential)]
            public class CWPRETSTRUCT
            {
                public IntPtr lResult;
                public IntPtr lParam;
                public IntPtr wParam;
                public uint message;
                public IntPtr hWnd;
            }

            [Flags()]
            public enum RawMouseFlags : ushort
            {
                /// <summary>Relative to the last position.</summary>
                MoveRelative = 0,
                /// <summary>Absolute positioning.</summary>
                MoveAbsolute = 1,
                /// <summary>Coordinate data is mapped to a virtual desktop.</summary>
                VirtualDesktop = 2,
                /// <summary>Attributes for the mouse have changed.</summary>
                AttributesChanged = 4
            }
            [Flags()]
            public enum RawMouseButtons : ushort
            {
                /// <summary>No button.</summary>
                None = 0,
                /// <summary>Left (button 1) down.</summary>
                LeftDown = 0x0001,
                /// <summary>Left (button 1) up.</summary>
                LeftUp = 0x0002,
                /// <summary>Right (button 2) down.</summary>
                RightDown = 0x0004,
                /// <summary>Right (button 2) up.</summary>
                RightUp = 0x0008,
                /// <summary>Middle (button 3) down.</summary>
                MiddleDown = 0x0010,
                /// <summary>Middle (button 3) up.</summary>
                MiddleUp = 0x0020,
                /// <summary>Button 4 down.</summary>
                Button4Down = 0x0040,
                /// <summary>Button 4 up.</summary>
                Button4Up = 0x0080,
                /// <summary>Button 5 down.</summary>
                Button5Down = 0x0100,
                /// <summary>Button 5 up.</summary>
                Button5Up = 0x0200,
                /// <summary>Mouse wheel moved.</summary>
                MouseWheel = 0x0400
            }


            [StructLayout(LayoutKind.Sequential)]
            public struct RAWINPUTDEVICE
            {
                [MarshalAs(UnmanagedType.U2)]
                public ushort usUsagePage;
                [MarshalAs(UnmanagedType.U2)]
                public ushort usUsage;
                [MarshalAs(UnmanagedType.U4)]
                public int dwFlags;
                public IntPtr hwndTarget;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct RAWHID
            {
                [MarshalAs(UnmanagedType.U4)]
                public int dwSizHid;
                [MarshalAs(UnmanagedType.U4)]
                public int dwCount;
            }
            [StructLayout(LayoutKind.Explicit)]
            public struct RawMouse
            {
                /// <summary>
                /// The mouse state.
                /// </summary>
                [FieldOffset(0)]
                public RawMouseFlags Flags;
                /// <summary>
                /// Flags for the event.
                /// </summary>
                [FieldOffset(4)]
                public RawMouseButtons ButtonFlags;
                /// <summary>
                /// If the mouse wheel is moved, this will contain the delta amount.
                /// </summary>
                [FieldOffset(6)]
                public ushort ButtonData;
                /// <summary>
                /// Raw button data.
                /// </summary>
                [FieldOffset(8)]
                public uint RawButtons;
                /// <summary>
                /// The motion in the X direction. This is signed relative motion or 
                /// absolute motion, depending on the value of usFlags. 
                /// </summary>
                [FieldOffset(12)]
                public int LastX;
                /// <summary>
                /// The motion in the Y direction. This is signed relative motion or absolute motion, 
                /// depending on the value of usFlags. 
                /// </summary>
                [FieldOffset(16)]
                public int LastY;
                /// <summary>
                /// The device-specific additional information for the event. 
                /// </summary>
                [FieldOffset(20)]
                public uint ExtraInformation;
            }
            [StructLayout(LayoutKind.Sequential)]
            public struct RAWKEYBOARD
            {
                [MarshalAs(UnmanagedType.U2)]
                public ushort MakeCode;
                [MarshalAs(UnmanagedType.U2)]
                public ushort Flags;
                [MarshalAs(UnmanagedType.U2)]
                public ushort Reserved;
                [MarshalAs(UnmanagedType.U2)]
                public ushort VKey;
                [MarshalAs(UnmanagedType.U4)]
                public uint Message;
                [MarshalAs(UnmanagedType.U4)]
                public uint ExtraInformation;
            }

            public enum RawInputType
            {
                /// <summary>
                /// Mouse input.
                /// </summary>
                Mouse = 0,
                /// <summary>
                /// Keyboard input.
                /// </summary>
                Keyboard = 1,
                /// <summary>
                /// Another device that is not the keyboard or the mouse.
                /// </summary>
                HID = 2
            }

            [StructLayout(LayoutKind.Explicit)]
            public struct RawInput
            {
                /// <summary>Header for the data.</summary>
                [FieldOffset(0)]
                public RawInputHeader Header;
                /// <summary>Mouse raw input data.</summary>
                [FieldOffset(16)]
                public RawMouse Mouse;
                /// <summary>Keyboard raw input data.</summary>
                [FieldOffset(16)]
                public RAWKEYBOARD Keyboard;
                /// <summary>HID raw input data.</summary>
                [FieldOffset(16)]
                public RAWHID Hid;
            }
            [StructLayout(LayoutKind.Sequential)]
            public struct RawInputHeader
            {
                /// <summary>Type of device the input is coming from.</summary>
                public RawInputType Type;
                /// <summary>Size of the packet of data.</summary>
                public int Size;
                /// <summary>Handle to the device sending the data.</summary>
                public IntPtr Device;
                /// <summary>wParam from the window message.</summary>
                public IntPtr wParam;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct POINT
            {
                public int X;
                public int Y;

                public POINT(int x, int y)
                {
                    this.X = x;
                    this.Y = y;
                }

                public static implicit operator System.Drawing.Point(POINT p)
                {
                    return new System.Drawing.Point(p.X, p.Y);
                }

                public static implicit operator POINT(System.Drawing.Point p)
                {
                    return new POINT(p.X, p.Y);
                }
            }
        }
    }
}
