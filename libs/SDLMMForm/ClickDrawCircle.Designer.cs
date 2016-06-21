namespace SDLMMForm
{
    partial class ClickDrawCircle
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
            this.sdlmmControl1.Location = new System.Drawing.Point(12, 12);
            this.sdlmmControl1.Name = "sdlmmControl1";
            this.sdlmmControl1.Size = new System.Drawing.Size(529, 458);
            this.sdlmmControl1.TabIndex = 0;
            // 
            // ClickDrawCircle
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.ClientSize = new System.Drawing.Size(553, 482);
            this.Controls.Add(this.sdlmmControl1);
            this.Name = "ClickDrawCircle";
            this.Text = "ClickDrawCircle";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ClickDrawCircle_FormClosing);
            this.ResumeLayout(false);

        }

        #endregion

        private SDLMMControl sdlmmControl1;
    }
}