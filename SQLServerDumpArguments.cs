using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using CommandLine.Utility;
using Helvartis.SQLServerDump.Properties;

namespace Helvartis.SQLServerDump
{
    // TODO: replace the Arguments utility by a more advanced one. One
    // that would allow defining which options to use, wether they are
    // flags or valued, their descriptions and which would generate
    // the usage text. This would be good as current implementation is
    // not DRY: arguments appear in the AllowedArgumentsList, in the
    // programming logic and in the usage text. It would also be better
    // to separate arguments definitions and logic.
    // The C# Command-Line Option Parsing Library could maybe do
    // (http://sourceforge.net/projects/csharpoptparse/) but will it handle
    // the orphan values (used for databases and objects) ?
    // Also, it would be nice to avoid as much as possible dependencies
    // on other DLLs, so you can fetch the .exe and it just runs. (We have
    // dependencies on other DLLs, but it's quite likely the user will
    // already have those DLLs)
    class SQLServerDumpArguments : Arguments
    {
        public string[] AllowedOptionsList = new string[] {
            "all-databases", "databases", "result-file", "server-name", "sql-engine",
            "username", "password", "connection-string", "system-databases", "system-objects", 
            "system-tables", "batch-separator", "drop-objects", "no-use-db", "no-users", "no-roles", "no-permissions",
            "no-data", "no-schema", "no-db-schemas", "no-tables", "no-indexes", "no-checks", "no-primary-key",
            "no-foreign-keys", "no-unique-keys", "no-views", "no-triggers", "no-synonyms",
            "no-stored-procedures", "no-user-defined-types", "no-user-defined-data-types",
            "no-user-defined-table-types", "no-user-defined-aggregates",
            "no-user-defined-functions"
        };
        public bool WrongOptions { get; private set; }
        public string ConnectionString { get; private set; }
        public string[] Databases { get; set; }
        public bool AllDatabases { get; private set; }
        public bool DropDb { get; private set; }
        public bool DropObjects { get; private set; }
        public bool IncludeSystemDatabases { get; private set; }
        public bool IncludeSystemObjects { get; private set; }
        public bool IncludeSystemTables { get; private set; }
        public bool IncludeBatchSeparator { get; private set; }
        public string Password { get; private set; }
        public string ResultFile { get; private set; }
        public string ServerName { get; set; }
        public bool ShowHelp { get; private set; }
        public string Username { get; private set; }
        public string[] DatabaseObjects { get; private set; } // null means ALL
        public bool NoUseDb { get; private set; }
        public bool NoCreateDb { get; private set; }
        public bool NoUsers { get; private set; }
        public bool NoRoles { get; private set; }
        public bool NoPermission { get; private set; }
        public bool NoData { get; private set; }
        public bool NoSchema { get; private set; }
        public bool NoDbSchemas { get; private set; }
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

            // Check all options supplied are allowed
            IEnumerable<string> unknownOptions =  Keys.Except(AllowedOptionsList);
            if (unknownOptions.Count() > 0)
            {
                WrongOptions = true;
                throw new SQLServerDumpArgumentsException(String.Format(Resources.ErrUsageUnknownOptions, String.Join(", ", unknownOptions)));
            }

            // Show help ?
            ShowHelp = ContainsKey("help") || ContainsKey("?");
            if (ShowHelp) { return; }

            // Using connection string ?
            ConnectionString = ContainsKey("connection-string") ? this["connection-string"] : null;
            if (ConnectionString != null && (ContainsKey("server-name") || ContainsKey("sql-engine") || ContainsKey("username")))
            {
                WrongOptions = true;
                throw new SQLServerDumpArgumentsException(Resources.ErrUsageOptionsConnectionStringIncompatibility);
            }

            // Which database(s) and table(s)
            AllDatabases = ContainsKey("all-databases");
            if (ContainsKey("databases") && AllDatabases)
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
            else if (AllDatabases)
            {
                Databases = new string[0];
            }
            else
            {
                if (OrphanValues.Count < 1)
                {
                    if (ConnectionString != null)
                    {
                        Databases = new String[] { new SqlConnection(ConnectionString).Database };
                    }
                    if (Databases == null || String.IsNullOrEmpty(Databases[0]))
                    {
                        WrongOptions = true;
                        throw new SQLServerDumpArgumentsException(Resources.ErrUsageDatabaseRequired);
                    }
                }
                if (OrphanValues.Count > 0)
                {
                    Databases = new string[1] { OrphanValues.First.Value };
                }
                if (OrphanValues.Count > 1)
                {
                    DatabaseObjects = OrphanValues.Skip(1).ToArray();
                }
            }

            NoUseDb = ContainsKey("no-use-db");
            NoCreateDb = ContainsKey("no-create-db");
            NoUsers = ContainsKey("no-users");
            NoRoles = ContainsKey("no-roles");
            NoPermission = ContainsKey("no-permissions");
            NoData = ContainsKey("no-data");
            NoSchema = ContainsKey("no-schema");
            NoDbSchemas = ContainsKey("no-db-schemas");
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
            DropObjects = ContainsKey("drop-objects");
            IncludeSystemDatabases = ContainsKey("system-databases");
            IncludeSystemObjects = ContainsKey("system-objects");
            IncludeSystemTables = ContainsKey("system-tables");
            IncludeBatchSeparator = ContainsKey("batch-separator");
            IsSqlEngine = ContainsKey("sql-engine");
            if (DropDb && DropObjects)
            {
                WrongOptions = true;
                throw new SQLServerDumpArgumentsException(Resources.ErrUsageOptionsDropDbAndDropObjectsIncompatibles);
            }
            if (NoSchema && NoData && !DropDb && !DropObjects)
            {
                WrongOptions = true;
                throw new SQLServerDumpArgumentsException(Resources.ErrUsageOptionsNoSchemaAndNoDataIncompatibles);
            }
            if ((DropDb || DropObjects) && NoSchema && !NoData)
            {
                WrongOptions = true;
                throw new SQLServerDumpArgumentsException(Resources.ErrUsageOptionsDropWithDataAndNoSchemaIncompatibles);
            }
            if (DropDb && NoCreateDb && !NoSchema && !NoData)
            {
                WrongOptions = true;
                throw new SQLServerDumpArgumentsException(Resources.ErrUsageOptionsDropDbWithNoCreateDbOnlyWhenNoSchemaAndNoData);
            }
            if (ContainsKey("server-name")) { ServerName = this["server-name"]; }
            if (ContainsKey("result-file")) { ResultFile = this["result-file"]; }
            if (ContainsKey("username")) { Username = this["username"]; }
            if (ContainsKey("password"))
            {
                if (!String.IsNullOrWhiteSpace(ConnectionString) && Regex.IsMatch(ConnectionString.ToLower(), "(^|\\W)(pwd|password)\\s*="))
                {
                    WrongOptions = true;
                    throw new SQLServerDumpArgumentsException(Resources.ErrUsagePasswordAlreadyInConnectionString);
                }
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
                if (!String.IsNullOrWhiteSpace(ConnectionString))
                {
                    ConnectionString = String.Format("Password={0};{1}", Password, ConnectionString);
                }
            }
        }
    }
}
