namespace VMTranslator
{
	class VMTranslator
	{
		static void Main(string[] args)
		{
			// string vmFilePath = args[0];

			// For debugging in VisualStudio
			//string t = "BasicLoop";
			//string vmFilePath = @"C:\Projects\nand-to-tetris\projects\08\ProgramFlow\" + t + @"\" + t + ".vm";

			string t = "StackTest";
			string vmFilePath = @"C:\Projects\nand-to-tetris\projects\07\StackArithmetic\" + t + @"\" + t + ".vm";
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
						break;
					case CommandType.C_IF:
						codeWriter.WriteIf(parser.GetArg1());
						break;
					case CommandType.C_FUNCTION:
						break;
					case CommandType.C_RETURN:
						break;
					case CommandType.C_CALL:
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
