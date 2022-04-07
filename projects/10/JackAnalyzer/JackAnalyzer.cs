using System;
using static JackAnalyzer.JackTokenizer;

namespace JackAnalyzer
{
	class JackAnalyzer 
	{
		static void Main(string[] args)
		{
			// Input: a single fileName.jack or a dir

			/*
			 For each file, goes through the following logic:
			- Creates a JackTokenizer from fileName.jack
			- Creates an output file name fileName.xml
			- Creates and uses a CompilationEngine to compile the input JackTokenizer into the output file
			 */

			// JackAnalyzer.exe Main.jack
			// string inputPath = args[0];

			// For debugging in VisualStudio
			//string inputPath = @"C:\Projects\nand-to-tetris\projects\10\ArrayTest\Main.jack";
			//string inputPath = @"C:\Projects\nand-to-tetris\projects\10\Square\Main.jack";
			//string inputPath = @"C:\Projects\nand-to-tetris\projects\10\Square\Square.jack";
			string inputPath = @"C:\Projects\nand-to-tetris\projects\10\Square\SquareGame.jack";
			//string inputPath = @"C:\Projects\nand-to-tetris\projects\10\ExpressionLessSquare\test.jack";
			JackTokenizer tokenizer = new(inputPath);

			Console.WriteLine("<tokens>");
			while (tokenizer.HasMoreTokens())
			{
				tokenizer.Advance();
				string tokenTypeStr = TokenTypeStr[tokenizer.GetTokenType()];
				Console.WriteLine("<" + tokenTypeStr + "> " + tokenizer.GetCurrentToken() + " </" + tokenTypeStr + ">");
			}
			Console.WriteLine("</tokens>");
		}
	}
}
