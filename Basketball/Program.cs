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
            AutoHotKey.RunAsAdmin = true;
            IntPtr handle = WindowWrapper.GetHandleFromName("Bluestacks App Player");
			while (true)
			{
				Console.Write("Input command: ");
				string raw = Console.ReadLine();
				string input = raw.ToUpper();
				switch (input)
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
                        break;
                    case "CALC":
                        Console.WriteLine(CalculateVelocity(handle, 500));
                        break;
					case "TEST":
                        //Thread.Sleep(100);
                        //AutoHotKey.RunAHK(@"AHK\MouseLeftDown");

                        //AutoHotKey.RunAHK(@"AHK\Test");

                        IntPtr self = WindowWrapper.GetForegroundWindow();

                        Stopwatch tmr = new Stopwatch();
                        double[] vel = CalculateVelocity(handle, 160);
                        double[] v = TransformVelocity(vel);
                        Rectangle rect = WindowWrapper.GetClientArea(handle);
                        tmr.Start();
                        Bitmap bmp = WindowWrapper.TakeClientPicture(handle);
                        Bitmap24 b24 = new Bitmap24(bmp);
                        b24.Lock();
                        Point ball = FindBasketball(b24);
                        Point rim = FindBasket(b24);
                        Point here = new Point(ball.X + rect.X, ball.Y + rect.Y);
                        Point basket = new Point(rim.X + rect.X, rim.Y + rect.Y);
                        b24.Unlock();
                        bmp.Dispose();

                        //Drag(here.X, here.Y, basket.X, basket.Y);
                        WindowWrapper.BringToFront(handle);
                        for (int i = 0; i < 8; i++)
                        {
                            double SHOT_TIME = 0.8;
                            int dx = (int)Math.Round((SHOT_TIME + tmr.ElapsedMilliseconds / 1000.0) * v[0]);
                            int dy = (int)Math.Round((SHOT_TIME + tmr.ElapsedMilliseconds / 1000.0) * v[1]);
                            Point start = new Point(here.X, here.Y);
                            Point target = new Point(basket.X + dx, basket.Y + dy);
                            Cursor.Position = start;
                            Shoot(start, target);
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

        private static void Shoot(Point ball, Point hoop)
        {
            double dx = (hoop.X - ball.X) * 0.7;
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
            Thread.Sleep(time);
            sw.Stop();
            Point p2 = FindBasket(handle);
            Console.WriteLine(p1.X - p2.X);
            Console.WriteLine(p1.Y - p2.Y);
            Console.WriteLine(sw.ElapsedMilliseconds);
            Console.WriteLine(1000.0 * (p2.X - p1.X) / sw.ElapsedMilliseconds);
            Console.WriteLine(1000.0 * (p2.Y - p1.Y) / sw.ElapsedMilliseconds);
            return new double[] { 1000.0 * (p2.X - p1.X) / sw.ElapsedMilliseconds, 1000.0 * (p2.Y - p1.Y) / sw.ElapsedMilliseconds };
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
			int dx = Math.Max(1, BASKET_WIDTH - 2);
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
    }
}
