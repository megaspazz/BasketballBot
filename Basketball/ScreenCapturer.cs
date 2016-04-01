using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Basketball
{
	static class ScreenCapturer
	{
		const int PW_CLIENTONLY = 0x0001;

		public static Bitmap SnapShot(IntPtr handle)
		{
			Rectangle clientRect = WindowWrapper.GetClientArea(handle);
			Bitmap bmp = new Bitmap(clientRect.Width - 1, clientRect.Height - 1);
			Graphics g = Graphics.FromImage(bmp);

			IntPtr destHdc = g.GetHdc();

			PrintWindow(handle, destHdc, PW_CLIENTONLY);

			g.ReleaseHdc(destHdc);
			g.Dispose();

			return bmp;
		}

		[DllImport("user32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool PrintWindow(IntPtr hwnd, IntPtr hDC, uint nFlags);

		public enum TernaryRasterOperations : uint
		{
			SRCCOPY = 0x00CC0020,
			SRCPAINT = 0x00EE0086,
			SRCAND = 0x008800C6,
			SRCINVERT = 0x00660046,
			SRCERASE = 0x00440328,
			NOTSRCCOPY = 0x00330008,
			NOTSRCERASE = 0x001100A6,
			MERGECOPY = 0x00C000CA,
			MERGEPAINT = 0x00BB0226,
			PATCOPY = 0x00F00021,
			PATPAINT = 0x00FB0A09,
			PATINVERT = 0x005A0049,
			DSTINVERT = 0x00550009,
			BLACKNESS = 0x00000042,
			WHITENESS = 0x00FF0062,
			CAPTUREBLT = 0x40000000
		}
	}
}
