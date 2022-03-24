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

		/// <summary>
		/// Informs the class that the translation of a new VM file has started
		/// Called by the Main prog of the VM Translator
		/// </summary>
		/// <param name="fileName">Name of the VM file</param>
		public void SetFileName(string fileName)
		{
			// TODO
		}

		/// <summary>
		/// Writes the assembly instructions that effect the bootstrap code that initializes the VM.
		/// This code must be placed at the beginning of the generated *.asm file.
		/// </summary>
		public void WriteInit()
		{

		}

		public void WriteComment(string comment)
		{
			WriteLblIns("// " + comment);
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
			WriteIns("A=A-1");
			WriteIns("M=M" + op + "D");
		}

		/// <summary>
		/// Write assembly instructions for a logic command
		/// </summary>
		/// <param name="jumpCond">Possible values: JEQ, JGT, JLT</param>
		private void WriteLogicCommand(string jumpCond)
		{
			string counterStr = Counter.ToString();
			PopFromStackToDReg();
			WriteIns("A=A-1");
			WriteIns("D=M-D");
			WriteIns("@IF_" + counterStr);
			WriteIns("D;" + jumpCond);
			WriteIns("D=0");
			WriteIns("@FINALLY_" + counterStr);
			WriteIns("0;JMP");
			WriteLblIns("(IF_" + counterStr + ")");
			WriteIns("D=-1");
			WriteLblIns("(FINALLY_" + counterStr + ")");
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
				WriteIns("@" + index); // D = i
				WriteIns("D=A");
				PushDRegToStack();
			}
			else if (segment == "pointer")
			{
				// Push the base address of THIS/THAT onto the stack
				// *SP = THIS/THAT, SP++
				if (index == 0)
				{
					WriteIns("@THIS");
				}
				else
				{
					WriteIns("@THAT");
				}

				WriteIns("D=M");   // D = THIS/THAT
				PushDRegToStack();
			}
			else if (segment == "static")
			{
				WriteIns("@" + FileName + "." + index.ToString());
				WriteIns("D=M");
				PushDRegToStack();
			}
			else
			{
				// addr = segmentPointer + i, *SP = *addr, SP++
				WriteIns("@" + index.ToString());          // D = i
				WriteIns("D=A");
				WriteIns("@" + _segmentToSymbol[segment]);

				if (segment == "temp")
				{
					WriteIns("A=A+D");
				}
				else
				{
					WriteIns("A=M+D"); // addr = segmentPointer + i
				}

				WriteIns("D=M");       // D = *addr
				PushDRegToStack();
			}
		}

		private void PushDRegToStack()
		{
			WriteIns("@SP");   // *SP = D
			WriteIns("A=M");
			WriteIns("M=D");
			WriteIns("@SP");   // SP++
			WriteIns("M=M+1");
		}

		private void WritePop(string segment, int index)
		{
			if (segment == "pointer")
			{
				// SP--, THIS/THAT = *SP
				PopFromStackToDReg();

				if (index == 0)
				{
					WriteIns("@THIS");
				}
				else
				{
					WriteIns("@THAT");
				}

				WriteIns("M=D");
			}
			else if (segment == "static")
			{
				PopFromStackToDReg();
				WriteIns("@" + FileName + "." + index.ToString());
				WriteIns("M=D");
			}
			else
			{
				// Possible segments: local, temp, argument, this, that
				// addr = segmentPointer + i, SP--, *addr = *SP
				WriteIns("@" + index.ToString());          // D = i
				WriteIns("D=A");
				WriteIns("@" + _segmentToSymbol[segment]);

				if (segment == "temp")
				{
					WriteIns("D=A+D");
				}
				else
				{
					WriteIns("D=M+D");
				}

				WriteIns("@R13");      // RAM[13] = segmentPointer + i
				WriteIns("M=D");
				PopFromStackToDReg();
				WriteIns("@R13");      // addr = RAM[13]
				WriteIns("A=M");
				WriteIns("M=D");
			}
		}

		private void PopFromStackToDReg()
		{
			WriteIns("@SP");   // SP--
			WriteIns("M=M-1");
			WriteIns("A=M");   // D = *SP
			WriteIns("D=M");
		}

		/// <summary>
		/// Writes assembly code that effects the `label` command
		/// </summary>
		/// <param name="label">name of the label</param>
		public void WriteLabel(string label)
		{
			WriteLblIns("(" + label + ")");
		}

		/// <summary>
		/// Writes assembly code that effects the goto command
		/// `goto label` jumps to execute the command just after `label`
		/// </summary>
		/// <param name="label">the label to go to</param>
		public void WriteGoto(string label)
		{
			// TODO
		}

		/// <summary>
		/// Writes assembly code that effects the `if-goto` command
		/// `cond` = pop the top value off the stack
		/// `if-goto label` jumps to execute the command just after `label` if `cond` is true
		/// </summary>
		/// <param name="label">the label to go to</param>
		public void WriteIf(string label)
		{
			PopFromStackToDReg();
			WriteIns("@" + label);
			WriteIns("D;JNE");		// If true = not equal to 0
		}

		/// <summary>
		/// Write assembly code that effects the `function` command
		/// </summary>
		/// <param name="functionName">name of the function</param>
		/// <param name="numVars">number of params</param>
		public void WriteFunction(string functionName, int numVars)
		{
			// TODO
		}

		/// <summary>
		/// Write assembly code that effects the `call` command
		/// </summary>
		/// <param name="functionName"></param>
		/// <param name="numArgs"></param>
		public void WriteCall(string functionName, int numArgs)
		{
			// TODO
		}

		/// <summary>
		/// Write assembly code that effects the `return` command
		/// </summary>
		public void WriteReturn()
		{
			// TODO
		}

		private void UpdateStackTopValTo(string newVal)
		{
			WriteIns("@SP");
			WriteIns("A=M-1");
			WriteIns("M=" + newVal);
		}

		public void Close()
		{
			OsStream.Close();
		}

		private void WriteLblIns(string line)
		{
			OsStream.WriteLine(line);
		}

		/// <summary>
		/// Write an assembly instruction with 3 spaces at the beginning of the line
		/// </summary>
		private void WriteIns(string line)
		{
			OsStream.WriteLine("   " + line);
		}
	}
}
