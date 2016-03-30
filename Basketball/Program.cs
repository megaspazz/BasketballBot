using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AutoHotKeyNET;

namespace Basketball
{
	class Program
	{
		static void Main(string[] args)
		{
			AutoHotKey.PathEXE = @"Exe\AutoHotkeyU64.exe";
			while (true)
			{
				Console.Write("Input command: ");
				string raw = Console.ReadLine();
				string input = raw.ToUpper();
				switch (input)
				{
					case "CURSOR":
						Point pt = Cursor.Position;
						Console.WriteLine("Cursor: ({0}, {1})", pt.X, pt.Y);
						break;
					case "TEST":
						Cursor.Position = new Point(2171, 382);
						AutoHotKey.RunAHK(@"AHK\MouseLeftClick");
						Thread.Sleep(500);
						Cursor.Position = new Point(2171, 382);
						AutoHotKey.RunAHK(@"AHK\MouseLeftClick");

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
	}
}
