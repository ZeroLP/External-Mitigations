using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using External_Mitigations.CommonUtilities;
using External_Mitigations.Enums;

namespace External_Mitigations.Modules
{
    class InputDetection
    {
        private IntPtr wMSHookInstance = IntPtr.Zero;
        private IntPtr wKBHookInstance = IntPtr.Zero;

        public RawInputUtils rIUtils;

        private NativeImport.WindowsHookAdditionals.HookProc winMSHookCallbackDelegate = null;
        private NativeImport.WindowsHookAdditionals.HookProc winKBHookCallbackDelegate = null;

        public void InstallHooks(IntPtr hwnd)
        {
            winMSHookCallbackDelegate = new NativeImport.WindowsHookAdditionals.HookProc(HookedMSWindowsCallback);
            winKBHookCallbackDelegate = new NativeImport.WindowsHookAdditionals.HookProc(HookedKBWindowsCallback);

            var hinstance = NativeImport.LoadLibrary("User32");

            wMSHookInstance = NativeImport.SetWindowsHookEx(NativeImport.WindowsHookAdditionals.HookType.WH_MOUSE_LL, winMSHookCallbackDelegate, hinstance, 0);
            wKBHookInstance = NativeImport.SetWindowsHookEx(NativeImport.WindowsHookAdditionals.HookType.WH_KEYBOARD_LL, winKBHookCallbackDelegate, hinstance, 0);

            rIUtils = new RawInputUtils(hwnd);

            Console.WriteLine("Installed input hooks");
        }

        public void UninstallHooks()
        {
            NativeImport.UnhookWindowsHookEx(wMSHookInstance);
            NativeImport.UnhookWindowsHookEx(wKBHookInstance);
        }

        private IntPtr HookedMSWindowsCallback(int code, IntPtr wParam, IntPtr lParam)
        {
            if (code >= 0 && NativeImport.GetActiveWindowTitle() == "TestView")
            {
                NativeImport.NativeStructs.MSLLHOOKSTRUCT mouseStruct = new NativeImport.NativeStructs.MSLLHOOKSTRUCT();
                Marshal.PtrToStructure(lParam, mouseStruct);

                if ((mouseStruct.flags & 1) != 0) //LLMHF_INJECTED flag
                {
                    Console.WriteLine($"Blocked non generic mouse input call - Input raised LLMHF_INJECTED flag");

                    TestView.ModuleInstanceStorage.scPrevention.SwitchDisplayContext(DisplayAffinity.Monitor);
                    return (IntPtr)1;
                }

                return NativeImport.CallNextHookEx(IntPtr.Zero, code, wParam, lParam);
            }
            else
                return NativeImport.CallNextHookEx(IntPtr.Zero, code, wParam, lParam);
        }

        private IntPtr HookedKBWindowsCallback(int code, IntPtr wParam, IntPtr lParam)
        {
            if (code >= 0 && NativeImport.GetActiveWindowTitle() == "TestView")
            {
                NativeImport.NativeStructs.KBDLLHOOKSTRUCT keyboardStruct = new NativeImport.NativeStructs.KBDLLHOOKSTRUCT();
                Marshal.PtrToStructure(lParam, keyboardStruct);

                if ((keyboardStruct.flags & 0x10) != 0) //LLKHF_INJECTED flag
                {
                    Console.WriteLine($"Blocked non generic keyboard input call - Input raised LLKHF_INJECTED");

                    TestView.ModuleInstanceStorage.scPrevention.SwitchDisplayContext(DisplayAffinity.Monitor);
                    return (IntPtr)1;
                }

                return NativeImport.CallNextHookEx(IntPtr.Zero, code, wParam, lParam);
            }
            else
                return NativeImport.CallNextHookEx(IntPtr.Zero, code, wParam, lParam);
        }
    }
}
