﻿using System;
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
        private Bitmap _windowBitmap;
        private readonly ServerInterface _server;

        public Bitmap WindowBitmap
        {
            get
            {
                lock (_windowBitmapLock)
                {
                    // We have to return a new Bitmap because _windowBitmap can be disposed at any time.
                    return new Bitmap(_windowBitmap);
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
            while (!_liveSplitProcess.HasExited)
            {
                try
                {
                    if (!_liveSplitProcess.GetProcessBitmap(out var bitmap)) 
                        continue;

                    var oldBitmap = _windowBitmap;

                    lock (_windowBitmapLock)
                    {
                        _windowBitmap = bitmap;
                    }

                    oldBitmap?.Dispose();
                } catch (Exception e)
                {
                    // TODO: Make error reporting be a global, static class?
                    _server.ReportException(e);
                }
            }
        }
    }
}
