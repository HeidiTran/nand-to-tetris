using System;
using System.IO;

namespace VMTranslator
{
	class VMTranslator
	{
		static void Main(string[] args)
		{
			// string inputPath = args[0];

			// For debugging in VisualStudio
			//string inputPath = @"C:\Projects\nand-to-tetris\projects\08\FunctionCalls\FibonacciElement";
			//string inputPath = @"C:\Projects\nand-to-tetris\projects\08\FunctionCalls\NestedCall";
			string inputPath = @"C:\Projects\nand-to-tetris\projects\08\FunctionCalls\StaticsTest";

			//string t = "SimpleFunction";
			//string inputPath = @"C:\Projects\nand-to-tetris\projects\08\FunctionCalls\" + t + @"\" + t + ".vm";
			//string inputPath = @"C:\Projects\nand-to-tetris\projects\07\MemoryAccess\" + t + @"\" + t + ".vm";
			////////////////////////////////

			// VMTranslator.exe BasicTest.vm

			bool isDir = Directory.Exists(inputPath);
			string outputFilePath = isDir ?
				inputPath + Path.DirectorySeparatorChar + new DirectoryInfo(inputPath).Name + ".asm" : 
				inputPath.Replace(".vm", ".asm");

			CodeWriter codeWriter = new(outputFilePath);

			if (!isDir)
			{
				string vmFile = inputPath;
				TranslateVmFile(codeWriter, vmFile);
			}
			else
			{
				codeWriter.WriteInit();	// Only generating startup code for directory
				string[] vmFiles = Directory.GetFiles(inputPath, "*.vm");
				foreach (string vmFile in vmFiles)
				{
					TranslateVmFile(codeWriter, vmFile);
				}
			}

			codeWriter.Close();
		}

		private static void TranslateVmFile(CodeWriter codeWriter, string vmFile)
		{
			Parser parser = new(vmFile);
			codeWriter.SetFileName(vmFile);

			while (parser.HasMoreCommands())
			{
				parser.Advance();
				if (parser.IsComment() || parser.IsEmptyLine())
				{
					continue;
				}

				codeWriter.WriteComment(parser.CurrentCommand);
				switch (parser.GetCurrentCommandType())
				{
					case CommandType.C_ARITHMETIC:
						codeWriter.WriteArithmetic(parser.GetArg1());
						break;
					case CommandType.C_PUSH:
						codeWriter.WritePushPop(parser.GetCurrentCommandType(), parser.GetArg1(), parser.GetArg2());
						break;
					case CommandType.C_POP:
						codeWriter.WritePushPop(parser.GetCurrentCommandType(), parser.GetArg1(), parser.GetArg2());
						break;
					case CommandType.C_LABEL:
						codeWriter.WriteLabel(parser.GetArg1());
						break;
					case CommandType.C_GOTO:
						codeWriter.WriteGoto(parser.GetArg1());
						break;
					case CommandType.C_IF:
						codeWriter.WriteIf(parser.GetArg1());
						break;
					case CommandType.C_FUNCTION:
						codeWriter.WriteFunction(parser.GetArg1(), parser.GetArg2());
						break;
					case CommandType.C_RETURN:
						codeWriter.WriteReturn();
						break;
					case CommandType.C_CALL:
						codeWriter.WriteCall(parser.GetArg1(), parser.GetArg2());
						break;
					default:
						break;
				}
			}

			parser.Close();
		}
	}
}
