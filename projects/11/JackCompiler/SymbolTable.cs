using System;
using System.Collections.Generic;

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
		public enum Kind
		{
			STATIC,
			FIELD,
			ARG,
			VAR,
			NONE
		}

		Dictionary<string, Tuple<string, Kind, int>> _classST;
		Dictionary<string, Tuple<string, Kind, int>> _subroutineST;

		public SymbolTable()
		{
			_classST = new();
			_subroutineST = new();
		}

		/// <summary>
		/// Starts a new subroutine scope (i.e. resets the subroutine's symbol table)
		/// </summary>
		public void StartSubroutine()
		{
			_subroutineST.Clear();
		}

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
			int res = 0;
			if (IsClassScope(kind))
			{
				foreach (var row in _classST)
				{
					if (row.Value.Item2 == kind)
					{
						res++;
					}
				}
			}
			else if (IsSubroutineScope(kind))
			{
				foreach (var row in _subroutineST)
				{
					if (row.Value.Item2 == kind)
					{
						res++;
					}
				}
			}
			else
			{
				throw new Exception("NONE is not a valid kind");
			}
			return res;
		}

		/// <summary>
		/// Returns the kind of the named identifier in the current scope. 
		/// If the identifier is unknown in the current scope, returns NONE
		/// </summary>
		public Kind KindOf(string name)
		{
			if (_subroutineST.ContainsKey(name))
			{
				return _subroutineST[name].Item2;
			} else if (_classST.ContainsKey(name))
			{
				return _classST[name].Item2;
			}

			return Kind.NONE;
		}

		/// <summary>
		/// Returns the type of the named identifier in the current scope
		/// </summary>
		public string TypeOf(string name)
		{
			if (_subroutineST.ContainsKey(name))
			{
				return _subroutineST[name].Item1;
			}
			else if (_classST.ContainsKey(name))
			{
				return _classST[name].Item1;
			}

			return string.Empty;
		}

		/// <summary>
		/// Returns the index assigned to the named identifier
		/// </summary>
		public int IndexOf(string name)
		{
			if (_subroutineST.ContainsKey(name))
			{
				return _subroutineST[name].Item3;
			}
			else if (_classST.ContainsKey(name))
			{
				return _classST[name].Item3;
			}

			return 0;
		}

		private static bool IsClassScope(Kind kind)
		{
			return kind == Kind.STATIC || kind == Kind.FIELD;
		}

		private static bool IsSubroutineScope(Kind kind)
		{
			return kind == Kind.ARG || kind == Kind.VAR;
		}
	}
}
