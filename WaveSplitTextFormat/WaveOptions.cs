using System;
using System.Collections.Generic;
using System.Text;

namespace WaveTextFormatter
{
    public class WaveOptions
    {
        public String filename;
        Boolean hasError = false;
        public double CutVolume = 0;
        public double CutMuteDuration = 0.60;
        public double MuteDuration = 1.10;
        public String AutoGenPostfix = "_c";
        public double AddHead = 0;
        public double AddEnd = 0;
        public Boolean auto2File = false;
        public String autoGenPostFixNext = null;

        public List<WaveOptions> chains = new List<WaveOptions>();
        Object arg;
        public OptionEnum option = OptionEnum.NONE;
        public enum OptionEnum
        {
            NONE,
            CUT_VOLUME_TEMP, CUT_MUTE_TEMP, MUTE_TEMP, AUTO_GEN_TEMP, AUTO_GEN_TEMP0, ADD_HEAD_TEMP, ADD_END_TEMP,
            FILENAME, AUTO_GEN_NEXT, AUTO_GEN_NEXT_TEMP,AUTO_2FILE
        }
        private WaveOptions(OptionEnum opt, object arg)
        {
            this.option = opt;
            this.arg = arg;
        }
        public WaveOptions()
        {

        }
        public WaveOptions auto2file(int i)
        {
            this.auto2File = (i != 0);
            return this;
        }
        public WaveOptions auto_gen_next(String s)
        {
            this.autoGenPostFixNext = s;
            return this;
        }
        public WaveOptions auto_gen_next_temp(String s)
        {
            this.chains.Add(new WaveOptions(OptionEnum.AUTO_GEN_NEXT_TEMP, s));
            return this;
        }
        public WaveOptions muteDuration(double v)
        {
            this.MuteDuration = v;
            return this;
        }
        public WaveOptions cutMute(double v)
        {
            this.CutMuteDuration = v;
            return this;
        }
        public WaveOptions cutVolume(double v)
        {
            this.CutVolume = v;
            return this;
        }
        public WaveOptions addHead(double v)
        {
            this.AddHead = v;
            return this;
        }
        public WaveOptions addEnd(double v)
        {
            this.AddEnd = v;
            return this;
        }
        public WaveOptions authGenPostFix(String s)
        {
            this.AutoGenPostfix = s;
            return this;
        }
        public WaveOptions cut_volume_temp(double val)
        {
            this.chains.Add(new WaveOptions(OptionEnum.CUT_VOLUME_TEMP, val));
            return this;
        }
        public WaveOptions cut_mute_temp(double val)
        {
            this.chains.Add(new WaveOptions(OptionEnum.CUT_MUTE_TEMP, val));
            return this;
        }
        public WaveOptions mute_temp(double val)
        {
            this.chains.Add(new WaveOptions(OptionEnum.MUTE_TEMP, val));
            return this;
        }
        public WaveOptions auto_gen_temp0()
        {
            this.chains.Add(new WaveOptions(OptionEnum.AUTO_GEN_TEMP0,(Double) 0));
            return this;
        }
        public WaveOptions auto_gen_temp(String val)
        {
            this.chains.Add(new WaveOptions(OptionEnum.AUTO_GEN_TEMP, val));
            return this;
        }
        
        public WaveOptions add_head_temp(double val)
        {
            this.chains.Add(new WaveOptions(OptionEnum.ADD_HEAD_TEMP, val));
            return this;
        }
        public WaveOptions add_end_temp(double val)
        {
            this.chains.Add(new WaveOptions(OptionEnum.ADD_END_TEMP, val));
            return this;
        }
        public static String SimpleConvert(String val)
        {
            byte[] simpleFill = new byte[val.Length];
            //Console.WriteLine("Convert {0}",val);
            for (int i = 0; i < simpleFill.Length; ++i)
            {
                simpleFill[i] = (byte)val[i];
              //  Console.Write(" {0}", (int)simpleFill[i]);
            }
            val = Encoding.Default.GetString(simpleFill);
            return val;
        }
        public WaveOptions file(String val)
        {
            this.chains.Add(new WaveOptions(OptionEnum.FILENAME, val));
            return this;
        }
        public void overrideOption(WaveOptions w)
        {
            switch (w.option)
            {
                case OptionEnum.FILENAME:
                    this.filename = (String) w.arg;
                    break;
                case OptionEnum.MUTE_TEMP:
                    this.MuteDuration = (Double)w.arg;
                    break;
                case OptionEnum.CUT_VOLUME_TEMP:
                    this.CutVolume = (double)w.arg;
                    break;
                case OptionEnum.CUT_MUTE_TEMP:
                    this.CutMuteDuration = (Double)w.arg;
                    break;
                case OptionEnum.AUTO_GEN_TEMP:
                    this.AutoGenPostfix = (String)w.arg;
                    break;
                case OptionEnum.AUTO_GEN_TEMP0:
                    this.AutoGenPostfix = "";
                    break;
                case OptionEnum.ADD_HEAD_TEMP:
                    this.AddHead = (Double)w.arg;
                    break;
                case OptionEnum.ADD_END_TEMP:
                    this.AddEnd = (Double)w.arg;
                    break;
            }
        }

        public String ToFinalString()
        {
            StringBuilder strb = new StringBuilder();
            strb.AppendFormat("{0} cut_volume {1} | cut_mute {2} | mute {3} | auto_gen {4} | add_head {5} | add_end {6} | auto_gen_next {7}",
                filename, CutVolume, CutMuteDuration, MuteDuration, AutoGenPostfix, AddHead, AddEnd, autoGenPostFixNext);
            return strb.ToString();
        }
        public override string ToString()
        {
            StringBuilder strb = new StringBuilder();
            switch (option)
            {
                case OptionEnum.NONE:
                    break;
                case OptionEnum.CUT_VOLUME_TEMP:
                case OptionEnum.CUT_MUTE_TEMP:
                case OptionEnum.MUTE_TEMP:
                case OptionEnum.ADD_END_TEMP:
                case OptionEnum.ADD_HEAD_TEMP:
                case OptionEnum.AUTO_GEN_TEMP0:
                    strb.Append( option.ToString().ToLower() + " " + ((Double)arg).ToString() );
                    break;
                case OptionEnum.AUTO_GEN_TEMP:               
                    strb.Append(option.ToString().ToLower() + " " + ((String)arg).ToString());
                    break;
                case OptionEnum.FILENAME:
                    strb.Append(((String)arg).ToString());
                    break;
            }
            for (int i = 0; i < chains.Count; ++i)
            {
                strb.Append(chains[i].ToString()+" ");
            }
            return strb.ToString();
        }
    }
}
