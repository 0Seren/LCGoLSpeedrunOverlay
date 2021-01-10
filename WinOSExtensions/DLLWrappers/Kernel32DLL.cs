using System;
using System.Runtime.InteropServices;

namespace WinOSExtensions.DLLWrappers
{
    public static class Kernel32DLL
    {
        [DllImport("kernel32.dll")]
        public static extern IntPtr GetModuleHandleA(string lpModuleName);
    }
}
