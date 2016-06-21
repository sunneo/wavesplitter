using System;
using System.Collections.Generic;
using System.Text;

namespace WaveTextFormatter
{
    public class Section: Expr
    {
        public Section(Expr expr) : base(Expr.OpCode.SECTION,(expr == null?"":expr.expv),expr)
        {
            if (expr != null && expr.op == OpCode.SECTION)
            {
                this.arguments.Clear();
                this.arguments.Add(expr.GetArgument(0));
                for (int i = 1; i < expr.GetNumArg(); ++i)
                {
                    AddArgument(expr.GetArgument(i));
                }
            }
        }
        public Expr GetSectionHeader()
        {
            return GetArgument(0);
        }
        public int GetNumStatements()
        {
            return GetNumArg() - 1;
        }
        public Expr GetStatement(int i)
        {
            return GetArgument(i + 1);
        }
        public void AddStatement(Expr e)
        {
            base.AddArgument(e);
        }
        public static Section ParseExpr(Expr e)
        {
            if (e.op == OpCode.SECTION)
            {
                Section ret = new Section(e.GetArgument(0));
                for (int i = 1; i < e.GetNumArg(); ++i)
                {
                    ret.AddArgument(e.GetArgument(i));
                }
                return ret;
            }
            return null;
        }

        public override string ToXMLString(int indent=0)
        {
            StringBuilder indentSpace = new StringBuilder();
            for (int i = 0; i < indent; ++i)
            {
                indentSpace.Append(' ');
            }
            StringBuilder ret = new StringBuilder();
            Expr directChild = GetArgument(0);
            StringBuilder strb = new StringBuilder();
            if (directChild != null)
            {
                
                if (directChild.IsList())
                {
                    int childCnt = directChild.GetNumArg();
                    for (int i = 0; i < childCnt; ++i)
                    {
                        strb.Append(directChild.GetArgument(i).ToString());
                        if (i < childCnt - 1)
                        {
                            strb.Append(",");
                        }
                    }

                }
                else
                {
                    strb.Append(directChild.ToString());
                }
            }
            else
            {
                strb.Append("DEFAULT");
            }
            ret.Append(indentSpace.ToString() + "<SECTION name=\"" + strb.ToString() + "\"" + String.Format(" statement_count=\"{0}\" />" + Environment.NewLine, GetNumArg() - 1));
            strb = null;

            for (int j = 0; j < GetNumStatements(); ++j)
            {
                ret.Append(GetStatement(j).ToXMLString(indent + 3) + Environment.NewLine);
            }
            ret.Append(indentSpace.ToString() + "</SECTION>" + Environment.NewLine);
            return ret.ToString();
        }
    }
}
