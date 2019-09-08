﻿using System;
using System.Drawing;
using System.Reactive.Linq;
using System.Threading;
using System.Windows.Forms;
using Dapplo.Windows.Common.Extensions;
using Dapplo.Windows.Common.Structs;
using Dapplo.Windows.Desktop;
using Dapplo.Windows.DesktopWindowsManager;
using Dapplo.Windows.DesktopWindowsManager.Structs;
using Dapplo.Windows.Enums;
using Dapplo.Windows.Messages;
using Dapplo.Windows.User32;
using Pip.Configuration;

namespace Pip.Ui
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class ThumbnailForm : Form
    {
        private const int HtCaption = 0x2;

        private readonly IPipConfiguration _pipConfiguration;
        private readonly IntPtr _hWnd;
        private readonly SynchronizationContext _uiSynchronizationContext;
        private IntPtr _phThumbnail;
        private readonly IDisposable _windowMonitor;

        public ThumbnailForm(IPipConfiguration pipConfiguration, IntPtr hWnd, SynchronizationContext uiSynchronizationContext)
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);

            _pipConfiguration = pipConfiguration;
            _hWnd = hWnd;
            _uiSynchronizationContext = uiSynchronizationContext;

            // Make sure the PIP closes when the source closes
            _windowMonitor = WinEventHook.Create(WinEvents.EVENT_OBJECT_DESTROY)
                .SubscribeOn(uiSynchronizationContext)
                .ObserveOn(uiSynchronizationContext)
                .Subscribe(info =>
                {
                    if (info.Handle == _hWnd)
                    {
                        Close();
                    }
                });
            var screenBounds = DisplayInfo.ScreenBounds;
            var pipNativeSize = new NativeSize(screenBounds.Width / 5, screenBounds.Height / 5);
            var pipLocation = new NativePoint(screenBounds.Width - pipNativeSize.Width, 0);

            Location = pipLocation;
            Size = pipNativeSize;
            StartPosition = FormStartPosition.Manual;
            TopMost = true;
            Text = "PIP";
            FormBorderStyle = FormBorderStyle.None;
            BackColor = Color.White;
            Enabled = false;
            ShowInTaskbar = false;
            Cursor = Cursors.Default;
        }

        /// <summary>
        /// Make the window movable
        /// </summary>
        /// <param name="message">Message</param>
        protected override void WndProc(ref Message message)
        {
            base.WndProc(ref message);
            if (message.Msg == (int) WindowsMessages.WM_NCHITTEST)
            {
                message.Result = (IntPtr)HtCaption;
            }
        }

        /// <summary>
        /// Make sure the monitoring is disabled and the Thumbnail is unregistered
        /// </summary>
        /// <param name="e">EventArgs</param>
        protected override void OnClosed(EventArgs e)
        {
            _windowMonitor.Dispose();
            Dwm.DwmUnregisterThumbnail(_phThumbnail);
            base.OnClosed(e);
        }

        /// <summary>
        /// Create the thumbnail if the form is shown
        /// </summary>
        /// <param name="e">EventArgs</param>
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            if (_phThumbnail == IntPtr.Zero)
            {
                var result = Dwm.DwmRegisterThumbnail(Handle, _hWnd, out _phThumbnail);
                if (result.Failed())
                {
                    Close();
                    return;
                }
                Update();
            }
            User32Api.BringWindowToTop(Handle);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Update()
        {
            Opacity = 255d / Math.Max(0x01l, _pipConfiguration.Opacity);
            // Prepare the displaying of the Thumbnail
            var props = new DwmThumbnailProperties
            {
                Opacity = _pipConfiguration.Opacity,
                Visible = true,
                SourceClientAreaOnly = _pipConfiguration.SourceClientAreaOnly,
                Destination = new NativeRect(0, 0, Width, Height)
            };
            Dwm.DwmUpdateThumbnailProperties(_phThumbnail, ref props);
        }
    }
}