using System.IO;

namespace JackAnalyzer
{
	class JackAnalyzer 
	{
		static void Main(string[] args)
		{
			// Input: a single fileName.jack or a dir
			// JackAnalyzer.exe Main.jack
			string inputPath = args[0];

			// For debugging in VisualStudio
			// string inputPath = @"C:\Projects\nand-to-tetris\projects\10\ArrayTest";
			//string inputPath = @"C:\Projects\nand-to-tetris\projects\10\ExpressionLessSquare\test.jack";

			bool isDir = Directory.Exists(inputPath);
			if (!isDir)
			{
				CompileJackFile(inputPath);
			}
			else
			{
				string[] jackFiles = Directory.GetFiles(inputPath, "*.jack");
				foreach (string jackFile in jackFiles)
				{
					CompileJackFile(jackFile);
				}
			}
		}

		private static void CompileJackFile(string inputPath)
		{
			JackTokenizer tokenizer = new(inputPath);
			CompilationEngine compilationEngine = new(tokenizer, inputPath.Replace(".jack", ".xml"));
			compilationEngine.CompileClass();
			tokenizer.Close();
			compilationEngine.Close();
		}
	}
}
