using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Diagnostics;
using System.Management;

namespace Basketball
{
    public static class WindowWrapper
    {
        public const int nChars = 256;

        public const int SW_MAXIMIZE = 3;
        public const int SW_MINIMIZE = 6;
        public const int SW_RESTORE = 9;

        public const int SWP_NORESIZE = 0x0001;
        public const int SWP_NOMOVE = 0x0002;
        public const int SWP_NOACTIVATE = 0x0010;

        public static void BringToFront(IntPtr handle)
        {
            SetForegroundWindow(handle);
        }

        public static void Restore(IntPtr handle = default(IntPtr))
        {
            if (handle == default(IntPtr)) handle = GetForegroundWindow();
            if (!GetText(handle).Equals("Program Manager"))
            {
                ShowWindow(handle, SW_RESTORE);
            }
        }

        public static void Maximize(IntPtr handle = default(IntPtr))
        {
            if (handle == default(IntPtr)) handle = GetForegroundWindow();
            if (!GetText(handle).Equals("Program Manager"))
            {
                ShowWindow(handle, SW_MAXIMIZE);
            }
        }

        public static void Minimize(IntPtr handle = default(IntPtr))
        {
            if (handle == default(IntPtr)) handle = GetForegroundWindow();
            if (!GetText(handle).Equals("Program Manager"))
            {
                ShowWindow(handle, SW_MINIMIZE);
            }
        }

        public static void MoveBehind(IntPtr handle, IntPtr insertAfter)
        {
            SetWindowPos(handle, insertAfter, 0, 0, 0, 0, SWP_NORESIZE | SWP_NOMOVE | SWP_NOACTIVATE);
        }

        public static void MoveAndResize(IntPtr handle, int x, int y, int w = 0, int h = 0)
        {
            SetWindowPos(handle, (IntPtr)0, x, y, w, h, (w == 0 && h == 0) ? 1 : 0);
        }

        public static string GetText(IntPtr handle = default(IntPtr))
        {
            if (handle == default(IntPtr)) handle = GetForegroundWindow();
            StringBuilder buff = new StringBuilder(nChars);
            GetWindowText(handle, buff, nChars);
            return buff.ToString();
        }

        public static Screen GetScreen(IntPtr handle = default(IntPtr))
        {
            if (handle == default(IntPtr)) handle = GetForegroundWindow();
            return Screen.FromHandle(handle);
        }

        public static void DockLeft(IntPtr handle = default(IntPtr))
        {
            if (handle == default(IntPtr)) handle = GetForegroundWindow();
            Screen sc = WindowWrapper.GetScreen(handle);
            WindowWrapper.Restore(handle);
            WindowWrapper.MoveAndResize(handle, sc.Bounds.X, sc.Bounds.Y, sc.WorkingArea.Width / 2, sc.WorkingArea.Height);
        }

        public static void DockRight(IntPtr handle = default(IntPtr))
        {
            if (handle == default(IntPtr)) handle = GetForegroundWindow();
            Screen sc = WindowWrapper.GetScreen(handle);
            WindowWrapper.Restore(handle);
            WindowWrapper.MoveAndResize(handle, sc.Bounds.X + sc.WorkingArea.Width / 2, sc.Bounds.Y, sc.WorkingArea.Width / 2, sc.WorkingArea.Height);
        }

        public static void DockTop(IntPtr handle = default(IntPtr))
        {
            if (handle == default(IntPtr)) handle = GetForegroundWindow();
            Screen sc = WindowWrapper.GetScreen(handle);
            WindowWrapper.Restore(handle);
            WindowWrapper.MoveAndResize(handle, sc.Bounds.X, sc.Bounds.Y, sc.WorkingArea.Width, sc.WorkingArea.Height / 2);
        }

        public static void DockBottom(IntPtr handle = default(IntPtr))
        {
            if (handle == default(IntPtr)) handle = GetForegroundWindow();
            Screen sc = WindowWrapper.GetScreen(handle);
            WindowWrapper.Restore(handle);
            WindowWrapper.MoveAndResize(handle, sc.Bounds.X, sc.Bounds.Y + sc.WorkingArea.Height / 2, sc.WorkingArea.Width, sc.WorkingArea.Height / 2);
        }

        public static void ForwardScreen(IntPtr handle = default(IntPtr))
        {
            if (handle == default(IntPtr)) handle = GetForegroundWindow();
            Screen sc = WindowWrapper.GetScreen(handle);
            Screen[] all = Screen.AllScreens;
            int num = 0;
            for (int i = 0; i < all.Length - 1; i++)
            {
                if (sc.Equals(all[i]))
                {
                    num = i + 1;
                    break;
                }
            }
            MoveToScreen(handle, all[num]);
        }

        public static void BackScreen(IntPtr handle = default(IntPtr))
        {
            if (handle == default(IntPtr)) handle = GetForegroundWindow();
            Screen sc = WindowWrapper.GetScreen(handle);
            Screen[] all = Screen.AllScreens;
            int num = all.Length - 1;
            for (int i = 1; i < all.Length - 1; i++)
            {
                if (sc.Equals(all[i]))
                {
                    num = i - 1;
                    break;
                }
            }
            MoveToScreen(handle, all[num]);
        }

        private static void MoveToScreen(IntPtr handle, Screen sc)
        {
            int state = WindowWrapper.GetState(handle);
            WindowWrapper.Restore(handle);
            Screen curr = WindowWrapper.GetScreen(handle);
            Point abs = WindowWrapper.GetBounds(handle).Location;
            Point rel = new Point(abs.X - curr.Bounds.X, abs.Y - curr.Bounds.Y);
            WindowWrapper.MoveAndResize(handle, rel.X + sc.Bounds.X, rel.Y + sc.Bounds.Y);
            ShowWindow(handle, state);
        }

        public static void HideWindow(IntPtr handle)
        {
            SetWindowPos(handle, IntPtr.Zero, 0, 0, 0, 0, 0x0283);
        }

        public static IntPtr GetHandleFromProcess(string proc)
        {
            Process[] processList = Process.GetProcesses();
            foreach (Process runProc in processList)
            {
                if (runProc.ProcessName.Equals(proc))
                {
                    return runProc.MainWindowHandle;
                }
            }
            return default(IntPtr);
        }

        public static IntPtr GetHandleFromName(string className, string windowName)
        {
            return FindWindow(className, windowName);
        }

        public static IntPtr GetHandleFromName(string windowName)
        {
            return GetHandleFromName(null, windowName);
        }

        public static IntPtr GetHandleFromPoint(Point pt)
        {
            return WindowWrapper.GetHandleFromPoint(pt.X, pt.Y);
        }

        public static IntPtr GetHandleFromNameRegex(string nameRegex)
        {
            Process[] processList = Process.GetProcesses();
            foreach (Process runProc in processList)
            {
                if (Regex.IsMatch(runProc.MainWindowTitle, nameRegex))
                {
                    return runProc.MainWindowHandle;
                }
            }
            return default(IntPtr);
        }

        public static Process HandleToProcess(IntPtr handle)
        {
            uint pid;
            GetWindowThreadProcessId(handle, out pid);
            return Process.GetProcessById((int)pid);
        }

        public static IntPtr GetHandleFromPoint(int x, int y)
        {
            POINT pt = new POINT(x, y);
            return WindowFromPoint(pt);
        }

        public static IntPtr GetHandleFromCursor()
        {
            return GetHandleFromPoint(Cursor.Position);
        }

        public static Rectangle GetBounds(IntPtr handle)
        {
            RECT r;
            GetWindowRect(handle, out r); ;
            return new Rectangle(r.Left, r.Top, r.Right - r.Left, r.Bottom - r.Top);
        }

        public static Rectangle GetClientArea(IntPtr handle)
        {
            RECT r;
            GetClientRect(handle, out r);
            Point topLeft = default(Point);
            ClientToScreen(handle, ref topLeft);
            return new Rectangle(topLeft.X, topLeft.Y, r.Right - r.Left + 1, r.Bottom - r.Top + 1);
        }

        public static Point ClientToScreenPoint(IntPtr handle, Point pt)
        {
            return WindowWrapper.ClientToScreenPoint(handle, pt.X, pt.Y);
        }

        public static Point ClientToScreenPoint(IntPtr handle, int x, int y)
        {
            Point topLeft = default(Point);
            ClientToScreen(handle, ref topLeft);
            return new Point(x + topLeft.X, y + topLeft.Y);
        }

        public static Rectangle ClientToScreenRect(IntPtr handle, Rectangle rect)
        {
            Point topLeft = WindowWrapper.ClientToScreenPoint(handle, rect.X, rect.Y);
            return new Rectangle(topLeft, rect.Size);
        }

        public static int GetState(IntPtr handle = default(IntPtr))
        {
            if (handle == default(IntPtr)) handle = GetForegroundWindow();
            WINDOWPLACEMENT wp = new WINDOWPLACEMENT();
            wp.Length = Marshal.SizeOf(wp);
            GetWindowPlacement(handle, out wp);
            return wp.ShowCmd;
        }

        public static Bitmap TakePicture(IntPtr handle = default(IntPtr))
        {
            if (handle == default(IntPtr)) handle = GetForegroundWindow();
            Rectangle rect = WindowWrapper.GetBounds(handle);
            return WindowWrapper.ScreenCapture(rect);
        }

        public static Bitmap TakeClientPicture(IntPtr handle = default(IntPtr))
        {
            if (handle == default(IntPtr)) handle = GetForegroundWindow();
            Rectangle rect = WindowWrapper.GetClientArea(handle);
            return WindowWrapper.ScreenCapture(rect);
        }

        public static Bitmap TakeClientPicture(IntPtr handle, Rectangle area)
        {
            Rectangle rect = ClientToScreenRect(handle, area);
            Bitmap bmp = WindowWrapper.ScreenCapture(rect);
            if (bmp == null)
                return null;

            return bmp;
        }

        public static Bitmap ScreenCapture(Rectangle rect)
        {
            Bitmap bmp;
            try
            {
                bmp = new Bitmap(rect.Width, rect.Height);
                Graphics g = Graphics.FromImage(bmp);
                g.CopyFromScreen(rect.Location, Point.Empty, rect.Size);
                return bmp;
            }
            catch
            {
                return null;
            }
        }

        public static Point ToClientPoint(IntPtr handle, Point screenPt)
        {
            POINT pt = new POINT(screenPt.X, screenPt.Y);
            ScreenToClient(handle, ref pt);
            return new Point(pt.X, pt.Y);
        }

        public static Point CursorPosition
        {
            get
            {
                POINT pt;
                GetCursorPos(out pt);
                return new Point(pt.X, pt.Y);
            }
            set
            {
                SetCursorPos(value.X, value.Y);
            }
        }

        public static Process GetProcessFromHandle(IntPtr handle)
        {
            int id = GetProcessId(handle);
            return Process.GetProcessById(id);
        }

        public static bool ValidHandle(IntPtr handle)
        {
            return IsWindow(handle);
        }

        public static bool ForceClose(IntPtr handle)
        {
            return DestroyWindow(handle);
        }

        public static IntPtr GetParentHandle(IntPtr handle)
        {
            return GetParent(handle);
        }

        public static IntPtr[] GetChildrenHandles(IntPtr handle, string title = null)
        {
            List<IntPtr> lst = new List<IntPtr>();
            IntPtr curr = IntPtr.Zero;
            while (true)
            {
                curr = FindWindowEx(handle, curr, null, title);
                if (curr != IntPtr.Zero)
                {
                    lst.Add(curr);
                } else
                {
                    break;
                }
            }
            return lst.ToArray();
        }

        public static IntPtr GetChildByName(IntPtr handle, string title)
        {
            return FindWindowEx(handle, IntPtr.Zero, null, title);
        }

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern void SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, int uFlags);

        [DllImport("user32.dll")]
        private static extern void ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern void GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        [DllImport("user32.dll")]
        private static extern void GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        static extern bool ClientToScreen(IntPtr hWnd, ref Point lpPoint);

        [DllImport("user32.dll")]
        private static extern void GetWindowPlacement(IntPtr hWnd, out WINDOWPLACEMENT lpwndpl);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        static extern IntPtr WindowFromPoint(POINT point);

        [DllImport("user32.dll")]
        static extern bool ScreenToClient(IntPtr hWnd, ref POINT lpPoint);

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        [DllImport("user32.dll")]
        static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll")]
        static extern bool SetCursorPos(int X, int Y);

        [DllImport("kernel32.dll")]
        static extern int GetProcessId(IntPtr handle);

        [DllImport("user32.dll")]
        static extern bool IsWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern bool DestroyWindow(IntPtr hWnd);

        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        static extern IntPtr GetParent(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className, string windowTitle);

        [Flags]
        private enum SnapshotFlags : uint
        {
            HeapList = 0x00000001,
            Process = 0x00000002,
            Thread = 0x00000004,
            Module = 0x00000008,
            Module32 = 0x00000010,
            All = (HeapList | Process | Thread | Module),
            Inherit = 0x80000000,
            NoHeaps = 0x40000000
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct PROCESSENTRY32
        {
            public uint dwSize;
            public uint cntUsage;
            public uint th32ProcessID;
            public IntPtr th32DefaultHeapID;
            public uint th32ModuleID;
            public uint cntThreads;
            public uint th32ParentProcessID;
            public int pcPriClassBase;
            public uint dwFlags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szExeFile;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct WINDOWPLACEMENT
        {
            public int Length;
            public int Flags;
            public int ShowCmd;
            public Point PtMinPosition;
            public Point PtMaxPosition;
            public Rectangle RcNormalPosition;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;

            public POINT(int x, int y)
            {
                this.X = x;
                this.Y = y;
            }
        }
    }
}