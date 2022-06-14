using System;
using System.Collections.Generic;
using System.Linq;
using static JackCompiler.Kind;

namespace JackCompiler
{
	/// <summary>
	/// Symbol table associates the identifier names found in the program with identifier properties needed for compilation: type, kind, and running index
	/// Each symbol has a scope from which it's visible in the source code
	/// The symbol table gives each symbol a running number (index) within the scope
	/// The index starts at 0, increments by 1 each time an identifier is added to the table
	/// and resets to 0 when starting a new scope.
	/// Type of identifiers: int, char, boolean, class name
	/// Kinds of identifiers: static, field, argument, var
	/// Scope of identifiers: class level, subroutine level
	/// </summary>
	class SymbolTable
	{
		private readonly Dictionary<string, Tuple<string, Kind, int>> _classST = new();
		private readonly Dictionary<string, Tuple<string, Kind, int>> _subroutineST = new();

		/// <summary>
		/// Starts a new subroutine scope (i.e. resets the subroutine's symbol table)
		/// </summary>
		public void StartSubroutine() => _subroutineST.Clear();

		/// <summary>
		/// Defines a new identifier and assigns it a running index
		/// STATIC and FIELD identifiers have a class scope, while ARG and VAR identifiers have a subroutine scope
		/// </summary>
		/// <param name="name">identifier name</param>
		/// <param name="type">identifier type</param>
		/// <param name="kind">identifier kind</param>
		public void Define(string name, string type, Kind kind)
		{
			Tuple<string, Kind, int> properties = new(type, kind, VarCount(kind));

			if (IsClassScope(kind))
			{
				_classST.Add(name, properties);
			}
			else if (IsSubroutineScope(kind))
			{
				_subroutineST.Add(name, properties);
			} else
			{
				throw new Exception("NONE is not a valid kind");
			}
		}

		/// <summary>
		/// Returns the number of variables of the given kind already define in the current scope
		/// </summary>
		public int VarCount(Kind kind)
		{
			if (IsClassScope(kind))
			{
				return _classST.Where(row => row.Value.Item2 == kind).Count();
			}
			
			if (IsSubroutineScope(kind))
			{
				return _subroutineST.Where(row => row.Value.Item2 == kind).Count();
			}

			throw new Exception("NONE is not a valid kind");
		}

		/// <summary>
		/// Returns the kind of the named identifier in the current scope. 
		/// If the identifier is unknown in the current scope, returns NONE
		/// </summary>
		public Kind KindOf(string name)
		{
			if (_subroutineST.TryGetValue(name, out var subroutinVar))
			{
				return subroutinVar.Item2;
			}

			if (_classST.TryGetValue(name, out var classVar))
			{
				return classVar.Item2;
			}

			return NONE;
		}

		/// <summary>
		/// Returns the type of the named identifier in the current scope
		/// </summary>
		public string TypeOf(string name)
		{
			if (_subroutineST.TryGetValue(name, out var subroutinVar))
			{
				return subroutinVar.Item1;
			}

			if (_classST.TryGetValue(name, out var classVar))
			{
				return classVar.Item1;
			}

			return string.Empty;
		}

		/// <summary>
		/// Returns the index assigned to the named identifier
		/// </summary>
		public int IndexOf(string name)
		{
			if (_subroutineST.TryGetValue(name, out var subroutinVar))
			{
				return subroutinVar.Item3;
			}

			if (_classST.TryGetValue(name, out var classVar))
			{
				return classVar.Item3;
			}

			return 0;
		}

		public bool IsInSymbolTable(string name) 
			=> _subroutineST.ContainsKey(name) || _classST.ContainsKey(name);

		private static bool IsClassScope(Kind kind) => kind == STATIC || kind == FIELD;

		private static bool IsSubroutineScope(Kind kind) => kind == ARG || kind == VAR;
	}
}