namespace SDLMMForm
{
    partial class NBody
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
            this.sdlmmControl1.Size = new System.Drawing.Size(582, 454);
            this.sdlmmControl1.TabIndex = 0;
            // 
            // NBody
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.ClientSize = new System.Drawing.Size(587, 457);
            this.Controls.Add(this.sdlmmControl1);
            this.Name = "NBody";
            this.Text = "NBody";
            this.SizeChanged += new System.EventHandler(this.NBody_SizeChanged);
            this.ResumeLayout(false);

        }

        #endregion

        private SDLMMControl sdlmmControl1;
    }
}