using System;
using System.Drawing;
using System.Windows.Forms;
using Automation.Win32API;
using Automation.WoW.Libs;

namespace Automation.WoW
{
	/// <summary>
	/// Abstract thread class derived from AutomationThread, encapsulated most common
	/// methods which interact with WoW game window.
	/// </summary>
	public abstract class WoWThread : AutomationThread
	{
		#region Constants
		/// <summary>
		/// Signal pixel colors
		/// </summary>
		public static readonly int Invalid = -1;
		public static readonly int Red = 0xff0000;
		public static readonly int Green = 0x00ff00;
		public static readonly int Blue = 0x0000ff;
		public static readonly int Yellow = 0xffff00;
		public static readonly int Cyan = 0x00ffff;
		public static readonly int Purple = 0xff00ff;

		/// <summary>
		/// Common locations of signal pixel (relative to WoW game window client area)
		/// </summary>
		public enum ClientPosition { Invalid = -1, TopLeft, Top, TopRight, Right, BottomRight, Bottom, BottomLeft, Left, Center };
		#endregion

		#region Public Properties
		/// <summary>
		/// Whether to automatically send anti-idle
		/// </summary>
		public bool AntiIdle { get; set; } = false;

		/// <summary>
		/// Whether to automatically accept LFD invitations
		/// </summary>
		public bool AutoLFD { get; set; } = false;		
		#endregion

		#region C'tors
		/// <summary>
		/// Default constructor
		/// </summary>
		public WoWThread()
		{
			TargetWndClass = "GxWindowClass"; // WOW window class
			m_ticker.IsBackground = true;
			m_ticker.OnTick += _CheckCenterPixel;
			m_ticker.Start(1500);
		}
		#endregion

		#region Overrides
		/// <summary>
		/// Stop the thread and accept LFD invitation, inherited thread need to define the actual keys
		/// </summary>
		protected virtual void JoinLFD()
		{
			Stop();
		}

		/// <summary>
		/// Send anti-idle, usually the {Space} key
		/// </summary>
		protected virtual void SendAntiIdle()
		{
			KeyStroke(Keys.Space);
		}

		/// <summary>
		/// Overload PreStart()
		/// </summary>
		protected override bool PreStart()
		{
			if (!base.PreStart())
			{
				return false;
			}

			if (!IsTargetWndForeground())
			{
				LastError = "Please set WoW window to foreground.";
				return false;
			}

			Rectangle gameRect = GetClientRect();
			Rectangle desktopRect = Window.GetClientRect(Window.GetDesktopWindow());
			if (gameRect.Width >= desktopRect.Width && gameRect.Height >= desktopRect.Height)
			{
				LastError = "Please set WoW to window mode.";
				return false;
			}

			return true;			
		}

		/// <summary>
		/// Stop the ticker
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			m_ticker.Dispose();
			base.Dispose(disposing);			
		}
		#endregion

		#region Extended Methods
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
		/// Translate a position into client coordinates
		/// <param name="position">Position</param>
		/// <returns>Client coordinates</returns>
		/// </summary>
		public Point TranslatePosition(ClientPosition position)
		{
			Rectangle rect = GetClientRect();
			Point point = new Point(0, 0);
			switch (position)
			{
				case ClientPosition.TopLeft:
					point.X = rect.Left;
					point.Y = rect.Top;
					break;

				case ClientPosition.Top:
					point.X = rect.Left + rect.Width / 2;
					point.Y = rect.Top;
					break;

				case ClientPosition.TopRight:
					point.X = rect.Right - 1;
					point.Y = rect.Top;
					break;

				case ClientPosition.Right:
					point.X = rect.Right - 1;
					point.Y = rect.Top + rect.Height / 2;
					break;

				case ClientPosition.BottomRight:
					point.X = rect.Right - 1;
					point.Y = rect.Bottom - 1;
					break;

				case ClientPosition.Bottom:
					point.X = rect.Left + rect.Width / 2;
					point.Y = rect.Bottom - 1;
					break;

				case ClientPosition.BottomLeft:
					point.X = rect.Left;
					point.Y = rect.Bottom - 1;
					break;

				case ClientPosition.Left:
					point.X = rect.Left;
					point.Y = rect.Top + rect.Height / 2;
					break;

				case ClientPosition.Center:
					point.X = rect.Left + rect.Width / 2;
					point.Y = rect.Top + rect.Height / 2;
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
				int pixel = GetPixel(x, y);
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

			return -1;
		}

		/// <summary>
		/// Record current cursor coords, to where all subsequent HideCursor() methods  will move cursor
		/// </summary>
		public void SetIdlePoint()
		{
			Point cursor = Input.GetCursorPos();
			Point offset = Window.ScreenToClient(TargetWnd);
			cursor.Offset(offset);

			Rectangle rect = GetClientRect();
			if (!rect.Contains(cursor))
			{
				cursor.X = rect.Left + rect.Width / 2 + 300;
				cursor.Y = rect.Top + rect.Height / 2 + 300;
			}
			m_idlePoint = cursor;
		}

		/// <summary>
		/// Move cursor to the coords recorded in a previous call of SetIdlePoint()
		/// </summary>
		public void HideCursor()
		{			
			MouseMove(m_idlePoint.X, m_idlePoint.Y);
		}
		#endregion

		#region Addon Operations
		/// <summary>
		/// Install an addon to WoW addon directory
		/// <param name="name">Addon name</param>
		/// <param name="sourcePath">Addon source directory</param>
		/// <returns>Return true if success, false otherwise</returns>
		/// </summary>
		public static bool InstallAddOn(string name, string sourcePath = null)
		{
			string AppTitle = Application.ProductName;
			AddonHelper addon = new AddonHelper();
			if (!addon.Valid)
			{
				MessageBox.Show("Cannot read WoW install path from registry, please specify.", AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

				while (true)
				{
					FolderBrowserDialog dialog = new FolderBrowserDialog();
					dialog.ShowNewFolderButton = false;
					dialog.ShowDialog();

					if (AddonHelper.IsWowPath(dialog.SelectedPath))
					{
						addon.WowPath = dialog.SelectedPath;
						addon.UpdateRegistryWowPath();
						break;
					}

					DialogResult res = MessageBox.Show("\"" + dialog.SelectedPath + "\" is not a correct WoW install path, please select the directory where WoW.exe resides.", AppTitle, MessageBoxButtons.RetryCancel);
					if (res == DialogResult.Cancel)
					{
						break;
					}
				}
			}

			if (!addon.Valid)
			{
				MessageBox.Show("Failed to install addon, " + AppTitle + " may not work properly.", AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				return false;
			}

			addon.InstallAddOn(sourcePath, name);

			if (Window.FindWindow("GxWindowClass", null) != IntPtr.Zero)
			{
				MessageBox.Show("WoW is currently running, you will need to restart the game before using " + AppTitle + ".", AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			}

			return true;
		}

		/// <summary>
		/// Uninstall an addon
		/// <param name="name">Addon name</param>
		/// <returns>Return true if success, false otherwise.</returns>
		/// </summary>
		public static bool UninstallAddOn(string name)
		{
			return new AddonHelper().UninstallAddOn(name);
		}

		/// <summary>
		/// Check whether an addon exists
		/// <param name="name">Addon name</param>
		/// <returns>Return true if the addon exists, false otherwise</returns>
		/// </summary>
		public static bool AddOnExists(string name)
		{
			return new AddonHelper().AddOnExists(name);
		}
		#endregion

		#region Private Members
		// Check the center pixel periodically
		private void _CheckCenterPixel()
		{			
			DateTime now = DateTime.Now;
			if ((now - m_lastCheckCenter).TotalSeconds < 1.5)
			{
				return;
			}

			if (TargetWnd == IntPtr.Zero || !IsTargetWndForeground())
			{
				return;
			}

			m_lastCheckCenter = now;
			int pixel = GetPixel(ClientPosition.Center);
			if (!IsKnownPixel(pixel))
			{
				pixel = 0;
			}

			if (pixel == Red)
			{
				if (AntiIdle)
				{
					SendAntiIdle();
				}
			}
			else if (pixel == Blue)
			{
				if (AutoLFD)
				{
					JoinLFD();
				}
			}

			Alerting = pixel == Purple; // Sound alarm if the in-game alert frame is shown
		}

		private DateTime m_lastCheckCenter = DateTime.Now;
		private Point m_idlePoint = new Point(0, 0);
		private TickThread m_ticker = new TickThread();		
		#endregion
	}
}
