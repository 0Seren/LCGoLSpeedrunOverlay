using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace WinOSExtensions.DLLWrappers
{
    public static class User32Dll
    {
        [DllImport("user32.dll", SetLastError = true, EntryPoint = "PrintWindow")]
        [return: MarshalAs(UnmanagedType.Bool)]
        // ReSharper disable once InconsistentNaming
        private static extern bool PrintWindow_Extern(IntPtr hwnd, IntPtr hDC, uint nFlags);

        [DllImport("user32.dll", EntryPoint = "GetWindowRect")]
        private static extern bool GetWindowRect_Extern(IntPtr handle, ref Rectangle rect);

        [DllImport("user32.dll", EntryPoint = "SetForegroundWindow")]
        private static extern bool SetForegroundWindow_Extern(IntPtr handle);

        [DllImport("user32.dll", EntryPoint = "ShowWindowAsync")]
        private static extern bool ShowWindowAsync_Extern(HandleRef hWnd, int nCmdShow);

        public static bool PrintWindow(IntPtr hwnd, IntPtr hDC, uint nFlags)
        {
            return PrintWindow_Extern(hwnd, hDC, nFlags);
        }

        public static bool GetWindowRect(IntPtr handle, ref Rectangle rect)
        {
            return GetWindowRect_Extern(handle, ref rect);
        }

        public static bool SetForegroundWindow(IntPtr handle)
        {
            return SetForegroundWindow_Extern(handle);
        }

        public static bool ShowWindowAsync(HandleRef hWnd, int nCmdShow)
        {
            return ShowWindowAsync_Extern(hWnd, nCmdShow);
        }
    }
}
