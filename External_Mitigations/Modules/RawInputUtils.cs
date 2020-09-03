using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using External_Mitigations.CommonUtilities;

namespace External_Mitigations.Modules
{
    class RawInputUtils
    {
        private const int RID_INPUT = 0x10000003;
        private const int RIDEV_INPUTSINK = 0x00000100;

        public RawInputUtils(IntPtr hwnd)
        {
            NativeImport.NativeStructs.RAWINPUTDEVICE[] rid = new NativeImport.NativeStructs.RAWINPUTDEVICE[1];

            rid[0].usUsagePage = 0x01;
            rid[0].usUsage = 0x02;
            rid[0].dwFlags = RIDEV_INPUTSINK;
            rid[0].hwndTarget = hwnd;

            if (!NativeImport.RegisterRawInputDevices(rid, (uint)rid.Length, (uint)Marshal.SizeOf(rid[0])))
            {
                throw new ApplicationException("Failed to register raw input device(s).");
            }
        }

        public NativeImport.NativeStructs.RawInput GetRawInputData(IntPtr lParam)
        {
            uint dwSize = 0;

            NativeImport.GetRawInputData(
                lParam,
                RID_INPUT,
                IntPtr.Zero,
                ref dwSize,
                (uint)Marshal.SizeOf(typeof(NativeImport.NativeStructs.RawInputHeader))
            );

            IntPtr buffer = Marshal.AllocHGlobal((int)dwSize);

            NativeImport.GetRawInputData(
                lParam,
                RID_INPUT,
                buffer,
                ref dwSize,
                (uint)Marshal.SizeOf(typeof(NativeImport.NativeStructs.RawInputHeader))
            );

            NativeImport.NativeStructs.RawInput raw = (NativeImport.NativeStructs.RawInput)Marshal.PtrToStructure(buffer, typeof(NativeImport.NativeStructs.RawInput));
            Marshal.FreeHGlobal(buffer);

            return raw;
        }
    }
}
