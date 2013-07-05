using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommandLine.Utility;
using System.IO;
using Helvartis.SQLServerDump.Properties;

namespace Helvartis.SQLServerDump
{
    class SQLServerDumpArguments : Arguments
    {
        public bool WrongOptions { get; private set; }
        public string[] Databases { get; set; }
        public bool IncludeSystemDatabases { get; private set; }
        public bool IncludeSystemObjects { get; private set; }
        public bool IncludeSystemTables { get; private set; } // RS
        public string Password { get; private set; }
        public string ResultFile { get; private set; }
        public string ServerName { get; set; }
        public bool ShowHelp { get; private set; }
        public string Username { get; private set; }
        public string[] DatabaseObjects { get; private set; } // null means ALL
        public bool NoData { get; private set; }
        public bool NoSchema { get; private set; }
        public bool NoTables { get; private set; }
        public bool NoIndexes { get; private set; }
        public bool NoChecks { get; private set; }
        public bool NoPrimaryKey { get; private set; }
        public bool NoForeignKeys { get; private set; }
        public bool NoUniqueKeys { get; private set; }
        public bool NoViews { get; private set; }
        public bool NoUserDefinedTypes { get; private set; }
        public bool NoUserDefinedDataTypes { get; private set; }
        public bool NoUserDefinedTableTypes { get; private set; }
        public bool NoUserDefinedAggregates { get; private set; }
        public bool NoUserDefinedFunctions { get; private set; }
        public bool NoStoredProcedures { get; private set; }
        public bool NoSynonyms { get; private set; }
        public bool NoTriggers { get; private set; }
        public bool IsSqlEngine { get; private set; }

        public SQLServerDumpArguments(string[] args) : base(args, false, true)
        {
            WrongOptions = false;

            // Show help ?
            ShowHelp = ContainsKey("help") || ContainsKey("?");
            if (ShowHelp) { return; }

            // Which database(s) and table(s)
            if (ContainsKey("databases") && ContainsKey("all-databases"))
            {
                WrongOptions = true;
                throw new SQLServerDumpArgumentsException(Resources.ErrUsageOptionsDatabaseAndAllDatabaseIncompatibles);
            }
            if (ContainsKey("databases"))
            {
                Databases = OrphanValues.ToArray();
                if (Databases.Length == 0)
                {
                    WrongOptions = true;
                }
            }
            else if (ContainsKey("all-databases"))
            {
                Databases = new string[0];
            }
            else
            {
                if (OrphanValues.Count < 1)
                {
                    WrongOptions = true;
                    throw new SQLServerDumpArgumentsException(Resources.ErrUsageDatabaseRequired);
                }
                Databases = new string[1] { OrphanValues.First.Value };
                if (OrphanValues.Count > 1)
                {
                    DatabaseObjects = OrphanValues.Skip(1).ToArray();
                }
            }

            NoData = ContainsKey("no-data");
            NoSchema = ContainsKey("no-schema");
            NoTables = ContainsKey("no-tables");
            NoIndexes = ContainsKey("no-indexes");
            NoChecks = ContainsKey("no-checks");
            NoPrimaryKey = ContainsKey("no-primary-key");
            NoForeignKeys = ContainsKey("no-foreign-keys");
            NoUniqueKeys = ContainsKey("no-unique-keys");
            NoViews = ContainsKey("no-views");
            NoTriggers = ContainsKey("no-triggers");
            NoSynonyms = ContainsKey("no-synonyms");
            NoStoredProcedures = ContainsKey("no-stored-procedures");
            NoUserDefinedTypes = ContainsKey("no-user-defined-types");
            NoUserDefinedDataTypes = ContainsKey("no-user-defined-data-types");
            NoUserDefinedTableTypes = ContainsKey("no-user-defined-table-types");
            NoUserDefinedAggregates = ContainsKey("no-user-defined-aggregates");
            NoUserDefinedFunctions = ContainsKey("no-user-defined-functions");
            IncludeSystemDatabases = ContainsKey("system-databases");
            IncludeSystemObjects = ContainsKey("system-objects");
            IncludeSystemTables = ContainsKey("system-tables"); // RS
            IsSqlEngine = ContainsKey("sql-engine");
            if (NoSchema && NoData)
            {
                WrongOptions = true;
                throw new SQLServerDumpArgumentsException(Resources.ErrUsageOptionsNoSchemaAndNoDataIncompatibles);
            }
            if (ContainsKey("server-name")) { ServerName = this["server-name"]; }
            if (ContainsKey("result-file")) { ResultFile = this["result-file"]; }
            if (ContainsKey("username")) { Username = this["username"]; }
            if (ContainsKey("password"))
            {
                Password = this["password"];
                if (Password == "true") // Password value not included in the command,
                {                        // let's read it from stdin without echoing it.
                    Password = "";
                    ConsoleKeyInfo keyInfo;
                    do
                    {
                        keyInfo = Console.ReadKey(true);
                        if (keyInfo.Key != ConsoleKey.Enter && keyInfo.KeyChar != '\0')
                        {
                            Password += keyInfo.KeyChar;
                        }
                    } while (keyInfo == null || keyInfo.Key != ConsoleKey.Enter);
                }
            }
        }
    }
}
