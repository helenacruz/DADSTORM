﻿using System.Windows.Forms;

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
            this.configButton.Location = new System.Drawing.Point(547, 38);
            this.configButton.Name = "configButton";
            this.configButton.Size = new System.Drawing.Size(193, 38);
            this.configButton.TabIndex = 0;
            this.configButton.Text = "Process all script";
            this.configButton.UseVisualStyleBackColor = true;
            this.configButton.Click += new System.EventHandler(this.button1_Click);
            // 
            // scriptButton
            // 
            this.scriptButton.Location = new System.Drawing.Point(547, 118);
            this.scriptButton.Name = "scriptButton";
            this.scriptButton.Size = new System.Drawing.Size(193, 38);
            this.scriptButton.TabIndex = 1;
            this.scriptButton.Text = "Process step-by-step";
            this.scriptButton.UseVisualStyleBackColor = true;
            this.scriptButton.Click += new System.EventHandler(this.button2_Click);
            // 
            // Result
            // 
            this.Result.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.Result.Location = new System.Drawing.Point(25, 38);
            this.Result.Multiline = true;
            this.Result.Name = "Result";
            this.Result.ReadOnly = true;
            this.Result.Size = new System.Drawing.Size(479, 379);
            this.Result.TabIndex = 2;
            this.Result.TextChanged += new System.EventHandler(this.Result_TextChanged);
            this.Result.ScrollBars = ScrollBars.Vertical;
            // 
            // commandTextBox
            // 
            this.commandTextBox.Location = new System.Drawing.Point(25, 436);
            this.commandTextBox.Multiline = true;
            this.commandTextBox.Name = "commandTextBox";
            this.commandTextBox.Size = new System.Drawing.Size(479, 38);
            this.commandTextBox.TabIndex = 3;
            // 
            // RunButton
            // 
            this.RunButton.Location = new System.Drawing.Point(547, 436);
            this.RunButton.Name = "RunButton";
            this.RunButton.Size = new System.Drawing.Size(193, 38);
            this.RunButton.TabIndex = 4;
            this.RunButton.Text = "Enter";
            this.RunButton.UseVisualStyleBackColor = true;
            this.RunButton.Click += new System.EventHandler(this.RunButton_Click);
            // 
            // PuppetMasterUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(788, 512);
            this.Controls.Add(this.RunButton);
            this.Controls.Add(this.commandTextBox);
            this.Controls.Add(this.Result);
            this.Controls.Add(this.scriptButton);
            this.Controls.Add(this.configButton);
            this.Name = "PuppetMasterUI";
            this.Text = "PuppetMaster";
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