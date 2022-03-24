using System;
using System.Collections.Generic;
using System.Text;

namespace VMTranslator
{
	enum CommandType
	{
		C_ARITHMETIC, // is returned for all arithmetic/logical commands
		C_PUSH,
		C_POP,
		C_LABEL,
		C_GOTO,
		C_IF,
		C_FUNCTION,
		C_CALL,
		C_RETURN
	}
}
