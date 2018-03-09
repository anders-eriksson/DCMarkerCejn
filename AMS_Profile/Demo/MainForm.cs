/**
 * AMS.Profile Demo
 * Version 2.0
 * 
 * Written by Alvaro Mendez
 * Copyright (c) 2005. All Rights Reserved.
 * 
 * Last Updated: Feb. 17, 2005
 */

using System;
using System.Windows.Forms;
using System.Diagnostics;
using AMS.Profile;

namespace MyFormProject 
{
	class MainForm : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Label label;
		private System.Windows.Forms.Button btnTestProfile;
		private System.Windows.Forms.TextBox txtValue;
		private System.Windows.Forms.ComboBox cboSection;
		private System.Windows.Forms.ComboBox cboProfile;
		private System.Windows.Forms.Button btnRemoveEntry;
		private System.Windows.Forms.Button btnExit;
		private System.Windows.Forms.Button btnRemoveSection;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Button btnRefreshProfile;
		private System.Windows.Forms.Button btnViewProfile;
		private System.Windows.Forms.Button btnRefreshValue;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Button btnSaveValue;
		private System.Windows.Forms.ComboBox cboEntry;

		const int PROFILE_REGISTRY = 0;		
		const int PROFILE_INI = 1;		
		const int PROFILE_XML = 2;		
		const int PROFILE_CONFIG = 3;		
		
		private Profile[] m_profiles = new Profile[4];

		public MainForm()
		{
			InitializeComponent();

			// Create the profile objects into the array
			m_profiles[PROFILE_REGISTRY] = new Registry();
			m_profiles[PROFILE_INI] = new Ini();
			m_profiles[PROFILE_XML] = new Xml();
			m_profiles[PROFILE_CONFIG] = new Config();
			
			// Add them to the combo box
			this.cboProfile.Items.Add("REG -- HKEY_CURRENT_USER\\" + m_profiles[PROFILE_REGISTRY].Name);
			this.cboProfile.Items.Add("INI -- " + m_profiles[PROFILE_INI].Name);
			this.cboProfile.Items.Add("XML -- " + m_profiles[PROFILE_XML].Name);
			this.cboProfile.Items.Add("CFG -- " + m_profiles[PROFILE_CONFIG].Name);
			
			this.cboProfile.SelectedIndex = PROFILE_REGISTRY;
			UpdateComponents();
		}

		private Profile SelectedProfile
		{
			get
			{
				return m_profiles[this.cboProfile.SelectedIndex];
			}
		}
		
		void btnExitClick(object sender, System.EventArgs e)
		{
			Close();
		}
		
		void cboProfileSelectedIndexChanged(object sender, System.EventArgs e)
		{
			using (Cursor.Current = Cursors.WaitCursor)
			{			
				this.cboSection.Items.Clear();
				
				// Update the list without clearing the values
				if (sender != null)
				{
					this.cboSection.Text = "";
					this.cboEntry.Text = "";
					this.txtValue.Text = "";
				}
				
				try
				{
					string[] sections = SelectedProfile.GetSectionNames();
					if (sections != null)
						this.cboSection.Items.AddRange(sections);				
				}
				catch (Exception ex)
				{				
					MessageBox.Show("" + ex);
				}
			}
		}
		
		void cboSectionSelectedIndexChanged(object sender, System.EventArgs e)
		{
			using (Cursor.Current = Cursors.WaitCursor)
			{						
				this.cboEntry.Items.Clear();
				
				// Update the list without clearing the values
				if (sender != null)
				{
					this.cboEntry.Text = "";
					this.txtValue.Text = "";	
				}
				
				try
				{
					string[] entries = SelectedProfile.GetEntryNames(this.cboSection.Text);
					if (entries != null)
						this.cboEntry.Items.AddRange(entries);				
				}
				catch (Exception ex)
				{				
					MessageBox.Show("" + ex);
				}
			}
			UpdateComponents();
		}
		
		void btnRefreshValueClick(object sender, System.EventArgs e)
		{
			UpdateComponents();			
			this.txtValue.Text = SelectedProfile.GetValue(this.cboSection.Text, this.cboEntry.Text, "");
		}

		void MainFormActivated(object sender, System.EventArgs e)
		{
			UpdateComponents();
		}
		
		void btnSaveValueClick(object sender, System.EventArgs e)
		{
			// Trim the section and entry so they may be selected on the list
			string section = this.cboSection.Text.Trim();	
			string entry = this.cboEntry.Text.Trim();	

			SelectedProfile.SetValue(section, entry, this.txtValue.Text);
			
			this.cboSection.Text = section;			
			this.cboEntry.Text = entry;
			
			// Update the lists without clearing the values
			cboProfileSelectedIndexChanged(null, e);
			cboSectionSelectedIndexChanged(null, e);
		}
		
		void btnViewProfileClick(object sender, System.EventArgs e)
		{
			try
			{
				if (this.cboProfile.SelectedIndex == PROFILE_REGISTRY)
					Process.Start("regedit.exe");
				else
					Process.Start(SelectedProfile.Name);								
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			}
		}
		
		void btnRemoveSectionClick(object sender, System.EventArgs e)
		{
			if (MessageBox.Show("This section and all its entries will be removed.", "Confirm Removal", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation) != DialogResult.OK)
				return;
			
			SelectedProfile.RemoveSection(this.cboSection.Text);
			cboProfileSelectedIndexChanged(sender, e);
		}
		
		void btnRemoveEntryClick(object sender, System.EventArgs e)
		{
			if (MessageBox.Show("This entry will be removed.", "Confirm Removal", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation) != DialogResult.OK)
				return;

			SelectedProfile.RemoveEntry(this.cboSection.Text, this.cboEntry.Text);
			cboSectionSelectedIndexChanged(sender, e);			
		}
		
		void btnTestProfileClick(object sender, System.EventArgs e)
		{
			try
			{
				using (Cursor.Current = Cursors.WaitCursor)
				{
					DateTime startTime = DateTime.Now;
					SelectedProfile.Test(true);
					MessageBox.Show("Passed!\n\n" + (DateTime.Now - startTime).TotalMilliseconds + " ms.", "Test Result", MessageBoxButtons.OK, MessageBoxIcon.Information);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("" + ex, "Test Result", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			}
		}
		
		void UpdateComponents()
		{
			using (Cursor.Current = Cursors.WaitCursor)
			{			
				bool enabled = (this.cboSection.Text != "");
				bool hasSection = (enabled && SelectedProfile.HasSection(this.cboSection.Text));
				this.cboEntry.Enabled = enabled;
				this.btnRemoveSection.Enabled = hasSection;
	
				enabled = (enabled && this.cboEntry.Text != "");
				bool hasEntry = (enabled && SelectedProfile.HasEntry(this.cboSection.Text, this.cboEntry.Text));
				this.txtValue.Enabled = enabled;
				this.btnSaveValue.Enabled = enabled;
				this.btnRemoveEntry.Enabled = hasEntry;				
				
				enabled = (enabled && hasSection && hasEntry);
				this.btnRefreshValue.Enabled = enabled;
			}
		}
		
		void InitializeComponent()
		{
			this.cboEntry = new System.Windows.Forms.ComboBox();
			this.btnSaveValue = new System.Windows.Forms.Button();
			this.label4 = new System.Windows.Forms.Label();
			this.btnRefreshValue = new System.Windows.Forms.Button();
			this.btnViewProfile = new System.Windows.Forms.Button();
			this.btnRefreshProfile = new System.Windows.Forms.Button();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.btnRemoveSection = new System.Windows.Forms.Button();
			this.btnExit = new System.Windows.Forms.Button();
			this.btnRemoveEntry = new System.Windows.Forms.Button();
			this.cboProfile = new System.Windows.Forms.ComboBox();
			this.cboSection = new System.Windows.Forms.ComboBox();
			this.txtValue = new System.Windows.Forms.TextBox();
			this.btnTestProfile = new System.Windows.Forms.Button();
			this.label = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// cboEntry
			// 
			this.cboEntry.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
						| System.Windows.Forms.AnchorStyles.Right);
			this.cboEntry.Location = new System.Drawing.Point(8, 153);
			this.cboEntry.Name = "cboEntry";
			this.cboEntry.Size = new System.Drawing.Size(448, 21);
			this.cboEntry.TabIndex = 6;
			this.cboEntry.TextChanged += new System.EventHandler(this.btnRefreshValueClick);
			this.cboEntry.SelectedIndexChanged += new System.EventHandler(this.btnRefreshValueClick);
			// 
			// btnSaveValue
			// 
			this.btnSaveValue.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.btnSaveValue.Location = new System.Drawing.Point(402, 192);
			this.btnSaveValue.Name = "btnSaveValue";
			this.btnSaveValue.Size = new System.Drawing.Size(52, 18);
			this.btnSaveValue.TabIndex = 9;
			this.btnSaveValue.Text = "Save";
			this.btnSaveValue.Click += new System.EventHandler(this.btnSaveValueClick);
			// 
			// label4
			// 
			this.label4.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
						| System.Windows.Forms.AnchorStyles.Right);
			this.label4.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.label4.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label4.Location = new System.Drawing.Point(8, 190);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(448, 22);
			this.label4.TabIndex = 7;
			this.label4.Text = "Value:";
			this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// btnRefreshValue
			// 
			this.btnRefreshValue.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.btnRefreshValue.Location = new System.Drawing.Point(349, 192);
			this.btnRefreshValue.Name = "btnRefreshValue";
			this.btnRefreshValue.Size = new System.Drawing.Size(52, 18);
			this.btnRefreshValue.TabIndex = 9;
			this.btnRefreshValue.Text = "Refresh";
			this.btnRefreshValue.Click += new System.EventHandler(this.btnRefreshValueClick);
			// 
			// btnViewProfile
			// 
			this.btnViewProfile.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.btnViewProfile.Location = new System.Drawing.Point(402, 14);
			this.btnViewProfile.Name = "btnViewProfile";
			this.btnViewProfile.Size = new System.Drawing.Size(52, 18);
			this.btnViewProfile.TabIndex = 2;
			this.btnViewProfile.Text = "View";
			this.btnViewProfile.Click += new System.EventHandler(this.btnViewProfileClick);
			// 
			// btnRefreshProfile
			// 
			this.btnRefreshProfile.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.btnRefreshProfile.Font = new System.Drawing.Font("Tahoma", 8F);
			this.btnRefreshProfile.Location = new System.Drawing.Point(349, 14);
			this.btnRefreshProfile.Name = "btnRefreshProfile";
			this.btnRefreshProfile.Size = new System.Drawing.Size(52, 18);
			this.btnRefreshProfile.TabIndex = 2;
			this.btnRefreshProfile.Text = "Refresh";
			this.btnRefreshProfile.Click += new System.EventHandler(this.cboProfileSelectedIndexChanged);
			// 
			// label2
			// 
			this.label2.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
						| System.Windows.Forms.AnchorStyles.Right);
			this.label2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.label2.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label2.Location = new System.Drawing.Point(8, 71);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(448, 22);
			this.label2.TabIndex = 4;
			this.label2.Text = "Section:";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label3
			// 
			this.label3.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
						| System.Windows.Forms.AnchorStyles.Right);
			this.label3.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.label3.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label3.Location = new System.Drawing.Point(8, 131);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(448, 22);
			this.label3.TabIndex = 5;
			this.label3.Text = "Entry:";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// btnRemoveSection
			// 
			this.btnRemoveSection.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.btnRemoveSection.Font = new System.Drawing.Font("Tahoma", 8F);
			this.btnRemoveSection.Location = new System.Drawing.Point(402, 73);
			this.btnRemoveSection.Name = "btnRemoveSection";
			this.btnRemoveSection.Size = new System.Drawing.Size(52, 18);
			this.btnRemoveSection.TabIndex = 2;
			this.btnRemoveSection.Text = "Remove";
			this.btnRemoveSection.Click += new System.EventHandler(this.btnRemoveSectionClick);
			// 
			// btnExit
			// 
			this.btnExit.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.btnExit.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnExit.Location = new System.Drawing.Point(381, 244);
			this.btnExit.Name = "btnExit";
			this.btnExit.TabIndex = 10;
			this.btnExit.Text = "Exit";
			this.btnExit.Click += new System.EventHandler(this.btnExitClick);
			// 
			// btnRemoveEntry
			// 
			this.btnRemoveEntry.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.btnRemoveEntry.Font = new System.Drawing.Font("Tahoma", 8F);
			this.btnRemoveEntry.Location = new System.Drawing.Point(402, 133);
			this.btnRemoveEntry.Name = "btnRemoveEntry";
			this.btnRemoveEntry.Size = new System.Drawing.Size(52, 18);
			this.btnRemoveEntry.TabIndex = 2;
			this.btnRemoveEntry.Text = "Remove";
			this.btnRemoveEntry.Click += new System.EventHandler(this.btnRemoveEntryClick);
			// 
			// cboProfile
			// 
			this.cboProfile.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
						| System.Windows.Forms.AnchorStyles.Right);
			this.cboProfile.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cboProfile.Location = new System.Drawing.Point(8, 34);
			this.cboProfile.Name = "cboProfile";
			this.cboProfile.Size = new System.Drawing.Size(448, 21);
			this.cboProfile.TabIndex = 1;
			this.cboProfile.SelectedIndexChanged += new System.EventHandler(this.cboProfileSelectedIndexChanged);
			// 
			// cboSection
			// 
			this.cboSection.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
						| System.Windows.Forms.AnchorStyles.Right);
			this.cboSection.Location = new System.Drawing.Point(8, 93);
			this.cboSection.Name = "cboSection";
			this.cboSection.Size = new System.Drawing.Size(448, 21);
			this.cboSection.TabIndex = 3;
			this.cboSection.TextChanged += new System.EventHandler(this.cboSectionSelectedIndexChanged);
			this.cboSection.SelectedIndexChanged += new System.EventHandler(this.cboSectionSelectedIndexChanged);
			// 
			// txtValue
			// 
			this.txtValue.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
						| System.Windows.Forms.AnchorStyles.Right);
			this.txtValue.Location = new System.Drawing.Point(8, 212);
			this.txtValue.Name = "txtValue";
			this.txtValue.Size = new System.Drawing.Size(448, 20);
			this.txtValue.TabIndex = 8;
			this.txtValue.Text = "";
			// 
			// btnTestProfile
			// 
			this.btnTestProfile.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.btnTestProfile.Font = new System.Drawing.Font("Tahoma", 8F);
			this.btnTestProfile.Location = new System.Drawing.Point(296, 14);
			this.btnTestProfile.Name = "btnTestProfile";
			this.btnTestProfile.Size = new System.Drawing.Size(52, 18);
			this.btnTestProfile.TabIndex = 2;
			this.btnTestProfile.Text = "Test";
			this.btnTestProfile.Click += new System.EventHandler(this.btnTestProfileClick);
			// 
			// label
			// 
			this.label.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
						| System.Windows.Forms.AnchorStyles.Right);
			this.label.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.label.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label.Location = new System.Drawing.Point(8, 12);
			this.label.Name = "label";
			this.label.Size = new System.Drawing.Size(448, 22);
			this.label.TabIndex = 0;
			this.label.Text = "Profile:";
			this.label.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// MainForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.btnExit;
			this.ClientSize = new System.Drawing.Size(462, 275);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
						this.btnExit,
						this.btnRefreshValue,
						this.txtValue,
						this.cboEntry,
						this.cboSection,
						this.btnViewProfile,
						this.cboProfile,
						this.btnSaveValue,
						this.btnRefreshProfile,
						this.btnRemoveSection,
						this.label2,
						this.btnRemoveEntry,
						this.label3,
						this.label4,
						this.btnTestProfile,
						this.label});
			this.MaximumSize = new System.Drawing.Size(4000, 304);
			this.MinimumSize = new System.Drawing.Size(472, 304);
			this.Name = "MainForm";
			this.Text = "Profile Demo";
			this.Activated += new System.EventHandler(this.MainFormActivated);
			this.ResumeLayout(false);
		}
			
		[STAThread]
		public static void Main(string[] args)
		{
			Application.Run(new MainForm());
		}
	}			
}
