using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

/// <summary>
/// Generates the compiler's output
/// </summary>
namespace JackAnalyzer
{
	class CompilationEngine
	{
		private StreamReader StreamReader;
		private StreamWriter _streamWriter;

		public CompilationEngine(string jackFilePath, string xmlFilePath)
		{
			_streamWriter = new StreamWriter(xmlFilePath);
			// TODO: The next function call must be CompileClass
		}

		/// <summary>
		/// Compiles a complete class.
		/// </summary>
		public void CompileClass()
		{

		}

		/// <summary>
		/// Compiles a static variable declaration, or a field declaration.
		/// </summary>
		public void CompileClassVarDec()
		{

		}

		/// <summary>
		/// Compiles a complete mehtod, function, or constructor.
		/// </summary>
		public void CompileSubroutineDec()
		{

		}

		/// <summary>
		/// Compiles a (possibly empty) parameter list. Does not handle the enclosing "()".
		/// </summary>
		public void CompileParameterList()
		{

		}

		/// <summary>
		/// Compiles a subroutin's body.
		/// </summary>
		public void CompileSubroutineBody()
		{

		}

		/// <summary>
		/// Compiles a var declaration.
		/// </summary>
		public void CompileVarDec()
		{

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

		/// <summary>
		/// Compiles a let statement
		/// </summary>
		public void CompileLet()
		{

		}

		/// <summary>
		/// Compiles an if statement, possible with a trailing else clause
		/// </summary>
		public void CompileIf()
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
		/// Compiles a do statement
		/// </summary>
		public void CompileDo()
		{

		}

		/// <summary>
		/// Compiles a return statement
		/// </summary>
		public void CompileReturn()
		{

		}

		private void Eat(string expectedToken)
		{
			// if currentToken != expectedToken
			// throw error
			// else
			// advance to the next token
		}
	}
}
