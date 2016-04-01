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
        private static string PRECOMP_DIR = @"Precomp";
		static void Main(string[] args)
		{
			AutoHotKey.PathEXE = @"Exe\AutoHotkeyU64.exe";
            IntPtr handle = FindBluestacksHandle();
            int level = 0;
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
						Bitmap24 bmp24 = new Bitmap24(bmpSS);
						bmp24.Lock();
						Stopwatch sw = new Stopwatch();
						sw.Start();
						Point ballPt = FindBasketball(bmp24);
						Point basketPt = FindBasket(bmp24);
						sw.Stop();
						bmp24.Unlock();
						bmpSS.Dispose();
						Console.WriteLine("basket: {0}, ball: {1}, {2} [ms]", basketPt, ballPt, sw.ElapsedMilliseconds);
						break;
					case "CURSOR":
						Point pt = Cursor.Position;
						Console.WriteLine("Cursor: ({0}, {1})", pt.X, pt.Y);
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
                    case "CALC":
                        double[] velArr = CalculateVelocity(handle, 500);
                        Console.WriteLine("velocity = <{0}, {1}>", velArr[0], velArr[1]);
                        break;
                    case "SIGN":
                        int[] sgnArr = GetVelocitySign(handle, 0);
                        Console.WriteLine("signs = <{0}, {1}>", sgnArr[0], sgnArr[1]);
                        break;
                    case "BOUNDS":
						Rectangle bounds = EstimateBasketBorder(handle, 16000);
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
                        //Thread.Sleep(100);
                        //AutoHotKey.RunAHK(@"AHK\MouseLeftDown");

                        //AutoHotKey.RunAHK(@"AHK\Test");

                        IntPtr self = WindowWrapper.GetForegroundWindow();

                        Stopwatch tmr = new Stopwatch();
                        //double[] vel = CalculateVelocity(handle, 160);
                        //double[] v = TransformVelocity(vel);
                        Rectangle rect = WindowWrapper.GetClientArea(handle);
                        tmr.Start();
                        Bitmap bmp = WindowWrapper.TakeClientPicture(handle);
                        bmp.Save("temp.bmp");
                        Bitmap24 b24 = new Bitmap24(bmp);
                        b24.Lock();
                        Point ball = FindBasketball(b24);
                        Point rim = FindBasket(b24);
                        Point here = new Point(ball.X + rect.X, ball.Y + rect.Y);
                        Point basket = new Point(rim.X + rect.X, rim.Y + rect.Y);
                        b24.Unlock();
                        bmp.Dispose();

                        Console.WriteLine("relative rim: {0}", rim);
                        Console.WriteLine("absolute ball: {0}", ball);
                        Console.WriteLine("absolute basket: {0}", basket);
                        if (!ball.IsEmpty && !rim.IsEmpty)
                        {
                            Rectangle levelBounds = GetBoundsFor(level);
                            int[] sgn = GetVelocitySign(handle, 100);
                            double[] vel = GetVelocitiesFor(level);
                            double[] v = { sgn[0] * vel[0], sgn[1] * vel[1] };
                            Console.WriteLine("v = <{0}, {1}>", v[0], v[1]);
                            Console.WriteLine("L = {0}, R = {1}, T = {2}, B = {3}", levelBounds.Left, levelBounds.Right, levelBounds.Top, levelBounds.Bottom);
                            //Console.WriteLine("raw velocity: {0}, {1}", vel[0], vel[1]);
                            //Console.WriteLine("corrected velocity: {0}, {1}", v[0], v[1]);
                            WindowWrapper.BringToFront(handle);
							for (int i = 1; i <= 8; i++)
                            {
                                double SHOT_TIME = 0.75;
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
                            level++;
                            Console.WriteLine("Advanced to level {0}", level);
						}
						else
						{
							Console.WriteLine("Error: failed to locate basket or ball.");
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
                        break;
                    case "RESET":
                        level = 0;
                        Console.WriteLine("Reset to level {0}", level);
                        break;
                    case "DERANK":
                        level--;
                        Console.WriteLine("Back to level {0}", level);
                        break;
                    case "UPRANK":
                        level++;
                        Console.WriteLine("Upped to level {0}", level);
                        break;
                    case "SETLEVEL":
                        if (input.Length >= 2)
                        {
                            int.TryParse(input[1], out level);
                            Console.WriteLine("Set to level {0}", level);
                        }
                        else
                        {
                            Console.WriteLine("Incorrect number of arguments.");
                        }
                        break;
                    case "QUIT":
						Console.WriteLine("Press any key to exit.");
						Console.ReadKey();
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
			return new Point(x + BALL_WIDTH / 2, BALL_Y);
        }

		private static int FindAnyBasketballPointX(Bitmap24 b24)
		{
			int dx = Math.Max(1, BALL_WIDTH - 2);
			for (int x = 200; x < 1080; x += dx)
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
            Bitmap bmp = WindowWrapper.TakeClientPicture(handle);
            Bitmap24 b24 = new Bitmap24(bmp);
            b24.Lock();
            Point rim = FindBasket(b24);
            b24.Unlock();
            bmp.Dispose();
            return rim;
        }

        private static double[] CalculateVelocity(IntPtr handle, int time)
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
			//Console.WriteLine(p1.X - p2.X);
			//Console.WriteLine(p1.Y - p2.Y);
			//Console.WriteLine(sw.ElapsedMilliseconds);
			//Console.WriteLine(1000.0 * (p2.X - p1.X) / sw.ElapsedMilliseconds);
			//Console.WriteLine(1000.0 * (p2.Y - p1.Y) / sw.ElapsedMilliseconds);
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
			for (int y = 50; y < b24.Bitmap.Height; y += dy)
			{
				for (int x = 5; x < b24.Bitmap.Width; x += dx)
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
                return new double[] { 175, 0 };
            }
            else if (level < 40)
            {
                return new double[] { 175, 43 };
            }
            else
            {
                return new double[] { 175, 87 };
            }
        }
    }
}
