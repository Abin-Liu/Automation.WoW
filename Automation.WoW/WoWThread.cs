using System;
using System.Drawing;
using System.Windows.Forms;
using Automation.Win32API;
using Automation.WoW.Libs;

namespace Automation.WoW
{
	/// <summary>
	/// 继承自AutomationThread的抽象线程类，封装了大部分与WoW窗口交互的通用方法和常量
	/// </summary>
	public abstract class WoWThread : AutomationThread
	{
		#region 常量定义
		/// <summary>
		/// 信号像素颜色定义
		/// </summary>
		public static readonly int Invalid = -1;
		public static readonly int Red = 0xff0000;
		public static readonly int Green = 0x00ff00;
		public static readonly int Blue = 0x0000ff;
		public static readonly int Yellow = 0xffff00;
		public static readonly int Cyan = 0x00ffff;
		public static readonly int Purple = 0xff00ff;

		/// <summary>
		/// 信号像素在客户端的通常位置
		/// </summary>
		public enum ClientPosition { Invalid = -1, TopLeft, Top, TopRight, Right, BottomRight, Bottom, BottomLeft, Left, Center };
		#endregion

		#region 公开方法
		/// <summary>
		/// 是否需要AntiIdle
		/// </summary>
		public bool NeedAntiIdle { get { return (DateTime.Now - m_lastAntiIdle).TotalSeconds > 30; } }

		/// <summary>
		/// 寻求组队UI是否已出现
		/// </summary>
		public bool HasLFDUI { get { return GetPixel(ClientPosition.Center) == Blue; } }

		/// <summary>
		/// WoW内报警UI是否已出现
		/// </summary>
		public bool HasAlertUI { get { return GetPixel(ClientPosition.Center) == Purple; } }

		/// <summary>
		/// WoW是否当前处于窗口模式
		/// </summary>
		public bool IsWindowMode
		{
			get
			{
				Rectangle gameRect = GetClientRect();
				Rectangle desktopRect = Window.GetClientRect(Window.GetDesktopWindow());
				return gameRect.Width < desktopRect.Width || gameRect.Height < desktopRect.Height;
			}
		}
		#endregion

		#region 构造函数
		/// <summary>
		/// 默认构造函数
		/// </summary>
		public WoWThread()
		{
			TargetWndClass = "GxWindowClass"; // WOW窗口类
		}
		#endregion

		#region 扩展方法
		/// <summary>
		/// GetPixel方法扩展，以信号像素位置为参数
		/// <param name="position">信号像素位置</param>
		/// <returns>像素RGB值</returns>
		/// </summary>
		public int GetPixel(ClientPosition position)
		{
			Point point = TranslatePosition(position);
			return GetPixel(point.X, point.Y);
		}

		/// <summary>
		/// 将信号像素位置转换为客户端坐标值
		/// <param name="position">信号像素位置</param>
		/// <returns>客户端坐标值</returns>
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
		/// 检查像素颜色是否为信号像素通用RGB值
		/// <param name="color">像素颜色</param>
		/// <returns>如果像素颜色符合则返回true，否则返回false</returns>
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
		/// 以当前鼠标位置为idle位置，后续HideCursor方法将把鼠标移动到此处
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
		/// 将把鼠标移动到前面调用SetIdlePoint时的位置
		/// </summary>
		public void HideCursor()
		{			
			MouseMove(m_idlePoint.X, m_idlePoint.Y);
		}

		/// <summary>
		/// 向WoW窗口发送空格键并记录当前时间
		/// </summary>
		public void SendAntiIdle()
		{
			KeyStroke(Keys.Space);
			m_lastAntiIdle = DateTime.Now;
		}
		#endregion

		#region 插件相关
		/// <summary>
		/// 向WoW安装一个插件
		/// <param name="name">插件名称</param>
		/// <param name="sourcePath">插件源文件所在目录</param>
		/// <returns>如果安装成功则返回true，否则返回false</returns>
		/// </summary>
		public static bool InstallAddOn(string name, string sourcePath = null)
		{
			AddonHelper addon = new AddonHelper();
			if (!addon.Valid)
			{
				MessageBox.Show("无法从系统注册表获得WOW安装目录，请手动指定WOW安装目录（Wow.exe可执行文件所在目录）。", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

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

					DialogResult res = MessageBox.Show("\"" + dialog.SelectedPath + "\" 不是一个正确的WOW安装目录，请重新选择（Wow.exe可执行文件所在目录）。", Application.ProductName, MessageBoxButtons.RetryCancel);
					if (res == DialogResult.Cancel)
					{
						break;
					}
				}
			}

			if (!addon.Valid)
			{
				MessageBox.Show("未能安装游戏内插件，" + Application.ProductName + "可能无法正常工作。", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				return false;
			}

			addon.InstallAddOn(sourcePath, name);

			if (Window.FindWindow("GxWindowClass", null) != IntPtr.Zero)
			{
				MessageBox.Show("安装程序检测到WOW正在运行中，你需要大退游戏才能使游戏内插件生效。", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			}

			return true;
		}

		/// <summary>
		/// 卸载一个WoW插件
		/// <param name="name">插件名称</param>
		/// <returns>如果卸载成功则返回true，否则返回false</returns>
		/// </summary>
		public static bool UninstallAddOn(string name)
		{
			return new AddonHelper().UninstallAddOn(name);
		}

		/// <summary>
		/// 检查某个WoW插件是否存在
		/// <param name="name">插件名称</param>
		/// <returns>如果插件存在则返回true，否则返回false</returns>
		/// </summary>
		public static bool AddOnExists(string name)
		{
			return new AddonHelper().AddOnExists(name);
		}
		#endregion

		#region 私有成员
		private DateTime m_lastAntiIdle = DateTime.Now;
		Point m_idlePoint = new Point(0, 0);
		#endregion
	}
}
