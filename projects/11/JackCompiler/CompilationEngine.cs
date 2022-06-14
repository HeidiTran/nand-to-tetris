using System;
using System.Collections.Generic;
using static JackCompiler.TokenType;
using static JackCompiler.Keyword;

/// <summary>
/// Generates the compiler's output
/// </summary>
namespace JackCompiler
{
	class CompilationEngine
	{
		private readonly JackTokenizer _tokenizer;
		private readonly VMWriter _vmWriter;
		private SymbolTable _symbolTable;
		private string _currentClassName;
		private int _ifCnt;
		private int _whileCnt;

		private static readonly HashSet<Keyword> _subroutineKws = new()
		{
			CONSTRUCTOR,
			FUNCTION,
			METHOD
		};

		private static readonly Dictionary<Kind, Segment> _kindToSegment = new()
		{
			{ Kind.VAR, Segment.LOCAL },
			{ Kind.STATIC, Segment.STATIC },
			{ Kind.ARG, Segment.ARGUMENT },
			{ Kind.FIELD, Segment.THIS }    // TODO: Double check this
		};

		public CompilationEngine(JackTokenizer tokenizer, string vmFilePath)
		{
			_tokenizer = tokenizer;
			_vmWriter = new VMWriter(vmFilePath);
		}

		/// <summary>
		/// Compiles a complete class.
		/// </summary>
		public void CompileClass()
		{
			if (!_tokenizer.HasMoreTokens()) return;

			_symbolTable = new();
			_tokenizer.Advance();

			// class: 'class' className '{' classVarDec* subroutineDec* '}'
			Eat("class");
			MustHaveMoreTokens();
			_currentClassName = _tokenizer.GetIdentifier();
			MustHaveMoreTokens();
			Eat("{");
			MustHaveMoreTokens();
			while (IsKeyword("static") || IsKeyword("field"))
			{
				CompileClassVarDec();
			}

			while (_tokenizer.GetTokenType() == KEYWORD &&
				_subroutineKws.Contains(_tokenizer.GetKeyWord()))
			{
				CompileSubroutineDec();
			}
			Eat("}");
		}

		/// <summary>
		/// Compiles a static variable declaration, or a field declaration.
		/// </summary>
		public void CompileClassVarDec()
		{
			// classVarDec: ('static' | 'field') type varName (',' varName)* ';'
			string name, type;
			Kind kind;
			kind = Enum.Parse<Kind>(_tokenizer.GetKeyWord().ToString());
			if (kind != Kind.STATIC && kind != Kind.FIELD)
			{
				throw new Exception("Class variables must have static/field kind!");
			}
			MustHaveMoreTokens();

			type = CompileTypeAndReturn();
			name = CompileNameAndReturn();
			_symbolTable.Define(name, type, kind);

			while (IsSymbol(','))
			{
				MustHaveMoreTokens();
				name = _tokenizer.GetIdentifier();
				_symbolTable.Define(name, type, kind);
				MustHaveMoreTokens();
			}
			Eat(";");
			MustHaveMoreTokens();
		}

		/// <summary>
		/// Compiles a complete method, function, or constructor.
		/// </summary>
		public void CompileSubroutineDec()
		{
			_symbolTable.StartSubroutine();
			_ifCnt = 0;
			_whileCnt = 0;

			// subroutineDec: ('constructor' | 'function' | 'method') ('void' | type) subroutineName '(' parameterList ')' subroutineBody
			Keyword keyword = _tokenizer.GetKeyWord();  // 'constructor' | 'function' | 'method'
			if (keyword == METHOD)
			{
				_symbolTable.Define("this", _currentClassName, Kind.ARG);
			}
			MustHaveMoreTokens();

			TokenType tokenType = _tokenizer.GetTokenType();
			bool isBoolOrIntOrChar = tokenType == KEYWORD &&
				_varTypes.Contains(_tokenizer.GetKeyWord());
			bool isVoid = tokenType == KEYWORD &&
				_tokenizer.GetKeyWord() == VOID;
			if (!(tokenType == IDENTIFIER || isBoolOrIntOrChar || isVoid))
			{
				throw new Exception("Expect a type. Found: " + _tokenizer.GetCurrentToken());
			}
			MustHaveMoreTokens();
			string subroutineName = _tokenizer.GetIdentifier();
			MustHaveMoreTokens();
			Eat("(");
			MustHaveMoreTokens();
			CompileParameterList();
			Eat(")");
			MustHaveMoreTokens();
			CompileSubroutineBody(subroutineName, keyword);
			MustHaveMoreTokens();
		}

		/// <summary>
		/// Compiles a (possibly empty) parameter list. Does not handle the enclosing "()".
		/// </summary>
		public void CompileParameterList()
		{
			// paramterList: ((type varName) (',' type varName)*)?
			if (!IsSymbol(')'))
			{
				string type = CompileTypeAndReturn();
				string name = CompileNameAndReturn();
				_symbolTable.Define(name, type, Kind.ARG);

				while (IsSymbol(','))
				{
					MustHaveMoreTokens();
					type = CompileTypeAndReturn();
					name = CompileNameAndReturn();
					_symbolTable.Define(name, type, Kind.ARG);
				}
			}
		}

		/// <summary>
		/// Compiles a subroutine's body.
		/// </summary>
		/// <param name="keyword">constructor | method | function</param>
		public void CompileSubroutineBody(string subroutineName, Keyword keyword)
		{
			// subroutineBody: '{' varDec* statements '}'
			Eat("{");
			MustHaveMoreTokens();
			while (IsKeyword("var"))
			{
				CompileVarDec();
			}

			_vmWriter.WriteFunction($"{_currentClassName}.{subroutineName}", _symbolTable.VarCount(Kind.VAR));

			if (keyword == CONSTRUCTOR)
			{
				_vmWriter.WritePush(Segment.CONSTANT,
					_symbolTable.VarCount(Kind.FIELD)); // size of obj of this class
				_vmWriter.WriteCall("Memory.alloc", 1);
				_vmWriter.WritePop(Segment.POINTER, 0); // anchor this at the base address
			}
			else if (keyword == METHOD)
			{
				_vmWriter.WritePush(Segment.ARGUMENT, 0);   // Map to this
				_vmWriter.WritePop(Segment.POINTER, 0);     // Set this pointer to the addr of current obj
			}

			CompileStatements();
			Eat("}");
		}

		/// <summary>
		/// Compiles a var declaration.
		/// </summary>
		public void CompileVarDec()
		{
			// varDec: 'var' type varName (',' varName)* ';'
			string name, type;
			Kind kind;

			kind = Enum.Parse<Kind>(_tokenizer.GetKeyWord().ToString());
			if (kind != Kind.VAR)
			{
				throw new Exception("Variables must have var kind!");
			}
			
			MustHaveMoreTokens();

			type = CompileTypeAndReturn();
			name = CompileNameAndReturn();
			_symbolTable.Define(name, type, kind);

			while (IsSymbol(','))
			{
				MustHaveMoreTokens();
				name = _tokenizer.GetIdentifier();
				_symbolTable.Define(name, type, kind);
				MustHaveMoreTokens();
			}
			Eat(";");
			MustHaveMoreTokens();
		}

		private static readonly HashSet<Keyword> _statementTypes = new()
		{
			LET,
			IF,
			WHILE,
			DO,
			RETURN
		};

		/// <summary>
		/// Compiles a sequence of statements. Does not handle the enclosing "()"
		/// </summary>
		public void CompileStatements()
		{
			// statments: statement*
			while (_tokenizer.GetTokenType() == KEYWORD &&
				_statementTypes.Contains(_tokenizer.GetKeyWord()))
			{
				switch (_tokenizer.GetKeyWord())
				{
					case LET:
						CompileLet();
						break;
					case DO:
						CompileDo();
						break;
					case IF:
						CompileIf();
						break;
					case WHILE:
						CompileWhile();
						break;
					case RETURN:
						CompileReturn();
						break;
					default:
						break;
				}
			}
		}

		/// <summary>
		/// Compiles a do statement
		/// </summary>
		public void CompileDo()
		{
			Eat("do");
			MustHaveMoreTokens();
			CompileSubroutineCall();
			MustHaveMoreTokens();
			Eat(";");

			// Since it's a do statement, we discard the return value
			_vmWriter.WritePop(Segment.TEMP, 0);
			MustHaveMoreTokens();
		}

		public void CompileSubroutineCall()
		{
			// subroutineCall: subroutineName '(' expressionList ')' |
			// (className | varName) '.' subroutineName '(' expressionList ')'
			char oneCharAhead = _tokenizer.LookOneCharAhead();
			string name = _tokenizer.GetIdentifier();   // the first token
			int nArgs = 0;
			string vmFunctionName;
			if (oneCharAhead == '(')
			{
				vmFunctionName = $"{_currentClassName}.{name}";
				MustHaveMoreTokens();
				Eat("(");
				MustHaveMoreTokens();
				_vmWriter.WritePush(Segment.POINTER, 0);    // method has this as an implied arg
				nArgs += CompileExpressionListAndReturnNArgs();
				nArgs++;
				Eat(")");
			}
			else if (oneCharAhead == '.')
			{
				string className;
				if (_symbolTable.IsInSymbolTable(name))
				{
					Kind kind = _symbolTable.KindOf(name);
					int idx = _symbolTable.IndexOf(name);
					className = _symbolTable.TypeOf(name);
					_vmWriter.WritePush(_kindToSegment[kind], idx);
					nArgs++;    // method has this as an implied arg
				}
				else
				{
					className = name;
				}
				MustHaveMoreTokens();
				Eat(".");
				MustHaveMoreTokens();
				vmFunctionName = $"{className}.{_tokenizer.GetIdentifier()}";
				MustHaveMoreTokens();
				Eat("(");
				MustHaveMoreTokens();
				nArgs += CompileExpressionListAndReturnNArgs();
				Eat(")");
			}
			else
			{
				throw new Exception("Unrecognized subroutine call structure!");
			}

			_vmWriter.WriteCall(vmFunctionName, nArgs);
		}

		/// <summary>
		/// Compiles a let statement
		/// </summary>
		public void CompileLet()
		{
			// letStatement: 'let' varName ('[' expression ']')? '=' expression ';'
			Eat("let");
			MustHaveMoreTokens();

			string name = _tokenizer.GetIdentifier();
			Kind kind = _symbolTable.KindOf(name);
			int idx = _symbolTable.IndexOf(name);
			MustHaveMoreTokens();

			bool isArrayAccess = false;
			while (IsSymbol('['))
			{
				isArrayAccess = true;
				Eat("[");
				MustHaveMoreTokens();
				CompileExpression();
				Eat("]");
				MustHaveMoreTokens();
				_vmWriter.WritePush(_kindToSegment[kind], idx);
				_vmWriter.WriteArithmetic(Command.ADD);
			}

			Eat("=");
			MustHaveMoreTokens();
			CompileExpression();
			Eat(";");

			if (isArrayAccess)
			{
				_vmWriter.WritePop(Segment.TEMP, 0);
				_vmWriter.WritePop(Segment.POINTER, 1);
				_vmWriter.WritePush(Segment.TEMP, 0);
				_vmWriter.WritePop(Segment.THAT, 0);
			}
			else
			{
				_vmWriter.WritePop(_kindToSegment[kind], idx);
			}
			MustHaveMoreTokens();
		}

		/// <summary>
		/// Compiles a while statement
		/// </summary>
		public void CompileWhile()
		{
			// whileStatement: '(' expression ')' '{' statements '}'
			Eat("while");
			MustHaveMoreTokens();
			int whileIdx = _whileCnt;
			_whileCnt++;
			_vmWriter.WriteLabel($"WHILE_EXP{whileIdx}");
			Eat("(");
			MustHaveMoreTokens();
			CompileExpression();
			Eat(")");
			MustHaveMoreTokens();
			_vmWriter.WriteArithmetic(Command.NOT);
			_vmWriter.WriteIf($"WHILE_END{whileIdx}");
			Eat("{");
			MustHaveMoreTokens();
			CompileStatements();
			Eat("}");
			_vmWriter.WriteGoto($"WHILE_EXP{whileIdx}");
			_vmWriter.WriteLabel($"WHILE_END{whileIdx}");
			MustHaveMoreTokens();
		}

		/// <summary>
		/// Compiles a return statement
		/// </summary>
		public void CompileReturn()
		{
			// returnStatement: 'return' expression? ';'
			Eat("return");
			MustHaveMoreTokens();
			if (IsSymbol(';'))
			{
				// We still have to fulfill the call-and-return contract, so we push constant 0
				_vmWriter.WritePush(Segment.CONSTANT, 0);
				_vmWriter.WriteReturn();
			}
			else
			{
				CompileExpression();
				Eat(";");
				_vmWriter.WriteReturn();
			}
			MustHaveMoreTokens();
		}

		/// <summary>
		/// Compiles an if statement, possible with a trailing else clause
		/// </summary>
		public void CompileIf()
		{
			// 'if' '(' expression ')' '{' statements '}'
			// ('else' '{' statements '}') ?
			string ifTrue = $"IF_TRUE{_ifCnt}";
			string ifFalse = $"IF_FALSE{_ifCnt}";
			string ifEnd = $"IF_END{_ifCnt}";
			_ifCnt++;
			Eat("if");
			MustHaveMoreTokens();
			Eat("(");
			MustHaveMoreTokens();
			CompileExpression();
			Eat(")");
			_vmWriter.WriteIf(ifTrue);
			_vmWriter.WriteGoto(ifFalse);
			MustHaveMoreTokens();
			Eat("{");
			MustHaveMoreTokens();
			_vmWriter.WriteLabel(ifTrue);
			CompileStatements();

			Eat("}");
			MustHaveMoreTokens();

			if (IsKeyword("else"))
			{
				_vmWriter.WriteGoto(ifEnd);
			}

			_vmWriter.WriteLabel(ifFalse);
			if (IsKeyword("else"))
			{
				Eat("else");
				MustHaveMoreTokens();
				Eat("{");
				MustHaveMoreTokens();
				CompileStatements();
				Eat("}");
				MustHaveMoreTokens();
				_vmWriter.WriteLabel(ifEnd);
			}
		}

		private static readonly HashSet<char> _ops = new() { '+', '-', '*', '/', '&', '|', '<', '>', '=' };
		private static readonly Dictionary<char, Command> _opToVmCommand = new()
		{
			{ '+', Command.ADD },
			{ '-', Command.SUB },
			{ '&', Command.AND },
			{ '|', Command.OR },
			{ '<', Command.LT },
			{ '>', Command.GT },
			{ '=', Command.EQ },
		};

		/// <summary>
		/// Compiles an expression.
		/// </summary>
		public void CompileExpression()
		{
			// expression: term (op term)*
			CompileTerm();
			MustHaveMoreTokens();
			while (_tokenizer.GetTokenType() == SYMBOL &&
				_ops.Contains(_tokenizer.GetSymbol()))
			{
				char op = _tokenizer.GetSymbol();

				MustHaveMoreTokens();
				CompileTerm();
				MustHaveMoreTokens();
				CodeWriteArithmeticOp(op);
			}
		}

		private void CodeWriteArithmeticOp(char op)
		{
			if (op == '*')
			{
				_vmWriter.WriteCall("Math.multiply", 2);
			}
			else if (op == '/')
			{
				_vmWriter.WriteCall("Math.divide", 2);
			}
			else
			{
				_vmWriter.WriteArithmetic(_opToVmCommand[op]);
			}
		}

		/// <summary>
		/// Compiles a term. If the current token is an identifier, the routine 
		/// must distinguish between a variable, an array entry
		/// or a subroutine call.
		/// </summary>
		public void CompileTerm()
		{
			TokenType tokenType = _tokenizer.GetTokenType();
			if (tokenType == INT_CONST)
			{
				_vmWriter.WritePush(Segment.CONSTANT, _tokenizer.GetIntVal());
			}
			else if (tokenType == STRING_CONST)
			{
				string s = _tokenizer.GetStringVal();
				_vmWriter.WritePush(Segment.CONSTANT, s.Length);
				_vmWriter.WriteCall("String.new", 1);
				foreach (char c in s)
				{
					_vmWriter.WritePush(Segment.CONSTANT, c);
					_vmWriter.WriteCall("String.appendChar", 2);
				}
			}
			else if (tokenType == KEYWORD)
			{
				Keyword keyword = _tokenizer.GetKeyWord();
				if (keyword == FALSE || keyword == NULL)
				{
					_vmWriter.WritePush(Segment.CONSTANT, 0);
				}
				else if (keyword == TRUE)
				{
					_vmWriter.WritePush(Segment.CONSTANT, 0);
					_vmWriter.WriteArithmetic(Command.NOT);
				}
				else if (keyword == THIS)
				{
					_vmWriter.WritePush(Segment.POINTER, 0);    // THIS is stored at pointer 0
				} else
				{
					throw new Exception("Expect a keyword constant true/false/null/this. Found: " + _tokenizer.GetCurrentToken());
				}
			}
			else if (tokenType == IDENTIFIER)
			{
				if (!_tokenizer.HasMoreTokens())
				{
					throw new Exception("Unfinished structure");
				}
				char nextChar = _tokenizer.LookOneCharAhead();
				if (nextChar == '[')
				{
					// varName '[' expression ']'
					string name = _tokenizer.GetIdentifier();
					Kind kind = _symbolTable.KindOf(name);
					int idx = _symbolTable.IndexOf(name);
					MustHaveMoreTokens();
					Eat("[");
					MustHaveMoreTokens();
					CompileExpression();
					Eat("]");
					_vmWriter.WritePush(_kindToSegment[kind], idx);
					_vmWriter.WriteArithmetic(Command.ADD);
					_vmWriter.WritePop(Segment.POINTER, 1);
					_vmWriter.WritePush(Segment.THAT, 0);
				}
				else if (nextChar == '(' || nextChar == '.')
				{
					CompileSubroutineCall();
				}
				else
				{
					// Case: varName
					string name = _tokenizer.GetIdentifier();
					Kind kind = _symbolTable.KindOf(name);
					int idx = _symbolTable.IndexOf(name);
					_vmWriter.WritePush(_kindToSegment[kind], idx);
				}
			}
			else if (tokenType == SYMBOL)
			{
				char symbol = _tokenizer.GetSymbol();
				if (symbol == '(')
				{
					Eat("(");
					MustHaveMoreTokens();
					CompileExpression();
					Eat(")");
				}
				else if (symbol == '-' || symbol == '~')
				{
					MustHaveMoreTokens();
					CompileTerm();
					if (symbol == '-')
					{
						_vmWriter.WriteArithmetic(Command.NEG);
					}
					else
					{
						_vmWriter.WriteArithmetic(Command.NOT);
					}
				}
				else
				{
					throw new Exception("Illegal symbol. Found: " + _tokenizer.GetCurrentToken());
				}
			}
			else
			{
				throw new Exception("Unknown term structure. Found: " + _tokenizer.GetCurrentToken());
			}
		}

		/// <summary>
		/// Compiles a (possibly empty) comma-separated list of expressions.
		/// </summary>
		public int CompileExpressionListAndReturnNArgs()
		{
			// expressionList: (expression (',' expression)* )?
			int nArgs = 0;
			if (!IsSymbol(')'))
			{
				nArgs++;
				CompileExpression();

				while (IsSymbol(','))
				{
					MustHaveMoreTokens();
					nArgs++;
					CompileExpression();
				}
			}

			return nArgs;
		}

		public void Close()
		{
			_vmWriter.Close();
		}

		private string CompileNameAndReturn()
		{
			if (_tokenizer.GetTokenType() == IDENTIFIER)
			{
				string name = _tokenizer.GetIdentifier();
				MustHaveMoreTokens();
				return name;
			}
			else
			{
				throw new Exception("Expect a name. Found: " + _tokenizer.GetCurrentToken());
			}
		}

		private static readonly HashSet<Keyword> _varTypes = new()
		{
			BOOLEAN,
			INT,
			CHAR
		};

		private void Eat(string expectedToken)
		{
			TokenType tokenType = _tokenizer.GetTokenType();
			if (tokenType == SYMBOL)
			{
				string symbol = _tokenizer.GetSymbol().ToString();
				if (symbol != expectedToken)
				{
					throw new Exception("Exptected: " + expectedToken + " Found: " + symbol);
				}

			}
			else if (tokenType == KEYWORD)
			{
				string kw = _tokenizer.GetKeyWord().ToString().ToLower();
				if (kw != expectedToken)
				{
					throw new Exception("Exptected: " + expectedToken + " Found: " + kw);
				}
			}
			else
			{
				throw new Exception("Expected Symbol or  Found: " + tokenType.ToString());
			}
		}

		private bool IsSymbol(char symbol)
		{
			return _tokenizer.GetTokenType() == SYMBOL &&
				_tokenizer.GetSymbol() == symbol;
		}

		private bool IsKeyword(string kw)
		{
			return _tokenizer.GetTokenType() == KEYWORD &&
				_tokenizer.GetKeyWord().ToString().ToLower() == kw;
		}

		private string CompileTypeAndReturn()
		{
			string type;
			TokenType tokenType = _tokenizer.GetTokenType();
			if (tokenType == IDENTIFIER)
			{
				type = _tokenizer.GetIdentifier();
			}
			else if (tokenType == KEYWORD && _varTypes.Contains(_tokenizer.GetKeyWord()))
			{
				type = _tokenizer.GetKeyWord().ToString();
			}
			else
			{
				throw new Exception("Unknown type!");
			}
			MustHaveMoreTokens();
			return type;
		}

		private void MustHaveMoreTokens()
		{
			if (!_tokenizer.HasMoreTokens())
			{
				throw new Exception("Incomplete structure!");
			}
			else
			{
				_tokenizer.Advance();
			}
		}
	}
}
