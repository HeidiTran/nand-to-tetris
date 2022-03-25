namespace VMTranslator
{
	class VMTranslator
	{
		static void Main(string[] args)
		{
			// string vmFilePath = args[0];

			// For debugging in VisualStudio
			string t = "SimpleFunction";
			string vmFilePath = @"C:\Projects\nand-to-tetris\projects\08\FunctionCalls\" + t + @"\" + t + ".vm";

			//string vmFilePath = @"C:\Projects\nand-to-tetris\projects\07\MemoryAccess\" + t + @"\" + t + ".vm";
			////////////////////////////////

			// VMTranslator.exe BasicTest.vm
			Parser parser = new(vmFilePath);
			CodeWriter codeWriter = new(vmFilePath);

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
			codeWriter.Close();
		}
	}
}
