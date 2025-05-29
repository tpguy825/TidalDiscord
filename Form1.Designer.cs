namespace TidalDiscord
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            label4 = new Label();
            label3 = new Label();
            label2 = new Label();
            progressBar1 = new ProgressBar();
            pictureBox1 = new PictureBox();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // splitContainer1
            // 
			Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(progressBar1);
            Controls.Add(pictureBox1);
            // 
            // label3 - Song Title
            // 
            label3.AutoSize = true;
			label3.Location = new Point(75 - 2, 35);
            label3.Name = "";
			label3.Font = new Font(Font.FontFamily, 13F, FontStyle.Bold, GraphicsUnit.Point, 0);
			label3.ForeColor = Color.White;
			label3.BackColor = Color.FromArgb(0, 0, 0, 75);
            label3.Size = new Size(0, 30);
            label3.TabIndex = 3;
            label3.Text = "";
            // 
            // label4 - Artist
            // 
            label4.AutoSize = true;
            label4.Location = new Point(75, 60);
			label3.BackColor = Color.FromArgb(0, 0, 0, 75);
			label3.ForeColor = Color.White;
            label4.Name = "";
            label4.Size = new Size(0, 15);
            label4.TabIndex = 2;
            label4.Text = "";
            // 
            // label2 - Duration
            // 
            label2.AutoSize = true;
            label2.Location = new Point(0, 0);
            label2.Name = "";
            label2.Size = new Size(38, 15);
            label2.TabIndex = 1;
            label2.Text = "";
			label2.Hide();
            // 
            // progressBar1
            // 
            progressBar1.Location = new Point(205, 180);
            progressBar1.Name = "";
            progressBar1.Size = new Size(140, 15);
            progressBar1.TabIndex = 0;
			progressBar1.Hide();
            // 
            // pictureBox1
            // 
            pictureBox1.Location = new Point(0, 0);
            pictureBox1.Name = "";
            pictureBox1.Size = new Size(75, 75);
            pictureBox1.TabIndex = 4;
			pictureBox1.BringToFront();
            pictureBox1.TabStop = false;
			pictureBox1.Visible = false;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(75, 75);
			int taskbarHeight = Screen.PrimaryScreen.Bounds.Height - Screen.PrimaryScreen.WorkingArea.Height;
			Location = new Point(0, Screen.PrimaryScreen.Bounds.Height - 75 - taskbarHeight);
			AutoSize = true;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "Form1";
            Text = "TidalDiscord";
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private PictureBox pictureBox1;
        private ProgressBar progressBar1;
        private Label label4;
        private Label label3;
        private Label label2;
    }
}
