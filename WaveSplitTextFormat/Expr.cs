using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace WaveTextFormatter
{
    public class Expr
    {
        public enum OpCode
        {
            SECTION,
            SYMBOL,           
            STRING_CONSTANT,
            INT_CONSTANT,
            FLOAT_CONSTANT,
            PATH,           
            LIST,
            INIT_SETTING_LIST,
            NAME_ARG_MAP_LIST,
            COMMENT,
            INPUT_DIRECTORY,OUTPUT_DIRECTORY,CUT_VOLUME,CUT_MUTE,MUTE,AUTO_GEN,ADD_HEAD,ADD_END,
            AUTO_GEN_NEXT, AUTO_GEN_NEXT_TEMP, AUTO_2FILE,
            CUT_VOLUME_TEMP, CUT_MUTE_TEMP, MUTE_TEMP, AUTO_GEN_TEMP, ADD_HEAD_TEMP, ADD_END_TEMP
        }
        public String expv;
        public OpCode op;
        public List<Expr> arguments = new List<Expr>();
        public bool IsTerminal()
        {
            switch(op)
            {
                default: return false;
                case OpCode.SYMBOL:
                case OpCode.PATH:
                case OpCode.INT_CONSTANT:
                case OpCode.STRING_CONSTANT:
                case OpCode.FLOAT_CONSTANT:
                    return true;
            }
        }

        public bool IsSection()
        {
            return op == OpCode.SECTION;
        }
        public bool IsComment()
        {
            return op == OpCode.COMMENT;
        }
        public bool IsSymbol()
        {
            return op == OpCode.SYMBOL;
        }
        public bool IsList()
        {
            return op == OpCode.LIST;
        }
        public void AddArgument(Expr e)
        {
            if (this.IsComment() && e.IsComment())
            {
                this.expv += e.expv;
                return;
            }
            arguments.Add(e);
        }
        public void AddAll(params Expr[] e)
        {
            for (int i = 0; i < e.Length; ++i )
            {
                arguments.Add(e[i]);
            }
 
        }
        public Expr GetArgument(int i)
        {
            return arguments[i];
        }
        public Expr GetLeft()
        {
            return GetArgument(0);
        }
        public Expr GetRight()
        {
            return GetArgument(1);
        }

        public int GetNumArg()
        {
            return arguments.Count;
        }
        public bool IsPath()
        {
            return op == OpCode.PATH;
        }
        public String GetPath()
        {
            if (IsPath())
            {
                return expv;
            }
            return "";
        }
        public String GetSymbol()
        {
            if (IsSymbol())
            {
                return expv;
            }
            return "";
        }
        public Expr(Expr.OpCode op, String expv, params Expr[] args)
        {
            this.op = op;
            this.expv = expv;
            for (int i = 0; i < args.Length; ++i)
            {
                arguments.Add(args[i]);
            }
        }

        public virtual string ToXMLString(int indent=0)
        {
            StringBuilder indentSpace = new StringBuilder();
            for (int i = 0; i < indent; ++i)
            {
                indentSpace.Append(' ');
            }
            switch (op)
            {

                case OpCode.SECTION:
                    return indentSpace.ToString() + "<SECTION name=\"" + (GetArgument(0)==null?"DEFAULT":GetArgument(0).ToString()) + "\"" + String.Format(" statement_count=\"{0}\" />" + Environment.NewLine, GetNumArg() - 1);
                case OpCode.STRING_CONSTANT:
                case OpCode.PATH:
                case OpCode.COMMENT:
                case OpCode.SYMBOL:
                case OpCode.INT_CONSTANT:
                case OpCode.FLOAT_CONSTANT:
                    return indentSpace.ToString() + "<" + op.ToString() + " >" + expv + "</" + op.ToString() + ">" + Environment.NewLine;
                default:
                case OpCode.LIST:
                    {
                        StringBuilder ret = new StringBuilder();
                        ret.Append(indentSpace.ToString() + "<" + op.ToString() + " NumArg=\"" + GetNumArg() + "\">" + Environment.NewLine);
                        int i = 0;
                        if(GetNumArg() > 0)
                        {
                            Expr item = GetArgument(i);
                            String itemString = indentSpace.ToString() + indentSpace.ToString() + "<null>" + Environment.NewLine;
                            if (item != null)
                            {
                                itemString = item.ToXMLString(indent+6);
                            }
                            ret.Append(itemString);
                        }
                        for (i=1; i < GetNumArg(); ++i)
                        {
                            Expr item = GetArgument(i);
                            String itemString = indentSpace.ToString() + indentSpace.ToString() + "<null>" + Environment.NewLine;
                            if (item != null)
                            {
                                if (item == this)
                                {
                                    continue;
                                }
                                itemString = item.ToXMLString(indent + 6);
                            }
                            ret.Append(itemString);
                        }
                        ret.Append(indentSpace.ToString() + "</" + op.ToString() + ">" + Environment.NewLine);
                        return ret.ToString();
                    }
            }
        }
        public override string ToString()
        {
            return ToXMLString();
        }
    }
}
