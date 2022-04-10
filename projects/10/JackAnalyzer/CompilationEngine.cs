using System;
using System.IO;

/// <summary>
/// Generates the compiler's output
/// </summary>
namespace JackAnalyzer
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

		/// <summary>
		/// Compiles a complete class.
		/// </summary>
		public void CompileClass()
		{
			if (!_tokenizer.HasMoreTokens()) return;
			_tokenizer.Advance();
			WriteL("<class>");
			_indentLevel++;

			Eat("class");
			MustHaveMoreTokens();
			CompileName();
			Eat("{");
			MustHaveMoreTokens();
			while (_tokenizer.IsClassVarType())
			{
				WriteL("<classVarDec>");
				_indentLevel++;
				WriteTerminalElem();
				MustHaveMoreTokens();
				CompileClassVarDec();
				_indentLevel--;
				WriteL("</classVarDec>");
				MustHaveMoreTokens();
			}

			while (_tokenizer.IsSubroutineKw())
			{
				WriteL("<subroutineDec>");
				_indentLevel++;
				WriteTerminalElem();
				MustHaveMoreTokens();
				CompileSubroutineDec();
				_indentLevel--;
				WriteL("</subroutineDec>");
				MustHaveMoreTokens();
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
			CompileVarTypeVarName();

			while (_tokenizer.IsComma())
			{
				WriteTerminalElem();    // ,
				MustHaveMoreTokens();
				WriteTerminalElem();    // identifier
				MustHaveMoreTokens();
			}
			Eat(";");
		}

		/// <summary>
		/// Compiles a complete method, function, or constructor.
		/// </summary>
		public void CompileSubroutineDec()
		{
			// TODO: come up with a better way to check if it's a valid type
			CompileType(_tokenizer.IsSubroutineType());
			CompileName();
			Eat("(");
			MustHaveMoreTokens();

			WriteL("<parameterList>");
			_indentLevel++;
			CompileParameterList();
			_indentLevel--;
			WriteL("</parameterList>");

			Eat(")");
			MustHaveMoreTokens();

			WriteL("<subroutineBody>");
			_indentLevel++;
			CompileSubroutineBody();
			_indentLevel--;
			WriteL("</subroutineBody>");
		}

		/// <summary>
		/// Compiles a (possibly empty) parameter list. Does not handle the enclosing "()".
		/// </summary>
		public void CompileParameterList()
		{
			if (_tokenizer.IsVarType())
			{
				CompileVarTypeVarName();

				while (_tokenizer.IsComma())
				{
					WriteTerminalElem();    // ,
					MustHaveMoreTokens();
					CompileVarTypeVarName();
				}
			}
		}

		/// <summary>
		/// Compiles a subroutine's body.
		/// </summary>
		public void CompileSubroutineBody()
		{
			Eat("{");
			MustHaveMoreTokens();
			while (_tokenizer.IsSubroutineVarType())
			{
				WriteL("<varDec>");
				_indentLevel++;
				WriteTerminalElem();    // var
				MustHaveMoreTokens();
				CompileVarDec();
				_indentLevel--;
				WriteL("</varDec>");
				MustHaveMoreTokens();
			}

			CompileStatements();
			Eat("}");
		}

		/// <summary>
		/// Compiles a var declaration.
		/// </summary>
		public void CompileVarDec()
		{
			CompileVarTypeVarName();

			while (_tokenizer.IsComma())
			{
				WriteTerminalElem();    // ,
				MustHaveMoreTokens();
				WriteTerminalElem();    // identifier
				MustHaveMoreTokens();
			}
			Eat(";");
		}

		/// <summary>
		/// Compiles a sequence of statements. Does not handle the enclosing "()"
		/// </summary>
		public void CompileStatements()
		{
			WriteL("<statements>");
			_indentLevel++;

			while (_tokenizer.IsStatementType())
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
			if (_tokenizer.GetTokenType() == JackTokenizer.TokenType.IDENTIFIER)
			{
				WriteTerminalElem();
				MustHaveMoreTokens();
				if (_tokenizer.IsOpenParen())
				{
					Eat("(");
					MustHaveMoreTokens();
					CompileExpressionList();
					Eat(")");
				}
				else if (_tokenizer.IsDot())
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
			else
			{
				throw new Exception("Subroutine call must begin with a function name, class name or a var name");
			}
		}

		/// <summary>
		/// Compiles a let statement
		/// </summary>
		public void CompileLet()
		{
			WriteL("<letStatement>");
			_indentLevel++;
			Eat("let");
			MustHaveMoreTokens();
			CompileName();

			while (_tokenizer.IsOpenSquareBracket())
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
			Eat("return");
			MustHaveMoreTokens();
			if (_tokenizer.IsSemiColon())
			{
				WriteTerminalElem();    // ;
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

			if (_tokenizer.IsElseKw())
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

		/// <summary>
		/// Compiles an expression.
		/// </summary>
		public void CompileExpression()
		{
			WriteL("<expression>");
			_indentLevel++;

			CompileTerm();
			MustHaveMoreTokens();
			while (_tokenizer.IsOp())
			{
				WriteTerminalElem();
				MustHaveMoreTokens();
				CompileTerm();
				MustHaveMoreTokens();
			}
			_indentLevel--;
			WriteL("</expression>");
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
			if (tokenType == JackTokenizer.TokenType.INT_CONST ||
				tokenType == JackTokenizer.TokenType.STRING_CONST ||
				_tokenizer.IsKeywordConst())
			{
				WriteTerminalElem();
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
					WriteTerminalElem();    // varName
					MustHaveMoreTokens();
					Eat("[");
					MustHaveMoreTokens();
					CompileExpression();
					Eat("]");
				} else if (nextChar == "(" || nextChar == ".") {
					CompileSubroutineCall();
				} else
				{
					WriteTerminalElem();    // case varName
				}
			}
			else if (_tokenizer.IsOpenParen())
			{
				Eat("(");
				MustHaveMoreTokens();
				CompileExpression();
				Eat(")");
			}
			else if (_tokenizer.IsUnaryOp())
			{
				WriteTerminalElem();
				MustHaveMoreTokens();
				CompileTerm();
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

			if (!_tokenizer.IsCloseParen())
			{
				CompileExpression();

				while (_tokenizer.IsComma())
				{
					WriteTerminalElem();    // ,
					MustHaveMoreTokens();
					CompileExpression();
				}
			}

			_indentLevel--;
			WriteL("</expressionList>");
		}

		private void CompileName()
		{
			if (_tokenizer.GetTokenType() == JackTokenizer.TokenType.IDENTIFIER)
			{
				WriteTerminalElem();
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
				WriteTerminalElem();
				MustHaveMoreTokens();
			}
			else
			{
				throw new Exception("Expect a type. Found: " + _tokenizer.GetCurrentToken());
			}
		}

		private void CompileVarTypeVarName()
		{
			// TODO: come up with a better way to check if it's a valid type
			CompileType(_tokenizer.IsVarType());
			CompileName();
		}

		private void Eat(string expectedToken)
		{
			string curToken = _tokenizer.GetCurrentToken();
			if (curToken != expectedToken)
			{
				throw new Exception("Exptected: " + expectedToken + " Found: " + curToken);
			}

			WriteTerminalElem();
		}

		private void WriteTerminalElem()
		{
			string curToken = _tokenizer.GetCurrentToken();
			string tokenTypeStr = JackTokenizer.TokenTypeStr[_tokenizer.GetTokenType()];
			WriteL("<" + tokenTypeStr + "> " + curToken + " </" + tokenTypeStr + ">");
		}

		private void WriteL(string text)
		{
			//_streamWriter.WriteLine(new string(' ', indentLevel * 2) + text);
			Console.WriteLine(new string(' ', _indentLevel * 2) + text);
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
