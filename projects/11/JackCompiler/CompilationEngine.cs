using System;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// Generates the compiler's output
/// </summary>
namespace JackCompiler
{
	class CompilationEngine
	{
		private readonly StreamWriter _streamWriter;
		private readonly JackTokenizer _tokenizer;
		private readonly VMWriter _vmWriter;
		private SymbolTable _symbolTable;
		private int _indentLevel;
		private string _currentClassName;
		private int _subroutineCnt;

		public CompilationEngine(JackTokenizer tokenizer, string xmlFilePath)
		{
			_tokenizer = tokenizer;
			_streamWriter = new StreamWriter(xmlFilePath);
			_vmWriter = new VMWriter(xmlFilePath.Replace(".xml", ".vm"));
			_indentLevel = 0;
			_subroutineCnt = 0;
		}

		private static readonly HashSet<Keyword> _subroutineKws = new()
		{
			Keyword.CONSTRUCTOR,
			Keyword.FUNCTION,
			Keyword.METHOD
		};

		/// <summary>
		/// Compiles a complete class.
		/// </summary>
		public void CompileClass()
		{
			if (!_tokenizer.HasMoreTokens()) return;

			_symbolTable = new();
			_tokenizer.Advance();
			WriteL("<class>");
			_indentLevel++;

			// class: 'class' className '{' classVarDec* subroutineDec* '}'
			Eat("class");
			MustHaveMoreTokens();
			_currentClassName = _tokenizer.GetIdentifier();
			WriteIdentifierNew(_currentClassName, "class", true, false, -1);	// className
			MustHaveMoreTokens();
			Eat("{");
			MustHaveMoreTokens();
			while (IsKeyword("static") || IsKeyword("field"))
			{
				CompileClassVarDec();
			}

			while (_tokenizer.GetTokenType() == TokenType.KEYWORD &&
				_subroutineKws.Contains(_tokenizer.GetKeyWord()))
			{
				CompileSubroutineDec();
			}
			Eat("}");
			_indentLevel--;
			WriteL("</class>");
		}

		/// <summary>
		/// Compiles a static variable declaration, or a field declaration.
		/// </summary>
		public void CompileClassVarDec()
		{
			WriteL("<classVarDec>");
			_indentLevel++;

			string name, type;
			SymbolTable.Kind kind;

			// classVarDec: ('static' | 'field') type varName (',' varName)* ';'
			kind = Enum.Parse<SymbolTable.Kind>(_tokenizer.GetKeyWord().ToString());
			if (kind != SymbolTable.Kind.STATIC && kind != SymbolTable.Kind.FIELD)
			{
				throw new Exception("Class variables must have static/field kind!");
			}

			WriteKeyword();
			MustHaveMoreTokens();

			type = CompileTypeAndReturn();
			name = CompileNameAndReturn();
			_symbolTable.Define(name, type, kind);
			WriteIdentifierNew(name, kind.ToString().ToLower(), true, true, _symbolTable.IndexOf(name));

			while (IsSymbol(','))
			{
				WriteSymbol();
				MustHaveMoreTokens();

				name = _tokenizer.GetIdentifier();
				_symbolTable.Define(name, type, kind);
				WriteIdentifierNew(name, kind.ToString().ToLower(), true, true, _symbolTable.IndexOf(name));
				MustHaveMoreTokens();
			}
			Eat(";");

			_indentLevel--;
			WriteL("</classVarDec>");
			MustHaveMoreTokens();
		}

		/// <summary>
		/// Compiles a complete method, function, or constructor.
		/// </summary>
		public void CompileSubroutineDec()
		{
			_symbolTable.StartSubroutine();
			WriteL("<subroutineDec>");
			_indentLevel++;

			// subroutineDec: ('constructor' | 'function' | 'method') ('void' | type) subroutineName '(' parameterList ')' subroutineBody
			if (_tokenizer.GetKeyWord() == Keyword.METHOD)
			{
				_symbolTable.Define("this", _currentClassName, SymbolTable.Kind.ARG);
			}
			WriteKeyword();
			MustHaveMoreTokens();

			TokenType tokenType = _tokenizer.GetTokenType();
			bool isBoolOrIntOrChar = tokenType == TokenType.KEYWORD &&
				_varTypes.Contains(_tokenizer.GetKeyWord());
			bool isVoid = tokenType == TokenType.KEYWORD &&
				_tokenizer.GetKeyWord() == Keyword.VOID;
			CompileType(tokenType == TokenType.IDENTIFIER || isBoolOrIntOrChar || isVoid);
			string subroutineName = _tokenizer.GetIdentifier();
			WriteIdentifierNew(subroutineName, "subroutine", true, false, -1);
			MustHaveMoreTokens();
			Eat("(");
			MustHaveMoreTokens();
			CompileParameterList();
			Eat(")");
			MustHaveMoreTokens();
			CompileSubroutineBody(subroutineName);

			_indentLevel--;
			WriteL("</subroutineDec>");
			_subroutineCnt++;
			MustHaveMoreTokens();
		}

		/// <summary>
		/// Compiles a (possibly empty) parameter list. Does not handle the enclosing "()".
		/// </summary>
		public void CompileParameterList()
		{
			WriteL("<parameterList>");
			_indentLevel++;

			// paramterList: ((type varName) (',' type varName)*)?
			if (!IsSymbol(')'))
			{
				string name, type;
				SymbolTable.Kind kind = SymbolTable.Kind.ARG;

				type = CompileTypeAndReturn();
				name = CompileNameAndReturn();
				_symbolTable.Define(name, type, kind);
				WriteIdentifierNew(name, kind.ToString().ToLower(), true, true, _symbolTable.IndexOf(name));

				while (IsSymbol(','))
				{
					WriteSymbol();
					MustHaveMoreTokens();

					type = CompileTypeAndReturn();
					name = CompileNameAndReturn();
					_symbolTable.Define(name, type, kind);
					WriteIdentifierNew(name, kind.ToString().ToLower(), true, true, _symbolTable.IndexOf(name));
				}
			}

			_indentLevel--;
			WriteL("</parameterList>");
		}

		/// <summary>
		/// Compiles a subroutine's body.
		/// </summary>
		public void CompileSubroutineBody(string subroutineName)
		{
			WriteL("<subroutineBody>");
			_indentLevel++;

			// subroutineBody: '{' varDec* statements '}'
			Eat("{");
			MustHaveMoreTokens();
			while (IsKeyword("var"))
			{
				CompileVarDec();
			}

			_vmWriter.WriteFunction($"{_currentClassName}.{subroutineName}", _symbolTable.VarCount(SymbolTable.Kind.VAR));

			CompileStatements();
			Eat("}");
			_indentLevel--;
			WriteL("</subroutineBody>");
		}

		/// <summary>
		/// Compiles a var declaration.
		/// </summary>
		public void CompileVarDec()
		{
			WriteL("<varDec>");
			_indentLevel++;

			// varDec: 'var' type varName (',' varName)* ';'

			string name, type;
			SymbolTable.Kind kind;

			kind = Enum.Parse<SymbolTable.Kind>(_tokenizer.GetKeyWord().ToString());
			if (kind != SymbolTable.Kind.VAR)
			{
				throw new Exception("Variables must have var kind!");
			}
			WriteKeyword();
			MustHaveMoreTokens();

			type = CompileTypeAndReturn();
			name = CompileNameAndReturn();
			_symbolTable.Define(name, type, kind);
			WriteIdentifierNew(name, kind.ToString().ToLower(), true, true, _symbolTable.IndexOf(name));

			while (IsSymbol(','))
			{
				WriteSymbol();
				MustHaveMoreTokens();
				name = _tokenizer.GetIdentifier();
				_symbolTable.Define(name, type, kind);
				WriteIdentifierNew(name, kind.ToString().ToLower(), true, true, _symbolTable.IndexOf(name));
				MustHaveMoreTokens();
			}
			Eat(";");

			_indentLevel--;
			WriteL("</varDec>");
			MustHaveMoreTokens();
		}

		private static readonly HashSet<Keyword> _statementTypes = new()
		{
			Keyword.LET,
			Keyword.IF,
			Keyword.WHILE,
			Keyword.DO,
			Keyword.RETURN
		};

		/// <summary>
		/// Compiles a sequence of statements. Does not handle the enclosing "()"
		/// </summary>
		public void CompileStatements()
		{
			WriteL("<statements>");
			_indentLevel++;

			// statments: statement*
			while (_tokenizer.GetTokenType() == TokenType.KEYWORD &&
				_statementTypes.Contains(_tokenizer.GetKeyWord()))
			{
				switch (_tokenizer.GetKeyWord())
				{
					case Keyword.LET:
						CompileLet();
						break;
					case Keyword.DO:
						CompileDo();
						break;
					case Keyword.IF:
						CompileIf();
						break;
					case Keyword.WHILE:
						CompileWhile();
						break;
					case Keyword.RETURN:
						CompileReturn();
						break;
					default:
						break;
				}
			}

			_indentLevel--;
			WriteL("</statements>");
		}

		/// <summary>
		/// Compiles a do statement
		/// </summary>
		public void CompileDo()
		{
			WriteL("<doStatement>");
			_indentLevel++;
			Eat("do");
			MustHaveMoreTokens();
			CompileSubroutineCall();
			MustHaveMoreTokens();
			Eat(";");

			// Since it's a do statement, we discard the return value
			// We still have to fulfill the call-and-return contract, so we push constant 0
			_vmWriter.WritePop(Segment.TEMP, 0);
			_vmWriter.WritePush(Segment.CONSTANT, 0);
			_indentLevel--;
			WriteL("</doStatement>");
			MustHaveMoreTokens();
		}

		public void CompileSubroutineCall()
		{
			// subroutineCall: subroutineName '(' expressionList ')' |
			// (className | varName) '.' subroutineName '(' expressionList ')'
			char oneCharAhead = _tokenizer.LookOneCharAhead();
			string name = _tokenizer.GetIdentifier();   // the first token
			string vmFunctionName = string.Empty;
			int nArgs = 0;
			if (oneCharAhead == '(')
			{
				vmFunctionName = $"{_currentClassName}.{name}";
				WriteIdentifierNew(name, "subroutine", false, false, -1);
				MustHaveMoreTokens();
				Eat("(");
				MustHaveMoreTokens();
				nArgs = CompileExpressionListAndReturnNArgs();
				Eat(")");
			} else if (oneCharAhead == '.')
			{
				if (_symbolTable.IsInSymbolTable(name))
				{
					SymbolTable.Kind kind = _symbolTable.KindOf(name);
					int idx = _symbolTable.IndexOf(name);
					WriteIdentifierNew(name, kind.ToString().ToLower(), false, true, idx);
				} else
				{
					WriteIdentifierNew(name, "class", false, false, -1);	   // className
				}
				MustHaveMoreTokens();
				Eat(".");
				MustHaveMoreTokens();
				vmFunctionName = $"{name}.{_tokenizer.GetIdentifier()}";
				WriteIdentifierNew(_tokenizer.GetIdentifier(), "subroutine", false, false, -1);
				MustHaveMoreTokens();
				Eat("(");
				MustHaveMoreTokens();
				nArgs = CompileExpressionListAndReturnNArgs();
				Eat(")");
			} else
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
			WriteL("<letStatement>");
			_indentLevel++;

			// letStatement: 'let' varName ('[' expression ']')? '=' expression ';'
			Eat("let");
			MustHaveMoreTokens();

			string name = _tokenizer.GetIdentifier();
			SymbolTable.Kind kind = _symbolTable.KindOf(name);
			int idx = _symbolTable.IndexOf(name);
			WriteIdentifierNew(name, kind.ToString().ToLower(), false, true, idx);
			MustHaveMoreTokens();

			while (IsSymbol('['))
			{
				Eat("[");
				MustHaveMoreTokens();
				CompileExpression();
				Eat("]");
				MustHaveMoreTokens();
			}

			Eat("=");
			MustHaveMoreTokens();
			CompileExpression();
			Eat(";");

			_indentLevel--;
			WriteL("</letStatement>");
			MustHaveMoreTokens();
		}

		/// <summary>
		/// Compiles a while statement
		/// </summary>
		public void CompileWhile()
		{
			WriteL("<whileStatement>");
			_indentLevel++;

			// whileStatement: '(' expression ')' '{' statements '}'
			Eat("while");
			MustHaveMoreTokens();
			Eat("(");
			MustHaveMoreTokens();
			CompileExpression();
			Eat(")");
			MustHaveMoreTokens();
			Eat("{");
			MustHaveMoreTokens();
			CompileStatements();
			Eat("}");
			_indentLevel--;
			WriteL("</whileStatement>");
			MustHaveMoreTokens();
		}

		/// <summary>
		/// Compiles a return statement
		/// </summary>
		public void CompileReturn()
		{
			WriteL("<returnStatement>");
			_indentLevel++;

			// returnStatement: 'return' expression? ';'
			Eat("return");
			MustHaveMoreTokens();
			if (IsSymbol(';'))
			{
				_vmWriter.WriteReturn();
				WriteSymbol();
			}
			else
			{
				CompileExpression();
				Eat(";");
			}
			_indentLevel--;
			WriteL("</returnStatement>");
			MustHaveMoreTokens();
		}

		/// <summary>
		/// Compiles an if statement, possible with a trailing else clause
		/// </summary>
		public void CompileIf()
		{
			WriteL("<ifStatement>");
			_indentLevel++;

			// 'if' '(' expression ')' '{' statements '}'
			// ('else' '{' statements '}') ?
			Eat("if");
			MustHaveMoreTokens();
			Eat("(");
			MustHaveMoreTokens();
			CompileExpression();
			Eat(")");
			MustHaveMoreTokens();
			Eat("{");
			MustHaveMoreTokens();
			CompileStatements();
			Eat("}");
			MustHaveMoreTokens();

			if (IsKeyword("else"))
			{
				Eat("else");
				MustHaveMoreTokens();
				Eat("{");
				MustHaveMoreTokens();
				CompileStatements();
				Eat("}");
				MustHaveMoreTokens();
			}

			_indentLevel--;
			WriteL("</ifStatement>");
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
			WriteL("<expression>");
			_indentLevel++;

			// expression: term (op term)*
			CompileTerm();
			MustHaveMoreTokens();
			while (_tokenizer.GetTokenType() == TokenType.SYMBOL &&
				_ops.Contains(_tokenizer.GetSymbol()))
			{
				char op = _tokenizer.GetSymbol();
				WriteSymbol();
				MustHaveMoreTokens();
				CompileTerm();
				MustHaveMoreTokens();
				CodeWriteArithmeticOp(op);
			}
			_indentLevel--;
			WriteL("</expression>");
		}

		private void CodeWriteArithmeticOp(char op)
		{
			if (op == '*')
			{
				_vmWriter.WriteCall("Math.multiply", 2);
			} else if (op == '/')
			{
				_vmWriter.WriteCall("Math.divide", 2);
			} else
			{
				_vmWriter.WriteArithmetic(_opToVmCommand[op]);
			}
		}

		private bool IsKeywordConst()
		{
			HashSet<Keyword> keywordsConts = new()
			{
				Keyword.TRUE,
				Keyword.FALSE,
				Keyword.NULL,
				Keyword.THIS
			};

			if (_tokenizer.GetTokenType() == TokenType.KEYWORD)
			{
				Keyword kw = _tokenizer.GetKeyWord();
				return keywordsConts.Contains(kw);
			}
			else
			{
				return false;
			}
		}

		/// <summary>
		/// Compiles a term. If the current token is an identifier, the routine 
		/// must distinguish between a variable, an array entry
		/// or a subroutine call.
		/// </summary>
		public void CompileTerm()
		{
			WriteL("<term>");
			_indentLevel++;
			TokenType tokenType = _tokenizer.GetTokenType();
			if (tokenType == TokenType.INT_CONST)
			{
				_vmWriter.WritePush(Segment.CONSTANT, _tokenizer.GetIntVal());
				WriteL("<integerConstant> " + _tokenizer.GetIntVal().ToString() + " </integerConstant>");
			}
			else if (tokenType == TokenType.STRING_CONST)
			{
				WriteL("<stringConstant> " + _tokenizer.GetStringVal() + " </stringConstant>");
			}
			else if (IsKeywordConst())
			{
				WriteKeyword();
			}
			else if (tokenType == TokenType.IDENTIFIER)
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
					SymbolTable.Kind kind = _symbolTable.KindOf(name);
					int idx = _symbolTable.IndexOf(name);
					WriteIdentifierNew(name, kind.ToString().ToLower(), false, true, idx);
					MustHaveMoreTokens();
					Eat("[");
					MustHaveMoreTokens();
					CompileExpression();
					Eat("]");
				}
				else if (nextChar == '(' || nextChar == '.')
				{
					CompileSubroutineCall();
				}
				else
				{
					string name = _tokenizer.GetIdentifier();
					SymbolTable.Kind kind = _symbolTable.KindOf(name);
					int idx = _symbolTable.IndexOf(name);
					WriteIdentifierNew(name, kind.ToString().ToLower(), false, true, idx);
				}
			}
			else if (tokenType == TokenType.SYMBOL)
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
					WriteSymbol();
					MustHaveMoreTokens();
					CompileTerm();
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
			_indentLevel--;
			WriteL("</term>");
		}

		/// <summary>
		/// Compiles a (possibly empty) comma-separated list of expressions.
		/// </summary>
		public int CompileExpressionListAndReturnNArgs()
		{
			WriteL("<expressionList>");
			_indentLevel++;

			// expressionList: (expression (',' expression)* )?
			int nArgs = 0;	
			if (!IsSymbol(')'))
			{
				nArgs++;
				CompileExpression();

				while (IsSymbol(','))
				{
					WriteSymbol();
					MustHaveMoreTokens();
					nArgs++;
					CompileExpression();
				}
			}

			_indentLevel--;
			WriteL("</expressionList>");
			return nArgs;
		}

		public void Close()
		{
			_streamWriter.Close();
			_vmWriter.Close();
		}

		private string CompileNameAndReturn()
		{
			if (_tokenizer.GetTokenType() == TokenType.IDENTIFIER)
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

		private void CompileType(bool isAllowedType)
		{
			if (isAllowedType)
			{
				TokenType tokenType = _tokenizer.GetTokenType();
				if (tokenType == TokenType.KEYWORD)
				{
					WriteKeyword();
				}
				else if (tokenType == TokenType.IDENTIFIER)
				{
					WriteIdentifierNew(_tokenizer.GetIdentifier(), "class", false, false, -1);	// className
				}
				MustHaveMoreTokens();
			}
			else
			{
				throw new Exception("Expect a type. Found: " + _tokenizer.GetCurrentToken());
			}
		}

		private static readonly HashSet<Keyword> _varTypes = new()
		{
			Keyword.BOOLEAN,
			Keyword.INT,
			Keyword.CHAR
		};

		private void Eat(string expectedToken)
		{
			TokenType tokenType = _tokenizer.GetTokenType();
			if (tokenType == TokenType.SYMBOL)
			{
				string symbol = _tokenizer.GetSymbol().ToString();
				if (symbol != expectedToken)
				{
					throw new Exception("Exptected: " + expectedToken + " Found: " + symbol);
				}
				WriteSymbol();
			}
			else if (tokenType == TokenType.KEYWORD)
			{
				string kw = _tokenizer.GetKeyWord().ToString().ToLower();
				if (kw != expectedToken)
				{
					throw new Exception("Exptected: " + expectedToken + " Found: " + kw);
				}
				WriteKeyword();
			}
			else
			{
				throw new Exception("Expected Symbol or Keyword. Found: " + tokenType.ToString());
			}
		}

		/// <summary>
		/// Output the identifier with extra info
		/// </summary>
		/// <param name="category">var, argument, static, field, class, subroutine</param>
		/// <param name="isBeingDefined">whether the identifier is presently being defined or used</param>
		/// <param name="isOneOf4Kinds">whether the identifier represents a variable of one of the four kinds (var, argument, static, field)</param>
		/// <param name="runningIdx">running index assigned to the identifier by the symbol table</param>
		private void WriteIdentifierNew(string name, string category, bool isBeingDefined, bool isOneOf4Kinds, int runningIdx)
		{
			WriteL("<identifier>");
			_indentLevel++;
			WriteL("<identifier> " + name + " </identifier>");
			WriteL("<category> " + category + " </category>");
			WriteL("<isBeingDefined> " + isBeingDefined + " </isBeingDefined>");
			WriteL("<isOneOf4Kinds> " + isOneOf4Kinds + " </isOneOf4Kinds>");
			WriteL("<runningIdx> " + runningIdx + " </runningIdx>");
			_indentLevel--;
			WriteL("</identifier>");
		}

		private static readonly Dictionary<string, string> _symbolsToXMLConvention = new()
		{
			{ "<", "&lt;" },
			{ ">", "&gt;" },
			{ "\"", "&quot;" },
			{ "&", "&amp;" }
		};

		private bool IsSymbol(char symbol)
		{
			return _tokenizer.GetTokenType() == TokenType.SYMBOL &&
				_tokenizer.GetSymbol() == symbol;
		}

		private void WriteSymbol()
		{
			string symbol = _tokenizer.GetSymbol().ToString();
			if (_symbolsToXMLConvention.ContainsKey(symbol))
			{
				symbol = _symbolsToXMLConvention[symbol];
			}

			WriteL("<symbol> " + symbol + " </symbol>");
		}

		private bool IsKeyword(string kw)
		{
			return _tokenizer.GetTokenType() == TokenType.KEYWORD &&
				_tokenizer.GetKeyWord().ToString().ToLower() == kw;
		}

		private void WriteKeyword()
		{
			WriteL("<keyword> " + _tokenizer.GetKeyWord().ToString().ToLower() + " </keyword>");
		}

		private void WriteL(string text)
		{
			_streamWriter.WriteLine(new string(' ', _indentLevel * 2) + text);
			// Console.WriteLine(new string(' ', _indentLevel * 2) + text);
		}

		private string CompileTypeAndReturn()
		{
			string type;
			TokenType tokenType = _tokenizer.GetTokenType();
			if (tokenType == TokenType.IDENTIFIER)
			{
				type = _tokenizer.GetIdentifier();
				WriteIdentifierNew(type, "class", false, false, -1);  // className
			}
			else if (tokenType == TokenType.KEYWORD && _varTypes.Contains(_tokenizer.GetKeyWord()))
			{
				type = _tokenizer.GetKeyWord().ToString();
				WriteKeyword();
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
