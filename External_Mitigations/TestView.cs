using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32.SafeHandles;
using System.IO;
using System.Windows.Forms;
using External_Mitigations.Modules;
using External_Mitigations.CommonUtilities;

namespace External_Mitigations
{
    public partial class TestView : Form
    {
        public TestView()
        {
            InitializeComponent();
        }

        internal class ModuleInstanceStorage
        {
            public static InputDetection inpDetection;
            public static ScreenCapturePrevention scPrevention;
        }

        private void TestView_Load(object sender, EventArgs e)
        {
            SetUpConsole();

            Application.ApplicationExit += new EventHandler(OnExit);

            ModuleInstanceStorage.inpDetection = new InputDetection();
            ModuleInstanceStorage.inpDetection.InstallHooks(this.Handle);

            ModuleInstanceStorage.scPrevention = new ScreenCapturePrevention(this.Handle);
        }

        /// <summary>
        /// C# implementation for CallWndProc hook
        /// </summary>
        /// <param name="message"></param>
        protected override void WndProc(ref Message message)
        {
            if(ModuleInstanceStorage.inpDetection != null)
            {
                var RID = ModuleInstanceStorage.inpDetection.rIUtils.GetRawInputData(message.LParam);

                if (RID.Header.Type == NativeImport.NativeStructs.RawInputType.Mouse)
                {
                    if(RID.Header.Device == IntPtr.Zero) //0 = non generic input from unknown driver
                    {
                        Console.WriteLine("Blocked non generic mouse input call - Device Handle is 0");
                        return;
                    }
                }
            }

            base.WndProc(ref message);
        }

        private void OnExit(object sender, EventArgs eArgs)
        {
            ModuleInstanceStorage.inpDetection.UninstallHooks();
        }

        private void SetUpConsole()
        {
            NativeImport.AllocConsole();

            var outFile = NativeImport.CreateFile("CONOUT$", NativeImport.GENERIC_WRITE | NativeImport.GENERIC_READ, NativeImport.FILE_SHARE_WRITE, 0, NativeImport.OPEN_EXISTING, /*FILE_ATTRIBUTE_NORMAL*/0, 0);
            var safeHandle = new SafeFileHandle(outFile, true);

            NativeImport.SetStdHandle(-11, outFile);

            FileStream fs = new FileStream(safeHandle, FileAccess.Write);
            StreamWriter writer = new StreamWriter(fs) { AutoFlush = true };

            Console.SetOut(writer);

            if (NativeImport.GetConsoleMode(outFile, out var cMode)) NativeImport.SetConsoleMode(outFile, cMode | 0x0200);
        }
    }
}
