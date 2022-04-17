using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JackCompiler
{
	enum Segment
	{
		CONSTANT,
		ARG,
		LOCAL,
		STATIC,
		THIS,
		THAT,
		POINTER,
		TEMP
	}
}
