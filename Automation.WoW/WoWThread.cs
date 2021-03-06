﻿using System;
using System.Drawing;
using System.Windows.Forms;
using Automation;
using MFGLib;
using Win32API;

namespace Automation.WoW
{
	/// <summary>
	/// Client positions where signal-pixels can appear
	/// </summary>
	public enum ClientPosition { Invalid = -1, TopLeft, Top, TopRight, Right, BottomRight, Bottom, BottomLeft, Left, Center };

	/// <summary>
	/// Abstract thread class derived from AutomationThread, encapsulated most common
	/// methods which interact with WoW game window.
	/// </summary>
	public abstract class WoWThread : AutomationThread
	{
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
			m_ticker.IsBackground = true;
			m_ticker.OnTick += _CheckCenterPixel;
			m_ticker.Start(1500);
			RegisterLocales(); // Static method for localization
		}
		#endregion

		#region Overrides
		public override IntPtr FindTargetWnd()
		{
			return FindWindow(WOW_WND_CLASS, null);
		}

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
				LastError = Localize("Please set WoW window to foreground.");
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

		#region Target Window Pixel Access
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
					point.X = ClientRect.Width / 2;
					point.Y = 0;
					break;

				case ClientPosition.TopRight:
					point.X = ClientRect.Width - 1;
					point.Y = 0;
					break;

				case ClientPosition.Right:
					point.X = ClientRect.Width - 1;
					point.Y = ClientRect.Height / 2;
					break;

				case ClientPosition.BottomRight:
					point.X = ClientRect.Width - 1;
					point.Y = ClientRect.Height - 1;
					break;

				case ClientPosition.Bottom:
					point.X = ClientRect.Width / 2;
					point.Y = ClientRect.Height - 1;
					break;

				case ClientPosition.BottomLeft:
					point.X = 0;
					point.Y = ClientRect.Height - 1;
					break;

				case ClientPosition.Left:
					point.X = 0;
					point.Y = ClientRect.Height / 2;
					break;

				case ClientPosition.Center:
					point.X = ClientRect.Width / 2;
					point.Y = ClientRect.Height / 2;
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
			return WoWDC.IsKnownPixel(color);
		}

		/// <summary>
		/// Extended GetPixel method
		/// <param name="position">Signal pixel position</param>
		/// <returns>RGB value</returns>
		/// </summary>
		public int GetPixel(ClientPosition position)
		{
			Point point = TranslatePosition(position);
			point.Offset(ClientToScreen);
			WoWDC dc = new WoWDC();			
			return dc.CaptureAndGetPixel(point.X, point.Y);
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
			point.Offset(ClientToScreen);
			WoWDC dc = new WoWDC();
			return dc.WaitForPixel(point.X, point.Y, color, timeout);
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
			point.Offset(ClientToScreen);
			WoWDC dc = new WoWDC();
			return dc.WaitForKnownPixel(point.X, point.Y, timeout);
		}
		#endregion

		#region Extended Methods
		/// <summary>
		/// Record current cursor coords, to where all subsequent HideCursor() methods  will move cursor
		/// </summary>
		public void SetIdlePoint()
		{
			Point cursor = Input.GetCursorPos();
			Point offset = Window.ScreenToClient(TargetWnd);
			cursor.Offset(offset);

			Rectangle rect = ClientRect;
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
				MessageBox.Show(Localize("Cannot read WoW install path from registry, please specify."), AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

				while (true)
				{
					FolderBrowserDialog dialog = new FolderBrowserDialog();
					dialog.ShowNewFolderButton = false;
					DialogResult res = dialog.ShowDialog();
					if (res != DialogResult.OK)
					{
						break;
					}

					if (AddonHelper.IsWowPath(dialog.SelectedPath))
					{
						addon.WowPath = dialog.SelectedPath;
						addon.UpdateRegistryWowPath();
						break;
					}

					res = MessageBox.Show(string.Format(Localize("{0} is not a correct WoW install path, please select the directory where WoW.exe resides."), dialog.SelectedPath), AppTitle, MessageBoxButtons.RetryCancel);
					if (res == DialogResult.Cancel)
					{
						break;
					}
				}
			}

			if (!addon.Valid)
			{
				MessageBox.Show(Localize("Failed to install addon."), AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				return false;
			}

			addon.InstallAddOn(name, sourcePath);

			if (FindWindow(WOW_WND_CLASS, null) != IntPtr.Zero)
			{
				MessageBox.Show(Localize("WoW is currently running, you will need to restart the game before the installed addon takes effect."), AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
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
			switch (pixel)
			{
				case WoWDC.Red:
					if (AntiIdle)
					{
						SendAntiIdle();
					}
					break;

				case WoWDC.Blue:
					if (AutoLFD)
					{
						JoinLFD();
					}
					break;

				default:
					break;
			}		

			Alerting = pixel == WoWDC.Purple; // Sound alarm if the in-game alert frame is shown
		}

		private static void RegisterLocales()
		{
			Locale locale;

			locale = RegisterLocale("zh-CN");
			locale["Please set WoW window to foreground."] = "请先将WoW窗口置于前台。";
			locale["Please set WoW to window mode."] = "请先将WoW窗口设置为窗口模式。";
			locale["Cannot read WoW install path from registry, please specify."] = "无法从系统注册表获取WoW安装路径。请手动指定。";
			locale["{0} is not a correct WoW install path, please select the directory where WoW.exe resides."] = "{0} 不是一个正确的WoW安装目录，请选择WoW.exe文件所在目录。";
			locale["Failed to install addon."] = "插件安装失败。";
			locale["WoW is currently running, you will need to restart the game before the installed addon takes effect."] = "WoW正在运行中，你需要重启游戏才能让插件生效。";

			locale = RegisterLocale("zh-TW");
			locale["Please set WoW window to foreground."] = "請先將WoW窗體置於前台。";
			locale["Please set WoW to window mode."] = "請先將WoW窗體設置為窗口模式。";
			locale["Cannot read WoW install path from registry, please specify."] = "無法從系統註冊表獲取WoW安裝路徑，請手動指定。";
			locale["{0} is not a correct WoW install path, please select the directory where WoW.exe resides."] = "{0} 不是一個正確的WoW安裝目錄，請選擇WoW.exe文件所在目錄。";
			locale["Failed to install addon."] = "插件安裝失敗。";
			locale["WoW is currently running, you will need to restart the game before the installed addon takes effect."] = "WoW正在運行中，你需要重啟遊戲才能讓插件生效。";

		}

		public static readonly string WOW_WND_CLASS = "GxWindowClass";
		private DateTime m_lastCheckCenter = DateTime.Now;
		private Point m_idlePoint = new Point(0, 0);
		private TickEventThread m_ticker = new TickEventThread();			 
		#endregion
	}
}
