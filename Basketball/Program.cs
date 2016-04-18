using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

// my own DLLs
using FastBitmap;

// external DLLs
using WindowsInput;

namespace Basketball
{
    class Program
    {
        private static InputSimulator SIM = new InputSimulator();
        private static int LEVEL = 0;
        private static IntPtr HANDLE;

        static void Main(string[] args)
        {
            HANDLE = FindBluestacksHandle();
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
                        int boundsTime;
                        if (input.Length < 2 || !int.TryParse(input[1], out boundsTime))
                        {
                            boundsTime = 16000;
                        }
                        Rectangle bounds = EstimateBasketBorder(HANDLE, boundsTime);
                        Console.WriteLine(bounds);
                        Console.WriteLine("left = " + bounds.Left);
                        Console.WriteLine("rite = " + bounds.Right);
                        Console.WriteLine("top  = " + bounds.Top);
                        Console.WriteLine("bot  = " + bounds.Bottom);
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

        private static void MoveMouseTo(Point dest)
        {
            Point init = Cursor.Position;
            SIM.Mouse.MoveMouseBy(dest.X - init.X, dest.Y - init.Y);
        }
        
        private static Point Shoot(Point ball, Point hoop)
        {
            int dx = hoop.X - ball.X;
            int dy = hoop.Y - ball.Y;

            double r = Math.Sqrt(dx * dx + dy * dy);
            int x = (int)Math.Round(dx / r * 72);
            int y = (int)Math.Round(dy / r * 72);
            Cursor.Position = ball;
            Thread.Sleep(5);
            SIM.Mouse.LeftButtonDown();
            Thread.Sleep(5);
            SIM.Mouse.MoveMouseBy(x, y);
            Thread.Sleep(5);
            SIM.Mouse.LeftButtonUp();
            Thread.Sleep(5);

            return new Point(x, y);
        }

        private static readonly int BALL_Y = 643;
        private static readonly int BALL_WIDTH = 130;
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
            double[] vf;
            return PredictPosition(start, v, t, bounds, out vf);
        }

        private static Point PredictPosition(Point start, double[] v, double t, Rectangle bounds, out double[] vf)
        {
            int dx = (int)(v[0] * t);
            int dy = (int)(v[1] * t);
            vf = new double[2];
            vf[0] = v[0];
            vf[1] = v[1];
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
                    vf[0] *= -1;
                }
                if (x > bounds.Right)
                {
                    x = Reflect(x, bounds.Right);
                    vf[0] *= -1;
                }
                if (y < bounds.Top)
                {
                    y = Reflect(y, bounds.Top);
                    vf[0] *= -1;
                }
                if (y > bounds.Bottom)
                {
                    y = Reflect(y, bounds.Bottom);
                    vf[0] *= -1;
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
                return new Rectangle(641, 241, 0, 0);
            }
            else if (level < 20)
            {
                return new Rectangle(480, 241, 321, 0);
            }
            else if (level < 30)
            {
                return new Rectangle(480, 241, 321, 0);
            }
            else if (level < 40)
            {
                return new Rectangle(480, 241, 321, 65);
            }
            else
            {
                return new Rectangle(480, 241, 321, 130);
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
            int dx = pred.X - ball.X;
            int dy = pred.Y - ball.Y;
            if (level < 10)
            {
                return true;
            }
            else if (level < 20)
            {
                return Math.Abs(dx) <= 200;
            }
            else if (level < 30)
            {
                return Math.Abs(dx) <= 100;
            }
            else if (level < 40)
            {
                return Math.Abs(dx) <= 40;
            }
            else
            {
                return Math.Abs(dx) <= 20 && Math.Abs(dx * 20) <= Math.Abs(dy);
            }
        }

        private static void AutoAim()
        {
            AutoAim(new bool[1]);
        }

        private static int MIN_SHOTS = 3;
        private static void AutoAim(bool[] stop)
        {
            IntPtr self = WindowWrapper.GetForegroundWindow();
            Rectangle rect = WindowWrapper.GetClientArea(HANDLE);
            Rectangle levelBounds = GetBoundsFor(LEVEL);
            int levelShots = GetShotsFor(LEVEL);
            Stopwatch tmr = new Stopwatch();

            int shots = 0;
            while (!stop[0] && shots <= MIN_SHOTS)
            {
                Bitmap24 b24;
                double[] vel = CalculateVelocity(HANDLE, 100, out b24);
                tmr.Restart();
                Point ball = FindBasketball(b24);
                Point rim = FindBasket(b24);
                double[] v = ValidateVelocityFor(LEVEL, vel);
                if (!ball.IsEmpty && !rim.IsEmpty && v != null)
                {
                    for (int i = 1; i <= levelShots && !stop[0]; i++)
                    {
                        double BASE_TIME = 0.750;
                        double XTRA_TIME = 0.020;
                        double SHOT_TIME = BASE_TIME + XTRA_TIME;
                        int dx = (int)Math.Round((SHOT_TIME + tmr.ElapsedMilliseconds / 1000.0) * v[0]);
                        int dy = (int)Math.Round((SHOT_TIME + tmr.ElapsedMilliseconds / 1000.0) * v[1]);
                        double[] vf;
                        Point pred = PredictPosition(rim, v, SHOT_TIME + tmr.ElapsedMilliseconds / 1000.0, levelBounds, out vf);
                        if (TakeShot(LEVEL, ball, pred, vf) && !stop[0])
                        {
                            if (shots == 0)
                            {
                                WindowWrapper.BringToFront(HANDLE);
                            }
                            Point start = new Point(ball.X + rect.X, ball.Y + rect.Y);
                            Point target = new Point(pred.X + rect.X, pred.Y + rect.Y);
                            Point shot = Shoot(start, target);
                            Console.WriteLine("  -> Shot {0}: pred = {1}, target = {2}, vector = <{3}, {4}>", i, pred, target, shot.X, shot.Y);
                            shots++;
                        }
                    }

                    if (shots > 0)
                    {
                        WindowWrapper.BringToFront(self);
                        if (shots <= 2)
                        {
                            shots = 0;
                            Console.WriteLine("Too few shots taken, remained at level {0}", LEVEL);
                        }
                        else
                        {
                            LEVEL++;
                            Console.WriteLine("Advanced to level {0}", LEVEL);
                        }
                    }
                }
                b24.Dispose();
            }
        }
    }
}
