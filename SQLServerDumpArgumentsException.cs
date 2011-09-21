using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Helvartis.SQLServerDump
{
    class SQLServerDumpArgumentsException : Exception
    {
        public SQLServerDumpArgumentsException(string message) : base(message) { }
    }
}
