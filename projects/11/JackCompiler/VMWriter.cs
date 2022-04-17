using System;
using System.IO;

namespace JackCompiler
{
	/// <summary>
	/// Emits VM commands into a file, using the VM command syntax
	/// </summary>
	class VMWriter
	{
		private readonly StreamWriter _streamWriter;

		public VMWriter(string vmFilePath)
		{
			_streamWriter = new StreamWriter(vmFilePath);
		}

		/// <summary>
		/// Push the value of segment[index] onto the stack
		/// </summary>
		/// <param name="idx"></param>
		public void WritePush(Segment segment, int idx)
		{
			WriteL($"push {segment.ToString().ToLower()} {idx}");
		}

		/// <summary>
		/// Pop the top stack value and store it in segment[index]
		/// </summary>
		/// <param name="segment">Memory segment</param>
		/// <param name="idx">index</param>
		public void WritePop(Segment segment, int idx)
		{
			WriteL($"pop {segment.ToString().ToLower()} {idx}");
		}

		/// <summary>
		/// Writes a VM arithmetic command.
		/// </summary>
		/// <param name="command">Type of command</param>
		public void WriteArithmetic(Command command)
		{
			WriteL(command.ToString().ToLower());
		}

		/// <summary>
		/// This command labels the current location in the function's code
		/// </summary>
		public void WriteLabel(string label)
		{
			WriteL($"label {label}");
		}

		/// <summary>
		/// This command effects an unconditional goto operation, causing execution 
		/// to continue from the location marked by the label.
		/// The jump destination must be located in the same function.
		/// </summary>
		public void WriteGoto(string label)
		{
			WriteL($"goto {label}");
		}

		/// <summary>
		/// This command effects a conditional goto operation.
		/// The stack's topmost value is popped if the val is NOT zero, execution continues from the location marked by the label
		/// Otherwise, execution continues from the next command in the program.
		/// The jump destination must be location in the same function
		/// </summary>
		public void WriteIf(string label)
		{
			WriteL($"if-goto {label}");
		}

		/// <summary>
		/// Writes a VM call command
		/// </summary>
		/// <param name="name">ClassName.SubroutineName</param>
		/// <param name="nArgs">Number of argument.</param>
		public void WriteCall(string name, int nArgs)
		{
			WriteL($"call {name} {nArgs}");
		}

		/// <summary>
		/// Writes a VM function command
		/// </summary>
		/// <param name="name">ClassName.SubroutineName</param>
		/// <param name="nLocals">Number of local variables</param>
		public void WriteFunction(string name, int nLocals)
		{
			WriteL($"function {name} {nLocals}");
		}

		/// <summary>
		/// Transfer control back to the calling function
		/// </summary>
		public void WriteReturn()
		{
			WriteL("return");
		}

		/// <summary>
		/// Closes the output file
		/// </summary>
		public void Close()
		{
			_streamWriter.Close();
		}

		private void WriteL(string text)
		{
			_streamWriter.WriteLine(text);
			//Console.WriteLine(text);
		}
	}
}
