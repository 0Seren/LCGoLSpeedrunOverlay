using System;
using System.Runtime.InteropServices;

namespace LCGoLOverlayProcess.Helpers
{
    public static class Kernel32dll
    {
        // TODO: I think GetModuleHandleA can be removed. Maybe all of Kernel32dll...
        [DllImport("kernel32.dll")]
        public static extern IntPtr GetModuleHandleA(string lpModuleName);
    }
}
