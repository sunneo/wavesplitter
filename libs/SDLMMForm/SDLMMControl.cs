using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Imaging;

namespace SDLMMForm
{
    public partial class SDLMMControl : UserControl
    {
        Bitmap canvas;
        Graphics graphic;
        public static readonly uint alphaMask = 0xff000000u;
        bool useAlpha = false;
        public delegate void OnMouseButtonAction(int x, int y, int btn, bool ison);
        public delegate void OnMouseMoveAction(int x, int y, int btn, bool ison);
        public delegate void OnMouseWhellAction(int x, int y, int scrollAmount);
        public delegate void OnKeyboardAction(int keycode,bool ctrl,bool ison);
        public OnMouseButtonAction onMouseClickHandler;
        public OnMouseMoveAction onMouseMoveHandler;
        public OnKeyboardAction onKeyboard;
        public OnMouseWhellAction onMouseWhell;
        public static readonly int MOUSE_LEFT = 0;
        public static readonly int MOUSE_MIDDLE = 1;
        public static readonly int MOUSE_RIGHT = 2;
        private int mouseIdx(MouseButtons btn)
        {
            switch(btn)
            { 
                default:
                case System.Windows.Forms.MouseButtons.Left:
                    return MOUSE_LEFT;
                case System.Windows.Forms.MouseButtons.Middle:
                    return MOUSE_MIDDLE;
                case System.Windows.Forms.MouseButtons.Right:
                    return MOUSE_RIGHT;
            }
        }
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if(onMouseWhell!= null){
                onMouseWhell(e.X, e.Y, e.Delta * SystemInformation.MouseWheelScrollLines / 120);
            }
            base.OnMouseWheel(e);
        }
        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (onMouseClickHandler != null)
            {
                onMouseClickHandler(e.X, e.Y, mouseIdx(e.Button), true);
            }
            base.OnMouseDown(e);
        }
        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (onMouseClickHandler != null)
            {
                onMouseClickHandler(e.X, e.Y, mouseIdx(e.Button), false);
            }
            base.OnMouseDown(e);
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (onMouseMoveHandler != null)
            {
                onMouseMoveHandler(e.X, e.Y, mouseIdx(e.Button), e.Button != System.Windows.Forms.MouseButtons.None);
            }
            base.OnMouseMove(e);
        }
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (onKeyboard!= null)
            {
                onKeyboard((int)e.KeyCode, e.Control, true);
            }
            base.OnKeyDown(e);
        }
        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (onKeyboard != null)
            {
                onKeyboard((int)e.KeyCode, e.Control, false);
            }
            base.OnKeyUp(e);
        }
        public SDLMMControl()
        {
            InitializeComponent();
            canvas = new Bitmap(this.Width, this.Height);
            graphic = Graphics.FromImage(canvas);
            this.AutoScaleDimensions = new SizeF(1, 1);

        }
        public SizeF MeasureString(String s, Font font = null,int maxsize=-1)
        {
            if (font == null) font = this.Font;
            if (maxsize < 0)
            {
                return graphic.MeasureString(s, font);
            }
            return graphic.MeasureString(s, font, maxsize);
        }
  
        public void setUseAlpha(Boolean buse)
        {
            useAlpha = buse;
        }
        private Color coveredColor(int color)
        {
            if (!useAlpha)
            {
                color =(int) ((uint)color | alphaMask);
            }
            return Color.FromArgb(color);
        }
        public void getScreen(out int[] outpixels, out int w, out int h)
        {

            int[] outputArray = new int[canvas.Width * canvas.Height];
            BitmapData bmpData = canvas.LockBits(new Rectangle(0, 0, canvas.Width, canvas.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, canvas.PixelFormat);
            unsafe
            {
                int* pixelsMap = (int*)bmpData.Scan0;

                Parallel.For(0, outputArray.Length, (i) =>
                {
                    outputArray[i] = pixelsMap[i];
                });
            }
            canvas.UnlockBits(bmpData);
            outpixels = outputArray;
            w = canvas.Width;
            h = canvas.Height;

        }
        public void getScreen(out Bitmap bmp)
        {
            bmp = new Bitmap(canvas);
        }
        public void drawPixel(int x, int y, int color)
        {
            if (x < 0 || y < 0 || x >= Width || y >= Height) return;
            //lock (canvas)
            {
                canvas.SetPixel(x, y, Color.FromArgb(color));
            }
        }
        public void drawCircle(int n_cx, int n_cy, int radius, int pixel)
        {
            //lock (canvas)
            {
                Pen pen = new Pen(coveredColor(pixel));
                graphic.DrawEllipse(pen, n_cx-radius, n_cy-radius, radius * 2, radius* 2);
            }
        }
        public void fillCircle(int x, int y, int r, int color)
        {
            //lock (canvas)
            {
                Brush brush = new SolidBrush(coveredColor(color));
                graphic.FillEllipse(brush, x-r, y-r, r * 2, r * 2);
            }
        }
        public void drawRect(int x, int y, int w, int h, int color)
        {
            //lock (canvas)
            {
                Pen pen = new Pen(coveredColor(color));
                graphic.DrawRectangle(pen, x, y, w, h);
            }
        }
        public void fillRect(int x, int y, int w, int h, int color)
        {
            //lock (canvas)
            {
                Brush brush = new SolidBrush(coveredColor(color));
                graphic.FillRectangle(brush, x, y, w, h);
            }
        }
        public void drawLine(int x, int y, int x2, int y2,int color,int stride=1)
        {
            //lock(canvas)
            {
                 Pen pen = new Pen(coveredColor(color));
                 pen.Width = stride;
                 graphic.DrawLine(pen, x, y, x2, y2);
            }
        }
        public void drawPixels(int[] pixels, int x, int y, int w, int h,int transkey)
        {
            //lock (canvas)
            {
                int origh = pixels.Length / w;
                Bitmap bmp = new Bitmap(w, origh);
                BitmapData bmpData= bmp.LockBits(new Rectangle(0, 0, w, h), System.Drawing.Imaging.ImageLockMode.WriteOnly, bmp.PixelFormat);
                unsafe
                {
                    int* pixelsMap = (int*)bmpData.Scan0;
#if false
                for(int i=0;i<pixels.Length; ++i){
                   Color c;
                   if ((pixels[i]&0xffffff) == transkey)
                   {
                       c = Color.FromArgb(0);
                   }
                   else
                   {
                       c = coveredColor(pixels[i]);
                   }
                   bmp.SetPixel(i % w, i / w, c);      
                }
#else
                    Parallel.For(0, pixels.Length, (i) =>
                    {
                        Color c;
                        if ((pixels[i] & 0xffffff) == transkey)
                        {
                            c = Color.FromArgb(0);
                        }
                        else
                        {
                            c = coveredColor(pixels[i]);
                        }
                        pixelsMap[i] = c.ToArgb();
                        //bmp.SetPixel(i % w, i / w, c);
                    });
                }
#endif
                bmp.UnlockBits(bmpData);
                graphic.DrawImageUnscaledAndClipped(bmp,  new Rectangle(x, y, w, h));
            }
        }
        public void stretchpixels(int[] pixels, int w, int h, int[] output, int w2, int h2)
        {
            float dw = ((float)w2) / w;
            float dh = ((float)h2) / h;
            int ii, e;
            e = h2 * w2;
            //#pragma omp parallel for firstprivate(output,pixels,dw,dh)
            for (ii = 0; ii < e; ii += 1)
            {
                int origi, origj;
                float i = ii % w2;
                float j = ii / w2;
                //for(j=0; j<h2; j+=1){
                //for(i=0; i<w2; i+=1){
                origi = (int)(i / dw);
                origj = (int)(j / dh);
                output[(int)(j * w2 + i)] = pixels[(int)(origj * w + origi)];
                //}
            }
        }
        public void stretchpixels2(ref int[] pixels, int w, int h, int w2, int h2)
        {
            int[] newbg = new int[w2 * h2];
            int[] org = pixels;
            stretchpixels(org, w, h, newbg, w2, h2);
            pixels = newbg;
        }
        public void drawPixels(int[] pixels, int x, int y, int w, int h)
        {
            if (w <= 0 || h <= 0) return;
            //lock (canvas)
            {
                int origh = pixels.Length / w;
                Bitmap bmp = new Bitmap(w, origh);
                if (h > origh)
                {
                    h = origh;
                }
                BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, w, h), System.Drawing.Imaging.ImageLockMode.WriteOnly, bmp.PixelFormat);
                unsafe
                {
                    int* pixelsMap = (int*)bmpData.Scan0;
#if false
                for(int i=0;i<pixels.Length; ++i)
                {
                    Color c = coveredColor(pixels[i]);
                    bmp.SetPixel(i % w, i / w, c);
                }
#else
                    Parallel.For(0, pixels.Length, (i) =>
                    {
                        Color c = coveredColor(pixels[i]);
                        pixelsMap[i] = c.ToArgb();
                        //bmp.SetPixel(i % w, i / w, c);
                    });
                }
#endif
                bmp.UnlockBits(bmpData);
                graphic.DrawImageUnscaledAndClipped(bmp, new Rectangle(x, y, w, h));
            }
        }
        public void loadimage(String name, out int[] pixels, out int w, out int h)
        {
            Bitmap bmp = (Bitmap)Bitmap.FromFile(name);
            
            w = bmp.Width;
            h = bmp.Height;
#if false
            int W = bmp.Width;
            int H = bmp.Height;
            int totalLength = W * H;
            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, bmp.PixelFormat);
            int[] outputPixels = new int[W * H];
            unsafe
            {
               
                int* pixelData = (int*)bmpData.Scan0;
                for (int i = 0; i < H; ++i)
                {
                    for (int j = 0; j < W; ++j)
                    {
                        outputPixels[i * W + j] = pixelData[i * W + j];
                    }
                }
            }
            bmp.UnlockBits(bmpData);
           
#else
            int W = bmp.Width;
            int H = bmp.Height;
            int[] outputPixels = new int[W * H];
            for (int i = 0; i < bmp.Height; ++i)
            {
                for (int j = 0; j < bmp.Width; ++j)
                {
                    outputPixels[i * bmp.Width + j] = bmp.GetPixel(j, i).ToArgb();
                }
            }
            pixels = outputPixels;
#endif   
        }
        public void drawImage(Bitmap bmp, int x, int y, int w, int h)
        {
            //lock (canvas)
            {
                graphic.DrawImage(bmp, x, y, w, h);
            }
        }
        public int[] bitmapToPixels(Bitmap bmp)
        {
            int[] outputarr = new int[bmp.Width * bmp.Height];
            int Length = outputarr.Length;
            BitmapData bmpData0 = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, bmp.PixelFormat);
            unsafe
            {
                int* pixelsMap0 = (int*)bmpData0.Scan0;
                Parallel.For(0, Length, (i) =>
                {
                    outputarr[i] = pixelsMap0[i];
                });
            }
            bmp.UnlockBits(bmpData0);
            return outputarr;
        }
        public void drawImage(Bitmap bmp, int x, int y, int w, int h,float alpha)
        {
            if (alpha < 0) alpha = 0;
            Bitmap newBmp = new Bitmap(bmp);
            int Length = bmp.Width * bmp.Height;
            BitmapData bmpData0 = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, bmp.PixelFormat);
            BitmapData bmpData = newBmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.Imaging.ImageLockMode.WriteOnly, bmp.PixelFormat);
            unsafe
            {
                int* pixelsMap0 = (int*)bmpData0.Scan0;
                int* pixelsMap = (int*)bmpData.Scan0;
                Parallel.For(0, Length, (i) =>
                {
                    Color c = Color.FromArgb(pixelsMap0[i]);
                    pixelsMap[i] = Color.FromArgb((int)(c.A*alpha),c.R,c.G,c.B).ToArgb();
                    //bmp.SetPixel(i % w, i / w, c);
                });
            }
            bmp.UnlockBits(bmpData0);
            newBmp.UnlockBits(bmpData);
            drawImage(newBmp, x, y, w, h);
        }
        public void Clear(int color)
        {
            this.graphic.Clear( coveredColor(color) );
        }
        public void drawString(String str, int x, int y,int color,Font font=null)
        {
            //lock (canvas)
            {
                if (font == null)
                {
                    font = System.Drawing.SystemFonts.DefaultFont;
                }
                Brush brush = new SolidBrush(coveredColor(color));
                graphic.DrawString(str, font, brush, x, y);
            }
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            //lock (canvas)
            {
                e.Graphics.DrawImageUnscaled(canvas, 0, 0, canvas.Width, canvas.Height);
            }
            base.OnPaint(e);
        }
        
        public Bitmap flushToBMP()
        {
            graphic.Flush();
            return canvas;
        }
        public Bitmap flushToBMP(int left,int top,int w,int h)
        {
            graphic.Flush();
            if (left < 0)
            {
                left = 0;
            }
            if (top < 0)
            {
                top = 0;
            }
            if (left + w > canvas.Width)
            {
                w = canvas.Width - left;
            }
            if (top + h > canvas.Height)
            {
                h = canvas.Height - top;
            }

            return canvas.Clone(new Rectangle(left, top, w, h), canvas.PixelFormat);
        }
        public void flush()
        {
            //lock (canvas)
            {
                
                graphic.Flush();
                
            }
            this.Invalidate();
        }
        
		public void mode7Render(double angle, int vx, int vy, int[] pixels, int bw, int bh, int x, int y, int w,
				int h) {
			mode7render_internal(0.5, 1.5, 2, 1, angle, vx, vy, pixels, bw, bh, x, y, w, h);
		}

		int[] elesArray;
		int[] mode7ToDraw;
		int mode7ToDrawW, mode7ToDrawH;

		public void mode7render_internal(double groundFactor,  double xFac,  double yFac,
				 int scanlineJump,  double angle,  int vx,  int vy,  int[] bg,  int bw,
				 int bh,  int tx,  int ty, int _w, int _h) {
			if (tx + _w >= this.Width) {
				_w = this.Width - tx;
			}
			if (ty + _h > this.Height) {
				_h =  this.Height- ty;
			}
			int w = _w;
			int h = _h;
			if (mode7ToDraw != null) {
				if (w * h > mode7ToDrawW * mode7ToDrawH) {
					mode7ToDraw = new int[w * h];
				}
			} else {
				mode7ToDraw = new int[w * h];
			}
			if (mode7ToDrawW != w) {
				mode7ToDrawW = w;
			}
			if (mode7ToDrawH != h) {
				mode7ToDrawH = h;
			}
			 int[] toDraw = mode7ToDraw;
			 int lev = w / scanlineJump;
			 int x;
			 double ca = Math.Cos(angle) * 48 * groundFactor * xFac;
			 double sa = Math.Sin(angle) * 48 * groundFactor * xFac;
			 double can = Math.Cos(angle + 3.1415926 / 2) * 16 * groundFactor * yFac;
			 double san = Math.Sin(angle + 3.1415926 / 2) * 16 * groundFactor * yFac;

             for (x = 0; x < lev; ++x)
             {
                 int y;
                 double xr = -(((double)x / lev) - 0.5);
                 double cax = (ca * xr) + can;
                 double sax = (sa * xr) + san;
                 for (y = 0; y < h; ++y)
                 {
                     double zf = ((double)h) / y;
                     int xd = (int)(vx + zf * cax);
                     int yd = (int)(vy + zf * sax);
                     if (yd < bh && xd < bw && yd > 0 && xd > 0)
                     {
                         toDraw[y * w + x] = bg[yd * bw + xd];
                     }
                 }
             }
			
			drawPixels(toDraw, tx, ty, w, h);
		}

        private void SDLMMControl_SizeChanged(object sender, EventArgs e)
        {
            lock (canvas)
            {
                if (graphic != null)
                {
                    graphic.Flush();
                    int width = this.Width;
                    int height = this.Height;
                    if (width <= 0 || height <= 0)
                    {
                        return;
                    }
                    if (width <= 0) width = 32;
                    if (height <= 0) height = 32;
                    canvas = new Bitmap(canvas, width, height);
                    graphic = Graphics.FromImage(canvas);
                }
            }
        }
    }
}
