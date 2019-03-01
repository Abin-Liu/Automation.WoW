/////////////////////////////////////////////////
// AddonHelper
//
// 1：从注册表获取本机WOW安装目录。
// 2：负责WOW插件的安装和卸载。
//
// Abin
// 2019-01-02
/////////////////////////////////////////////////

using System;
using Microsoft.Win32;
using System.IO;

namespace Automation.WoW.Libs
{
	class AddonHelper
	{
		public static readonly string ADDON_FOLDER = "\\_retail_\\Interface\\AddOns"; // 8.10后路径中添加了_retail_
		public bool Valid { get { return !string.IsNullOrEmpty(WowPath); } }
		public string WowPath { get; set; } // WOW目录，不含尾\\
		public string AddonPath { get { return WowPath + ADDON_FOLDER; } } // 插件目录，不含尾\\

		public AddonHelper()
		{
			WowPath = "";
			try
			{
				// 从注册表中读取本机WOW安装路径，可能失败。
				RegistryKey root = Registry.LocalMachine;
				RegistryKey rk = root.OpenSubKey("SOFTWARE\\Blizzard Entertainment\\World of Warcraft", false);
				if (rk != null)				
				{
					WowPath = ((string)rk.GetValue("InstallPath", "")).TrimEnd(new char[] { '/', '\\' }); // 去除尾\\
				}
			}
			catch
			{
			}						
		}

		// 将WOW安装路径写入注册表，需要admin权限
		public void UpdateRegistryWowPath()
		{
			if (!Valid)
			{
				return;
			}

			try
			{
				RegistryKey root = Registry.LocalMachine;
				RegistryKey rk = root.CreateSubKey("SOFTWARE\\Blizzard Entertainment\\World of Warcraft");
				if (rk != null)
				{
					rk.SetValue("InstallPath", WowPath + '\\');
					rk.Close();
				}
			}
			catch
			{
			}			
		}

		// 判断是否正确的WOW安装路径（路径中必须存在wow.exe）
		public static bool IsWowPath(string path)
		{
			if (string.IsNullOrEmpty(path))
			{
				return false;
			}

			return File.Exists(path + "\\Wow.exe");
		}

		// 判断插件是否存在（路径中必须存在插件名.toc）
		public bool AddOnExists(string name)
		{
			if (string.IsNullOrEmpty(name) || !Valid)
			{
				return false;
			}

			return File.Exists(AddonPath + '\\' + name + '\\' + name + ".toc");
		}

		// 安装插件
		public bool InstallAddOn(string name, string sourcePath = null)
		{
			if (!Valid || !Directory.Exists(sourcePath))
			{
				return false;
			}
			
			if (string.IsNullOrEmpty(sourcePath))
			{
				sourcePath = AppDomain.CurrentDomain.BaseDirectory + "ui";
			}

			UninstallAddOn(name);
			CopyDirectory(sourcePath, AddonPath + '\\' + name, true);
			return true;
		}

		// 卸载插件
		public bool UninstallAddOn(string name)
		{
			if (!Valid || string.IsNullOrEmpty(name))
			{
				return false;
			}

			string path = AddonPath + '\\' + name;
			if (!Directory.Exists(path))
			{
				return false;
			}

			try
			{
				Directory.Delete(path, true);
				return true;
			}
			catch
			{
				return false;
			}			
		}

		// 带结构复制文件夹
		public static void CopyDirectory(string source, string dest, bool overwrite)
		{
			if (!Directory.Exists(source))
			{
				return;
			}			

			try
			{
				Directory.CreateDirectory(dest);

				foreach (string fls in Directory.GetFiles(source))
				{
					FileInfo flinfo = new FileInfo(fls);
					flinfo.CopyTo(dest + '\\' + flinfo.Name, overwrite);
				}

				foreach (string drs in Directory.GetDirectories(source))
				{
					DirectoryInfo drinfo = new DirectoryInfo(drs);
					CopyDirectory(drs, dest + '\\' + drinfo.Name, overwrite);
				}
			}
			catch
			{
			}
		}
	}
}
