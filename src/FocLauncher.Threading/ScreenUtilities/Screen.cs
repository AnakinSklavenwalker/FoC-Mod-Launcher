﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using FocLauncher.NativeMethods;
using Microsoft.Win32;

namespace FocLauncher.ScreenUtilities
{
    public static class Screen
    {
        public static event EventHandler DisplayConfigChanged;

        private static readonly List<DisplayInfo> Displays = new List<DisplayInfo>();

        public static int DisplayCount => Displays.Count;

        static Screen()
        {
            SystemEvents.DisplaySettingsChanged += OnDisplayChanged;
            UpdateDisplays();
        }

        public static double LogicalToDeviceUnitsX(int display, double value)
        {
            var x = GetMonitorDpi(display).X;
            return ScaleLogicalToDevice(x, value);
        }

        public static double LogicalToDeviceUnitsY(int display, double value)
        {
            var y = GetMonitorDpi(display).Y;
            return ScaleLogicalToDevice(y, value);
        }

        public static void SetInitialWindowRect(IntPtr hwnd, Window window, Int32Rect windowBounds)
        {
            if (hwnd == IntPtr.Zero)
                throw new ArgumentException(nameof(hwnd));
            if (window == null)
                throw new ArgumentNullException(nameof(window));
            SetWindowDpi(hwnd, window, windowBounds);
            window.Top = window.DeviceToLogicalUnitsY(windowBounds.Y);
            window.Left = window.DeviceToLogicalUnitsX(windowBounds.X);
            window.Width = window.DeviceToLogicalUnitsX(windowBounds.Width);
            window.Height = window.DeviceToLogicalUnitsY(windowBounds.Height);
        }

        public static int FindDisplayForWindowRect(Rect windowRect)
        {
            var num1 = -1;
            var lprcSrc2 = new RECT(windowRect);
            long num2 = 0;
            for (var index = 0; index < Displays.Count; ++index)
            {
                var rcWork = Displays[index].MonitorInfo.RcWork;
                NativeMethods.NativeMethods.IntersectRect(out var lprcDst, ref rcWork, ref lprcSrc2);
                long num3 = lprcDst.Width * lprcDst.Height;
                if (num3 > num2)
                {
                    num1 = index;
                    num2 = num3;
                }
            }
            if (-1 == num1)
            {
                var num3 = double.MaxValue;
                for (var index = 0; index < Displays.Count; ++index)
                {
                    var num4 = Distance(Displays[index].MonitorInfo.RcMonitor, lprcSrc2);
                    if (num4 < num3)
                    {
                        num1 = index;
                        num3 = num4;
                    }
                }
            }
            return num1;
        }

        public static int FindDisplayForAbsolutePosition(Point absolutePosition)
        {
            for (var index = 0; index < Displays.Count; ++index)
            {
                var rcMonitor = Displays[index].MonitorInfo.RcMonitor;
                if (rcMonitor.Left <= absolutePosition.X && rcMonitor.Right >= absolutePosition.X && (rcMonitor.Top <= absolutePosition.Y && rcMonitor.Bottom >= absolutePosition.Y))
                    return index;
            }
            var num1 = -1;
            var num2 = double.MaxValue;
            for (var index = 0; index < Displays.Count; ++index)
            {
                var num3 = Distance(absolutePosition, Displays[index].MonitorInfo.RcMonitor);
                if (num3 < num2)
                {
                    num1 = index;
                    num2 = num3;
                }
            }
            return num1;
        }

        public static void FindMonitorRectsFromPoint(Point point, out Rect monitorRect, out Rect workAreaRect)
        {
            var absolutePosition = FindDisplayForAbsolutePosition(point);
            var  monitorInfo = new MonitorInfo();
            if (absolutePosition == -1)
                GetMonitorInfo(User32.MonitorFromPoint(POINT.FromPoint(point), 2), ref monitorInfo);
            else
                monitorInfo = Displays[absolutePosition].MonitorInfo;
            GetMonitorRects(monitorInfo, out monitorRect, out workAreaRect);
        }

        public static Point RelativePositionToAbsolutePosition(int display, double left, double top)
        {
            if (display < 0)
                throw new ArgumentOutOfRangeException(nameof(display));
            RECT rect1;
            if (display >= Displays.Count)
            {
                var rect2 = Displays[Displays.Count - 1].Rect;
                rect1 = new RECT(rect2.Left + rect2.Width, rect2.Top, rect2.Right + rect2.Width, rect2.Bottom);
            }
            else
                rect1 = Displays[display].Rect;
            return new Point(rect1.Left + left, rect1.Top + top);
        }

        public static Rect GetOnScreenPosition(Rect floatRect, int fallbackDisplay = -1, bool forceOnScreen = false, bool topOnly = false)
        {
            var windowRect = floatRect;
            FindMaximumSingleMonitorRectangle(windowRect, out var screenSubRect, out _);
            if ((screenSubRect.Width == 0.0 || screenSubRect.Height == 0.0) | forceOnScreen)
            {
                Rect workAreaRect;
                if (fallbackDisplay == -1)
                    FindMonitorRectsFromPoint(User32.GetCursorPos(), out _, out workAreaRect);
                else
                    FindMonitorRectsFromPoint(RelativePositionToAbsolutePosition(fallbackDisplay, 0.0, 0.0), out _, out workAreaRect);
                if (windowRect.Width > workAreaRect.Width)
                    windowRect.Width = workAreaRect.Width;
                if (windowRect.Height > workAreaRect.Height)
                    windowRect.Height = workAreaRect.Height;
                if (!topOnly)
                {
                    if (windowRect.Right > workAreaRect.Right)
                        windowRect.X = workAreaRect.Right - windowRect.Width;
                    if (windowRect.Left < workAreaRect.Left)
                        windowRect.X = workAreaRect.Left;
                    if (windowRect.Bottom > workAreaRect.Bottom)
                        windowRect.Y = workAreaRect.Bottom - windowRect.Height;
                }
                if (windowRect.Top < workAreaRect.Top)
                    windowRect.Y = workAreaRect.Top;
            }
            return windowRect;
        }

        internal static void FindMaximumSingleMonitorRectangle(Rect windowRect, out Rect screenSubRect, out Rect monitorRect)
        {
            FindMaximumSingleMonitorRectangle(new RECT(windowRect), out var screenSubRect1, out var monitorRect1);
            screenSubRect = new Rect(screenSubRect1.Position, screenSubRect1.Size);
            monitorRect = new Rect(monitorRect1.Position, monitorRect1.Size);
        }

        internal static void FindMaximumSingleMonitorRectangle(RECT windowRect, out RECT screenSubRect, out RECT monitorRect)
        {
            var displayForWindowRect = FindDisplayForWindowRect(windowRect.ToRect());
            screenSubRect = new RECT
            {
                Left = 0,
                Right = 0,
                Top = 0,
                Bottom = 0
            };
            monitorRect = new RECT
            {
                Left = 0,
                Right = 0,
                Top = 0,
                Bottom = 0
            };
            if (-1 == displayForWindowRect)
                return;
            var monitorInfo = Displays[displayForWindowRect].MonitorInfo;
            var rcWork = monitorInfo.RcWork;
            User32.IntersectRect(out var lprcDst, ref rcWork, ref windowRect);
            screenSubRect = lprcDst;
            monitorRect = monitorInfo.RcWork;
        }

        private static double ScaleLogicalToDevice(double dpi, double value)
        {
            return value * dpi / 96.0;
        }

        private static double Distance(Point point, RECT rect)
        {
            return Distance(point, GetRectCenter(rect));
        }

        private static double Distance(RECT rect1, RECT rect2)
        {
            return Distance(GetRectCenter(rect1), GetRectCenter(rect2));
        }

        private static double Distance(Point point1, Point point2)
        {
            return Math.Sqrt(Math.Pow(point1.X - point2.X, 2.0) + Math.Pow(point1.Y - point2.Y, 2.0));
        }

        private static Point GetRectCenter(RECT rect)
        {
            return new Point(rect.Left + rect.Width / 2, rect.Top + rect.Height / 2);
        }

        private static void GetMonitorInfo(IntPtr monitor, ref MonitorInfo monitorInfo)
        {
            if (!(monitor != IntPtr.Zero))
                return;
            monitorInfo.CbSize = (uint)Marshal.SizeOf(typeof(MonitorInfo));
            User32.GetMonitorInfo(monitor, ref monitorInfo);
        }

        private static void GetMonitorRects(MonitorInfo monitorInfo, out Rect monitorRect, out Rect workAreaRect)
        {
            if (monitorInfo.CbSize != 0U)
            {
                monitorRect = new Rect(monitorInfo.RcMonitor.Position, monitorInfo.RcMonitor.Size);
                workAreaRect = new Rect(monitorInfo.RcWork.Position, monitorInfo.RcWork.Size);
            }
            else
            {
                monitorRect = new Rect(0.0, 0.0, 0.0, 0.0);
                workAreaRect = new Rect(0.0, 0.0, 0.0, 0.0);
            }
        }

        private static void UpdateDisplays()
        {
            Displays.Clear();
            NativeMethods.NativeMethods.EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, (IntPtr hMonitor, IntPtr hdcMonitor, ref RECT rect, IntPtr lpData) =>
            {
                var monitorInfo = new MonitorInfo { CbSize = (uint)Marshal.SizeOf(typeof(MonitorInfo)) };
                if (NativeMethods.NativeMethods.GetMonitorInfo(hMonitor, ref monitorInfo))
                    Displays.Add(new DisplayInfo(hMonitor, monitorInfo));
                return true;
            }, IntPtr.Zero);
            DisplayConfigChanged?.Invoke(null, EventArgs.Empty);
        }

        private static DpiScale GetMonitorDpiScale(double x, double y)
        {
            var absolutePosition = FindDisplayForAbsolutePosition(new Point(x, y));
            return new DpiScale(LogicalToDeviceUnitsX(absolutePosition, 1.0), LogicalToDeviceUnitsY(absolutePosition, 1.0));
        }

        private static void SetWindowDpi(IntPtr hwnd, Window window, Int32Rect windowBounds)
        {
            var structure = RECT.FromInt32Rect(windowBounds);
            var monitorDpiScale = GetMonitorDpiScale(windowBounds.X, windowBounds.Y);
            var wParam = new IntPtr((int)monitorDpiScale.PixelsPerInchX | (int)monitorDpiScale.PixelsPerInchY << 16);
            var num = Marshal.AllocCoTaskMem(Marshal.SizeOf(structure));
            try
            {
                Marshal.StructureToPtr(structure, num, false);
                NativeMethods.NativeMethods.SendMessage(hwnd, 736, wParam, num);
                VisualTreeHelper.SetRootDpi(window, monitorDpiScale);
            }
            finally
            {
                Marshal.FreeCoTaskMem(num);
            }
        }

        private static void OnDisplayChanged(object sender, EventArgs e)
        {
            UpdateDisplays();
        }

        private static Dpi GetMonitorDpi(int display)
        {
            try
            {
                return Displays[display].MonitorHandle.GetMonitorDpi();
            }
            catch (DpiErrorException)
            {
                return Dpi.Default;
            }
        }

        private class DisplayInfo
        {
            public IntPtr MonitorHandle { get; }

            public MonitorInfo MonitorInfo { get; }

            public RECT Rect => MonitorInfo.RcMonitor;

            public DisplayInfo(IntPtr hMonitor, MonitorInfo monitorInfo)
            {
                MonitorHandle = hMonitor;
                MonitorInfo = monitorInfo;
            }
        }
    }
}