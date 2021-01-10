using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace WinOSExtensions.DLLWrappers
{
    public static class User32DLL
    {
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool PrintWindow(IntPtr hwnd, IntPtr hDC, uint nFlags);

        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr handle, ref Rectangle rect);
    }
}
