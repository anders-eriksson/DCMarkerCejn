namespace PlcSimulator
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
            this.MessageTextbox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.IpAddressTextbox = new System.Windows.Forms.TextBox();
            this.ConnectButton = new System.Windows.Forms.Button();
            this.ArtNoButton = new System.Windows.Forms.Button();
            this.ArtNoTextbox = new System.Windows.Forms.TextBox();
            this.StartOKButton = new System.Windows.Forms.Button();
            this.ExternalStartButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // MessageTextbox
            // 
            this.MessageTextbox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.MessageTextbox.Location = new System.Drawing.Point(13, 161);
            this.MessageTextbox.Multiline = true;
            this.MessageTextbox.Name = "MessageTextbox";
            this.MessageTextbox.Size = new System.Drawing.Size(511, 89);
            this.MessageTextbox.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(57, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "IP-address";
            // 
            // IpAddressTextbox
            // 
            this.IpAddressTextbox.Location = new System.Drawing.Point(76, 18);
            this.IpAddressTextbox.Name = "IpAddressTextbox";
            this.IpAddressTextbox.Size = new System.Drawing.Size(100, 20);
            this.IpAddressTextbox.TabIndex = 2;
            // 
            // ConnectButton
            // 
            this.ConnectButton.Location = new System.Drawing.Point(183, 17);
            this.ConnectButton.Name = "ConnectButton";
            this.ConnectButton.Size = new System.Drawing.Size(75, 23);
            this.ConnectButton.TabIndex = 3;
            this.ConnectButton.Text = "Connect";
            this.ConnectButton.UseVisualStyleBackColor = true;
            this.ConnectButton.Click += new System.EventHandler(this.ConnectButton_Click);
            // 
            // ArtNoButton
            // 
            this.ArtNoButton.Location = new System.Drawing.Point(183, 42);
            this.ArtNoButton.Name = "ArtNoButton";
            this.ArtNoButton.Size = new System.Drawing.Size(75, 23);
            this.ArtNoButton.TabIndex = 4;
            this.ArtNoButton.Text = "ArtNo";
            this.ArtNoButton.UseVisualStyleBackColor = true;
            this.ArtNoButton.Click += new System.EventHandler(this.ArtNoButton_Click);
            // 
            // ArtNoTextbox
            // 
            this.ArtNoTextbox.Location = new System.Drawing.Point(76, 44);
            this.ArtNoTextbox.Name = "ArtNoTextbox";
            this.ArtNoTextbox.Size = new System.Drawing.Size(100, 20);
            this.ArtNoTextbox.TabIndex = 5;
            // 
            // StartOKButton
            // 
            this.StartOKButton.Location = new System.Drawing.Point(183, 71);
            this.StartOKButton.Name = "StartOKButton";
            this.StartOKButton.Size = new System.Drawing.Size(75, 23);
            this.StartOKButton.TabIndex = 6;
            this.StartOKButton.Text = "StartOK";
            this.StartOKButton.UseVisualStyleBackColor = true;
            this.StartOKButton.Click += new System.EventHandler(this.StartOKButton_Click);
            // 
            // ExternalStartButton
            // 
            this.ExternalStartButton.Location = new System.Drawing.Point(183, 100);
            this.ExternalStartButton.Name = "ExternalStartButton";
            this.ExternalStartButton.Size = new System.Drawing.Size(75, 23);
            this.ExternalStartButton.TabIndex = 8;
            this.ExternalStartButton.Text = "Ext Start";
            this.ExternalStartButton.UseVisualStyleBackColor = true;
            this.ExternalStartButton.Click += new System.EventHandler(this.ExternalStartButton_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(536, 262);
            this.Controls.Add(this.ExternalStartButton);
            this.Controls.Add(this.StartOKButton);
            this.Controls.Add(this.ArtNoTextbox);
            this.Controls.Add(this.ArtNoButton);
            this.Controls.Add(this.ConnectButton);
            this.Controls.Add(this.IpAddressTextbox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.MessageTextbox);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox MessageTextbox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox IpAddressTextbox;
        private System.Windows.Forms.Button ConnectButton;
        private System.Windows.Forms.Button ArtNoButton;
        private System.Windows.Forms.TextBox ArtNoTextbox;
        private System.Windows.Forms.Button StartOKButton;
        private System.Windows.Forms.Button ExternalStartButton;
    }
}

