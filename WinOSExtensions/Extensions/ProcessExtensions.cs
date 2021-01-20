using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using WinOSExtensions.DLLWrappers;

namespace WinOSExtensions.Extensions
{
    public static class ProcessExtensions
    {
        /// <summary>
        /// Gets the Application Name for a Process.
        /// </summary>
        /// <param name="process">The process to find the Application Name for.</param>
        /// <returns>The Application Name of the Process.</returns>
        public static string GetApplicationName(this Process process)
        {
            if (process == null)
                return null;
            
            string applicationName = process.MainWindowTitle.Split('-').Last().Trim();

            if (!string.IsNullOrWhiteSpace(applicationName))
                return applicationName;

            applicationName = process.MainWindowTitle.Trim();

            if (string.IsNullOrWhiteSpace(applicationName))
            {
                applicationName = process.ProcessName.Trim();
            }

            return applicationName;

        }

        public static bool GetProcessBitmap(this Process process, out Bitmap bitmap, bool innerWindowOnly = true)
        {
            bitmap = null;

            if (process == null)
                return false;

            var wHandle = process.MainWindowHandle;
            var rect = Rectangle.Empty;

            if (wHandle == IntPtr.Zero || !User32Dll.GetWindowRect(wHandle, ref rect))
                return false;

            // GetWindowRect returns Top/Left and Bottom/Right, so we need to fix it.
            rect.Width -= rect.X;
            rect.Height -= rect.Y;

            if (rect.Width <= 0 || rect.Height <= 0)
                return false;

            bitmap = new Bitmap(rect.Width, rect.Height);
            using (var g = Graphics.FromImage(bitmap))
            {
                var hdc = g.GetHdc();
                unsafe
                {
                    // 0x1 for nFlags captures only the client area of the window. 0x0 for nFlags captures full window.
                    return User32Dll.PrintWindow(wHandle, hdc, *(byte*)&innerWindowOnly);
                }
            }

        }
    }
}
