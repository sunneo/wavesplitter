%namespace WaveTextFormatter
%union {
	public int integer;
	public string stringVal;
	public Expr expr;
	public Tokens token;
	public int line;
}

%token WORD DIGIT FLOATVALUE STRING  FILENAME KEYWORDS KEYWORDS_TEMP
%token COMMENT NEWLINE STAR SHARP

%%

program : statement_list {  }
		| EOF
        ;

statement_list: 
	|statement{ 
	   if(!$1.expr.IsSection()) {
	      parserStatus.currentSection.AddArgument($1.expr); 
	   }
	   parserStatus.CurrentExpr=$$.expr=$1.expr; 
	   parserStatus.flatStatements.Add($1.expr);
	}
	| statement_list optional_comment_or_newline{
	   $$.expr=$1.expr;
	}
	| statement_list statement
	{ 
	   if(!$2.expr.IsSection()){
	      parserStatus.currentSection.AddArgument($2.expr);
	   }
	   parserStatus.CurrentExpr=$$.expr=$1.expr; 
	   parserStatus.flatStatements.Add($2.expr);
	}
	| statement_list error NEWLINE { 
		if(onParseErrorListener!=null){
		Console.WriteLine("ERROR...{0} near line {1}",$1.stringVal,parserStatus.CurrentLine); 
			onParseErrorListener(parserStatus.CurrentLine,$1.stringVal,parserStatus.CurrentExpr);
		}
		else{
		   Console.WriteLine("ERROR...{0} near line {1}",$1.stringVal,parserStatus.CurrentLine); 
		}
		yyerrok(); 
	}
	;
statement: 
	sound_define_block_statement
	;
sound_define_block_statement: initial_sound_define_block begin_filenamelist filenamelist  end_filenamelist
	{
	   $$.expr=Xcons.List($1.expr,$3.expr);
	}
	;

initial_sound_define_block:
	initial_setting optional_comment_or_newline{ $$.expr=Xcons.InitSetList($1.expr); }
	| initial_sound_define_block initial_setting optional_comment_or_newline { $$.expr=$1.expr; $1.expr.AddArgument($2.expr); }
	;
initial_setting: KEYWORDS arg {$$.expr=Xcons.KeyWord($1.stringVal,$2.expr); }
	;
begin_filenamelist: STAR optional_comment_or_newline {  } ;
end_filenamelist: SHARP optional_comment_or_newline { } ;
filenamelist: linedefine comment_or_newline {$$.expr=Xcons.NameArgMapList($1.expr); }
	| filenamelist linedefine comment_or_newline {$$.expr=$1.expr;  $1.expr.AddArgument($2.expr); }
	| filenamelist comment_or_newline { $$.expr=$1.expr; }
	| filenamelist error NEWLINE {
	   $$.expr = $1.expr;
	   if(onParseErrorListener!=null){
			onParseErrorListener(parserStatus.CurrentLine,$1.stringVal,parserStatus.CurrentExpr);
		}
		else{
		   Console.WriteLine("ERROR...{0} near line {1}",$1.stringVal,parserStatus.CurrentLine); 
		}
	}
	;
linedefine: lineitem { $$.expr=Xcons.List($1.expr); }
	| linedefine lineitem { $1.expr.AddArgument($2.expr); $$.expr=$1.expr; }
	;
lineitem: symbol | temp_setting;

temp_setting: KEYWORDS_TEMP arg  {$$.expr=Xcons.KeyWord($1.stringVal,$2.expr); }
	;

arg: float|digit|filename|symbol ;


optional_comment_or_newline: |optional_comment_or_newline comment_or_newline;
comment_or_newline:
	COMMENT | NEWLINE 
	;
symbol: WORD { parserStatus.CurrentExpr=$$.expr=Xcons.Symbol($1.stringVal); } 
	;

float:	FLOATVALUE{ parserStatus.CurrentExpr=$$.expr=Xcons.FloatConstant($1.stringVal); };
digit: DIGIT { parserStatus.CurrentExpr=$$.expr=Xcons.IntConstant($1.stringVal); };
string: STRING { parserStatus.CurrentExpr=$$.expr=Xcons.StringConstant($1.stringVal); };
filename: FILENAME { parserStatus.CurrentExpr=$$.expr=Xcons.PATH($1.stringVal); };
%%

internal Parser(Scanner lex,ParserStatus parserStatus=null) : base(lex) {
		parserStatus.Sections.Add(parserStatus.currentSection);
		this.parserStatus=parserStatus;
}
private ParserStatus parserStatus;
public string ErrorMessage { get; private set; }
public int ErrorCode { get; private set; }

public delegate void OnParseErrorListener(int line,String near,Expr expr);
public OnParseErrorListener onParseErrorListener;
