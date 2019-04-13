using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Automation;
using Automation.WoW;

namespace Automation.WoW.DemoApp
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
		}

		private void btnInstallAddon_Click(object sender, EventArgs e)
		{
			bool result = WoWThread.InstallAddOn("AbinTest", ".\\ui");
			MessageBox.Show(result ? "Success" : "Fail");
		}

		private void btnUninstallAddon_Click(object sender, EventArgs e)
		{
			WoWThread.UninstallAddOn("AbinTest");
		}
	}
}
