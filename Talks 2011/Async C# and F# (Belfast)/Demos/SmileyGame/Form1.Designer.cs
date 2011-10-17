namespace SmileyGame
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
      this.smileyBox = new System.Windows.Forms.PictureBox();
      this.scoreLabel = new System.Windows.Forms.Label();
      ((System.ComponentModel.ISupportInitialize)(this.smileyBox)).BeginInit();
      this.SuspendLayout();
      // 
      // smileyBox
      // 
      this.smileyBox.Image = ((System.Drawing.Image)(resources.GetObject("smileyBox.Image")));
      this.smileyBox.Location = new System.Drawing.Point(208, 127);
      this.smileyBox.Name = "smileyBox";
      this.smileyBox.Size = new System.Drawing.Size(100, 100);
      this.smileyBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
      this.smileyBox.TabIndex = 1;
      this.smileyBox.TabStop = false;
      // 
      // scoreLabel
      // 
      this.scoreLabel.BackColor = System.Drawing.SystemColors.ControlDarkDark;
      this.scoreLabel.Dock = System.Windows.Forms.DockStyle.Top;
      this.scoreLabel.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
      this.scoreLabel.ForeColor = System.Drawing.SystemColors.ControlLightLight;
      this.scoreLabel.Location = new System.Drawing.Point(0, 0);
      this.scoreLabel.Name = "scoreLabel";
      this.scoreLabel.Size = new System.Drawing.Size(559, 20);
      this.scoreLabel.TabIndex = 2;
      this.scoreLabel.Text = "Score: 0";
      // 
      // Form1
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(559, 352);
      this.Controls.Add(this.scoreLabel);
      this.Controls.Add(this.smileyBox);
      this.Name = "Form1";
      this.Text = "Smiley Game";
      ((System.ComponentModel.ISupportInitialize)(this.smileyBox)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.PictureBox smileyBox;
    private System.Windows.Forms.Label scoreLabel;
  }
}

