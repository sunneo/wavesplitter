namespace SDLMMForm
{
    partial class MissileCommand
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
            this.sdlmmControl1 = new SDLMMForm.SDLMMControl();
            this.SuspendLayout();
            // 
            // sdlmmControl1
            // 
            this.sdlmmControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.sdlmmControl1.Location = new System.Drawing.Point(1, 1);
            this.sdlmmControl1.Name = "sdlmmControl1";
            this.sdlmmControl1.Size = new System.Drawing.Size(782, 558);
            this.sdlmmControl1.TabIndex = 0;
            // 
            // MissileCommand
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.ClientSize = new System.Drawing.Size(784, 562);
            this.Controls.Add(this.sdlmmControl1);
            this.DoubleBuffered = true;
            this.Name = "MissileCommand";
            this.Text = "MissileCommand";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MissileCommand_FormClosing);
            this.Load += new System.EventHandler(this.MissileCommand_Load);
            this.SizeChanged += new System.EventHandler(this.MissileCommand_SizeChanged);
            this.ResumeLayout(false);

        }

        #endregion

        private SDLMMControl sdlmmControl1;
    }
}