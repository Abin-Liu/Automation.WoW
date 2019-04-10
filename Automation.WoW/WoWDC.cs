using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Automation;

namespace Automation.WoW
{
	/// <summary>
	/// Client positions where signal-pixels can appear
	/// </summary>
	public enum ClientPosition { Invalid = -1, TopLeft, Top, TopRight, Right, BottomRight, Bottom, BottomLeft, Left, Center };

	public class WoWDC : ClientDC
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
		/// Translate a position into client coordinates
		/// <param name="position">Position</param>
		/// <returns>Client coordinates</returns>
		/// </summary>
		public Point TranslatePosition(ClientPosition position)
		{
			Point point = new Point(0, 0);
			switch (position)
			{
				case ClientPosition.TopLeft:
					point.X = 0;
					point.Y = 0;
					break;

				case ClientPosition.Top:
					point.X = ClientSize.Width / 2;
					point.Y = 0;
					break;

				case ClientPosition.TopRight:
					point.X = ClientSize.Width - 1;
					point.Y = 0;
					break;

				case ClientPosition.Right:
					point.X = ClientSize.Width - 1;
					point.Y = ClientSize.Height / 2;
					break;

				case ClientPosition.BottomRight:
					point.X = ClientSize.Width - 1;
					point.Y = ClientSize.Height - 1;
					break;

				case ClientPosition.Bottom:
					point.X = ClientSize.Width / 2;
					point.Y = ClientSize.Height - 1;
					break;

				case ClientPosition.BottomLeft:
					point.X = 0;
					point.Y = ClientSize.Height - 1;
					break;

				case ClientPosition.Left:
					point.X = 0;
					point.Y = ClientSize.Height / 2;
					break;

				case ClientPosition.Center:
					point.X = ClientSize.Width / 2;
					point.Y = ClientSize.Height / 2;
					break;

				default:
					break;
			}

			return point;
		}

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
		/// Extended GetPixel method
		/// <param name="position">Signal pixel position</param>
		/// <returns>RGB value</returns>
		/// </summary>
		public int GetPixel(ClientPosition position)
		{
			Point point = TranslatePosition(position);
			return GetPixel(point.X, point.Y);
		}

		/// <summary>
		/// Capture single pixel and read it
		/// </summary>
		/// <param name="position">Signal pixel position</param>		
		/// <returns>Return RGB value if success, 0 otherwise.</returns>
		public int CaptureAndGetPixcel(ClientPosition position)
		{
			Point point = TranslatePosition(position);
			return CaptureAndGetPixcel(point.X, point.Y);
		}

		/// <summary> 
		/// Keeps checking whether a pixel of the target window matches specified RGB values
		/// <param name="position">Signal pixel position</param>		
		/// <param name="r">R component</param> 
		/// <param name="g">G component</param> 
		/// <param name="b">B component</param> 
		/// <param name="timeout">Maximum milliseconds before timeout, 0 to check infinitely</param>
		/// <returns>Return true if the pixel matches before timeout, false otherwise</returns>
		/// </summary>
		public bool WaitForPixel(ClientPosition position, byte r, byte g, byte b, int timeout)
		{
			return WaitForPixel(position, RGB(r, g, b), timeout);
		}

		/// <summary> 
		/// Keeps checking whether a pixel of the target window matches specified RGB values
		/// <param name="position">Signal pixel position</param>		
		/// <param name="color">The RGB value</param> 		
		/// <param name="timeout">Maximum milliseconds before timeout, 0 to check infinitely</param>
		/// <returns>Return true if the pixel matches before timeout, false otherwise</returns>
		/// </summary>
		public bool WaitForPixel(ClientPosition position, int color, int timeout)
		{
			Point point = TranslatePosition(position);
			return WaitForPixel(point.X, point.Y, color, timeout);
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
				int pixel = CaptureAndGetPixcel(x, y);
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

		/// <summary>
		/// Wait until the pixel matches predefined color or timeout
		/// <param name="position">Client position.</param>
		/// <param name="timeout">Timeout in milliseconds, wait infinitely if this parameter is 0..</param>
		/// <returns>Return RGB value if matches, -1 otherwise.</returns>
		/// </summary>
		public int WaitForKnownPixel(ClientPosition position, int timeout)
		{
			Point point = TranslatePosition(position);
			return WaitForKnownPixel(point.X, point.Y, timeout);
		}
	}
}
