using System.Windows.Forms;

namespace PuppetMaster
{
    partial class PuppetMasterUI
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
            this.configButton = new System.Windows.Forms.Button();
            this.scriptButton = new System.Windows.Forms.Button();
            this.Result = new System.Windows.Forms.TextBox();
            this.commandTextBox = new System.Windows.Forms.TextBox();
            this.RunButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // configButton
            // 
            this.configButton.Location = new System.Drawing.Point(410, 31);
            this.configButton.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.configButton.Name = "configButton";
            this.configButton.Size = new System.Drawing.Size(145, 31);
            this.configButton.TabIndex = 0;
            this.configButton.Text = "Process all script";
            this.configButton.UseVisualStyleBackColor = true;
            this.configButton.Click += new System.EventHandler(this.button1_Click);
            // 
            // scriptButton
            // 
            this.scriptButton.Location = new System.Drawing.Point(410, 96);
            this.scriptButton.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.scriptButton.Name = "scriptButton";
            this.scriptButton.Size = new System.Drawing.Size(145, 31);
            this.scriptButton.TabIndex = 1;
            this.scriptButton.Text = "Process step-by-step";
            this.scriptButton.UseVisualStyleBackColor = true;
            this.scriptButton.Click += new System.EventHandler(this.button2_Click);
            // 
            // Result
            // 
            this.Result.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.Result.Location = new System.Drawing.Point(19, 31);
            this.Result.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Result.Multiline = true;
            this.Result.Name = "Result";
            this.Result.ReadOnly = true;
            this.Result.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.Result.Size = new System.Drawing.Size(360, 385);
            this.Result.TabIndex = 2;
            this.Result.TextChanged += new System.EventHandler(this.Result_TextChanged);
            // 
            // commandTextBox
            // 
            this.commandTextBox.Location = new System.Drawing.Point(19, 437);
            this.commandTextBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.commandTextBox.Multiline = true;
            this.commandTextBox.Name = "commandTextBox";
            this.commandTextBox.Size = new System.Drawing.Size(360, 32);
            this.commandTextBox.TabIndex = 3;
            // 
            // RunButton
            // 
            this.RunButton.Location = new System.Drawing.Point(410, 438);
            this.RunButton.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.RunButton.Name = "RunButton";
            this.RunButton.Size = new System.Drawing.Size(145, 31);
            this.RunButton.TabIndex = 4;
            this.RunButton.Text = "Enter";
            this.RunButton.UseVisualStyleBackColor = true;
            this.RunButton.Click += new System.EventHandler(this.RunButton_Click);
            // 
            // PuppetMasterUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(591, 496);
            this.Controls.Add(this.RunButton);
            this.Controls.Add(this.commandTextBox);
            this.Controls.Add(this.Result);
            this.Controls.Add(this.scriptButton);
            this.Controls.Add(this.configButton);
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "PuppetMasterUI";
            this.Text = "PuppetMaster";
            this.Load += new System.EventHandler(this.PuppetMasterUI_Load_1);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button configButton;
        private System.Windows.Forms.Button scriptButton;
        private System.Windows.Forms.TextBox Result;
        private System.Windows.Forms.TextBox commandTextBox;
        private System.Windows.Forms.Button RunButton;
    }
}