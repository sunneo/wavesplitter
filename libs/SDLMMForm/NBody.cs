using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SDLMMForm
{
    public partial class NBody : Form
    {
        System.Windows.Forms.Timer timer;
        public NBody()
        {
            InitializeComponent();
            tmstart = getDoubleTime();
            
            X_axis = allocateBody();
            Y_axis = allocateBody();
            Z_axis = allocateBody();
            X_Velocity = allocateBody();
            Y_Velocity = allocateBody();
            Z_Velocity = allocateBody();
            Mass = allocateBody();
            newX_velocity = allocateBody();
            newY_velocity = allocateBody();
            newZ_velocity = allocateBody();
            Init_AllBody();
            sdlmmControl1.onKeyboard = kbfnc;
            sdlmmControl1.onMouseClickHandler = mouse;
            sdlmmControl1.onMouseMoveHandler = mouse;
            timer = new System.Windows.Forms.Timer();
            timer.Interval = 1;
            timer.Tick += main_run;
            timer.Start();
            this.FormClosing += NBody_FormClosing;
        }

        void NBody_FormClosing(object sender, FormClosingEventArgs e)
        {
            timer.Stop();
        }

        private void main_run(object sender, EventArgs e)
        {
            this.timer.Stop();
            this.main_run();
            this.timer.Start();
        }
        static int SCREENX = 1024;
        static int SCREENY = 768;
        static int LOOP = 200;
        static int NUM_BODY = 1000;
        static int MAX_X_axis = 1000;
        static int MIN_X_axis = 0;
        static int MAX_Y_axis = 1000;
        static int MIN_Y_axis = 0;
        static int MAX_Velocity = 200;
        static int MIN_velocity = -200;
        static int MAX_Mass = 150;
        static int MIN_Mass = 3;
        bool mouseIsOn = false;
        int SZ = NUM_BODY;
        bool showhelp = true;
        int showmode = 3;
        float simulatetime_factor = 0.01f;
        bool random_simulatefactor = true;
        bool centralize = false;

        float[] X_axis, Y_axis, Z_axis, X_Velocity, Y_Velocity, Z_Velocity, Mass;
        float[] newX_velocity, newY_velocity, newZ_velocity;
        Random random = new Random();
        double getDoubleTime()
        {
            return (double)DateTime.Now.Subtract(DateTime.FromBinary(0)).TotalMilliseconds;
        }

        int rand()
        {
            return (int)(random.Next());
        }

        float clamp(float v, float minv, float maxv)
        {
            if (v > maxv)
                v = (v + maxv) / 2;
            if (v < minv)
                v = (v + minv) / 2;
            return v;
        }

        float[] allocateBody()
        {
            return new float[SZ];
        }

        void Init_AllBody()
        {
            int i = 0;
            for (i = 0; i < SZ; i++)
            {
                X_axis[i] = rand() % (MAX_X_axis - MIN_X_axis) + MIN_X_axis;
                Y_axis[i] = rand() % (MAX_Y_axis - MIN_Y_axis) + MIN_Y_axis;
                Z_axis[i] = rand() % (MAX_Y_axis - MIN_Y_axis) + MIN_Y_axis;
                X_Velocity[i] = newX_velocity[i] = 0;
                Y_Velocity[i] = newY_velocity[i] = 0;
                Z_Velocity[i] = newZ_velocity[i] = 0;
                Mass[i] = rand() % (MAX_Mass - MIN_Mass) + MIN_Mass;
            }
        }
        private void kbfnc(int key, bool ctrl, bool ison)
        {
            if (!ison)
                return;
            switch (key)
            {
                case '0':
                case '1':
                case '2':
                case '3':
                    showmode = key - '0';
                    break;
                case 'c':
                case 'C':
                    centralize = !centralize;
                    break;
                case 'r':
                case 'R':
                    random_simulatefactor = !random_simulatefactor;
                    break;
                case 'h':
                case 'H':
                    showhelp = !showhelp;
                    break;
            }
        }
        private void mouse(int x, int y, int btn, bool ison)
        {
            mouseIsOn = ison;
            if (!ison)
                return;
            if (y > 60 && y < 80 && x >= 320 && x <= 320 + 400)
            {
                float value = (float)(2.0 * ((float)(x - 320)) / 400);
                simulatetime_factor = value;
            }
        }

        void Nbody(int i, int sz, float[] X_axis, float[] Y_axis, float[] Z_axis, float[] newX_velocity,
                float[] newY_velocity, float[] newZ_velocity, float[] Mass, float simulatetime)
        {
            int j;
            float sumX = 0, sumY = 0, sumZ = 0;
            float Gravity_Coef = 3.3f;
            for (j = 0; j < sz; j++)
            {
                float X_position, Y_position, Z_position;
                float Distance;
                float Force = 0;
                if (j == i)
                    continue;
                X_position = X_axis[j] - X_axis[i];
                Y_position = Y_axis[j] - Y_axis[i];
                Z_position = Z_axis[j] - Z_axis[i];
                Distance = (float)Math.Sqrt(X_position * X_position + Y_position * Y_position + Z_position * Z_position);
                if (Distance == 0)
                    continue;
                Force = Gravity_Coef * Mass[i] / (Distance * Distance);
                sumX += Force * X_position;
                sumY += Force * Y_position;
                sumZ += Force * Z_position;
            }
            newX_velocity[i] += sumX * simulatetime;
            newY_velocity[i] += sumY * simulatetime;
            newZ_velocity[i] += sumZ * simulatetime;
            X_axis[i] += clamp(newX_velocity[i], MIN_velocity, MAX_Velocity) * simulatetime;
            Y_axis[i] += clamp(newY_velocity[i], MIN_velocity, MAX_Velocity) * simulatetime;
            Z_axis[i] += clamp(newZ_velocity[i], MIN_velocity, MAX_Velocity) * simulatetime;
            X_Velocity[i] = newX_velocity[i];
            Y_Velocity[i] = newY_velocity[i];
            Z_Velocity[i] = newZ_velocity[i];
        }
        void drawBodys(int loop, int totalLoop, double tm, float avgX, float avgY, float avgZ)
        {

            int i;
            double rendert1, rendert2;
            String buf;
            rendert1 = getDoubleTime();
            //sdlmmControl1.fillRect(0, 0, SCREENX, SCREENY, 0x2f2f2f);
            sdlmmControl1.Clear(0x2f2f2f);
            for (i = 0; i < SZ; ++i)
            {
                int x, y, r, c;
                if (centralize)
                {
                    x = (int)(SCREENX / 2 * (X_axis[i] / avgX));
                    y = (int)(SCREENY / 2 * (Y_axis[i] / avgY));
                }
                else
                {
                    x = (int)(avgX - X_axis[i] + SCREENX / 2);
                    y = (int)(avgY - Y_axis[i] + SCREENY / 2);
                }
                r = (int)(5 * (Z_axis[i] / avgZ));
                if (r < 0 || r > 255)
                    continue;
                if (showmode == 0)
                {
                    sdlmmControl1.drawCircle(x, y, r, 0x00D0D0 | (r * 0xa0a000));
                }
                else if (showmode == 1)
                {
                    sdlmmControl1.drawPixel(x, y, 0x00D0D0 | (r * 0xa0a000));
                }
                else if (showmode == 2)
                {
                    sdlmmControl1.drawPixel(x, y, 0xfffff);
                    sdlmmControl1.drawCircle(x, y, r, 0x00D0D0 | (r * 0xa0a000));
                }
                else if (showmode == 3)
                {
                    sdlmmControl1.fillCircle(x, y, r, 0x00D0D0 | (r * 0xa0a000));
                }
            }
            if (showhelp)
            {
                buf = String.Format("[{0}/{1}] tm:{2}", loop, totalLoop, tm);
                sdlmmControl1.drawString(buf, 0, 0, 0xffffff);
                buf = String.Format("show mode: {0}[0-3]", showmode);
                sdlmmControl1.drawString(buf, 0, 20, 0xffffff);
                buf = String.Format("simulate factor: {0}", simulatetime_factor);
                sdlmmControl1.drawString(buf, 0, 40, 0xffffff);
                buf = String.Format("randomize simulate factor:{0}[r]", random_simulatefactor ? "on" : "off");
                sdlmmControl1.drawString(buf, 0, 60, 0xffffff);
                sdlmmControl1.fillRect(320, 60, (int)(400 * (simulatetime_factor / 2)), 20, 0xfdfd00);
                sdlmmControl1.drawRect(320, 60, 400, 20, 0xffffff);
                buf = String.Format("centralize: {0}[c]", centralize ? "on" : "off");
                sdlmmControl1.drawString(buf, 0, 80, 0xffffff);
            }
            rendert2 = getDoubleTime();
            sdlmmControl1.flush();
            tm += (rendert2 - rendert1);

            if (tm < 1.0 / 60)
            {
               // Thread.Sleep((int)((1.0 / 60) * 1000 - tm));
            }

        }
        int loop;
        double tmstart, tmend;
        double totalTime = 0.0;
        double fps_time_1, fps_time_2;
        float avgX = 0, avgY = 0, avgZ = 0;

        private void main_run()
        {
            ++loop;
            //for (loop = 0; loop < LOOP; loop++)
            {
                
                // printf("loop %d (%f,%f)\n",loop,avgX,avgY);
                avgX = 0;
                avgY = 0;
                avgZ = 0;
                Application.DoEvents();
                fps_time_1 = getDoubleTime();
                Parallel.For(0, SZ, (i) => {
                    Nbody(i, SZ, X_axis, Y_axis, Z_axis, newX_velocity, newY_velocity, newZ_velocity, Mass,
                            simulatetime_factor);
                });
                for (int i = 0; i < SZ; i++)
                {
                
                    avgX += X_axis[i];
                    avgY += Y_axis[i];
                    avgZ += Z_axis[i];
                }
                avgX /= SZ;
                avgY /= SZ;
                avgZ /= SZ;
                fps_time_2 = getDoubleTime();
                this.Invoke(new Action(() => { drawBodys(loop, LOOP, fps_time_2 - fps_time_1, avgX, avgY, avgZ); }));
                
            }
            tmend = getDoubleTime();
            if (loop >= LOOP)
            {
                loop = 0;
                Console.WriteLine("{0} {1}", NUM_BODY, tmend - tmstart);
                if (random_simulatefactor)
                {
                    simulatetime_factor = (float)random.NextDouble();
                }
                Init_AllBody();
            }

        }

        private void NBody_SizeChanged(object sender, EventArgs e)
        {
            int w = Width;
            int h = Height;
            if (w <= 0) w = 32;
            if (h <= 0) h = 32;
            SCREENX =w;
            SCREENY =h;
        }
    }
}
