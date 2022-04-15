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
		private int _indentLevel;

		public CompilationEngine(JackTokenizer tokenizer, string xmlFilePath)
		{
			_tokenizer = tokenizer;
			_streamWriter = new StreamWriter(xmlFilePath);
			_indentLevel = 0;
		}

		private static readonly HashSet<JackTokenizer.KeyWord> _subroutineKws = new()
		{
			JackTokenizer.KeyWord.CONSTRUCTOR,
			JackTokenizer.KeyWord.FUNCTION,
			JackTokenizer.KeyWord.METHOD
		};

		/// <summary>
		/// Compiles a complete class.
		/// </summary>
		public void CompileClass()
		{
			if (!_tokenizer.HasMoreTokens()) return;
			_tokenizer.Advance();
			WriteL("<class>");
			_indentLevel++;

			// class: 'class' className '{' classVarDec* subroutineDec* '}'
			Eat("class");
			MustHaveMoreTokens();
			CompileName();
			Eat("{");
			MustHaveMoreTokens();
			while (IsKeyword("static") || IsKeyword("field"))
			{
				CompileClassVarDec();
			}

			while (_tokenizer.GetTokenType() == JackTokenizer.TokenType.KEYWORD &&
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

			// classVarDec: ('static' | 'field') type varName (',' varName)* ';'
			WriteKeyword();
			MustHaveMoreTokens();
			CompileVarTypeVarName();

			while (IsSymbol(','))
			{
				WriteSymbol();
				MustHaveMoreTokens();
				CompileName();
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
			WriteL("<subroutineDec>");
			_indentLevel++;

			// subroutineDec: ('constructor' | 'function' | 'method') ('void' | type) subroutineName '(' parameterList ')' subroutineBody
			WriteKeyword();
			MustHaveMoreTokens();

			JackTokenizer.TokenType tokenType = _tokenizer.GetTokenType();
			bool isBoolOrIntOrChar = tokenType == JackTokenizer.TokenType.KEYWORD &&
				_varTypes.Contains(_tokenizer.GetKeyWord());
			bool isVoid = tokenType == JackTokenizer.TokenType.KEYWORD &&
				_tokenizer.GetKeyWord() == JackTokenizer.KeyWord.VOID;
			CompileType(tokenType == JackTokenizer.TokenType.IDENTIFIER || isBoolOrIntOrChar || isVoid);
			CompileName();
			Eat("(");
			MustHaveMoreTokens();
			CompileParameterList();
			Eat(")");
			MustHaveMoreTokens();
			CompileSubroutineBody();

			_indentLevel--;
			WriteL("</subroutineDec>");
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
				CompileVarTypeVarName();

				while (IsSymbol(','))
				{
					WriteSymbol();
					MustHaveMoreTokens();
					CompileVarTypeVarName();
				}
			}

			_indentLevel--;
			WriteL("</parameterList>");
		}

		/// <summary>
		/// Compiles a subroutine's body.
		/// </summary>
		public void CompileSubroutineBody()
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
			WriteKeyword();
			MustHaveMoreTokens();
			CompileVarTypeVarName();

			while (IsSymbol(','))
			{
				WriteSymbol();
				MustHaveMoreTokens();
				CompileName();
			}
			Eat(";");

			_indentLevel--;
			WriteL("</varDec>");
			MustHaveMoreTokens();
		}

		private static readonly HashSet<JackTokenizer.KeyWord> _statementTypes = new()
		{
			JackTokenizer.KeyWord.LET,
			JackTokenizer.KeyWord.IF,
			JackTokenizer.KeyWord.WHILE,
			JackTokenizer.KeyWord.DO,
			JackTokenizer.KeyWord.RETURN
		};

		/// <summary>
		/// Compiles a sequence of statements. Does not handle the enclosing "()"
		/// </summary>
		public void CompileStatements()
		{
			WriteL("<statements>");
			_indentLevel++;

			// statments: statement*
			while (_tokenizer.GetTokenType() == JackTokenizer.TokenType.KEYWORD &&
				_statementTypes.Contains(_tokenizer.GetKeyWord()))
			{
				switch (_tokenizer.GetKeyWord())
				{
					case JackTokenizer.KeyWord.LET:
						CompileLet();
						break;
					case JackTokenizer.KeyWord.DO:
						CompileDo();
						break;
					case JackTokenizer.KeyWord.IF:
						CompileIf();
						break;
					case JackTokenizer.KeyWord.WHILE:
						CompileWhile();
						break;
					case JackTokenizer.KeyWord.RETURN:
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
			_indentLevel--;
			WriteL("</doStatement>");
			MustHaveMoreTokens();
		}

		public void CompileSubroutineCall()
		{
			// subroutineCall: subroutineName '(' expressionList ')' | (className | varName) '.' subroutineName '(' expressionList ')'
			CompileName();

			if (_tokenizer.GetTokenType() != JackTokenizer.TokenType.SYMBOL)
			{
				throw new Exception("Expected a symbol. Found: " + _tokenizer.GetCurrentToken());
			}

			char symbol = _tokenizer.GetSymbol();
			if (symbol == '(')
			{
				Eat("(");
				MustHaveMoreTokens();
				CompileExpressionList();
				Eat(")");
			}
			else if (symbol == '.')
			{
				Eat(".");
				MustHaveMoreTokens();
				CompileName();
				Eat("(");
				MustHaveMoreTokens();
				CompileExpressionList();
				Eat(")");
			}
			else
			{
				throw new Exception("Expected a '(' or a '.'. Found: " + _tokenizer.GetCurrentToken());
			}
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
			CompileName();

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
			while (_tokenizer.GetTokenType() == JackTokenizer.TokenType.SYMBOL &&
				_ops.Contains(_tokenizer.GetSymbol()))
			{
				WriteSymbol();
				MustHaveMoreTokens();
				CompileTerm();
				MustHaveMoreTokens();
			}
			_indentLevel--;
			WriteL("</expression>");
		}

		private bool IsKeywordConst()
		{
			HashSet<JackTokenizer.KeyWord> keywordsConts = new()
			{
				JackTokenizer.KeyWord.TRUE,
				JackTokenizer.KeyWord.FALSE,
				JackTokenizer.KeyWord.NULL,
				JackTokenizer.KeyWord.THIS
			};

			if (_tokenizer.GetTokenType() == JackTokenizer.TokenType.KEYWORD)
			{
				JackTokenizer.KeyWord kw = _tokenizer.GetKeyWord();
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
			JackTokenizer.TokenType tokenType = _tokenizer.GetTokenType();
			if (tokenType == JackTokenizer.TokenType.INT_CONST)
			{
				WriteL("<integerConstant> " + _tokenizer.GetIntVal().ToString() + " </integerConstant>");
			}
			else if (tokenType == JackTokenizer.TokenType.STRING_CONST)
			{
				WriteL("<stringConstant> " + _tokenizer.GetStringVal() + " </stringConstant>");
			}
			else if (IsKeywordConst())
			{
				WriteKeyword();
			}
			else if (tokenType == JackTokenizer.TokenType.IDENTIFIER)
			{
				if (!_tokenizer.HasMoreTokens())
				{
					throw new Exception("Unfinished structure");
				}
				string nextChar = _tokenizer.LookOneCharAhead();
				if (nextChar == "[")
				{
					CompileName();
					Eat("[");
					MustHaveMoreTokens();
					CompileExpression();
					Eat("]");
				}
				else if (nextChar == "(" || nextChar == ".")
				{
					CompileSubroutineCall();
				}
				else
				{
					WriteIdentifier();    // case varName
				}
			}
			else if (tokenType == JackTokenizer.TokenType.SYMBOL)
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
		public void CompileExpressionList()
		{
			WriteL("<expressionList>");
			_indentLevel++;

			// expressionList: (expression (',' expression)* )?
			if (!IsSymbol(')'))
			{
				CompileExpression();

				while (IsSymbol(','))
				{
					WriteSymbol();
					MustHaveMoreTokens();
					CompileExpression();
				}
			}

			_indentLevel--;
			WriteL("</expressionList>");
		}

		public void Close()
		{
			_streamWriter.Close();
		}

		private void CompileName()
		{
			if (_tokenizer.GetTokenType() == JackTokenizer.TokenType.IDENTIFIER)
			{
				WriteIdentifier();
				MustHaveMoreTokens();
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
				JackTokenizer.TokenType tokenType = _tokenizer.GetTokenType();
				if (tokenType == JackTokenizer.TokenType.KEYWORD)
				{
					WriteKeyword();
				}
				else if (tokenType == JackTokenizer.TokenType.IDENTIFIER)
				{
					WriteIdentifier();
				}
				MustHaveMoreTokens();
			}
			else
			{
				throw new Exception("Expect a type. Found: " + _tokenizer.GetCurrentToken());
			}
		}

		private static readonly HashSet<JackTokenizer.KeyWord> _varTypes = new()
		{
			JackTokenizer.KeyWord.BOOLEAN,
			JackTokenizer.KeyWord.INT,
			JackTokenizer.KeyWord.CHAR
		};

		private void CompileVarTypeVarName()
		{
			JackTokenizer.TokenType tokenType = _tokenizer.GetTokenType();
			bool isBoolOrIntOrChar = tokenType == JackTokenizer.TokenType.KEYWORD &&
				_varTypes.Contains(_tokenizer.GetKeyWord());
			CompileType(tokenType == JackTokenizer.TokenType.IDENTIFIER || isBoolOrIntOrChar);
			CompileName();
		}

		private void Eat(string expectedToken)
		{
			JackTokenizer.TokenType tokenType = _tokenizer.GetTokenType();
			if (tokenType == JackTokenizer.TokenType.SYMBOL)
			{
				string symbol = _tokenizer.GetSymbol().ToString();
				if (symbol != expectedToken)
				{
					throw new Exception("Exptected: " + expectedToken + " Found: " + symbol);
				}
				WriteSymbol();
			}
			else if (tokenType == JackTokenizer.TokenType.KEYWORD)
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

		private void WriteIdentifier()
		{
			WriteL("<identifier> " + _tokenizer.GetIdentifier() + " </identifier>");
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
			return _tokenizer.GetTokenType() == JackTokenizer.TokenType.SYMBOL &&
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
			return _tokenizer.GetTokenType() == JackTokenizer.TokenType.KEYWORD &&
				_tokenizer.GetKeyWord().ToString().ToLower() == kw;
		}

		private void WriteKeyword()
		{
			WriteL("<keyword> " + _tokenizer.GetKeyWord().ToString().ToLower() + " </keyword>");
		}

		private void WriteL(string text)
		{
			_streamWriter.WriteLine(new string(' ', _indentLevel * 2) + text);
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
