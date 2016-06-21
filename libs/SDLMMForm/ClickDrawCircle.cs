using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SDLMMForm
{
    public partial class ClickDrawCircle : Form
    {
        Timer timer;
        public ClickDrawCircle()
        {
            InitializeComponent();
            timer = new Timer();
            timer.Interval = 1;
            timer.Tick += timer_Tick;
            sdlmmControl1.onMouseClickHandler = mouseFnc;
            sdlmmControl1.onMouseMoveHandler = mouseFnc;
            sdlmmControl1.setUseAlpha(false);
            timer.Start();
        }
        static int MAXCNT = 500;
        static int MAXRAD = 300;
        int[] r = new int[MAXCNT];
        int[] xs = new int[MAXCNT];
        int[] ys = new int[MAXCNT];
        int[] c = new int[MAXCNT];
        bool[] active = new bool[MAXCNT];
        volatile int cnt = 0;
        volatile bool clicking = false;
        Random random = new Random();
        void mouseFnc(int x, int y, int btn, bool clicking)
        {
            if (clicking)
            {
                int select = cnt;
                r[select] = 1;
                xs[select] = x;
                ys[select] = y;
                c[select] = (int)(random.NextDouble() * 0xffffff);
                active[select] = true;
                cnt = cnt + 1;
                if (cnt >= MAXCNT)
                    cnt = 0;
            }
        }
        void timer_Tick(object sender, EventArgs e)
        {
            timer.Stop();
            sdlmmControl1.fillRect(0, 0, sdlmmControl1.Width, sdlmmControl1.Height, (int)0xffffff);

            for (int i = 0; i < cnt; ++i)
            {
                if (active[i])
                {
                    if (r[i] + 1 < MAXRAD)
                    {
                        sdlmmControl1.drawCircle(xs[i], ys[i], r[i], c[i]);
                        ++r[i];
                    }
                    else
                    {
                        active[i] = false;
                    }
                }
            }
            sdlmmControl1.fillRect(0, 0, 120, 20, 000000);
            sdlmmControl1.drawString("Click&Drag Mouse", 0, 0, 0xffffff);
            sdlmmControl1.flush();
            timer.Start();
        }

        private void ClickDrawCircle_FormClosing(object sender, FormClosingEventArgs e)
        {
            timer.Stop();
        }
    }
}
