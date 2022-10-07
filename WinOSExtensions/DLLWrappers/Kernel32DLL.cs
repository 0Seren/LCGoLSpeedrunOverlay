using System;
using System.Runtime.InteropServices;

namespace WinOSExtensions.DLLWrappers
{
    public static class Kernel32Dll
    {
        [DllImport("kernel32.dll", EntryPoint = "GetModuleHandleA")]
        private static extern IntPtr GetModuleHandleA_Extern(string lpModuleName);

        [DllImport("Kernel32.dll", EntryPoint = "ReadProcessMemory")]
        private static extern bool ReadProcessMemory_Extern(IntPtr hProcess, IntPtr lpBaseAddress, out byte[] lpBuffer, int dwSize, out IntPtr lpNumberOfBytesRead);

        public static IntPtr GetModuleHandleA(string lpModuleName)
        {
            return GetModuleHandleA_Extern(lpModuleName);
        }

        public static bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, out byte[] lpBuffer, int dwSize, out IntPtr lpNumberOfBytesRead)
        {
            return ReadProcessMemory_Extern(hProcess, lpBaseAddress, out lpBuffer, dwSize, out lpNumberOfBytesRead);
        }
    }
}
