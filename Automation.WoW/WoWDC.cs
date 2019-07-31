using System;
using MFGLib;

namespace Automation.WoW
{
	public class WoWDC : MemDC
	{
		/// <summary>
		/// Signal pixel colors
		/// </summary>
		public const int Red = 0xff0000;
		public const int Green = 0x00ff00;
		public const int Blue = 0x0000ff;
		public const int Yellow = 0xffff00;
		public const int Cyan = 0x00ffff;
		public const int Purple = 0xff00ff;		

		/// <summary>
		/// Check whether an RGB value is a predefined sinal color.
		/// <param name="color">RGB value</param>
		/// <returns>Return true of matches, false otherwise</returns>
		/// </summary>
		public static bool IsKnownPixel(int color)
		{
			return color == Red
				|| color == Green
				|| color == Blue
				|| color == Yellow
				|| color == Purple
				|| color == Cyan;
		}		

		/// <summary>
		/// Wait until the pixel matches predefined color or timeout
		/// <param name="x">Client x coords.</param>
		/// <param name="y">Client y coords.</param>
		/// <param name="timeout">Timeout in milliseconds, wait infinitely if this parameter is 0..</param>
		/// <returns>Return RGB value if matches, -1 otherwise.</returns>
		/// </summary>
		public int WaitForKnownPixel(int x, int y, int timeout)
		{
			DateTime startTime = DateTime.Now;
			while (true)
			{
				int pixel = CaptureAndGetPixel(x, y);
				if (IsKnownPixel(pixel))
				{
					return pixel;
				}

				if (timeout > 0 && (DateTime.Now - startTime).TotalMilliseconds > timeout)
				{
					break;
				}

				startTime = DateTime.Now;
			}

			return COLOR_INVALID;
		}		
	}
}
