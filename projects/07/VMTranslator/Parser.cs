using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace VMTranslator
{
	/// <summary>
	/// Handle the parsing of a single .vm file
	/// </summary>
	class Parser
	{
		private StreamReader IsStream { get; set; }

		public string CurrentCommand { get; private set; }

		private readonly HashSet<string> _arithmeticCommands = new() { "add", "sub", "neg", "eq", "gt", "lt" };

		private readonly HashSet<string> _logicalCommands = new() { "and", "or", "not" };

		private readonly Dictionary<string, CommandType> _commandToCommandType = new()
		{
			{ "pop", CommandType.C_POP },
			{ "push", CommandType.C_PUSH },
			{ "label", CommandType.C_LABEL },
			{ "goto", CommandType.C_IF },
			{ "function", CommandType.C_FUNCTION },
			{ "call", CommandType.C_CALL },
			{ "return", CommandType.C_RETURN },
		};

		public Parser(string vmFilePath)
		{
			IsStream = new StreamReader(vmFilePath);
			CurrentCommand = string.Empty;
		}

		public bool HasMoreCommands()
		{
			return !IsStream.EndOfStream;
		}

		/// <summary>
		/// Reads the next command from the input and makes it the current command.
		/// Should be called only if HasMoreCommands() is true
		/// Initially there is no current command.
		/// </summary>
		public void Advance()
		{
			CurrentCommand = IsStream.ReadLine().Trim();
		}

		/// <summary>
		/// Returns a constant representing the type of the current command
		/// </summary>
		public CommandType GetCurrentCommandType()
		{
			string[] tokens = CurrentCommand.Split(" ");
			if (IsArithmeticCommand())
			{
				return CommandType.C_ARITHMETIC;
			}
			else if (_commandToCommandType.ContainsKey(tokens[0]))
			{
				return _commandToCommandType[tokens[0]];
			}

			throw new Exception("Unknown command type");
		}

		/// <summary>
		/// Returns the 1st arg of the current command.
		/// In the case of C_ARITHMETIC the command itself (add, sub, etc) is returned.
		/// Should not be called if the current command is C_RETURN.
		/// </summary>
		public string GetArg1()
		{
			if (GetCurrentCommandType() == CommandType.C_RETURN)
			{
				throw new Exception("GetArg1 should not be called for C_RETURN command!");
			}

			string[] tokens = CurrentCommand.Split(" ");
			return GetCurrentCommandType() == CommandType.C_ARITHMETIC ? tokens[0] : tokens[1];
		}

		/// <summary>
		/// Returns the 2nd arg of the current command.
		/// Should be called only if the current command is C_PUSH, C_POP, C_FUNCTION, or C_CALL
		/// </summary>
		public int GetArg2()
		{
			CommandType commandType = GetCurrentCommandType();
			string[] tokens = CurrentCommand.Split(" ");
			if (commandType == CommandType.C_PUSH ||
				commandType == CommandType.C_POP ||
				commandType == CommandType.C_FUNCTION ||
				commandType == CommandType.C_CALL)
			{
				return int.Parse(tokens[2]);
			}

			throw new Exception("GetArg2 should not be called for " + commandType.ToString() + " command!");
		}

		public void Close()
		{
			IsStream.Close();
		}

		public bool IsComment()
		{
			return CurrentCommand.StartsWith("//");
		}

		public bool IsEmptyLine()
		{
			return CurrentCommand == string.Empty;
		}

		bool IsArithmeticCommand()
		{
			return _arithmeticCommands.Contains(CurrentCommand) || _logicalCommands.Contains(CurrentCommand);
		}

	}
}
