using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;
using WinOSExtensions.Extensions;

namespace LCGoLOverlayProcess.Helpers
{
    public class LiveSplitHelper
    {
        private readonly Process _liveSplitProcess;
        private readonly object _windowBitmapLock = new object();
        private byte[] _windowBitmap;
        private readonly ServerInterface _server;

        public byte[] WindowBitmap
        {
            get
            {
                lock (_windowBitmapLock)
                {
                    return _windowBitmap;
                }
            }
        }

        // TODO: Create a channel for sending messages back and forth between this and LiveSplit
        public LiveSplitHelper(Process liveSplitProcess, string channelName, ServerInterface server)
        {
            _liveSplitProcess = liveSplitProcess;
            _windowBitmap = null;
            _server = server;

            Task.Run(WindowGrabBackgroundTask);
        }

        private void WindowGrabBackgroundTask()
        {
            var converter = new ImageConverter();

            while (!_liveSplitProcess.HasExited)
            {
                try
                {
                    if (_liveSplitProcess.GetProcessBitmap(out var bitmap))
                    {
                        var bytes = (byte[])converter.ConvertTo(bitmap, typeof(byte[]));
                        lock (_windowBitmapLock)
                        {
                            _windowBitmap = bytes;
                        }
                        bitmap.Dispose();
                    }
                } catch (Exception e)
                {
                    // TODO: Make error reporting be a global, static class?
                    _server.ReportException(e);
                }
            }
        }
    }
}
