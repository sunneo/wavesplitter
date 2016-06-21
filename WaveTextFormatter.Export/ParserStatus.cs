using System;
using System.Collections.Generic;
using System.Text;

namespace WaveTextFormatter
{
    public class ParserStatus
    {
        public bool IsInBuildOption = false;
        public volatile bool requireLineBreak = false;       
        public Expr CurrentExpr = null;
        public int CurrentLine = 0;
        public List<Expr> flatStatements = new List<Expr>();
        public Expr currentSection = Xcons.Section(Xcons.Symbol("DEFAULT"));
        public List<Expr> Sections = new List<Expr>();
    }
}
