using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

using AutoHotKeyNET;
using FastBitmap;
using System.Diagnostics;

using WindowsInput;

namespace Basketball
{
    class Program
    {
        private static readonly string PRECOMP_DIR = @"Precomp";
        private static int LEVEL = 0;
        private static IntPtr HANDLE;

        static Program()
        {
            HANDLE = FindBluestacksHandle();
            AutoHotKey.PathEXE = @"Exe\AutoHotkeyU64.exe";
        }

        static void Main(string[] args)
        {
            while (true)
            {
                string raw;
                string[] input;
                do
                {
                    Console.Write("Input command: ");
                    raw = Console.ReadLine();
                    input = raw.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                } while (input.Length == 0);
                switch (input[0].ToUpper())
                {
                    case "GEN":
                        if (!Directory.Exists(PRECOMP_DIR))
                        {
                            Directory.CreateDirectory(PRECOMP_DIR);
                        }
                        for (int x = -72; x <= 72; x++)
                        {
                            for (int y = -72; y <= 72; y++)
                            {
                                string file = string.Format("{0}_{1}.ahk", x, y);
                                string path = Path.Combine(PRECOMP_DIR, file);
                                string cmd = string.Format("CoordMode, Mouse, Screen\nMouseClickDrag, Left, 0, 0, {0}, {1}, 1, R", x, y);
                                File.WriteAllText(path, cmd);
                                Console.WriteLine("Finished: {0}, {1}", x, y);
                            }
                        }
                        break;
                    case "BENCH":
                        Bitmap bmpSS = new Bitmap("trial.png");
                        using (Bitmap24 bmp24 = Bitmap24.FromImage(bmpSS))
                        {
                            bmp24.Lock();
                            Stopwatch sw = new Stopwatch();
                            sw.Start();
                            Point ballPt = FindBasketball(bmp24);
                            Point basketPt = FindBasket(bmp24);
                            sw.Stop();
                            Console.WriteLine("basket: {0}, ball: {1}, {2} [ms]", basketPt, ballPt, sw.ElapsedMilliseconds);
                        }
                        break;
                    case "CURSOR":
                        Point pt = Cursor.Position;
                        Console.WriteLine("Cursor: ({0}, {1})", pt.X, pt.Y);
                        break;
                    case "COMPARE":
                        Bitmap bmpTest = WindowWrapper.TakeClientPicture(HANDLE);
                        using (Bitmap24 bmpTest24 = Bitmap24.FromImage(bmpTest))
                        {
                            bmpTest24.Lock();
                            for (int y = 0; y < bmpTest.Height; y++)
                            {
                                for (int x = 0; x < bmpTest.Width; x++)
                                {
                                    Color col = bmpTest.GetPixel(x, y);
                                    int[] arr = bmpTest24.GetPixel(x, y);
                                    if (col.R != arr[0] || col.G != arr[1] || col.B != arr[2])
                                    {
                                        Console.WriteLine("MISMATCH AT ({0}, {1})", x, y);
                                    }
                                }
                            }
                        }
                        break;
                    case "IMAGE":
                        Stopwatch stopwatch = new Stopwatch();
                        stopwatch.Start();
                        IntPtr hWnd = WindowWrapper.GetHandleFromCursor();
                        Console.WriteLine("window name: " + WindowWrapper.GetText(hWnd));
                        Bitmap img = WindowWrapper.TakeClientPicture(hWnd);
                        Console.WriteLine("pixels captured = {0} x {1} = {2}", img.Width, img.Height, img.Width * img.Height);
                        Console.WriteLine("pixels per ms: {0} px/ms", (double)img.Width * img.Height / stopwatch.ElapsedMilliseconds);
                        Console.WriteLine("ss time: {0} [ms]", stopwatch.ElapsedMilliseconds);
                        stopwatch.Restart();
                        img.Save("test.bmp");
                        Console.WriteLine("save time: {0} [ms]", stopwatch.ElapsedMilliseconds);
                        Console.WriteLine("Parent hierarchy:");
                        IntPtr curr = hWnd;
                        while (curr != IntPtr.Zero)
                        {
                            Console.WriteLine("{0}: {1}", curr, WindowWrapper.GetText(curr));
                            curr = WindowWrapper.GetParentHandle(curr);
                        }
                        break;
					case "HANDLE":
						HANDLE = FindBluestacksHandle();
						Console.WriteLine("Handle = {0}", HANDLE);
						break;
					case "WINDOW":
						HANDLE = WindowWrapper.GetHandleFromCursor();
						Console.WriteLine("Handle = {0}", HANDLE);
						break;
                    case "CALC":
                        double[] velArr = CalculateVelocity(HANDLE, 500);
                        Console.WriteLine("velocity = <{0}, {1}>", velArr[0], velArr[1]);
                        break;
                    case "SIGN":
                        int[] sgnArr = GetVelocitySign(HANDLE, 0);
                        Console.WriteLine("signs = <{0}, {1}>", sgnArr[0], sgnArr[1]);
                        break;
                    case "BOUNDS":
                        Rectangle bounds = EstimateBasketBorder(HANDLE, 16000);
                        Console.WriteLine(bounds);
                        Console.WriteLine("left = " + bounds.Left);
                        Console.WriteLine("rite = " + bounds.Right);
                        Console.WriteLine("top  = " + bounds.Top);
                        Console.WriteLine("bot  = " + bounds.Bottom);
                        break;
                    case "BOUNCE":
                        Rectangle fakeBounds = new Rectangle(0, 0, 17, 10);
                        Point ans = PredictPosition(new Point(3, 1), new double[] { 1, 1 }, 119, fakeBounds);
                        Console.WriteLine(fakeBounds);
                        Console.WriteLine(fakeBounds.Bottom);
                        Console.WriteLine(ans);
                        break;
                    case "TEST":
                        DoSingleRun();
                        break;
                    case "AUTO":
                        AutoAim();
                        break;
                    case "FREELO":
                        bool[] stop = new bool[1];
                        Task.Factory.StartNew(() =>
                        {
                            int amt = (input.Length >= 2 && int.TryParse(input[1], out amt)) ? amt : int.MaxValue;
                            for (int i = 0; i < amt && !stop[0]; i++)
                            {
                                AutoAim(stop);
                                Point ball;
                                do
                                {
                                    Thread.Sleep(100);
                                    ball = FindBasketball(HANDLE);

                                } while (ball.IsEmpty);
                                Thread.Sleep(1000);
                            }
                            if (!stop[0])
                            {
                                Console.WriteLine("Successfully acquired freelo.  Press any key to continue.");
                                stop[0] = true;
                            }
                        });
                        Console.ReadKey();
                        if (!stop[0])
                        {
                            Console.WriteLine("The operation was terminated by the user.");
                            stop[0] = true;
                        }
                        break;
                    case "RESET":
                        LEVEL = 0;
                        Console.WriteLine("Reset to level {0}", LEVEL);
                        break;
                    case "DERANK":
                        LEVEL--;
                        Console.WriteLine("Back to level {0}", LEVEL);
                        break;
                    case "UPRANK":
                        LEVEL++;
                        Console.WriteLine("Upped to level {0}", LEVEL);
                        break;
                    case "SETLEVEL":
                        if (input.Length >= 2)
                        {
                            int.TryParse(input[1], out LEVEL);
                            Console.WriteLine("Set to level {0}", LEVEL);
                        }
                        else
                        {
                            Console.WriteLine("Incorrect number of arguments.");
                        }
                        break;
                    case "QUIT":
                        return;
                    default:
                        Console.WriteLine("Invalid input.");
                        break;
                }
            }
        }

        private static string TEMP_FILE = @"Temp.ahk";
        private static bool RunAHKString(string command, string tempFile)
        {
            File.WriteAllText(tempFile, command);
            return AutoHotKey.RunAHK(tempFile);
        }

        private static Point Shoot(Point ball, Point hoop)
        {
            double dx = (hoop.X - ball.X) * 0.75;
            double dy = hoop.Y - ball.Y;
            double r = Math.Sqrt(dx * dx + dy * dy);
            int x = (int)(dx / r * 72);
            int y = (int)(dy / r * 72);

            //string cmd = string.Format("CoordMode, Mouse, Screen\nMouseClickDrag, Left, 0, 0, {0}, {1}, 0, R", x, y);
            //RunAHKString(cmd, TEMP_FILE);

            InputSimulator sim = new InputSimulator();
            sim.Mouse.LeftButtonDown();
            Thread.Sleep(5);
            sim.Mouse.MoveMouseBy(x, y);
            Thread.Sleep(5);
            sim.Mouse.LeftButtonUp();
            Thread.Sleep(5);

            return new Point(x, y);

            //string file = string.Format("{0}_{1}.ahk", x, y);
            //string path = Path.Combine(PRECOMP_DIR, file);
            //AutoHotKey.RunAHK(path);

            //AutoHotKey.RunAHK(@"AHK\MouseLeftDown");
            //Thread.Sleep(15);
            //Cursor.Position = new Point(ball.X + x, ball.Y + y);
            //Console.WriteLine(Cursor.Position);
            //Thread.Sleep(15);
            //AutoHotKey.RunAHK(@"AHK\MouseLeftUp");
        }

        private static void Drag(int x1, int y1, int x2, int y2)
        {
            //string cmd = string.Format("CoordMode, Mouse, Screen\nMouseClickDrag, Left, {0}, {1}, {2}, {3}, 20", x1, y1, x2, y2);
            //RunAHKString(cmd, TEMP_FILE);
            int IVL = 100;
            double dx = (double)(x2 - x1) / IVL;
            double dy = (double)(y2 - y1) / IVL;
            AutoHotKey.RunAHK(@"AHK\MouseLeftClick");
            Thread.Sleep(500);
            AutoHotKey.RunAHK(@"AHK\MouseLeftDown");
            Thread.Sleep(500);
            //for (int i = 0; i <= IVL; i++)
            //{
            //    int x = (int)(x1 + i * dx);
            //    int y = (int)(y1 + i * dy);
            //    Cursor.Position = new Point(x, y);
            //    Thread.Sleep(5);
            //}
            RunAHKString(string.Format("CoordMode, Mouse, Screen\nMouseMove, {0}, {1}, 20", x2, y2), TEMP_FILE);
            Thread.Sleep(500);
            AutoHotKey.RunAHK(@"AHK\MouseLeftUp");
        }

        private static readonly int BALL_Y = 643;
        private static readonly int BALL_WIDTH = 132;
        private static Point FindBasketball(Bitmap24 b24)
        {
            int xp = FindAnyBasketballPointX(b24);
            if (xp == 0)
            {
                return Point.Empty;
            }
            int left = Math.Max(200, xp - BALL_WIDTH);
            int rite = xp;
            int x = xp;
            while (left <= rite)
            {
                int mid = (left + rite) / 2;
                int[] arr = b24.GetPixel(mid, BALL_Y);
                if (!WhitePixel(arr))
                {
                    x = mid;
                    rite = mid - 1;
                }
                else
                {
                    left = mid + 1;
                }
            }
            int half = BALL_WIDTH / 2;
            int xc = x + half;
            int leftPos = xc - half + 2;
            int ritePos = xc + half - 2;
            if (leftPos < 200 || WhitePixel(b24.GetPixel(leftPos, BALL_Y)) || ritePos > 1080 || WhitePixel(b24.GetPixel(ritePos, BALL_Y)))
            {
                return Point.Empty;
            }
            return new Point(x + BALL_WIDTH / 2, BALL_Y);
        }

        private static Point FindBasketball(IntPtr handle)
        {
            Bitmap bmp = WindowWrapper.TakeClientPicture(handle);
            using (Bitmap24 b24 = Bitmap24.FromImage(bmp))
            {
                b24.Lock();
                Point rim = FindBasketball(b24);
                b24.Unlock();
                bmp.Dispose();
                return rim;
            }
        }

        private static int FindAnyBasketballPointX(Bitmap24 b24)
        {
            int dx = Math.Max(1, BALL_WIDTH - 2);
            for (int x = 402; x <= 877; x += dx)
            {
                int[] arr = b24.GetPixel(x, BALL_Y);
                if (!WhitePixel(arr))
                {
                    return x;
                }
            }
            return 0;
        }

        private static bool WhitePixel(int[] arr)
        {
            return (arr[0] == 255 && arr[1] == 255 && arr[2] == 255);
        }

        private static Point FindBasket(IntPtr handle)
        {
            Bitmap24 b24;
            Point rim = FindBasket(handle, out b24);
            b24.Unlock();
            b24.Bitmap.Dispose();
            return rim;
        }

        private static Point FindBasket(IntPtr handle, out Bitmap24 b24)
        {
            Bitmap bmp = WindowWrapper.TakeClientPicture(handle);
            b24 = Bitmap24.FromImage(bmp);
            b24.Lock();
            Point rim = FindBasket(b24);
            return rim;
        }

        private static double[] CalculateVelocity(IntPtr handle, int time)
        {
            Bitmap24 b24;
            double[] v = CalculateVelocity(handle, time, out b24);
            b24.Unlock();
            b24.Bitmap.Dispose();
            return v;
        }

        private static double[] CalculateVelocity(IntPtr handle, int time, out Bitmap24 b24)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            Point p1 = FindBasket(handle);
            int rem = (int)(time - sw.ElapsedMilliseconds);
            if (rem > 0)
            {
                Thread.Sleep(rem);
            }
            sw.Stop();
            Point p2 = FindBasket(handle, out b24);
            return new double[] { 1000.0 * (p2.X - p1.X) / sw.ElapsedMilliseconds, 1000.0 * (p2.Y - p1.Y) / sw.ElapsedMilliseconds };
        }

        private static int[] GetVelocitySign(IntPtr handle, int time = 0)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            Point p1 = FindBasket(handle);
            int rem = (int)(time - sw.ElapsedMilliseconds);
            if (rem > 0)
            {
                Thread.Sleep(rem);
            }
            sw.Stop();
            Point p2 = FindBasket(handle);
            return new int[] { Math.Sign(p2.X - p1.X), Math.Sign(p2.Y - p1.Y) };
        }

        private static Rectangle EstimateBasketBorder(IntPtr handle, int time)
        {
            int left = int.MaxValue, rite = 0, top = int.MaxValue, bot = 0;
            Stopwatch sw = new Stopwatch();
            sw.Start();
            while (sw.ElapsedMilliseconds < time)
            {
                Point pt = FindBasket(handle);
                left = Math.Min(left, pt.X);
                rite = Math.Max(rite, pt.X);
                top = Math.Min(top, pt.Y);
                bot = Math.Max(bot, pt.Y);
            }
            if (left > rite || top > bot)
            {
                return Rectangle.Empty;
            }
            return new Rectangle(left, top, rite - left, bot - top);
        }

        private static readonly double[] VELOCITIES_X = { 0, 85, 170 };
        private static readonly double[] VELOCITIES_Y = { 0 };
        private static double[] TransformVelocity(double[] vel)
        {
            int sgnX = Math.Sign(vel[0]);
            double posX = Math.Abs(vel[0]);
            double bestX = int.MaxValue;
            foreach (int vx in VELOCITIES_X)
            {
                if (Math.Abs(posX - vx) < Math.Abs(posX - bestX))
                {
                    bestX = vx;
                }
            }
            int sgnY = Math.Sign(vel[1]);
            double posY = Math.Abs(vel[1]);
            double bestY = int.MaxValue;
            foreach (int vy in VELOCITIES_Y)
            {
                if (Math.Abs(posY - vy) < Math.Abs(posY - bestY))
                {
                    bestY = vy;
                }
            }
            return new double[] { sgnX * bestX, sgnY * bestY };
        }

        private static readonly int BASKET_WIDTH = 112;
        private static readonly int BASKET_HEIGHT = 6;
        private static Point FindBasket(Bitmap24 b24)
        {
            Point pt = FindAnyBasketPoint(b24);
            if (pt.IsEmpty)
            {
                return Point.Empty;
            }
            int[] arr = GetBasketHeight(b24, pt.X, pt.Y);
            if (arr[0] < BASKET_HEIGHT)
            {
                pt.X -= 5;
                arr = GetBasketHeight(b24, pt.X, pt.Y);
            }
            if (arr[0] < BASKET_HEIGHT)
            {
                pt.X += 10;
                arr = GetBasketHeight(b24, pt.X, pt.Y);
            }
            if (arr[0] < BASKET_HEIGHT)
            {
                return Point.Empty;
            }
            int yp = arr[1] - BASKET_HEIGHT / 2;
            int left = Math.Max(5, pt.X - BASKET_WIDTH);
            int rite = pt.X;
            int xp = pt.X;
            while (left <= rite)
            {
                int mid = (left + rite) / 2;
                int[] color = b24.GetPixel(mid, yp);
                if (RedBasketPixel(color))
                {
                    rite = mid - 1;
                    xp = mid;
                }
                else
                {
                    left = mid + 1;
                }
            }
            return new Point(xp + BASKET_WIDTH / 2, yp);
        }

        private static Point FindAnyBasketPoint(Bitmap24 b24)
        {
            int dx = Math.Max(1, BASKET_WIDTH - 4);
            int dy = Math.Max(1, BASKET_HEIGHT - 2);
            for (int y = 230; y <= 372; y += dy)
            {
                for (int x = 402; x <= 877; x += dx)
                {
                    int[] arr = b24.GetPixel(x, y);
                    if (RedBasketPixel(arr))
                    {
                        return new Point(x, y);
                    }
                }
            }
            return Point.Empty;
        }

        private static Point PredictPosition(Point start, double[] v, double t, Rectangle bounds)
        {
            int dx = (int)(v[0] * t);
            int dy = (int)(v[1] * t);
            if (bounds.IsEmpty)
            {
                return new Point(start.X + dx, start.Y + dy);
            }
            dx %= Math.Max(1, 2 * bounds.Width);
            dy %= Math.Max(1, 2 * bounds.Height);
            int x = (int)(start.X + dx);
            int y = (int)(start.Y + dy);
            for (int i = 0; i < 2; i++)
            {
                if (x < bounds.Left)
                {
                    x = Reflect(x, bounds.Left);
                }
                if (x > bounds.Right)
                {
                    x = Reflect(x, bounds.Right);
                }
                if (y < bounds.Top)
                {
                    y = Reflect(y, bounds.Top);
                }
                if (y > bounds.Bottom)
                {
                    y = Reflect(y, bounds.Bottom);
                }
            }
            return new Point(x, y);
        }

        private static int Reflect(int pos, int axis)
        {
            return 2 * axis - pos;
        }

        private static int[] GetBasketHeight(Bitmap24 b24, int x, int y)
        {
            int last = 0;
            int cnt = 0;
            for (int dy = -BASKET_HEIGHT; dy <= BASKET_HEIGHT; dy++)
            {
                int[] arr = b24.GetPixel(x, y + dy);
                if (RedBasketPixel(arr))
                {
                    cnt++;
                    last = y + dy;
                }
            }
            return new int[] { cnt, last };
        }

        private static bool RedBasketPixel(int[] arr)
        {
            return arr[0] == 255 && arr[1] == 38 && arr[2] == 15;
        }

        private static string[] HANDLE_HIERARCHY = { "Bluestacks App Player", "", "Messenger", "BlueStacks Android Plugin" };
        private static IntPtr FindBluestacksHandle()
        {
            IntPtr parent = WindowWrapper.GetHandleFromName(HANDLE_HIERARCHY[0]);
            Stack<IntPtr> stack = new Stack<IntPtr>();
            Stack<int> depth = new Stack<int>();
            stack.Push(parent);
            depth.Push(0);
            while (stack.Count > 0)
            {
                IntPtr curr = stack.Pop();
                int d = depth.Pop();
                string title = WindowWrapper.GetText(curr);
                if (d == HANDLE_HIERARCHY.Length - 1)
                {
                    return curr;
                }
                IntPtr[] children = WindowWrapper.GetChildrenHandles(curr, HANDLE_HIERARCHY[d + 1]);
                foreach (IntPtr hWnd in children)
                {
                    stack.Push(hWnd);
                    depth.Push(d + 1);
                }
            }
            return IntPtr.Zero;
        }

        private static Rectangle GetBoundsFor(int level)
        {
            if (level < 10)
            {
                return new Rectangle(640, 235, 0, 0);
            }
            else if (level < 20)
            {
                return new Rectangle(477, 235, 325, 0);
            }
            else if (level < 30)
            {
                return new Rectangle(477, 235, 325, 66);
            }
            else if (level < 40)
            {
                return new Rectangle(477, 235, 325, 66);
            }
            else
            {
                return new Rectangle(477, 235, 325, 132);
            }
        }

        private static double[] GetVelocitiesFor(int level)
        {
            if (level < 10)
            {
                return new double[] { 0, 0 };
            }
            else if (level < 20)
            {
                return new double[] { 88, 0 };
            }
            else if (level < 30)
            {
                return new double[] { 176, 0 };
            }
            else if (level < 40)
            {
                return new double[] { 176, 44 };
            }
            else
            {
                return new double[] { 176, 88 };
            }
        }

        private static int GetShotsFor(int level)
        {
            if (level < 40)
            {
                return 20;
            } else
            {
                return 8;
            }
        }

        private static double[] ValidateVelocityFor(int level, double[] v)
        {
            double[] vel = GetVelocitiesFor(level);
            double vx = Math.Abs(v[0]);
            double vy = Math.Abs(v[1]);
            if (Math.Abs(vx - vel[0]) < 20 && Math.Abs(vy - vel[1]) < 20)
            {
                return new double[] { Math.Sign(v[0]) * vel[0], Math.Sign(v[1]) * vel[1] };
            }
            else
            {
                return null;
            }
        }

        private static bool TakeShot(int level, Point ball, Point pred, double[] v)
        {
            if (level < 10)
            {
                return true;
            }
            else if (level < 20)
            {
                return Math.Abs(pred.X - ball.X) <= 200;
            }
            else if (level < 30)
            {
                return Math.Abs(pred.X - ball.X) <= 150;
            }
            else if (level < 40)
            {
                return Math.Abs(pred.X - ball.X) <= 125 && v[1] < 0;
            }
            else
            {
                return Math.Abs(pred.X - ball.X) <= 100 && pred.Y <= 300 && v[1] < 0;
            }
        }

        private static bool AcceptableBasket(int level, Point rim)
        {
            if (level < 10)
            {
                return true;
            }
            else if (level < 30)
            {
                return rim.X > 500 && rim.X < 790;
            }
            else
            {
                return rim.X > 500 && rim.X < 790 && rim.Y > 240 && rim.Y < 350;
            }
        }

        private static void DoSingleRun()
        {
            //Thread.Sleep(100);
            //AutoHotKey.RunAHK(@"AHK\MouseLeftDown");

            //AutoHotKey.RunAHK(@"AHK\Test");

            IntPtr self = WindowWrapper.GetForegroundWindow();

            Stopwatch tmr = new Stopwatch();
            //double[] vel = CalculateVelocity(handle, 160);
            //double[] v = TransformVelocity(vel);
            Rectangle rect = WindowWrapper.GetClientArea(HANDLE);
            tmr.Start();
            Bitmap bmp = WindowWrapper.TakeClientPicture(HANDLE);
            using (Bitmap24 b24 = Bitmap24.FromImage(bmp))
            {
                b24.Lock();
                Point ball = FindBasketball(b24);
                Point rim = FindBasket(b24);
                Point here = new Point(ball.X + rect.X, ball.Y + rect.Y);
                Point basket = new Point(rim.X + rect.X, rim.Y + rect.Y);

                Console.WriteLine("relative rim: {0}", rim);
                Console.WriteLine("absolute ball: {0}", ball);
                Console.WriteLine("absolute basket: {0}", basket);
                if (!ball.IsEmpty && !rim.IsEmpty)
                {
                    Rectangle levelBounds = GetBoundsFor(LEVEL);
                    int[] sgn = GetVelocitySign(HANDLE, 100);
                    double[] vel = GetVelocitiesFor(LEVEL);
                    double[] v = { sgn[0] * vel[0], sgn[1] * vel[1] };
                    Console.WriteLine("v = <{0}, {1}>", v[0], v[1]);
                    Console.WriteLine("L = {0}, R = {1}, T = {2}, B = {3}", levelBounds.Left, levelBounds.Right, levelBounds.Top, levelBounds.Bottom);
                    //Console.WriteLine("raw velocity: {0}, {1}", vel[0], vel[1]);
                    //Console.WriteLine("corrected velocity: {0}, {1}", v[0], v[1]);
                    WindowWrapper.BringToFront(HANDLE);
                    for (int i = 1; i <= 8; i++)
                    {
                        double SHOT_TIME = 0.8;
                        int dx = (int)Math.Round((SHOT_TIME + tmr.ElapsedMilliseconds / 1000.0) * v[0]);
                        int dy = (int)Math.Round((SHOT_TIME + tmr.ElapsedMilliseconds / 1000.0) * v[1]);
                        Point start = new Point(here.X, here.Y);
                        Point pred = PredictPosition(rim, v, SHOT_TIME + tmr.ElapsedMilliseconds / 1000.0, levelBounds);
                        Point target = new Point(pred.X + rect.X, pred.Y + rect.Y);
                        //Point target = new Point(basket.X + dx, basket.Y + dy);
                        Cursor.Position = start;
                        Point shot = Shoot(start, target);
                        Console.WriteLine("  -> Shot {0}: pred = {1}, target = {2}, vector = <{3}, {4}>", i, pred, target, shot.X, shot.Y);
                        //Rectangle upRect = new Rectangle(levelBounds.Location, new Size(levelBounds.Width + 1, levelBounds.Height + 1));
                        //if (!upRect.Contains(pred))
                        //{
                        //    Console.WriteLine("      VIOLATION: shot out of defined bounds!");
                        //}
                    }
                    LEVEL++;
                    Console.WriteLine("Advanced to level {0}", LEVEL);
                }
                else
                {
                    Console.WriteLine("Error: failed to locate basket or ball.");
                }
                b24.Unlock();
                bmp.Dispose();
            }
            WindowWrapper.BringToFront(self);

            //Cursor.Position = new Point(2171, 382);
            //AutoHotKey.RunAHK(@"AHK\MouseLeftClick");
            //Thread.Sleep(500);
            //Cursor.Position = new Point(2171, 382);
            //AutoHotKey.RunAHK(@"AHK\MouseLeftClick");

            //Cursor.Position = new Point(2724, 540);
            //AutoHotKey.RunAHK(@"AHK\MouseLeftClick");
            //AutoHotKey.RunAHK(@"AHK\MouseLeftClick");
            //Cursor.Position = new Point(2724, 539);
            //Thread.Sleep(1000);
            //AutoHotKey.RunAHK(@"AHK\MouseLeftDown");
            //Thread.Sleep(100);
            //Cursor.Position = new Point(2257, 535);
            //AutoHotKey.RunAHK(@"AHK\MouseLeftUp");
        }

        private static void AutoAim()
        {
            AutoAim(new bool[1]);
        }

        private static void AutoAim(bool[] stop)
        {
            IntPtr self = WindowWrapper.GetForegroundWindow();
            Rectangle rect = WindowWrapper.GetClientArea(HANDLE);
            Rectangle levelBounds = GetBoundsFor(LEVEL);
            int levelShots = GetShotsFor(LEVEL);
            Stopwatch tmr = new Stopwatch();

            while (!stop[0])
            {
                Bitmap24 b24;
                double[] vel = CalculateVelocity(HANDLE, 100, out b24);
                tmr.Restart();
                Point ball = FindBasketball(b24);
                Point rim = FindBasket(b24);
                double[] v = ValidateVelocityFor(LEVEL, vel);
                if (!ball.IsEmpty && !rim.IsEmpty && v != null)
                {
                    int shots = 0;
                    for (int i = 1; i <= levelShots && !stop[0]; i++)
                    {
                        double SHOT_TIME = 0.75;
                        int dx = (int)Math.Round((SHOT_TIME + tmr.ElapsedMilliseconds / 1000.0) * v[0]);
                        int dy = (int)Math.Round((SHOT_TIME + tmr.ElapsedMilliseconds / 1000.0) * v[1]);
                        Point pred = PredictPosition(rim, v, SHOT_TIME + tmr.ElapsedMilliseconds / 1000.0, levelBounds);
                        if (TakeShot(LEVEL, ball, pred, v) && !stop[0])
                        {
                            if (shots == 0)
                            {
                                WindowWrapper.BringToFront(HANDLE);
                            }
                            Point start = new Point(ball.X + rect.X, ball.Y + rect.Y);
                            Point target = new Point(pred.X + rect.X, pred.Y + rect.Y);
                            Cursor.Position = start;
                            Point shot = Shoot(start, target);
                            Console.WriteLine("  -> Shot {0}: pred = {1}, target = {2}, vector = <{3}, {4}>", i, pred, target, shot.X, shot.Y);
                            shots++;
                        }
                    }

                    if (shots > 0)
                    {
                        WindowWrapper.BringToFront(self);
                        if (shots < 4)
                        {
                            Console.WriteLine("Too few shots taken, remained at level {0}", LEVEL);
                        }
                        else
                        {
                            LEVEL++;
                            Console.WriteLine("Advanced to level {0}", LEVEL);
                            break;
                        }
                    }
                }
                b24.Dispose();
            }
        }
    }
}
