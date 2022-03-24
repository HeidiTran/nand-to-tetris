namespace VMTranslator
{
	class VMTranslator
	{
		static void Main(string[] args)
		{
			// For debugging in VisualStudio
			string t = "StackTest";
			args[0] = @"C:\Projects\nand-to-tetris\projects\07\StackArithmetic\" + t + @"\" + t + ".vm";
			////////////////////////////////

			// VMTranslator.exe BasicTest.vm
			Parser parser = new(args[0]);
			CodeWriter codeWriter = new(args[0]);

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
						break;
					case CommandType.C_GOTO:
						break;
					case CommandType.C_IF:
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
