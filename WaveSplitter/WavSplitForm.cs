#define WAVEDRAWER_ENABLE
using NAudio.MediaFoundation;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WaveTextFormatter;

namespace WaveSplitter
{

    public partial class WavSplitForm : Form
    {
        String soundFileName;
        String txtFile;
        String outputFolder;
        bool IsReady = false;
        AudioFileReader reader;
        bool soundDrawingPrepared = false;
        bool canDrawWave = false;
        bool IsMP3 = false;
        volatile bool terminateFired = false;
        FileNameTableDocument FileNameDoc;       
        System.Windows.Forms.Timer sdlTimer = new System.Windows.Forms.Timer() { Interval = 32 };
        class DrawingContext
        {
            protected WavSplitForm Parent;
            public virtual void Draw()
            {
                if (Parent.sdlmmControl1.Width != Parent.Width)
                {
                    if (Parent.Width > 0)
                    {
                        Parent.sdlmmControl1.Width = Parent.Width;
                    }
                }
                Parent.sdlmmControl1.Clear(0x0000ff);
                SizeF notLoaded = Parent.sdlmmControl1.MeasureString("Sound Not Load");
                Parent.sdlmmControl1.fillRect((int)(Parent.sdlmmControl1.Width / 2 - notLoaded.Width / 2), (int)(Parent.sdlmmControl1.Height / 2 - notLoaded.Height / 2), (int)notLoaded.Width, (int)notLoaded.Height, 0xffffff);
                Parent.sdlmmControl1.drawRect((int)(Parent.sdlmmControl1.Width / 2 - notLoaded.Width / 2), (int)(Parent.sdlmmControl1.Height / 2 - notLoaded.Height / 2), (int)notLoaded.Width, (int)notLoaded.Height, 0xffff00);
                Parent.sdlmmControl1.drawString("Sound Not Load",(int)(Parent.sdlmmControl1.Width / 2 - notLoaded.Width / 2), (int)(Parent.sdlmmControl1.Height / 2 - notLoaded.Height / 2), 0x0000ff);
                Parent.sdlmmControl1.flush();
            }
            public DrawingContext(WavSplitForm parent)
            {
                this.Parent = parent;
            }
            public virtual void MouseClick(int x, int y, int btn, bool inon)
            {

            }
            public virtual void MouseMove(int x, int y, int btn, bool inon)
            {

            }
        }
        class DrawingContextNone : DrawingContext
        {
            public override void Draw()
            {
              
            }
            public DrawingContextNone(WavSplitForm parent) : base(parent)
            {
                
            }
            public override void MouseClick(int x, int y, int btn, bool inon)
            {

            }
            public override void MouseMove(int x, int y, int btn, bool inon)
            {
            }
        }
#if WAVEDRAWER_ENABLE
        class WaveBufferDrawer : DrawingContext
        {
            int CurrentWidth = 0;
            int CurrentHeight = 0;
            bool IsScrollDragging = false;
            int mouseX;
            int clickXOffset = 0;
            int heightOfScroll = 20;
            double ScrollRatio = 1;
            int scrollX = 0;
            int pixelsPerSecond = 100;
            int CountPerAvg
            {
                get
                {
                    return (int)(Parent.reader.WaveFormat.SampleRate * Parent.reader.WaveFormat.Channels / pixelsPerSecond);
                }
            }
            Rectangle? scrollBarRect;
            bool HasScrollBar = false;
            bool IsClickOn(Rectangle r, Point pt)
            {
                return r.IntersectsWith(new Rectangle(pt, new Size(1, 1)));
            }
            bool IsScrollBarClicked(int x, int y)
            {
                if (scrollBarRect != null)
                {
                    Point pt = new Point(x, y);
                    if (IsClickOn(scrollBarRect.Value, pt))
                    {
                        return true;
                    }
                }

                return false;
            }
            public void GenerateScrollBar()
            {
                int rectWidth = 0;
                double ratio = ((double)Parent.sdlmmControl1.Width) / avgBuf.Count;
                ScrollRatio = ratio;
                if (ratio < 1)
                {
                    HasScrollBar = true;
                    rectWidth = (int)Math.Max(ratio * Parent.sdlmmControl1.Width, 30);
                    scrollBarRect = new Rectangle(0, Parent.sdlmmControl1.Height - heightOfScroll, rectWidth, heightOfScroll);
                }
                else
                {
                    HasScrollBar = false;
                    scrollBarRect = null;
                }
            }
            volatile int currentPos = 0;
            List<float> Buffer = new List<float>();
            List<double> avgBuf = new List<double>();
            double wholeAverage = 0;
            public double maximum = 0;
            public double minimum = 0;
            public void AverageBuffer(List<double> avgBuf, List<float> buf, int pixelsPerSecond)
            {
                if (buf.Count > 0)
                {
                    wholeAverage /= buf.Count;
                }
                avgBuf.Clear();
                double sum = 0;
                int countPerAvg = (int)((Parent.reader.WaveFormat.SampleRate * Parent.reader.WaveFormat.Channels) / pixelsPerSecond);
                wholeAverage = 0;
                for (int i = 0; i < buf.Count; ++i)
                {
                    wholeAverage += Math.Abs(buf[i]);
                    maximum = Math.Max(Math.Abs(buf[i]), maximum);
                    minimum = Math.Min(Math.Abs(buf[i]), maximum);
                }
                for (int i = 0; i < buf.Count; ++i)
                {
                    sum = Math.Max(sum, Math.Abs(buf[i]));
                    if (i > 0 && (i % countPerAvg == 0))
                    {
                        //sum = (sum / countPerAvg)/(maximum-minimum);
                        avgBuf.Add(sum);
                        sum = 0;
                    }
                }

            }
            public override void MouseClick(int x, int y, int btn, bool inon)
            {
                if (inon)
                {
                    if (HasScrollBar && scrollBarRect != null && IsScrollBarClicked(x, y))
                    {
                        clickXOffset = x - scrollBarRect.Value.Left;
                        IsScrollDragging = true;
                    }
                }
                else
                {
                    IsScrollDragging = false;
                }
            }
            String strMousePos;
            SizeF strMousePosSize;
            public override void MouseMove(int x, int y, int btn, bool inon)
            {
                mouseX = x + scrollX;
                if (avgBuf != null)
                {
                    if (mouseX < 0) mouseX = 0;
                    if (mouseX >= avgBuf.Count) mouseX = avgBuf.Count - 1;

                    strMousePos = String.Format("{0}({1}~{2})", avgBuf[mouseX], minimum, maximum);
                    strMousePosSize = Parent.sdlmmControl1.MeasureString(strMousePos);
                }
                if (inon)
                {
                    if (HasScrollBar && scrollBarRect != null && this.IsScrollDragging)
                    {
                        int deltaX = x - clickXOffset;
                        if (deltaX > 0)
                        {
                            if (scrollBarRect.Value.Width + deltaX < Parent.sdlmmControl1.Right)
                            {
                                scrollBarRect = new Rectangle(deltaX, scrollBarRect.Value.Top, scrollBarRect.Value.Width, scrollBarRect.Value.Height);
                            }
                            else
                            {
                                scrollBarRect = new Rectangle(Parent.sdlmmControl1.Right - scrollBarRect.Value.Width, scrollBarRect.Value.Top, scrollBarRect.Value.Width, scrollBarRect.Value.Height);
                            }
                        }
                        else
                        {
                            if (deltaX + scrollBarRect.Value.Left > 0)
                            {
                                scrollBarRect = new Rectangle(deltaX, scrollBarRect.Value.Top, scrollBarRect.Value.Width, scrollBarRect.Value.Height);
                            }
                            else
                            {
                                scrollBarRect = new Rectangle(0, scrollBarRect.Value.Top, scrollBarRect.Value.Width, scrollBarRect.Value.Height);
                            }
                        }
                        scrollX = (int)(scrollBarRect.Value.Left / ScrollRatio);
                        if (scrollX < 0) scrollX = 0;
                    }
                }
                else
                {
                    IsScrollDragging = false;
                }
            }
            public void SetPos(int c)
            {
                currentPos = c;
            }
            public void setPixelsPerSecond(int i)
            {
                this.pixelsPerSecond = i;
                AverageBuffer(avgBuf, this.Buffer, i);
            }
            public void SetBuffer(SoundFileBuffer buf)
            {
                this.Buffer = new List<float>();
                this.Buffer.AddRange(buf.GetRange(0, (int) buf.Count));
                AverageBuffer(avgBuf, this.Buffer, pixelsPerSecond);
                // Parent.sdlmmControl1.Width = Math.Max(avgBuf.Count, Parent.Width);
                GenerateScrollBar();
            }
            public override void Draw()
            {
                if (Parent.sdlmmControl1.Width != CurrentWidth || CurrentHeight != Parent.sdlmmControl1.Height)
                {
                    CurrentWidth = Parent.sdlmmControl1.Width;
                    CurrentHeight = Parent.sdlmmControl1.Height;
                    GenerateScrollBar();
                    return;
                }

                Parent.sdlmmControl1.Clear(unchecked((int)0xff0000ff));
                int width = Parent.sdlmmControl1.Width;
                int start = scrollX;
                int end = scrollX + width;
                if (end >= avgBuf.Count)
                {
                    start -= end - avgBuf.Count;
                    end = avgBuf.Count;
                }
                for (int i = 0; i < width; ++i)
                {
                    int iPos = i + scrollX;
                    if (iPos >= avgBuf.Count) iPos = avgBuf.Count - 1;
                    double height = avgBuf[iPos] * Parent.sdlmmControl1.Height;
                    if (iPos % pixelsPerSecond == 0)
                    {
                        String stringTodraw = (iPos / pixelsPerSecond).ToString() + "s";
                        Parent.sdlmmControl1.drawLine(i, 0, i, Parent.sdlmmControl1.Height, unchecked((int)0xffcccccc));
                        Parent.sdlmmControl1.drawString(stringTodraw, i, 0, unchecked((int)0xffffffff));
                    }
                    Parent.sdlmmControl1.drawLine(i, Parent.sdlmmControl1.Height - (int)height, i, Parent.sdlmmControl1.Height - heightOfScroll, unchecked((int)0xffffff00));
                }
                int currentPosLineX = (currentPos / pixelsPerSecond) - scrollX;
                if (currentPosLineX >= 0 && currentPosLineX < Parent.sdlmmControl1.Width)
                {
                    Parent.sdlmmControl1.drawLine(currentPosLineX, 0, currentPosLineX, Parent.sdlmmControl1.Height - heightOfScroll, unchecked((int)0xff00ff00));
                }




                Parent.sdlmmControl1.drawLine(mouseX - scrollX, 0, mouseX - scrollX, Parent.sdlmmControl1.Height - heightOfScroll, unchecked((int)0xffff00ff));

                if (mouseX - scrollX + (int)strMousePosSize.Width < Parent.sdlmmControl1.Width)
                {
                    Parent.sdlmmControl1.fillRect(mouseX - scrollX, Parent.sdlmmControl1.Height / 2, (int)strMousePosSize.Width, (int)strMousePosSize.Height, unchecked((int)0xffffffff));
                    Parent.sdlmmControl1.drawString(strMousePos, mouseX - scrollX, Parent.sdlmmControl1.Height / 2, unchecked((int)0xff000000));
                }
                else
                {
                    Parent.sdlmmControl1.fillRect(mouseX - (int)strMousePosSize.Width, Parent.sdlmmControl1.Height / 2, (int)strMousePosSize.Width, (int)strMousePosSize.Height, unchecked((int)0xffffffff));
                    Parent.sdlmmControl1.drawString(strMousePos, mouseX - (int)strMousePosSize.Width, Parent.sdlmmControl1.Height / 2, unchecked((int)0xff000000));
                }
                if (Parent.dataGridView1.SelectedRows.Count > 0 && Parent.dataGridView1.SelectedRows[0].Cells[0].RowIndex < Parent.FileNameDoc.WaveSplitOptionLines.Count)
                {
                    WaveOptions opt = Parent.FileNameDoc.WaveSplitOptionLines[Parent.dataGridView1.SelectedRows[0].Cells[0].RowIndex];
                    int dy = Parent.sdlmmControl1.Height - heightOfScroll - (int)(Parent.sdlmmControl1.Height * opt.CutVolume);
                    Parent.sdlmmControl1.drawLine(0, dy, Parent.sdlmmControl1.Width, dy, unchecked((int)0xffcccccc));
                }
                if (HasScrollBar)
                {
                    Parent.sdlmmControl1.fillRect(0, scrollBarRect.Value.Top, Parent.sdlmmControl1.Width, scrollBarRect.Value.Height, unchecked((int)0xffaaaaaa));
                    Parent.sdlmmControl1.fillRect(scrollBarRect.Value.Left, scrollBarRect.Value.Top, scrollBarRect.Value.Width, scrollBarRect.Value.Height, unchecked((int)0xffcccccc));
                    Parent.sdlmmControl1.drawRect(scrollBarRect.Value.Left, scrollBarRect.Value.Top, scrollBarRect.Value.Width, scrollBarRect.Value.Height, unchecked((int)0xffffffff));
                }
                Parent.sdlmmControl1.flush();
            }
            public WaveBufferDrawer(WavSplitForm parent)
                : base(parent)
            {

            }
        }

        DrawingContextNone noneDrawingContext = null;
        DrawingContext emptyDrawingContext = null;
        DrawingContext drawingScene = null;
        WaveBufferDrawer waveDrawer = null;
        private void switchToDrawWaveScene(DrawingContext ctx)
        {
            if (checkBox1.Checked)
            {
                drawingScene = ctx;
            }
        }
        void onClick(int x, int y, int btn, bool ison)
        {
            if (drawingScene != null)
            {
                drawingScene.MouseClick(x,y,btn,ison);
            }
        }
        void onMove(int x, int y, int btn, bool ison)
        {
            if (drawingScene != null)
            {
                drawingScene.MouseMove(x, y, btn, ison);
            }
        }
#endif
        public WavSplitForm()
        {
            InitializeComponent();
            
#if WAVEDRAWER_ENABLE
            checkBox1.Visible = true;
            splitContainer1.SplitterDistance=80;
            sdlmmControl1.Visible=true;
            emptyDrawingContext =  new DrawingContext(this);
            waveDrawer = new WaveBufferDrawer(this);
            sdlmmControl1.onMouseClickHandler = onClick;
            sdlmmControl1.onMouseMoveHandler = onMove;
            sdlmmControl1.setUseAlpha(false);
            drawingScene = emptyDrawingContext;
            noneDrawingContext = new DrawingContextNone(this);
#endif
        }

        private void WavSplitForm_Load(object sender, EventArgs e)
        {
#if WAVEDRAWER_ENABLE
            sdlTimer.Tick += sdlTimer_Tick;
            sdlTimer.Start();
#endif
        }

        void sdlTimer_Tick(object sender, EventArgs e)
        {
#if WAVEDRAWER_ENABLE
            sdlTimer.Stop();
            if (drawingScene != null)
            {
                drawingScene.Draw();
            }
            sdlTimer.Start();
#endif
        }
        class OnFinishJob
        {
            public virtual bool run()
            {
                return true;
            }
        }
        class DeleteOnFinishJob : OnFinishJob
        {
            List<String> filenames = new List<string>();
            public DeleteOnFinishJob(params String[] _filenames)
            {
                filenames.AddRange(_filenames);
            }
            public override bool run()
            {
                List<String> success = new List<string>();
                bool retval = true;
                foreach(String s in filenames)
                {
                    try
                    {
                        File.Delete(s);
                        success.Add(s);
                    }
                    catch (Exception ee)
                    {
                        retval = false;
                        Console.WriteLine(ee.ToString());
                    }
                }
                foreach (String s in success)
                {
                    filenames.Remove(s);
                }
                return retval;
            }
        }
        List<DeleteOnFinishJob> onFinishJobQueue = new List<DeleteOnFinishJob>();
        private void openSoundFileDialog()
        {
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Filter = "Supported Sound File(*.wav;*.mp3)|*.wav;*.mp3|Wave File(*.wav)|*.wav|MP3 File(*.mp3)|*.mp3";
            if (openFile.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                soundFileName = openFile.FileName;
                textBox1.Text = soundFileName;
                if (soundFileName.EndsWith(".mp3"))
                {
                    IsMP3 = true;
                    BackgroundWorker bgworkerMP3Wav = new BackgroundWorker();
                    bgworkerMP3Wav.DoWork += bgworkerMP3Wav_DoWork;
                    bgworkerMP3Wav.RunWorkerCompleted += audioLoader_RunWorkerCompleted;
                    bgworkerMP3Wav.RunWorkerAsync();
                    
                }
                else
                {
                    IsMP3 = false;
                    BackgroundWorker wavLoader = new BackgroundWorker();
                    wavLoader.DoWork += wavLoader_DoWork;
                    wavLoader.RunWorkerCompleted += audioLoader_RunWorkerCompleted;
                    wavLoader.RunWorkerAsync();
                }

               
            }
        }

        void audioLoader_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            IsReady = true;
#if WAVEDRAWER_ENABLE
            if (soundBuffer.Count > 512L * 1024 * 1024)
            {
                MessageBox.Show("由於聲音檔案取樣數量超過上限，故關閉顯示波形圖", "關閉波型圖通知", MessageBoxButtons.OK, MessageBoxIcon.Information);
                canDrawWave = false;
                drawingScene = this.emptyDrawingContext;
                sdlmmControl1.Visible = false;
                sdlTimer.Stop();
                checkBox1.Checked = false;
                checkBox1.Enabled = false;
                splitContainer1.SplitterDistance = 0;
            }
            else
            {
                
                canDrawWave = true;
                checkBox1.Checked = true;
                checkBox1.Enabled = true;
                sdlTimer.Start();
                sdlmmControl1.Visible = true;
                splitContainer1.SplitterDistance = 200;
                sdlmmControl1.setUseAlpha(true);
                switchToDrawWaveScene(waveDrawer);
                if (soundBuffer.Count * 4 > Process.GetCurrentProcess().PrivateMemorySize64)
                {
                    if (MessageBox.Show("顯示波型圖需要的記憶體空間將耗費大量的時間，確定顯示嗎?按[是]關閉波型圖", "超過可用實體記憶體", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
                    {
                        checkBox1.Checked = false;
                    }
                    else
                    {
                        checkBox1.Checked = true;
                    }
                }
                else
                {
                    checkBox1.Checked = true;
                }
                if (checkBox1.Checked)
                {
                    prepareSoundDrawingBuf();
                }
            }
            
            
#endif
        }

        void wavLoader_DoWork(object sender, DoWorkEventArgs e)
        {
            this.reader = new AudioFileReader(soundFileName);
            soundBuffer = new SoundFileBuffer(reader);
        }

        void bgworkerMP3Wav_DoWork(object sender, DoWorkEventArgs e)
        {
            /*String newSoundFileName = "__MP3ToWav__File" + ".wav";
            ConvertMP3ToWav(soundFileName, newSoundFileName);
            onFinishJobQueue.Add(new DeleteOnFinishJob(newSoundFileName));
            this.reader = new AudioFileReader(newSoundFileName); */
            this.reader = new AudioFileReader(soundFileName);
            soundBuffer = new SoundFileBuffer(reader);
        }
        private void button1_Click(object sender, EventArgs e)
        {
            openSoundFileDialog();
        }

        private void textBox1_Click(object sender, EventArgs e)
        {
            //openSoundFileDialog();
        }
        private DataGridViewTextBoxCell textCell(String s)
        {
            DataGridViewTextBoxCell ret = new DataGridViewTextBoxCell();
            ret.Value = s;
            return ret;
        }
        private void addDataRow(WaveOptions o)
        {
            DataGridViewRow row = new DataGridViewRow();
            row.Cells.Add(textCell(o.filename));
            row.Cells.Add(textCell(o.CutVolume.ToString()));
            row.Cells.Add(textCell(o.CutMuteDuration.ToString()));
            row.Cells.Add(textCell(o.MuteDuration.ToString()));
            row.Cells.Add(textCell(o.AddHead.ToString()));
            row.Cells.Add(textCell(o.AddEnd.ToString()));
            dataGridView1.Rows.Add(row);
        }
        volatile bool parserWorkDone = false;
        private void openTextFileDialog()
        {
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Filter = "Text File(*.txt)|*.txt";
            if (openFile.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                txtFile = openFile.FileName;
                textBox2.Text = openFile.FileName;
                FileNameDoc = new FileNameTableDocument(openFile.FileName);
                BackgroundWorker parserWork = new BackgroundWorker();
                parserWork.DoWork += parserWork_DoWork;
                parserWork.RunWorkerCompleted += parserWork_RunWorkerCompleted;
                progressBar1.Visible = true;
                button3.Enabled = false;
                parserWorkDone = false;
                parserWork.RunWorkerAsync();
                while (!parserWorkDone)
                {
                    if (this.Visible == false)
                    {
                        parserWork.CancelAsync();
                        break;
                    }
                    Thread.Sleep(100);
                    Application.DoEvents();
                    setProgress(((double)FileNameDoc.CurrentProgress) / FileNameDoc.MaxProgress);
                }
                button3.Enabled = true;
            }
        }

        void parserWork_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            setProgress(0);
            label1.Text = "0%";
            parserWorkDone = true;
            progressBar1.Visible = false;
            dataGridView1.Rows.Clear();
            foreach (WaveOptions o in FileNameDoc.WaveSplitOptionLines)
            {
                //String txt = o.ToFinalString();
                //listBox1.Items.Add(txt);
                addDataRow(o);
            }
        }

        void parserWork_DoWork(object sender, DoWorkEventArgs e)
        {
            FileNameDoc.Parse();
        }
        private void button2_Click(object sender, EventArgs e)
        {
            openTextFileDialog();
        }

        private void textBox2_Click(object sender, EventArgs e)
        {
           // openTextFileDialog();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            wordCount.Clear();
            if (String.IsNullOrEmpty(soundFileName))
            {
                MessageBox.Show("聲音檔不可以是空的", "無法開啟檔案", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (String.IsNullOrEmpty(txtFile))
            {
                MessageBox.Show("切音檔案不可以是空的", "無法開啟檔案", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (!File.Exists(soundFileName))
            {
                MessageBox.Show("聲音檔" + soundFileName + "不存在", "無法開啟檔案", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (!File.Exists(txtFile))
            {
                MessageBox.Show("切音檔" + txtFile + "不存在", "無法開啟檔案", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (String.IsNullOrEmpty(outputFolder))
            {
                MessageBox.Show("資料夾不可以是空的", "請選擇資料夾", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (!Directory.Exists(outputFolder))
            {
                MessageBox.Show("資料夾不存在", "請選擇正確的資料夾", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            
            BackgroundWorker bgworker = new BackgroundWorker();

            bool ONOFF = false;
            button1.Enabled = ONOFF;
            button2.Enabled = ONOFF;
            button3.Enabled = ONOFF;
            button4.Enabled = ONOFF;
            textBox1.Enabled = ONOFF;
            textBox2.Enabled = ONOFF;
            textBox3.Enabled = ONOFF;
            button5.Visible = !ONOFF;
            progressBar1.Visible = !ONOFF;
            terminateFired = false;
            bgworker.DoWork += bgworker_DoWork;
            bgworker.RunWorkerCompleted += bgworker_RunWorkerCompleted;
            bgworker.RunWorkerAsync();
        }

        void bgworker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (terminateFired)
            {
                MessageBox.Show("已取消切音", "終止", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                setProgress(1.0);
                MessageBox.Show("切音完畢", "完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            terminateFired = false;
            bool ONOFF = true;
            button1.Enabled = ONOFF;
            button2.Enabled = ONOFF;
            button3.Enabled = ONOFF;
            button4.Enabled = ONOFF;
            textBox1.Enabled = ONOFF;
            textBox2.Enabled = ONOFF;
            textBox3.Enabled = ONOFF;
            button5.Visible = !ONOFF;
            progressBar1.Visible = !ONOFF;
            label1.Text = "0%";
            performFinishJob();
        }
        List<Process> aliveProcesses = new List<Process>();
        
        private void ConvertWavToMP3_2(String inputFileName,String outputFileName)
        {
            using (var wavreader = new MediaFoundationReader(inputFileName))
            {
                try
                {
                    MediaFoundationEncoder.EncodeToMp3(wavreader,
                            outputFileName, 44100);
                }
                catch (Exception ee)
                {
                    Console.WriteLine(ee.ToString());
                }
            } 
        }
        private void ConvertWavToMP3(String inputFileName, String outputFileName)
        {
            String path = Program.ApplicationStartPath + "ffmpeg.exe";
            String arg = String.Format("-i \"{0}\" -y -vn -ar 44100 -ac {1} -ab 128k -f mp3 \"{2}\" ", inputFileName, reader.WaveFormat.Channels, outputFileName);
            Console.WriteLine(path + " " + arg);
            ProcessStartInfo info = new ProcessStartInfo(path, arg);
            info.WindowStyle = ProcessWindowStyle.Hidden;
            Process p = Process.Start(info);
            p.WaitForExit();
        }
        private List<float> prepareFinalBuffer(SoundFileBuffer buffer, WaveOptions w, long soundStartIdx, long iEndBoundStart)
        {
            long startMuteLen = (int)(w.AddHead * reader.WaveFormat.Channels * reader.WaveFormat.SampleRate);
            long endMuteLen = (int)(w.AddEnd * reader.WaveFormat.Channels * reader.WaveFormat.SampleRate);
            long paddingStart = 0;
            long paddingEnd = 0;
            long origLen = iEndBoundStart - soundStartIdx + 1;
            if (startMuteLen > 0)
            {
                soundStartIdx -= startMuteLen;
                if (soundStartIdx < 0)
                {
                    paddingStart = startMuteLen - soundStartIdx;
                    soundStartIdx = 0;
                }
            }
            if (endMuteLen > 0)
            {
                iEndBoundStart += endMuteLen;
                if (iEndBoundStart >= buffer.Count)
                {
                    paddingEnd = endMuteLen - (buffer.Count - iEndBoundStart);
                    iEndBoundStart = buffer.Count - 1;
                }
            }
            if (String.IsNullOrEmpty(w.filename))
            {
                w.filename = "unnamed-" + unnamedFileCount;
                ++unnamedFileCount;
            }

            List<float> finalBuffer = new List<float>();
            if (paddingStart > 0)
            {
                float[] padding = new float[paddingStart];
                finalBuffer.AddRange(padding);
            }
            finalBuffer.AddRange(buffer.GetRange((int)soundStartIdx, (int)(iEndBoundStart - soundStartIdx)));
            int origEnd = finalBuffer.Count;
            if (paddingEnd > 0)
            {
                float[] padding = new float[paddingEnd];
                finalBuffer.AddRange(padding);
            }
            float ratio = 0;
            float delta = 0;
            if (startMuteLen > 0)
            {
                float currentMax = 0;
                delta = 1.0f / startMuteLen;
                for (int i = 0; i < startMuteLen; ++i)
                {
#if false
                    // this mechanism will amplier noise louder
                    bool over = false;
                    float sign = 1.0f;
                    if (finalBuffer[i] < 0) sign = -1.0f;
                    if (finalBuffer[i] * sign > currentMax)
                    {
                        currentMax = finalBuffer[i];
                        over = true;
                    }
                    if (over)
                    {
                        finalBuffer[i] = currentMax * sign * ratio;
                    }
                    else
                    {
                        finalBuffer[i] *= ratio;
                    }
#endif
                    finalBuffer[i] *= ratio;
                    ratio += delta;
                    if (ratio > 1)
                    {
                        ratio = 1;
                    }
                }
            }

            if (endMuteLen > 0)
            {
                delta = 1.0f / endMuteLen;
                ratio = 1.0f;
                float currentMin = 1;
                for (long i = startMuteLen + origLen; i < origEnd + paddingEnd; ++i)
                {
#if false
                    Boolean over = false;
                    float sign = 1.0f;
                    if (finalBuffer[i] < 0) sign = -1.0f;
                    if (finalBuffer[i] * sign > currentMin)
                    {
                        currentMin = finalBuffer[i];
                        over = true;
                    }
                    if (over)
                    {
                        finalBuffer[i] = currentMin * sign * ratio;
                    }
                    else
                    {
                        finalBuffer[i] *= ratio;
                    }
#endif
                    finalBuffer[(int)i] *= ratio;
                    ratio -= delta;
                    if (ratio < 0)
                    {
                        ratio = 0;
                    }
                }
            }
            if (w.MuteDuration > 0)
            {
                int mutelen = (int)(w.MuteDuration * reader.WaveFormat.Channels * reader.WaveFormat.SampleRate);
                float[] padding = new float[mutelen];
                finalBuffer.AddRange(padding);
            }
            return finalBuffer;
        }
        void onBufferMatch2(List<OutputRequest> reqs)
        {
            List<float> buf1 = prepareFinalBuffer(reqs[0].soundBuffer, reqs[0].option, reqs[0].start, reqs[0].end);
            if (reqs.Count > 1)
            {
                List<float> buf2 = prepareFinalBuffer(reqs[1].soundBuffer, reqs[1].option, reqs[1].start, reqs[1].end);
                buf1.AddRange(buf2);
                buf2.Clear(); buf2 = null;
            }
            outputSoundFile(buf1, reqs[0].option);
        }
        Dictionary<String, int> wordCount = new Dictionary<string, int>();
        private void outputSoundFile(List<float> finalBuffer, WaveOptions w)
        {
            String outputDir = FileNameDoc.OutputDirectory;
            if (!String.IsNullOrEmpty(outputFolder))
            {
                outputDir = outputFolder;
            }
            String finalFileName = w.filename;
            
            String firstFinalFileName = finalFileName;
            String surfix = "";
            long cnt = 0;
            if (!wordCount.ContainsKey(firstFinalFileName))
            {
                surfix = "";
                wordCount[firstFinalFileName] = 1;
            }
            else
            {
                wordCount[firstFinalFileName] = wordCount[firstFinalFileName]+1;
                surfix = wordCount[firstFinalFileName].ToString();
            }
            finalFileName = firstFinalFileName + surfix;
            if (IsMP3)
            {
                /*
                while (File.Exists(outputDir + "\\" + finalFileName + ".wav") || File.Exists(outputDir + "\\" + finalFileName + ".mp3"))
                {
                    ++cnt;
                    surfix = cnt.ToString();
                    finalFileName = firstFinalFileName + surfix;
                }*/
                if (File.Exists(outputDir + "\\" + finalFileName + ".mp3"))
                {
                    File.Delete(outputDir + "\\" + finalFileName + ".mp3");
                }
            }
            else
            {
                /*while (File.Exists(outputDir + "\\" + finalFileName + ".wav"))
                {
                    ++cnt;
                    surfix = cnt.ToString();
                    finalFileName = firstFinalFileName + surfix;
                }*/
                if (File.Exists(outputDir + "\\" + finalFileName + ".wav"))
                {
                    File.Delete(outputDir + "\\" + finalFileName + ".wav");
                }
            }
            String fileName = "";
            
            if (IsMP3)
            {
                fileName = outputDir + "\\" + Guid.NewGuid().ToString() + ".wav";
            }
            else
            {
                fileName = outputDir + "\\" + finalFileName + ".wav";
            }
            FileStream fw = new FileStream(fileName, FileMode.Create, FileAccess.Write);
            WaveFileWriter writer = new WaveFileWriter(fw, reader.WaveFormat);
            writer.WriteSamples(finalBuffer.ToArray(), 0, finalBuffer.Count);
            writer.Flush();
            writer.Close();
            if (IsMP3)
            {
                ConvertWavToMP3(fileName, outputDir + "\\" + finalFileName + ".mp3");
                onFinishJobQueue.Add(new DeleteOnFinishJob(fileName));
            }
        }
        void onBufferMatch(SoundFileBuffer buffer, WaveOptions w, long soundStartIdx, long iEndBoundStart)
        {
            List<float> finalBuffer = prepareFinalBuffer(buffer, w, soundStartIdx, iEndBoundStart);
            outputSoundFile(finalBuffer, w);
        }
        SoundFileBuffer soundBuffer;
        public class SoundFileBuffer
        {
            AudioFileReader reader;
            long bufferOffset = 0;
            float[] buffer;
            public SoundFileBuffer(AudioFileReader reader)
            {
                this.reader = reader;
                buffer = new float[reader.WaveFormat.Channels*reader.WaveFormat.SampleRate * 120];
                long Length = Math.Min((int)buffer.Length, (int)(reader.Length/4 ));
                int readCnt = reader.Read(buffer, 0,(int) Length);
                if (readCnt >= 0)
                {
                    return;
                }
                else
                {
                    return;
                }
            }
            bool IdxInRange(long idx,long begin, long length)
            {
                return idx >= begin && idx < begin + length;
            }
            public float this[long idx]
            {
                get
                {
                    float ret = 0;
                    if (!IdxInRange(idx,bufferOffset,buffer.LongLength))
                    {
                        
                        bufferOffset = idx;
                        
                        reader.Position = bufferOffset*4;
                        reader.Read(buffer, 0, (int)buffer.LongLength);
                    }
                    ret = buffer[idx -bufferOffset];
                    return ret;
                }
            }
            public List<float> GetRange(int start, int count)
            {
                List<float> ret = new List<float>();
                float[] buf = new float[count];
                for (int i = 0; i < count; ++i)
                {
                    buf[i] = this[start + i];
                }
                ret.AddRange(buf);
                return ret;
            }
            public long Count
            {
                get
                {
                    return reader.Length/4;
                }
            }
        }
        private delegate void setProgressDelegate(double d);
        private void setProgress(double d)
        {
            if (InvokeRequired)
            {
                Invoke(new setProgressDelegate(setProgress), d);
                return;
            }
            label1.Text = String.Format("{0:0.00}%",(d * 100));
            int iVal = (int)Math.Floor(100 * d);
            if (iVal < 0) iVal = 0;
            if (iVal > 100) iVal = 100;
            progressBar1.Value = iVal;
        }
        private void prepareSoundDrawingBuf()
        {
#if WAVEDRAWER_ENABLE
            AudioFileReader reader = new AudioFileReader(soundFileName);
            soundBuffer = new SoundFileBuffer(reader);
            waveDrawer.SetBuffer(soundBuffer);
            soundDrawingPrepared = true;
#endif
        }
        double sampleToDB(float sample)
        {
            return  20 * Math.Log10(sample);
        }
        int unnamedFileCount = 0;
        public class OutputRequest
        {
            public SoundFileBuffer soundBuffer;
            public WaveOptions option;
            public long start;
            public long end;
            public OutputRequest(SoundFileBuffer soundBuffer, WaveOptions option, long start, long end)
            {
                this.soundBuffer = soundBuffer;
                this.option = option;
                this.start = start;
                this.end = end;
            }
        }
        List<OutputRequest> outputRequestQueue = new List<OutputRequest>();
        void bgworker_DoWork(object sender, DoWorkEventArgs e)
        {
            Thread.CurrentThread.Name = "Sound-Splitter";
            FileStream waveStream = new FileStream(soundFileName, FileMode.Open, FileAccess.Read);

            
            int idx = 0;
            WaveOptions option = FileNameDoc.WaveSplitOptionLines[idx];
            Boolean matchStart = false;
            long iEndBoundStart = -1;
            int rawIdx = 0;
            long soundStartIdx = -1;
            terminateFired = false;
#if false
            for (int i = 0; i < soundBuffer.Count; i+=reader.WaveFormat.Channels)
            {
                float input = soundBuffer[i];
                float absinput = (float)Math.Abs(input);
                waveDrawer.SetPos(rawIdx);
                ++rawIdx;
                
                if (!matchStart)
                {
                    if (absinput > Math.Abs(option.CutVolume))
                    {
                        matchStart = true;
                        soundStartIdx = i;
                        iEndBoundStart = -1;
                        continue;
                    }
                }
                else
                {
                    if (absinput <= Math.Abs(option.CutVolume))
                    {
                        if (iEndBoundStart == -1)
                        {
                            iEndBoundStart = i;
                        }

                    }
                    else
                    {

                        if (iEndBoundStart != -1)
                        {
                            if (i - iEndBoundStart > option.CutMuteDuration * reader.WaveFormat.SampleRate * reader.WaveFormat.Channels)
                            {
                                // found one range 
                                if (FileNameDoc.auto2file)
                                {
                                    if (option.isLineHead && outputRequestQueue.Count == 1 && outputRequestQueue[0].option.isLineHead)
                                    {
                                        // flush, next line arrived
                                        OutputRequest outputReq = outputRequestQueue[0];
                                        outputRequestQueue.Clear();
                                        onBufferMatch(outputReq.soundBuffer, outputReq.option, outputReq.start, outputReq.end);
                                    }
                                    outputRequestQueue.Add(new OutputRequest(soundBuffer, option, soundStartIdx, iEndBoundStart));
                                    if (outputRequestQueue.Count >= 2)
                                    {
                                        // flush
                                        onBufferMatch2(outputRequestQueue);
                                        outputRequestQueue.Clear();
                                    }
                                }
                                else
                                {
                                    onBufferMatch(soundBuffer, option, soundStartIdx, iEndBoundStart);
                                }
                                soundStartIdx = i;
                                iEndBoundStart = -1;
                                if (idx + 1 < FileNameDoc.WaveSplitOptionLines.Count)
                                {
                                    ++idx;
                                    changeListBox(idx);
                                    option = FileNameDoc.WaveSplitOptionLines[idx];
                                }
                                else
                                {
                                    option = FileNameDoc.DefaultWaveOption();
                                }
                                continue;
                            }
                            else
                            {
                                iEndBoundStart = -1;
                            }
                        }
                    }                   
                }
            }
#else
            DateTime dtNow = DateTime.Now;
            for (long i = 0; i < soundBuffer.Count; i += reader.WaveFormat.Channels)
            {
                if (terminateFired)
                {
                    break;
                }
                if (DateTime.Now.Subtract(dtNow).TotalSeconds > 1)
                {
                    setProgress(((double)i) / soundBuffer.Count);
                    dtNow = DateTime.Now;
                }
                float input = soundBuffer[i];
                float absinput = (float)Math.Abs(input);
#if WAVEDRAWER_ENABLE
                if (canDrawWave && checkBox1.Checked)
                {
                    waveDrawer.SetPos(rawIdx);
                }
#endif
                ++rawIdx;

                if (!matchStart)
                {
                    if (absinput > Math.Abs(option.CutVolume))
                    {
                        matchStart = true;
                        soundStartIdx = i;
                        iEndBoundStart = -1;
                        continue;
                    }
                }
                else
                {
                    if (absinput < Math.Abs(option.CutVolume))
                    {
                        // if lower volume continues, then it is split point
                        if (iEndBoundStart == -1)
                        {
                            iEndBoundStart = i;

                            for (long j = iEndBoundStart; j < soundBuffer.Count; j += reader.WaveFormat.Channels)
                            {
                                input = soundBuffer[j];
                                absinput = (float)Math.Abs(input);
                                if (canDrawWave && checkBox1.Checked)
                                {
                                    waveDrawer.SetPos(rawIdx);
                                }
                                ++rawIdx;
                                if (absinput < Math.Abs(option.CutVolume))
                                {
                                    if (j - iEndBoundStart > option.CutMuteDuration * reader.WaveFormat.SampleRate * reader.WaveFormat.Channels)
                                    {
                                        // found one range 
                                        if (FileNameDoc.auto2file)
                                        {
                                            if (option.isLineHead && outputRequestQueue.Count == 1 && outputRequestQueue[0].option.isLineHead)
                                            {
                                                // flush, next line arrived
                                                OutputRequest outputReq = outputRequestQueue[0];
                                                outputRequestQueue.Clear();
                                                onBufferMatch(outputReq.soundBuffer, outputReq.option, outputReq.start, outputReq.end);
                                            }
                                            outputRequestQueue.Add(new OutputRequest(soundBuffer, option, soundStartIdx, iEndBoundStart));
                                            if (outputRequestQueue.Count >= 2)
                                            {
                                                // flush
                                                onBufferMatch2(outputRequestQueue);
                                                outputRequestQueue.Clear();
                                            }
                                        }
                                        else
                                        {
                                            onBufferMatch(soundBuffer, option, soundStartIdx, iEndBoundStart);
                                        }
                                        matchStart = false;
                                        soundStartIdx = -1;
                                        iEndBoundStart = -1;
                                        if (idx + 1 < FileNameDoc.WaveSplitOptionLines.Count)
                                        {
                                            ++idx;
                                            changeListBox(idx);
                                            option = FileNameDoc.WaveSplitOptionLines[idx];
                                        }
                                        else
                                        {
                                            option = FileNameDoc.DefaultWaveOption();
                                        }
                                        i = j;
                                        break;
                                    }
                                }
                                else
                                {
                                    if (absinput >= Math.Abs(option.CutVolume))
                                    {
                                        iEndBoundStart = -1;
                                        break;
                                    }
                                }
                            }

                        }
                    }
                }
            }
#endif
            if (soundStartIdx != -1 && iEndBoundStart != -1 && iEndBoundStart > soundStartIdx)
            {
                // at end, but stream has arrive end of file
                // found one range 
                onBufferMatch(soundBuffer, option, soundStartIdx, iEndBoundStart);
            }
            else if (soundStartIdx != -1)
            {
                // at end, but stream has arrive end of file
                onBufferMatch(soundBuffer, option, soundStartIdx, soundBuffer.Count);
            }
            if (FileNameDoc.auto2file)
            {
                if (outputRequestQueue.Count != 0)
                {
                    // flush
                    onBufferMatch2(outputRequestQueue);
                    outputRequestQueue.Clear();
                }
            }
            waveStream.Close();
        }
        delegate void changeListboxDelegate(int i);
        private void changeListBox(int i)
        {
            if (InvokeRequired)
            {
                Invoke(new changeListboxDelegate(changeListBox), i);
                return;
            }
            if (i < dataGridView1.Rows.Count)
            {
                dataGridView1.Rows[i].Selected = true;
            }
        }
        private void performFinishJob()
        {
            foreach (Process p in aliveProcesses)
            {
                try
                {
                    if (Process.GetProcessById(p.Id) != null)
                    {
                        p.Kill();
                    }
                }
                catch (Exception ee)
                {
                    Console.WriteLine(ee.ToString());
                }
            }
            aliveProcesses.Clear();
            List<DeleteOnFinishJob> toDel = new List<DeleteOnFinishJob>();
            foreach (DeleteOnFinishJob o in onFinishJobQueue)
            {
                if(o.run())
                {
                    toDel.Add(o);
                }
            }
            foreach (DeleteOnFinishJob o in toDel)
            {
                onFinishJobQueue.Remove(o);
            }
        }
        private void WavSplitForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            sdlTimer.Stop();
            performFinishJob();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
#if WAVEDRAWER_ENABLE
            if(checkBox1.Checked)
            {
                sdlTimer.Start();
                sdlTimer.Enabled = true;
                sdlmmControl1.Visible = true;
                if (IsReady)
                {
                    if (!soundDrawingPrepared)
                    {
                        prepareSoundDrawingBuf();
                    }
                    drawingScene = waveDrawer;
                    
                }
                else
                {
                    drawingScene = emptyDrawingContext;
                }
            }
            else
            {
                sdlTimer.Stop();
                sdlTimer.Enabled = false;
                sdlmmControl1.Visible = false;
                drawingScene = noneDrawingContext;
            }
#endif
        }

        String GetFolderByBrowser()
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            if (String.IsNullOrWhiteSpace(textBox3.Text))
            {
                dialog.SelectedPath = Environment.CurrentDirectory;
            }
            else
            {
                if (Directory.Exists(textBox3.Text))
                {
                    dialog.SelectedPath = textBox3.Text;
                }
                else
                {
                    dialog.SelectedPath = Environment.CurrentDirectory;
                }
            }
            DialogResult result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                outputFolder = dialog.SelectedPath;
                return outputFolder;
            }
            return null;
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (GetFolderByBrowser() != null)
            {
                textBox3.Text = outputFolder;
            }
        }

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {

        }

        private void button5_Click(object sender, EventArgs e)
        {
            terminateFired = true;
        }


    }
}
