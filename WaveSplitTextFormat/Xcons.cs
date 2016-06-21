using System;
using System.Collections.Generic;
using System.Text;

namespace WaveTextFormatter
{
    public class Xcons
    {
        public static Expr KeyWord(String s,params Expr[] arg)
        {
            Expr.OpCode op = Expr.OpCode.SYMBOL;
            switch (s.ToUpper())
            {
                case "AUTO_2FILE":
                    op = Expr.OpCode.AUTO_2FILE;
                    break;
                case "AUTO_GEN_NEXT":
                    op = Expr.OpCode.AUTO_GEN_NEXT;
                    break;
                case "AUTO_GEN_NEXT_TEMP":
                    op = Expr.OpCode.AUTO_GEN_NEXT_TEMP;
                    break;
                case "INPUT_DIRECTORY":
                    op = Expr.OpCode.INPUT_DIRECTORY;
                    break;
                case "OUTPUT_DIRECTORY":
                    op = Expr.OpCode.OUTPUT_DIRECTORY;
                    break;
                case "CUT_VOLUME":
                    op = Expr.OpCode.CUT_VOLUME;
                    break;
                case "CUT_MUTE":
                    op = Expr.OpCode.CUT_MUTE;
                    break;
                case "MUTE":
                    op = Expr.OpCode.MUTE;
                    break;
                case "AUTO_GEN":
                    op = Expr.OpCode.AUTO_GEN;
                    break;
                case "ADD_HEAD":
                    op = Expr.OpCode.ADD_HEAD;
                    break;
                case "ADD_END":
                    op = Expr.OpCode.ADD_END;
                    break;
                case "$CUT_VOLUME_TEMP":
                    op = Expr.OpCode.CUT_VOLUME_TEMP;
                    break;
                case "$CUT_MUTE_TEMP":
                    op = Expr.OpCode.CUT_MUTE_TEMP;
                    break;
                case "$MUTE_TEMP":
                    op = Expr.OpCode.MUTE_TEMP;
                    break;
                case "$AUTO_GEN_TEMP":
                    op = Expr.OpCode.AUTO_GEN_TEMP;
                    break;
                case "$ADD_HEAD_TEMP":
                    op = Expr.OpCode.ADD_HEAD_TEMP;
                    break;
                case "$ADD_END_TEMP":
                    op = Expr.OpCode.ADD_END_TEMP;
                    break;
                default:
                    op = Expr.OpCode.SYMBOL;
                    break;
            }
            return new Expr(op, op.ToString(),arg);
        }
        public static Expr Symbol(String s)
        {
            return new Expr(Expr.OpCode.SYMBOL, s);
        }
        public static Expr IntConstant(int i)
        {
            return Xcons.IntConstant("0x"+Convert.ToString(i, 16));
        }
        public static Expr IntConstant(String v)
        {
            return new Expr(Expr.OpCode.INT_CONSTANT, v);
        }
        public static Expr StringConstant(String v)
        {
            return new Expr(Expr.OpCode.STRING_CONSTANT, v);
        }
        public static Expr FloatConstant(String v)
        {
            return new Expr(Expr.OpCode.FLOAT_CONSTANT, v);
        }
        public static Expr PATH(String v)
        {
            return new Expr(Expr.OpCode.PATH, v);
        }
         public static Expr Section(Expr e)
        {
            return new Expr(Expr.OpCode.SECTION, "[]",e);
        }
        public static Expr List(params Expr[] items)
        {
            return new Expr(Expr.OpCode.LIST,"",items);
        }
        public static Expr InitSetList(params Expr[] items)
        {
            return new Expr(Expr.OpCode.INIT_SETTING_LIST, "", items);
        }
        public static Expr NameArgMapList(params Expr[] items)
        {
            return new Expr(Expr.OpCode.NAME_ARG_MAP_LIST, "", items);
        }
    }
}
