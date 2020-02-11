using System;

namespace Helvartis.SqlServerDump
{
    class SqlServerDumpArgumentsException : Exception
    {
        public SqlServerDumpArgumentsException(string message) : base(message) { }
    }
}
