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
			CompileName();
			if (!_tokenizer.HasMoreTokens()) return;
			_tokenizer.Advance();
			Eat("{");

			while (_tokenizer.HasMoreTokens() && _tokenizer.IsClassVarType())
			{
				CompileClassVarDec();
			}

			while (_tokenizer.HasMoreTokens() && _tokenizer.IsSubroutineKw())
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
			if (_tokenizer.IsClassVarType())
			{
				WriteTerminalElem();
			}

			if (!_tokenizer.HasMoreTokens()) return;
			_tokenizer.Advance();
			CompileVarTypeVarName();

			if (!_tokenizer.HasMoreTokens()) return;
			_tokenizer.Advance();
			while (_tokenizer.IsComma())
			{
				WriteTerminalElem();
				if (!_tokenizer.HasMoreTokens()) return;
				_tokenizer.Advance();
				WriteTerminalElem();
				if (!_tokenizer.HasMoreTokens()) return;
				_tokenizer.Advance();
			}

			Eat(";");
			_indentLevel--;
			WriteL("</classVarDec>");
		}

		/// <summary>
		/// Compiles a complete method, function, or constructor.
		/// </summary>
		public void CompileSubroutineDec()
		{
			WriteL("<subroutineDec>");
			_indentLevel++;
			if (_tokenizer.IsSubroutineKw())
			{
				WriteTerminalElem();
			}

			if (!_tokenizer.HasMoreTokens()) return;
			_tokenizer.Advance();
			// TODO: come up with a better way to check if it's a valid type
			CompileType(_tokenizer.IsSubroutineType());

			if (!_tokenizer.HasMoreTokens()) return;
			_tokenizer.Advance();
			CompileName();

			if (!_tokenizer.HasMoreTokens()) return;
			_tokenizer.Advance();
			Eat("(");

			CompileParameterList();

			Eat(")");
			CompileSubroutineBody();

			_indentLevel--;
			WriteL("</subroutineDec>");
		}

		/// <summary>
		/// Compiles a (possibly empty) parameter list. Does not handle the enclosing "()".
		/// </summary>
		public void CompileParameterList()
		{
			WriteL("<parameterList>");
			_indentLevel++;

			if (_tokenizer.HasMoreTokens() && _tokenizer.IsVarType())
			{
				CompileVarTypeVarName();
				if (!_tokenizer.HasMoreTokens()) return;
				_tokenizer.Advance();

				while (_tokenizer.IsComma())
				{
					WriteTerminalElem();
					if (!_tokenizer.HasMoreTokens()) return;
					_tokenizer.Advance();
					CompileVarTypeVarName();
					if (!_tokenizer.HasMoreTokens()) return;
					_tokenizer.Advance();
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
			Eat("{");
			while (_tokenizer.HasMoreTokens() && _tokenizer.IsSubroutineVarType())
			{
				CompileVarDec();
			}
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
			if (!_tokenizer.HasMoreTokens()) return;
			if (_tokenizer.IsSubroutineVarType())
			{
				WriteTerminalElem();
			}

			if (!_tokenizer.HasMoreTokens()) return;
			_tokenizer.Advance();
			CompileVarTypeVarName();

			if (!_tokenizer.HasMoreTokens()) return;
			_tokenizer.Advance();
			while (_tokenizer.IsComma())
			{
				WriteTerminalElem();
				if (!_tokenizer.HasMoreTokens()) return;
				_tokenizer.Advance();
				WriteTerminalElem();
				if (!_tokenizer.HasMoreTokens()) return;
				_tokenizer.Advance();
			}

			Eat(";");
			_indentLevel--;
			WriteL("</varDec>");
		}

		/// <summary>
		/// Compiles a sequence of statements. Does not handle the enclosing "()"
		/// </summary>
		public void CompileStatements()
		{
			// Uses a loop to handle 0 or more statement instances, according to the left-most token
			// if that token is "if", "while"
			// it invokes compileIf, compileWhile, ...
		}

		/// <summary>
		/// Compiles a do statement
		/// </summary>
		public void CompileDo()
		{

		}

		/// <summary>
		/// Compiles a let statement
		/// </summary>
		public void CompileLet()
		{

		}

		/// <summary>
		/// Compiles a while statement
		/// </summary>
		public void CompileWhile()
		{
			Eat("while");
			Eat("(");
			CompileExpression();
			Eat(")");
			// more...
		}

		/// <summary>
		/// Compiles a return statement
		/// </summary>
		public void CompileReturn()
		{

		}

		/// <summary>
		/// Compiles an if statement, possible with a trailing else clause
		/// </summary>
		public void CompileIf()
		{

		}

		/// <summary>
		/// Compiles an expression.
		/// </summary>
		public void CompileExpression()
		{

		}

		/// <summary>
		/// Compiles a term. If the current token is an identifier, the routine 
		/// must distinguish between a variable, an array entry
		/// or a subroutine call.
		/// </summary>
		public void CompileTerm()
		{

		}

		/// <summary>
		/// Compiles a (possibly empty) comma-separated list of expressions.
		/// </summary>
		public void CompileExpressionList()
		{

		}

		private void CompileName()
		{
			if (_tokenizer.GetTokenType() == JackTokenizer.TokenType.IDENTIFIER)
			{
				WriteTerminalElem();
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

			if (!_tokenizer.HasMoreTokens()) return;
			_tokenizer.Advance();
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
			if (!_tokenizer.HasMoreTokens()) return;
			_tokenizer.Advance();
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
	}
}
