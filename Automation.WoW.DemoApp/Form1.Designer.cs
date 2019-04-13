namespace Automation.WoW.DemoApp
{
	partial class Form1
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.btnInstallAddon = new System.Windows.Forms.Button();
			this.btnUninstallAddon = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// btnInstallAddon
			// 
			this.btnInstallAddon.Location = new System.Drawing.Point(71, 178);
			this.btnInstallAddon.Name = "btnInstallAddon";
			this.btnInstallAddon.Size = new System.Drawing.Size(132, 23);
			this.btnInstallAddon.TabIndex = 0;
			this.btnInstallAddon.Text = "Install Addon";
			this.btnInstallAddon.UseVisualStyleBackColor = true;
			this.btnInstallAddon.Click += new System.EventHandler(this.btnInstallAddon_Click);
			// 
			// btnUninstallAddon
			// 
			this.btnUninstallAddon.Location = new System.Drawing.Point(238, 178);
			this.btnUninstallAddon.Name = "btnUninstallAddon";
			this.btnUninstallAddon.Size = new System.Drawing.Size(132, 23);
			this.btnUninstallAddon.TabIndex = 0;
			this.btnUninstallAddon.Text = "Uninstall Addon";
			this.btnUninstallAddon.UseVisualStyleBackColor = true;
			this.btnUninstallAddon.Click += new System.EventHandler(this.btnUninstallAddon_Click);
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(499, 237);
			this.Controls.Add(this.btnUninstallAddon);
			this.Controls.Add(this.btnInstallAddon);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Name = "Form1";
			this.Text = "Form1";
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button btnInstallAddon;
		private System.Windows.Forms.Button btnUninstallAddon;
	}
}

