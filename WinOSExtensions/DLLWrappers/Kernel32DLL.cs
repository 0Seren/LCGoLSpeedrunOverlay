using System;
using System.Runtime.InteropServices;

namespace WinOSExtensions.DLLWrappers
{
    public static class Kernel32Dll
    {
        [DllImport("kernel32.dll")]
        public static extern IntPtr GetModuleHandleA(string lpModuleName);

        [DllImport("Kernel32.dll")]
        public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer, int dwSize, out IntPtr lpNumberOfBytesRead);
    }
}
