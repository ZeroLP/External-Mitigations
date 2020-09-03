using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace External_Mitigations.Modules
{
    class ScreenCapturePrevention
    {
        public static IntPtr CurrentContextHandle;
        private bool IsContextStateMonitor;

        public ScreenCapturePrevention(IntPtr handle)
        {
            CurrentContextHandle = handle;
            Console.WriteLine("Handle registered for display context switching");
        }

        public void SwitchDisplayContext(Enums.DisplayAffinity dAffinity)
        {
            if (!IsContextStateMonitor)
            {
                Console.WriteLine($"Switching display context...");
                CommonUtilities.NativeImport.SetWindowDisplayAffinity(CurrentContextHandle, dAffinity);

                IsContextStateMonitor = true;
            }
        }
    }
}
