using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace WaveTextFormatter
{
    public class FileNameTableDocument
    {
        String filename;
        Boolean hasError=false;
        public String InputDirectory;
        public String OutputDirectory;
        public double CutVolume = 0;
        public double CutMuteDuration = 0.60;
        public double MuteDuration = 1.10;
        public String AutoGenPostfix = "_c";
        public double AddHead = 0;
        public double AddEnd = 0;
        public Boolean auto2file = false;
        public String autoGenNext = null;
        public List<WaveOptions> WaveSplitOptionLines = new List<WaveOptions>();
        public FileNameTableDocument(String filename)
        {
            this.filename = filename;
        }
        public override string ToString()
        {
            StringBuilder strb = new StringBuilder();
            strb.AppendFormat("input_directory {0}" + Environment.NewLine, InputDirectory);
            strb.AppendFormat("output_directory {0}" + Environment.NewLine,OutputDirectory);
            strb.AppendFormat("cut_volume {0}" + Environment.NewLine,CutVolume);
            strb.AppendFormat("cut_mute {0}" + Environment.NewLine, CutMuteDuration);
            strb.AppendFormat("mute {0}" + Environment.NewLine, MuteDuration);
            strb.AppendFormat("auto_gen {0}" + Environment.NewLine, AutoGenPostfix);
            strb.AppendFormat("add_head {0}" + Environment.NewLine, AddHead);
            strb.AppendFormat("add_end {0}" + Environment.NewLine, AddEnd);
            for (int i = 0; i < WaveSplitOptionLines.Count; ++i)
            {
                strb.AppendFormat("{0}" + Environment.NewLine, WaveSplitOptionLines[i].ToString());
            }
            return strb.ToString();
        }
        void OnParseErrorListener(int line, String near, Expr expr)
        {
            hasError = true;
            Console.WriteLine("Error Near Line {0} expr {1} : {2}", line, near, expr.ToXMLString());
        }
        private void ParseDefaultSettingList(Expr e)
        {
            for (int i = 0; i < e.GetNumArg(); ++i)
            {
                Expr child = e.GetArgument(i);
                switch(child.op)
                {
                    case Expr.OpCode.AUTO_GEN_NEXT:
                        this.autoGenNext = child.GetArgument(0).GetSymbol();
                        if (child.GetArgument(0).op == Expr.OpCode.INT_CONSTANT)
                        {
                            if (int.Parse(child.GetArgument(0).expv) == 0)
                            {
                                autoGenNext = null;
                            }
                        }
                        break;
                    case Expr.OpCode.AUTO_2FILE:
                        this.auto2file = (int.Parse(child.GetArgument(0).expv) != 0);
                        break;
                    case Expr.OpCode.INPUT_DIRECTORY:
                        this.InputDirectory = child.GetArgument(0).GetPath();
                        break;
                    case Expr.OpCode.OUTPUT_DIRECTORY:
                        this.OutputDirectory = child.GetArgument(0).GetPath();
                        break;
                    case Expr.OpCode.CUT_VOLUME:
                        this.CutVolume = Double.Parse(child.GetArgument(0).expv);
                        break;
                    case Expr.OpCode.CUT_MUTE:
                        this.CutMuteDuration = Double.Parse(child.GetArgument(0).expv);
                        break;
                    case Expr.OpCode.MUTE:
                        this.MuteDuration = Double.Parse(child.GetArgument(0).expv);
                        break;
                    case Expr.OpCode.AUTO_GEN:
                        this.AutoGenPostfix = child.GetArgument(0).GetSymbol();
                        break;
                    case Expr.OpCode.ADD_HEAD:
                        this.AddHead= Double.Parse(child.GetArgument(0).expv);
                        break;
                    case Expr.OpCode.ADD_END:
                        this.AddEnd = Double.Parse(child.GetArgument(0).expv);
                        break;
                }
            }
        }
        public WaveOptions DefaultWaveOption()
        {
            return new WaveOptions().addEnd(this.AddEnd).auto2file(this.auto2file ? 1 : 0).addHead(this.AddHead)
                        .cutMute(this.CutMuteDuration).cutVolume(this.CutVolume)
                        .muteDuration(this.MuteDuration).authGenPostFix(this.AutoGenPostfix).auto_gen_next(this.autoGenNext);
        }
        private void ParseChainingSettingList(Expr e)
        {
            for (int i = 0; i < e.GetNumArg(); ++i)
            {
                // prepare wave option with default configuration
                WaveOptions options = DefaultWaveOption();
                Expr childlist = e.GetArgument(i);
                for (int cidx = 0; cidx < childlist.GetNumArg(); ++cidx)
                {
                    Expr child = childlist.GetArgument(cidx);
                    switch (child.op)
                    {
                        case Expr.OpCode.SYMBOL:
                            options.file(child.GetSymbol());
                            break;
                        case Expr.OpCode.CUT_VOLUME_TEMP:
                            options.cut_volume_temp(Double.Parse(child.GetArgument(0).expv));
                            break;
                        case Expr.OpCode.CUT_MUTE_TEMP:
                            options.cut_mute_temp(Double.Parse(child.GetArgument(0).expv));
                            break;
                        case Expr.OpCode.MUTE_TEMP:
                            options.mute_temp(Double.Parse(child.GetArgument(0).expv));
                            break;
                        case Expr.OpCode.AUTO_GEN_TEMP:
                            {
                                Expr arg = child.GetArgument(0);
                                if (arg.op == Expr.OpCode.INT_CONSTANT)
                                {
                                    options.auto_gen_temp0();
                                }
                                else
                                {
                                    options.auto_gen_temp(child.GetArgument(0).GetSymbol());
                                }
                            }
                            break;
                        case Expr.OpCode.AUTO_GEN_NEXT_TEMP:
                            {
                                Expr arg = child.GetArgument(0);
                                if (arg.op == Expr.OpCode.INT_CONSTANT)
                                {
                                    options.auto_gen_next_temp(null);
                                }
                                else
                                {
                                    options.auto_gen_next_temp(child.GetArgument(0).GetSymbol());
                                }
                                
                                break;
                            }
                        case Expr.OpCode.ADD_HEAD_TEMP:
                            options.add_head_temp(Double.Parse(child.GetArgument(0).expv));
                            break;
                        case Expr.OpCode.ADD_END_TEMP:
                            options.add_end_temp(Double.Parse(child.GetArgument(0).expv));
                            break;
                    }
                }

                WaveOptions target = options;
                
                target.overrideOption(target);
                String prevFileName = "";
                for(int j=0; j<options.chains.Count; ++j)
                {
                    WaveOptions o = options.chains[j];
                    // when found file name, add it
                    if (o.option == WaveOptions.OptionEnum.FILENAME)
                    {
                        if (String.IsNullOrEmpty(target.filename))
                        {
                            target.overrideOption(o);
                            WaveSplitOptionLines.Add(target);
                            target = DefaultWaveOption();
                        }
                        else
                        {
                            WaveSplitOptionLines.Add(target);
                            target = DefaultWaveOption();
                        }
                        prevFileName = o.filename;
                    }
                    else
                    {
                        target.overrideOption(o);
                    }
                    if (j + 1 >= options.chains.Count)
                    {
                        if (String.IsNullOrEmpty(target.filename))
                        {
                            if (!String.IsNullOrWhiteSpace(prevFileName) && !String.IsNullOrWhiteSpace(target.autoGenPostFixNext))
                            {
                                target.filename = prevFileName + target.AutoGenPostfix + "_" + target.autoGenPostFixNext;
                            }
                        }
                    }
                }

            }
        }
        public Stream GenerateStreamFromString(string s,Encoding enc)
        {
            MemoryStream stream = new MemoryStream();
            if (enc == null)
            {
                enc = Encoding.Default;
            }
            StreamWriter writer = new StreamWriter(stream,enc);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
        public Boolean Parse()
        {
            StreamReader freader = new StreamReader(filename,Encoding.Default,true);
            String str = freader.ReadToEnd();
            freader.Close();
            Stream reader = GenerateStreamFromString(str,freader.CurrentEncoding);
            ParserStatus parserStatus = new ParserStatus();
            Scanner scanner = new Scanner(reader, parserStatus);
            Parser parser = new Parser(scanner, parserStatus);
            parser.onParseErrorListener = this.OnParseErrorListener;
            parser.Parse();
            reader.Close();
            if (parserStatus.flatStatements.Count > 0)
            {
                Expr list = parserStatus.flatStatements[0];
                Expr initSettingList = null;
                Expr filenameMapList = null;
                for (int i = 0; i < list.GetNumArg(); ++i)
                {
                    Expr e = list.GetArgument(i);
                    if (e.op == Expr.OpCode.INIT_SETTING_LIST)
                    {
                        initSettingList = e;
                    }
                    if (e.op == Expr.OpCode.NAME_ARG_MAP_LIST)
                    {
                        filenameMapList = e;
                    }
                }
                ParseDefaultSettingList(initSettingList);
                ParseChainingSettingList(filenameMapList);
            }
            else
            {
                //Console.WriteLine("FlatStatement.count={0}", parserStatus.flatStatements.Count);
            }
            return !hasError;
        }
    }
}
