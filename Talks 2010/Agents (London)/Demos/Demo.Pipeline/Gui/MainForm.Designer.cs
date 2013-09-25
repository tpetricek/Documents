//===============================================================================
// Microsoft patterns & practices
// Parallel Programming Guide
//===============================================================================
// Copyright © Microsoft Corporation.  All rights reserved.
// This code released under the terms of the 
// Microsoft patterns & practices license (http://parallelpatterns.codeplex.com/license).
//===============================================================================

namespace Demo.ImagePipeline.Gui
{
    partial class MainForm
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
          this.pictureBox1 = new System.Windows.Forms.PictureBox();
          this.quitButton = new System.Windows.Forms.Button();
          this.textBoxPhase1AvgTime = new System.Windows.Forms.TextBox();
          this.textBoxPhase2AvgTime = new System.Windows.Forms.TextBox();
          this.textBoxPhase3AvgTime = new System.Windows.Forms.TextBox();
          this.textBoxPhase4AvgTime = new System.Windows.Forms.TextBox();
          this.textBoxFps = new System.Windows.Forms.TextBox();
          this.textBoxQueue1AvgWait = new System.Windows.Forms.TextBox();
          this.textBoxQueue2AvgWait = new System.Windows.Forms.TextBox();
          this.textBoxQueue3AvgWait = new System.Windows.Forms.TextBox();
          this.textBoxQueueCount1 = new System.Windows.Forms.TextBox();
          this.textBoxQueueCount2 = new System.Windows.Forms.TextBox();
          this.textBoxQueueCount3 = new System.Windows.Forms.TextBox();
          this.label1 = new System.Windows.Forms.Label();
          this.label3 = new System.Windows.Forms.Label();
          this.label4 = new System.Windows.Forms.Label();
          this.label5 = new System.Windows.Forms.Label();
          this.label6 = new System.Windows.Forms.Label();
          this.label7 = new System.Windows.Forms.Label();
          this.label8 = new System.Windows.Forms.Label();
          this.label11 = new System.Windows.Forms.Label();
          this.buttonStart = new System.Windows.Forms.Button();
          this.buttonStop = new System.Windows.Forms.Button();
          this.label13 = new System.Windows.Forms.Label();
          this.label14 = new System.Windows.Forms.Label();
          ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
          this.SuspendLayout();
          // 
          // pictureBox1
          // 
          this.pictureBox1.Location = new System.Drawing.Point(9, 10);
          this.pictureBox1.Margin = new System.Windows.Forms.Padding(2);
          this.pictureBox1.Name = "pictureBox1";
          this.pictureBox1.Size = new System.Drawing.Size(400, 322);
          this.pictureBox1.TabIndex = 0;
          this.pictureBox1.TabStop = false;
          // 
          // quitButton
          // 
          this.quitButton.AutoSize = true;
          this.quitButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(0)))));
          this.quitButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
          this.quitButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
          this.quitButton.ForeColor = System.Drawing.Color.White;
          this.quitButton.Location = new System.Drawing.Point(439, 307);
          this.quitButton.Margin = new System.Windows.Forms.Padding(2);
          this.quitButton.Name = "quitButton";
          this.quitButton.Size = new System.Drawing.Size(81, 25);
          this.quitButton.TabIndex = 1;
          this.quitButton.Text = "Quit";
          this.quitButton.UseVisualStyleBackColor = false;
          // 
          // textBoxPhase1AvgTime
          // 
          this.textBoxPhase1AvgTime.BackColor = System.Drawing.Color.Black;
          this.textBoxPhase1AvgTime.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
          this.textBoxPhase1AvgTime.Location = new System.Drawing.Point(472, 11);
          this.textBoxPhase1AvgTime.Margin = new System.Windows.Forms.Padding(2);
          this.textBoxPhase1AvgTime.Name = "textBoxPhase1AvgTime";
          this.textBoxPhase1AvgTime.Size = new System.Drawing.Size(48, 20);
          this.textBoxPhase1AvgTime.TabIndex = 2;
          // 
          // textBoxPhase2AvgTime
          // 
          this.textBoxPhase2AvgTime.BackColor = System.Drawing.Color.Black;
          this.textBoxPhase2AvgTime.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
          this.textBoxPhase2AvgTime.Location = new System.Drawing.Point(472, 34);
          this.textBoxPhase2AvgTime.Margin = new System.Windows.Forms.Padding(2);
          this.textBoxPhase2AvgTime.Name = "textBoxPhase2AvgTime";
          this.textBoxPhase2AvgTime.Size = new System.Drawing.Size(48, 20);
          this.textBoxPhase2AvgTime.TabIndex = 2;
          // 
          // textBoxPhase3AvgTime
          // 
          this.textBoxPhase3AvgTime.BackColor = System.Drawing.Color.Black;
          this.textBoxPhase3AvgTime.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
          this.textBoxPhase3AvgTime.Location = new System.Drawing.Point(472, 56);
          this.textBoxPhase3AvgTime.Margin = new System.Windows.Forms.Padding(2);
          this.textBoxPhase3AvgTime.Name = "textBoxPhase3AvgTime";
          this.textBoxPhase3AvgTime.Size = new System.Drawing.Size(48, 20);
          this.textBoxPhase3AvgTime.TabIndex = 2;
          // 
          // textBoxPhase4AvgTime
          // 
          this.textBoxPhase4AvgTime.BackColor = System.Drawing.Color.Black;
          this.textBoxPhase4AvgTime.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
          this.textBoxPhase4AvgTime.Location = new System.Drawing.Point(472, 79);
          this.textBoxPhase4AvgTime.Margin = new System.Windows.Forms.Padding(2);
          this.textBoxPhase4AvgTime.Name = "textBoxPhase4AvgTime";
          this.textBoxPhase4AvgTime.Size = new System.Drawing.Size(48, 20);
          this.textBoxPhase4AvgTime.TabIndex = 2;
          // 
          // textBoxFps
          // 
          this.textBoxFps.BackColor = System.Drawing.Color.Black;
          this.textBoxFps.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
          this.textBoxFps.Location = new System.Drawing.Point(472, 103);
          this.textBoxFps.Margin = new System.Windows.Forms.Padding(2);
          this.textBoxFps.Name = "textBoxFps";
          this.textBoxFps.Size = new System.Drawing.Size(48, 20);
          this.textBoxFps.TabIndex = 2;
          // 
          // textBoxQueue1AvgWait
          // 
          this.textBoxQueue1AvgWait.BackColor = System.Drawing.Color.Black;
          this.textBoxQueue1AvgWait.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
          this.textBoxQueue1AvgWait.Location = new System.Drawing.Point(460, 154);
          this.textBoxQueue1AvgWait.Margin = new System.Windows.Forms.Padding(2);
          this.textBoxQueue1AvgWait.Name = "textBoxQueue1AvgWait";
          this.textBoxQueue1AvgWait.Size = new System.Drawing.Size(30, 20);
          this.textBoxQueue1AvgWait.TabIndex = 2;
          this.textBoxQueue1AvgWait.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
          // 
          // textBoxQueue2AvgWait
          // 
          this.textBoxQueue2AvgWait.BackColor = System.Drawing.Color.Black;
          this.textBoxQueue2AvgWait.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
          this.textBoxQueue2AvgWait.Location = new System.Drawing.Point(460, 177);
          this.textBoxQueue2AvgWait.Margin = new System.Windows.Forms.Padding(2);
          this.textBoxQueue2AvgWait.Name = "textBoxQueue2AvgWait";
          this.textBoxQueue2AvgWait.Size = new System.Drawing.Size(30, 20);
          this.textBoxQueue2AvgWait.TabIndex = 2;
          this.textBoxQueue2AvgWait.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
          // 
          // textBoxQueue3AvgWait
          // 
          this.textBoxQueue3AvgWait.BackColor = System.Drawing.Color.Black;
          this.textBoxQueue3AvgWait.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
          this.textBoxQueue3AvgWait.Location = new System.Drawing.Point(460, 200);
          this.textBoxQueue3AvgWait.Margin = new System.Windows.Forms.Padding(2);
          this.textBoxQueue3AvgWait.Name = "textBoxQueue3AvgWait";
          this.textBoxQueue3AvgWait.Size = new System.Drawing.Size(30, 20);
          this.textBoxQueue3AvgWait.TabIndex = 2;
          this.textBoxQueue3AvgWait.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
          // 
          // textBoxQueueCount1
          // 
          this.textBoxQueueCount1.BackColor = System.Drawing.Color.Black;
          this.textBoxQueueCount1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
          this.textBoxQueueCount1.Location = new System.Drawing.Point(496, 154);
          this.textBoxQueueCount1.Margin = new System.Windows.Forms.Padding(2);
          this.textBoxQueueCount1.Name = "textBoxQueueCount1";
          this.textBoxQueueCount1.Size = new System.Drawing.Size(22, 20);
          this.textBoxQueueCount1.TabIndex = 2;
          this.textBoxQueueCount1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
          // 
          // textBoxQueueCount2
          // 
          this.textBoxQueueCount2.BackColor = System.Drawing.Color.Black;
          this.textBoxQueueCount2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
          this.textBoxQueueCount2.Location = new System.Drawing.Point(496, 177);
          this.textBoxQueueCount2.Margin = new System.Windows.Forms.Padding(2);
          this.textBoxQueueCount2.Name = "textBoxQueueCount2";
          this.textBoxQueueCount2.Size = new System.Drawing.Size(22, 20);
          this.textBoxQueueCount2.TabIndex = 2;
          this.textBoxQueueCount2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
          // 
          // textBoxQueueCount3
          // 
          this.textBoxQueueCount3.BackColor = System.Drawing.Color.Black;
          this.textBoxQueueCount3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
          this.textBoxQueueCount3.Location = new System.Drawing.Point(496, 200);
          this.textBoxQueueCount3.Margin = new System.Windows.Forms.Padding(2);
          this.textBoxQueueCount3.Name = "textBoxQueueCount3";
          this.textBoxQueueCount3.Size = new System.Drawing.Size(22, 20);
          this.textBoxQueueCount3.TabIndex = 2;
          this.textBoxQueueCount3.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
          // 
          // label1
          // 
          this.label1.AutoSize = true;
          this.label1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
          this.label1.Location = new System.Drawing.Point(436, 14);
          this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
          this.label1.Name = "label1";
          this.label1.Size = new System.Drawing.Size(31, 13);
          this.label1.TabIndex = 4;
          this.label1.Text = "Load";
          this.label1.TextAlign = System.Drawing.ContentAlignment.TopRight;
          // 
          // label3
          // 
          this.label3.AutoSize = true;
          this.label3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
          this.label3.Location = new System.Drawing.Point(435, 180);
          this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
          this.label3.Name = "label3";
          this.label3.Size = new System.Drawing.Size(21, 13);
          this.label3.TabIndex = 4;
          this.label3.Text = "Q2";
          // 
          // label4
          // 
          this.label4.AutoSize = true;
          this.label4.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
          this.label4.Location = new System.Drawing.Point(435, 203);
          this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
          this.label4.Name = "label4";
          this.label4.Size = new System.Drawing.Size(21, 13);
          this.label4.TabIndex = 4;
          this.label4.Text = "Q3";
          // 
          // label5
          // 
          this.label5.AutoSize = true;
          this.label5.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
          this.label5.Location = new System.Drawing.Point(429, 36);
          this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
          this.label5.Name = "label5";
          this.label5.Size = new System.Drawing.Size(39, 13);
          this.label5.TabIndex = 4;
          this.label5.Text = "Resize";
          this.label5.TextAlign = System.Drawing.ContentAlignment.TopRight;
          // 
          // label6
          // 
          this.label6.AutoSize = true;
          this.label6.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
          this.label6.Location = new System.Drawing.Point(438, 59);
          this.label6.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
          this.label6.Name = "label6";
          this.label6.Size = new System.Drawing.Size(29, 13);
          this.label6.TabIndex = 4;
          this.label6.Text = "Filter";
          this.label6.TextAlign = System.Drawing.ContentAlignment.TopRight;
          // 
          // label7
          // 
          this.label7.AutoSize = true;
          this.label7.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
          this.label7.Location = new System.Drawing.Point(435, 157);
          this.label7.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
          this.label7.Name = "label7";
          this.label7.Size = new System.Drawing.Size(21, 13);
          this.label7.TabIndex = 4;
          this.label7.Text = "Q1";
          // 
          // label8
          // 
          this.label8.AutoSize = true;
          this.label8.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
          this.label8.Location = new System.Drawing.Point(427, 82);
          this.label8.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
          this.label8.Name = "label8";
          this.label8.Size = new System.Drawing.Size(41, 13);
          this.label8.TabIndex = 4;
          this.label8.Text = "Display";
          this.label8.TextAlign = System.Drawing.ContentAlignment.TopRight;
          // 
          // label11
          // 
          this.label11.AutoSize = true;
          this.label11.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
          this.label11.Location = new System.Drawing.Point(436, 106);
          this.label11.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
          this.label11.Name = "label11";
          this.label11.Size = new System.Drawing.Size(31, 13);
          this.label11.TabIndex = 5;
          this.label11.Text = "Total";
          this.label11.TextAlign = System.Drawing.ContentAlignment.TopRight;
          // 
          // buttonStart
          // 
          this.buttonStart.AutoSize = true;
          this.buttonStart.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(0)))));
          this.buttonStart.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
          this.buttonStart.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
          this.buttonStart.ForeColor = System.Drawing.Color.White;
          this.buttonStart.Location = new System.Drawing.Point(438, 249);
          this.buttonStart.Margin = new System.Windows.Forms.Padding(2);
          this.buttonStart.Name = "buttonStart";
          this.buttonStart.Size = new System.Drawing.Size(82, 25);
          this.buttonStart.TabIndex = 6;
          this.buttonStart.Text = "Start";
          this.buttonStart.UseVisualStyleBackColor = false;
          // 
          // buttonStop
          // 
          this.buttonStop.AutoSize = true;
          this.buttonStop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(0)))));
          this.buttonStop.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
          this.buttonStop.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
          this.buttonStop.ForeColor = System.Drawing.Color.White;
          this.buttonStop.Location = new System.Drawing.Point(438, 278);
          this.buttonStop.Margin = new System.Windows.Forms.Padding(2);
          this.buttonStop.Name = "buttonStop";
          this.buttonStop.Size = new System.Drawing.Size(82, 25);
          this.buttonStop.TabIndex = 9;
          this.buttonStop.Text = "Stop";
          this.buttonStop.UseVisualStyleBackColor = false;
          // 
          // label13
          // 
          this.label13.AutoSize = true;
          this.label13.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
          this.label13.Location = new System.Drawing.Point(493, 138);
          this.label13.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
          this.label13.Name = "label13";
          this.label13.Size = new System.Drawing.Size(27, 13);
          this.label13.TabIndex = 10;
          this.label13.Text = "Size";
          this.label13.TextAlign = System.Drawing.ContentAlignment.TopRight;
          // 
          // label14
          // 
          this.label14.AutoSize = true;
          this.label14.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
          this.label14.Location = new System.Drawing.Point(457, 138);
          this.label14.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
          this.label14.Name = "label14";
          this.label14.Size = new System.Drawing.Size(30, 13);
          this.label14.TabIndex = 10;
          this.label14.Text = "Time";
          this.label14.TextAlign = System.Drawing.ContentAlignment.TopRight;
          // 
          // MainForm
          // 
          this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
          this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
          this.BackColor = System.Drawing.Color.Black;
          this.ClientSize = new System.Drawing.Size(539, 344);
          this.Controls.Add(this.label8);
          this.Controls.Add(this.label11);
          this.Controls.Add(this.label6);
          this.Controls.Add(this.label5);
          this.Controls.Add(this.label14);
          this.Controls.Add(this.label1);
          this.Controls.Add(this.label13);
          this.Controls.Add(this.textBoxPhase4AvgTime);
          this.Controls.Add(this.buttonStop);
          this.Controls.Add(this.textBoxPhase3AvgTime);
          this.Controls.Add(this.textBoxPhase2AvgTime);
          this.Controls.Add(this.textBoxFps);
          this.Controls.Add(this.textBoxPhase1AvgTime);
          this.Controls.Add(this.buttonStart);
          this.Controls.Add(this.label4);
          this.Controls.Add(this.label7);
          this.Controls.Add(this.label3);
          this.Controls.Add(this.textBoxQueue3AvgWait);
          this.Controls.Add(this.textBoxQueue2AvgWait);
          this.Controls.Add(this.textBoxQueue1AvgWait);
          this.Controls.Add(this.textBoxQueueCount3);
          this.Controls.Add(this.textBoxQueueCount2);
          this.Controls.Add(this.textBoxQueueCount1);
          this.Controls.Add(this.quitButton);
          this.Controls.Add(this.pictureBox1);
          this.ForeColor = System.Drawing.Color.White;
          this.Margin = new System.Windows.Forms.Padding(2);
          this.Name = "MainForm";
          this.Text = "Image Pipeline";
          this.Load += new System.EventHandler(this.MainForm_Load);
          ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
          this.ResumeLayout(false);
          this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label14;
        public System.Windows.Forms.PictureBox pictureBox1;
        public System.Windows.Forms.Button quitButton;
        public System.Windows.Forms.TextBox textBoxPhase1AvgTime;
        public System.Windows.Forms.TextBox textBoxPhase2AvgTime;
        public System.Windows.Forms.TextBox textBoxPhase3AvgTime;
        public System.Windows.Forms.TextBox textBoxPhase4AvgTime;
        public System.Windows.Forms.TextBox textBoxFps;
        public System.Windows.Forms.TextBox textBoxQueue1AvgWait;
        public System.Windows.Forms.TextBox textBoxQueue2AvgWait;
        public System.Windows.Forms.TextBox textBoxQueue3AvgWait;
        public System.Windows.Forms.TextBox textBoxQueueCount1;
        public System.Windows.Forms.TextBox textBoxQueueCount2;
        public System.Windows.Forms.TextBox textBoxQueueCount3;
        public System.Windows.Forms.Button buttonStart;
        public System.Windows.Forms.Button buttonStop;
    }
}

