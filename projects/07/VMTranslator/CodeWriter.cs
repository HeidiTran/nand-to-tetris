using System;
using System.Collections.Generic;
using System.IO;

namespace VMTranslator
{
	/// <summary>
	/// Generates assembly code from the parsed VM command
	/// </summary>
	class CodeWriter
	{
		private StreamWriter OsStream { get; set; }
		private string FileName { get; set; }

		private readonly Dictionary<string, string> _segmentToSymbol = new()
		{
			{ "local", "LCL" },
			{ "argument", "ARG" },
			{ "this", "THIS" },
			{ "that", "THAT" },
			{ "temp", "5" },        // temp is mapped on RAM locations 5 - 12
		};

		private int Counter { get; set; }

		public CodeWriter(string vmFilePath)
		{
			OsStream = new StreamWriter(vmFilePath.Replace(".vm", ".asm")); ;
			FileName = Path.GetFileNameWithoutExtension(vmFilePath);
			Counter = 0;
		}

		public void WriteComment(string comment)
		{
			WriteLabel("// " + comment);
		}

		/// <summary>
		/// Writes to the output file the assembly code that 
		/// implements the given arithmetic command
		/// </summary>
		/// <param name="command">Arithmetic command</param>
		public void WriteArithmetic(string command)
		{
			if (command == "add")
			{
				WriteArithmeticForOp("+");
			}
			else if (command == "sub")
			{
				WriteArithmeticForOp("-");
			}
			else if (command == "neg")
			{
				UpdateStackTopValTo("-M");
			}
			else if (command == "eq")
			{
				WriteLogicCommand("JEQ");
			}
			else if (command == "gt")
			{
				WriteLogicCommand("JGT");
			}
			else if (command == "lt")
			{
				WriteLogicCommand("JLT");
			}
			else if (command == "and")
			{
				WriteArithmeticForOp("&");
			}
			else if (command == "or")
			{
				WriteArithmeticForOp("|");
			}
			else if (command == "not")
			{
				UpdateStackTopValTo("!M");
			}
			else
			{
				throw new Exception("WriteArithmetic does not recognize " + command + " command!");
			}
		}

		/// <summary>
		/// Write assembly instructions for a arithmetic command given the operator
		/// </summary>
		/// <param name="op">Possible values: +, -, &, |</param>
		private void WriteArithmeticForOp(string op)
		{
			PopFromStackToDReg();
			WriteLine("A=A-1");
			WriteLine("M=M" + op + "D");
		}

		/// <summary>
		/// Write assembly instructions for a logic command
		/// </summary>
		/// <param name="jumpCond">Possible values: JEQ, JGT, JLT</param>
		private void WriteLogicCommand(string jumpCond)
		{
			string counterStr = Counter.ToString();
			PopFromStackToDReg();
			WriteLine("A=A-1");
			WriteLine("D=M-D");
			WriteLine("@IF_" + counterStr);
			WriteLine("D;" + jumpCond);
			WriteLine("D=0");
			WriteLine("@FINALLY_" + counterStr);
			WriteLine("0;JMP");
			WriteLabel("(IF_" + counterStr + ")");
			WriteLine("D=-1");
			WriteLabel("(FINALLY_" + counterStr + ")");
			UpdateStackTopValTo("D");
			Counter++;
		}

		/// <summary>
		/// Writes to the output file the assembly code that 
		/// implements the given command where command is either C_PUSH or C_POP
		/// </summary>
		/// <param name="commandType">C_PUSH or C_POP</param>
		/// /// <param name="segment">Possible values: local, temp, argument, this, that, constant</param>
		public void WritePushPop(CommandType commandType, string segment, int index)
		{
			if (commandType == CommandType.C_PUSH)
			{
				WritePush(segment, index);
			}
			else if (commandType == CommandType.C_POP)
			{
				WritePop(segment, index);
			}
			else
			{
				throw new Exception("WritePushPop does not allow " + commandType.ToString() + " command!");
			}
		}

		private void WritePush(string segment, int index)
		{
			if (segment == "constant")
			{
				// *SP = i, SP++
				WriteLine("@" + index); // D = i
				WriteLine("D=A");
				PushDRegToStack();
			}
			else if (segment == "pointer")
			{
				// Push the base address of THIS/THAT onto the stack
				// *SP = THIS/THAT, SP++
				if (index == 0)
				{
					WriteLine("@THIS");
				}
				else
				{
					WriteLine("@THAT");
				}

				WriteLine("D=M");   // D = THIS/THAT
				PushDRegToStack();
			}
			else if (segment == "static")
			{
				WriteLine("@" + FileName + "." + index.ToString());
				WriteLine("D=M");
				PushDRegToStack();
			}
			else
			{
				// addr = segmentPointer + i, *SP = *addr, SP++
				WriteLine("@" + index.ToString());          // D = i
				WriteLine("D=A");
				WriteLine("@" + _segmentToSymbol[segment]);

				if (segment == "temp")
				{
					WriteLine("A=A+D");
				}
				else
				{
					WriteLine("A=M+D"); // addr = segmentPointer + i
				}

				WriteLine("D=M");       // D = *addr
				PushDRegToStack();
			}
		}

		private void PushDRegToStack()
		{
			WriteLine("@SP");   // *SP = D
			WriteLine("A=M");
			WriteLine("M=D");
			WriteLine("@SP");   // SP++
			WriteLine("M=M+1");
		}

		private void WritePop(string segment, int index)
		{
			if (segment == "pointer")
			{
				// SP--, THIS/THAT = *SP
				PopFromStackToDReg();

				if (index == 0)
				{
					WriteLine("@THIS");
				}
				else
				{
					WriteLine("@THAT");
				}

				WriteLine("M=D");
			}
			else if (segment == "static")
			{
				PopFromStackToDReg();
				WriteLine("@" + FileName + "." + index.ToString());
				WriteLine("M=D");
			}
			else
			{
				// Possible segments: local, temp, argument, this, that
				// addr = segmentPointer + i, SP--, *addr = *SP
				WriteLine("@" + index.ToString());          // D = i
				WriteLine("D=A");
				WriteLine("@" + _segmentToSymbol[segment]);

				if (segment == "temp")
				{
					WriteLine("D=A+D");
				}
				else
				{
					WriteLine("D=M+D");
				}

				WriteLine("@R13");      // RAM[13] = segmentPointer + i
				WriteLine("M=D");
				PopFromStackToDReg();
				WriteLine("@R13");      // addr = RAM[13]
				WriteLine("A=M");
				WriteLine("M=D");
			}
		}

		private void PopFromStackToDReg()
		{
			WriteLine("@SP");   // SP--
			WriteLine("M=M-1");
			WriteLine("A=M");   // D = *SP
			WriteLine("D=M");
		}

		private void UpdateStackTopValTo(string newVal)
		{
			WriteLine("@SP");
			WriteLine("A=M-1");
			WriteLine("M=" + newVal);
		}

		public void Close()
		{
			OsStream.Close();
		}

		private void WriteLabel(string line)
		{
			OsStream.WriteLine(line);
		}

		/// <summary>
		/// Write an assembly instruction with 3 spaces at the beginning of the line
		/// </summary>
		private void WriteLine(string line)
		{
			OsStream.WriteLine("   " + line);
		}
	}
}
