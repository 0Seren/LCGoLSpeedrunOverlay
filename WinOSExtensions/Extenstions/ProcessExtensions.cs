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
            if (process != null)
            {
                string applicationName = process?.MainWindowTitle?.Split('-').Last().Trim();

                if (string.IsNullOrWhiteSpace(applicationName))
                {
                    applicationName = process?.MainWindowTitle?.Trim();

                    if (string.IsNullOrWhiteSpace(applicationName))
                    {
                        applicationName = process?.ProcessName?.Trim();
                    }
                }

                return applicationName;
            }

            return null;
        }

        public static bool GetProcessBitmap(this Process process, out Bitmap bitmap, bool innerWindowOnly = true)
        {
            bitmap = null;
            if (process != null)
            {
                IntPtr wHandle = process.MainWindowHandle;
                Rectangle rect = Rectangle.Empty;
                if (wHandle != IntPtr.Zero && User32DLL.GetWindowRect(wHandle, ref rect))
                {
                    // GetWindowRect returns Top/Left and Bottom/Right, so we need to fix it.
                    rect.Width -= rect.X;
                    rect.Height -= rect.Y;

                    if (rect.Width > 0 && rect.Height > 0)
                    {
                        bitmap = new Bitmap(rect.Width, rect.Height);
                        using (Graphics g = Graphics.FromImage(bitmap))
                        {
                            IntPtr hdc = g.GetHdc();
                            unsafe
                            {
                                // 0x1 captures only the client area of the window.
                                return User32DLL.PrintWindow(wHandle, hdc, *(byte*)&innerWindowOnly);
                            }
                        }
                    }
                }
            }

            return false;
        }
    }
}
