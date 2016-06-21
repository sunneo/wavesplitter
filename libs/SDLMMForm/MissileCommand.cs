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
    public partial class MissileCommand : Form
    {
        private static int MAX_BUILD = 5;
        private static int MAX_ENERMY_SPEED = 2;
        private const int buildWidth = 128;
        private const int buildHeight = 128;
        private const int launcherWidth = 128;
        private const int launcherHeight = 128;
        private int width = 800;
        private int height = 600;
        private const int maxRadius = 32;
        private const int maxMissile = 16;
        private int score = 0;
        private int remainMissile = 45;
        private int remainGenEnermy = 40;
        private int remainEnermy = 40;
        private int maxEnemyMissile = 15;
        private int[][] buildTex = new int[2][];
        private int[] missileTex;
        private int[][] launcherTex = new int[5][];
        private int[] bg;
        volatile private int mx = 0;
        volatile private int my = 0;
        struct Missile
        {
            public int fx, fy, tx, ty;
            public float x, y, dx, dy;
            public bool alive, expl, ishit;
            public int r, targetBuild;
        }

        struct OurLaunchedMissile
        {
            public int tx, ty, r;
            public double x, y, dx, dy;
            public bool active, expl;
        }

        struct Build
        {
            public int left, top, right, bottom;
            public bool alive, isbuild;
        }
        private Build[] build = new Build[MAX_BUILD];
        private Missile[] enermy = new Missile[20];
        private OurLaunchedMissile[] launchedMissile = new OurLaunchedMissile[16];
        Random rand_generator = new Random();
        int rand()
        {
            return rand_generator.Next();
        }
        double frand()
        {
            return rand_generator.NextDouble();
        }
        void draw_enermy()
        {
            int i;
            for (i = 0; i < 20; ++i)
            {
                if (!enermy[i].alive) continue;
                if (enermy[i].expl)
                {
                    int color=(int)(((rand() << i) & 0xffee00) | 0xf0f000);
                    this.sdlmmControl1.fillCircle((int)enermy[i].x, (int)enermy[i].y, (int)enermy[i].r, color);
                }
                else
                {
                    this.sdlmmControl1.drawLine((int)enermy[i].fx, (int)enermy[i].fy, (int)enermy[i].x, (int)enermy[i].y, 0x0000ff);
                    this.sdlmmControl1.drawLine((int)enermy[i].fx - 1, (int)enermy[i].fy, (int)enermy[i].x, (int)enermy[i].y, 0x0000bb);
                    this.sdlmmControl1.drawLine((int)enermy[i].fx + 1, (int)enermy[i].fy, (int)enermy[i].x, (int)enermy[i].y, 0x0000aa);
                    //drawcircle(enermy[i].fx,enermy[i].fy,enermy[i].r,0xff0000);
                    this.sdlmmControl1.fillCircle((int)enermy[i].x, (int)enermy[i].y, (int)enermy[i].r + 1, 0xffff00);
                    this.sdlmmControl1.drawCircle((int)enermy[i].x, (int)enermy[i].y, (int)enermy[i].r, 0xff0000);
                }
            }
        }
        private void update_enermy()
        {
            int i;
            for (i = 0; i < 20; ++i)
            {
                if (!enermy[i].alive) continue;
                if (enermy[i].expl)
                {
                    if (enermy[i].r >= maxRadius * 2)
                    {
                        enermy[i].alive = false;
                        enermy[i].ishit = false;
                        enermy[i].expl = false;
                        if (remainEnermy - 1 >= 0)
                            --remainEnermy;
                    }
                    enermy[i].r += 2;
                }
                else
                {
                    {
                        int j;
                        for (j = 0; j < 16; ++j)
                        {
                            if (!launchedMissile[j].active) continue;
                            if (launchedMissile[j].expl)
                            {
                                double distX = (enermy[i].x - launchedMissile[j].x);
                                double distY = (enermy[i].y - launchedMissile[j].y);
                                if (distX * distX + distY * distY < launchedMissile[j].r * launchedMissile[j].r)
                                {
                                    enermy[i].expl = true;
                                    enermy[i].ishit = true;
                                    score += 100;
                                    return;
                                }
                            }
                        }
                        for (j = 0; j < 20; ++j)
                        {
                            if (j == i) continue;
                            if (!enermy[j].alive) continue;
                            if (!enermy[j].expl) continue;
                            if (!enermy[j].ishit) continue;
                            float distX = (enermy[i].x - enermy[j].x);
                            float distY = (enermy[i].y - enermy[j].y);
                            if (distX * distX + distY * distY < enermy[j].r * enermy[j].r)
                            {
                                enermy[i].expl = true;
                                enermy[i].ishit = true;
                                score += 100;
                                return;
                            }
                        }
                    }
                    enermy[i].x += enermy[i].dx;
                    enermy[i].y += enermy[i].dy;
                    if ((int)(enermy[i].tx - enermy[i].x) <= 0 && 0 >= (int)(enermy[i].ty - enermy[i].y))
                    {
                        enermy[i].expl = true;
                        build[enermy[i].targetBuild].alive = false;
                    }
                }
            }
        }
        void generate_enermy()
        {
            if (remainGenEnermy > 0)
            {
                int currentAlive = 0;
                int i;
                for (i = 0; i < 20; ++i)
                {
                    if (currentAlive >= maxEnemyMissile) break;
                    if (enermy[i].alive)
                    {
                        ++currentAlive;
                        continue;
                    }
                    else
                    {
                        int targetIdx, sx, sy, tx, ty;
                        targetIdx = (rand() % MAX_BUILD);
                        sx = (rand() % width); sy = 0;
                        tx = (build[targetIdx].left + build[targetIdx].right) / 2;
                        ty = build[targetIdx].top;
                        int enermySpeed = (rand() % MAX_ENERMY_SPEED);
                        if (enermySpeed == 0) enermySpeed = 1;
                        float dx = ((float)(tx - sx)) / (1024 / enermySpeed);
                        float dy = ((float)(ty - sy)) / (1024 / enermySpeed);
                        enermy[i].x = enermy[i].fx = sx;
                        enermy[i].y = enermy[i].fy = sy;
                        enermy[i].dx = dx;
                        enermy[i].dy = dy;
                        enermy[i].tx = tx;
                        enermy[i].ty = ty;
                        enermy[i].expl = false;
                        enermy[i].alive = true;
                        enermy[i].r = 2;
                        enermy[i].targetBuild = targetIdx;
                        ++currentAlive;
                        --remainGenEnermy;
                        if (remainGenEnermy == 0) return;
                    }
                }
            }
        }
        private void draw_launcher(int x, int y)
        {
            int split = width / MAX_BUILD;
            int idx = mx / split;
            int padding = 16;
            if (idx < 0) idx = 0;
            if (idx > 4) idx = 4;
            this.sdlmmControl1.drawPixels(launcherTex[idx], padding + x, y, launcherWidth, launcherHeight, 0xff0000);
        }
        void stretch_loadimage(String fname, out int[] pixels, ref int w, ref int h, int w2, int h2)
        {
            sdlmmControl1.loadimage(fname, out pixels, out w, out h);
            sdlmmControl1.stretchpixels2(ref pixels,w, h,w2,h2);
        }

        void load_tex()
        {
            int dummy1=0, dummy2=0;
            stretch_loadimage("missile-command/img/Lucid-burn400x300.bmp",  out bg, ref dummy1, ref dummy2, width, height);
            stretch_loadimage("missile-command/img/build0.bmp", out buildTex[0], ref dummy1, ref dummy2, buildWidth, buildHeight);
            stretch_loadimage("missile-command/img/build.bmp", out buildTex[1], ref dummy1, ref dummy2, buildWidth, buildHeight);
            stretch_loadimage("missile-command/img/launcher-L2.bmp", out launcherTex[0], ref dummy1, ref dummy2, launcherWidth, launcherHeight);
            stretch_loadimage("missile-command/img/launcher-L1.bmp", out launcherTex[1], ref dummy1, ref dummy2, launcherWidth, launcherHeight);
            stretch_loadimage("missile-command/img/launcher.bmp", out launcherTex[2], ref dummy1, ref dummy2, launcherWidth, launcherHeight);
            stretch_loadimage("missile-command/img/launcher-R1.bmp", out launcherTex[3], ref dummy1, ref dummy2, launcherWidth, launcherHeight);
            stretch_loadimage("missile-command/img/launcher-R2.bmp", out launcherTex[4], ref dummy1, ref dummy2, launcherWidth, launcherHeight);
            sdlmmControl1.loadimage("missile-command/img/missile.bmp", out missileTex, out dummy1, out dummy2);
        }

        void init_build(int cnt)
        {
            int i;
            int padding = 10;
            int buildtop = height - buildHeight;
            for (i = 0; i < cnt; ++i)
            {
                build[i].left = (padding + buildWidth) * i;
                build[i].top = buildtop;
                build[i].right = build[i].left + buildWidth;
                build[i].bottom = height;
                build[i].alive = true;
                build[i].isbuild = true;
            }
            build[cnt / 2].isbuild = false;
            build[cnt / 2].top = height - launcherHeight;
        }

        void draw_build(int cnt)
        {
            int i;
            for (i = 0; i < cnt; ++i)
            {
                if (build[i].isbuild)
                {
                    this.sdlmmControl1.drawPixels(buildTex[build[i].alive?1:0], build[i].left, build[i].top, buildWidth, buildHeight, 0xff0000);
                }
                else
                {
                    draw_launcher(build[i].left, build[i].top);
                }
            }
        }

        void update_missile()
        {
            int i;
            for (i = 0; i < maxMissile; ++i)
            {
                if (!launchedMissile[i].active) continue;
                if (!launchedMissile[i].expl)
                {
                    if ((int)(launchedMissile[i].tx - launchedMissile[i].x) == 0 && 0 == (int)(launchedMissile[i].ty - launchedMissile[i].y))
                    {
                        launchedMissile[i].expl = true;
                    }
                    launchedMissile[i].x += launchedMissile[i].dx;
                    launchedMissile[i].y += launchedMissile[i].dy;
                }
                else
                {
                    if (launchedMissile[i].r < maxRadius)
                    {
                        ++launchedMissile[i].r;
                    }
                    else
                    {
                        launchedMissile[i].active = false;
                        launchedMissile[i].expl = false;
                    }
                }
            }
        }
        private void drawMessage()
        {
            String cscore = String.Format("Score:{0}", score);
            String cmissile = String.Format(":{0}", remainMissile);
            String cenermy = String.Format("Enermy:{0}/{1}", remainEnermy, remainGenEnermy);
            this.sdlmmControl1.drawString(cscore, 0, 0, 0xffffff);
            this.sdlmmControl1.drawPixels(missileTex, width - 80 - 16, 24, 16, 64, 0xff0000);
            this.sdlmmControl1.drawString(cmissile, width - 80, 24, 0xffffff);
            this.sdlmmControl1.drawString(cenermy, width - 200, 0, 0xffffff);
        }
        void draw_missile()
        {
            int i;
            for (i = 0; i < maxMissile; ++i)
            {
                if (!launchedMissile[i].active) continue;
                if (!launchedMissile[i].expl)
                {
                    double dx = launchedMissile[i].dx;
                    double dy = launchedMissile[i].dy;
                    int j;
                    this.sdlmmControl1.fillCircle((int)launchedMissile[i].x, (int)launchedMissile[i].y, launchedMissile[i].r, 0xffff00);
                    for (j = 0; j < 8; ++j)
                    {
                        this.sdlmmControl1.fillCircle((int)(launchedMissile[i].x - j * dx), (int)(launchedMissile[i].y - j * dy), (int)launchedMissile[i].r + j, (int)(0xffffff - 0x101010 * (j + 1)));
                    }

                }
                else
                {
                    this.sdlmmControl1.fillCircle((int)launchedMissile[i].x, (int)launchedMissile[i].y, (int)launchedMissile[i].r, rand() << 9);
                }
            }
        }
        void reinit()
        {
            if (remainEnermy <= 0 && remainGenEnermy <= 0)
            {
                init_build(MAX_BUILD);
                remainEnermy = 40;
                remainGenEnermy = 40;
                remainMissile = 45;
            }
        }
        void drawfnc()
        {
            //fillrect(0,0,width,height,0x2200dd);

                reinit();
                this.sdlmmControl1.drawPixels(bg, 0, 0, width, height);
                draw_build(MAX_BUILD);
                draw_missile();
                draw_enermy();
                update_missile();
                update_enermy();
                drawMessage();
                if (rand() % 100 < 20) generate_enermy();
                this.sdlmmControl1.flush();
                //System.Threading.Thread.Sleep(1);
        }
        void onmotion(int x, int y, int btn, bool ison)
        {
            mx = x;
            my = y;
        }
        void generate_missile(int mx, int my)
        {
            int i;
            if (remainMissile <= 0) return;
            for (i = 0; i < maxMissile; ++i)
            {
                if (!launchedMissile[i].active)
                {
                    int sx = build[2].left + 32;
                    int sy = build[2].top;
                    float dx = ((float)(mx - sx)) / 50;
                    float dy = ((float)(my - sy)) / 50;
                    launchedMissile[i].active = true;
                    launchedMissile[i].x = sx;
                    launchedMissile[i].y = sy;
                    launchedMissile[i].r = 3;
                    launchedMissile[i].tx = mx;
                    launchedMissile[i].ty = my;
                    launchedMissile[i].dx = dx;
                    launchedMissile[i].dy = dy;
                    launchedMissile[i].expl = false;
                    --remainMissile;
                    break;
                }
            }
        }

        void onmouse(int x, int y, int btn, bool on)
        {
            mx = x;
            my = y;
            if (on)
            {
                generate_missile(mx, my);
            }
        }
        Timer timer = new Timer();
        void main_run()
        {
            this.sdlmmControl1.onMouseMoveHandler = onmotion;
            this.sdlmmControl1.onMouseClickHandler = onmouse;
            this.sdlmmControl1.setUseAlpha(false);
            load_tex();
            init_build(MAX_BUILD);
          
            timer.Interval = 1;
            timer.Tick += (obj, eventArg) => { drawfnc(); };
            timer.Start();
        }

        public MissileCommand()
        {
            InitializeComponent();

        }

        private void MissileCommand_SizeChanged(object sender, EventArgs e)
        {
            width = this.Width;
            height = this.Height;
            load_tex();
        }

        private void MissileCommand_Load(object sender, EventArgs e)
        {
            try
            {
                main_run();
            }
            catch (Exception ee)
            {
                Console.WriteLine(ee.ToString());
            }
        }

        private void MissileCommand_FormClosing(object sender, FormClosingEventArgs e)
        {
            timer.Stop();
        }
    }
}
