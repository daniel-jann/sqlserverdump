using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Text.RegularExpressions;
using Helvartis.SQLServerDump.Properties;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Sdk.Sfc;
using Microsoft.SqlServer.Management.Smo;

namespace Helvartis.SQLServerDump
{
    /// <summary>
    /// Known bugs:
    /// - When dumping the database created by the script from http://technet.microsoft.com/en-us/library/dd283095%28v=sql.100%29.aspx, GRANT XXX ON SCHEMA::[dbo] TO [Developer_Role] is missing.
    /// </summary>
    class Program
    {
        private SQLServerDumpArguments arguments;

        public static void Main(string[] args)
        {
            new Program().Run(args);
        }

        public void Run(string[] args)
        {
            if (args.Length == 0)
            {
                ShowUsage();
                return;
            }
            try
            {
                arguments = new SQLServerDumpArguments(args);
            }
            catch (SQLServerDumpArgumentsException ex)
            {
                Console.Error.WriteLine(ex.Message);
                return;
            }

            if (arguments.WrongOptions)
            {
                ShowUsage();
                return;
            }

            if (arguments.ShowHelp)
            {
                ShowHelp();
                return;
            }

            if (arguments.ServerName == null)
            {
                if (arguments.IsSqlEngine)
                {
                    arguments.ServerName = "127.0.0.1";
                }
                else if (arguments.ConnectionString == null)
                {
                    DataTable availableSqlServers = SmoApplication.EnumAvailableSqlServers(true);
                    if (availableSqlServers.Rows.Count > 1)
                    {
                        Console.Error.WriteLine(Resources.ErrMoreThanOneLocalInstance);
                        return;
                    }
                    if (availableSqlServers.Rows.Count == 0)
                    {
                        Console.Error.WriteLine(Resources.ErrNoLocalInstance);
                        return;
                    }
                    arguments.ServerName = availableSqlServers.Rows[0].Field<String>("Name");
                }
            }
            else if (!arguments.IsSqlEngine && !arguments.ServerName.Contains('\\'))
            {
                arguments.ServerName = ".\\" + arguments.ServerName;
            }

            Server server;
            try
            {
                if (arguments.ConnectionString != null)
                {
                    server = new Server(new ServerConnection(new SqlConnection(arguments.ConnectionString)));
                }
                else if (arguments.Username != null)
                {
                    server = new Server(new ServerConnection(arguments.ServerName, arguments.Username, arguments.Password == null ? "" : arguments.Password));
                }
                else
                {
                    server = new Server(arguments.ServerName);
                }
                server.Databases.Refresh(); // Try to connect to server
            }
            catch (ConnectionFailureException ex)
            {
                Console.Error.WriteLine(ex.Message + (ex.InnerException != null ? ": " + ex.InnerException.Message : ""));
                return;
            }

            if (arguments.Databases.Length == 0)
            {
                LinkedList<string> dbs = new LinkedList<string>();
                foreach (Database db in server.Databases)
                {
                    if (arguments.IncludeSystemDatabases || !db.IsSystemObject)
                    {
                        dbs.AddLast(db.Name);
                    }
                }
                arguments.Databases = dbs.ToArray();
            }
            else
            {
                // Check if databases exist
                bool hasError = false;
                foreach (string dbName in arguments.Databases)
                {
                    if (!server.Databases.Contains(dbName))
                    {
                        Console.Error.WriteLine(String.Format(Resources.ErrDatabaseNonExistent, dbName));
                        hasError = true;
                    }
                    else if (arguments.DatabaseObjects != null)
                    {
                        foreach (string objectName in arguments.DatabaseObjects)
                        {
                            if (!ContainsObject(server.Databases[dbName], objectName))
                            {
                                Console.Error.WriteLine(String.Format(Resources.ErrObjectNonExistentInDatabase, objectName, dbName));
                                hasError = true;
                            }
                        }
                    }
                }
                if (hasError)
                {
                    return;
                }
            }

            Scripter scrp = new Scripter(server)
            {
                Options = new ScriptingOptions()
                {
                    Permissions = !arguments.NoPermission,
                    ScriptSchema = !arguments.NoSchema,
                    ScriptData = !arguments.NoData,
                    // ScriptDrops = arguments.DropObjects, // Option set later as it requires special handling
                    Triggers = !arguments.NoTriggers,
                    Indexes = !arguments.NoIndexes,
                    DriChecks = !arguments.NoChecks,
                    DriPrimaryKey = !arguments.NoPrimaryKey,
                    DriForeignKeys = !arguments.NoForeignKeys,
                    DriUniqueKeys = !arguments.NoUniqueKeys,
                    ScriptBatchTerminator = true,
                    ScriptDataCompression = true,
                    ScriptOwner = true,
                    IncludeIfNotExists = true,
                    DriAll = true
                }
            };

            // Where to direct output
            TextWriter output;
            if (arguments.ResultFile != null)
            {
                try
                {
                    output = new StreamWriter(arguments.ResultFile);
                }
                catch (UnauthorizedAccessException)
                {
                    Console.Out.WriteLine(Resources.ErrResultFileUnauthorizedAccessException);
                    return;
                }
                catch (DirectoryNotFoundException)
                {
                    Console.Out.WriteLine(Resources.ErrResultFileDirectoryNotFoundException);
                    return;
                }
                catch (ArgumentException)
                {
                    if (arguments.ResultFile == String.Empty)
                    {
                        Console.Out.WriteLine(Resources.ErrResultFileArgumentExceptionEmpty);
                    }
                    else
                    {
                        Console.Out.WriteLine(Resources.ErrResultFileArgumentExceptionSystemDevice);
                    }
                    return;
                }
                catch (PathTooLongException)
                {
                    Console.Out.WriteLine(Resources.ErrResultFilePathTooLongException);
                    return;
                }
                catch (IOException)
                {
                    Console.Out.WriteLine(Resources.ErrResultFileIOException);
                    return;
                }
                catch (SecurityException)
                {
                    Console.Out.WriteLine(Resources.ErrResultFileSecurityException);
                    return;
                }
            }
            else
            {
                output = System.Console.Out;
            }

            try
            {
                foreach (string dbName in arguments.Databases)
                {
                    Database db = server.Databases[dbName];
                    if (arguments.DropDb)
                    {
                        scrp.Options.ScriptDrops = true;
                        String header = "-- DROP DATABASE\n";
                        Output(db, output, scrp, null, ref header); // Drop db
                        scrp.Options.ScriptDrops = false;
                    }
                    else if (arguments.DropObjects) // DropDb and DropObjects are mutually exclusive
                    {
                        scrp.Options.ScriptDrops = true;
                        if (!arguments.NoUseDb) { output.WriteLine(String.Format("USE {0};{1}", db.Name, arguments.IncludeBatchSeparator ? "\nGO" : "")); }
                        if (!arguments.NoTriggers) { Output(db.Triggers, output, scrp, "\n-- DROP TRIGGERS\n"); }
                        if (!arguments.NoSynonyms) { Output(db.Synonyms, output, scrp, "\n-- DROP SYNONYMS\n"); }
                        if (!arguments.NoStoredProcedures) { Output(db.StoredProcedures, output, scrp, "\n-- DROP STORED PROCEDURES\n"); }
                        if (!arguments.NoViews) { Output(db.Views, output, scrp, "\n-- DROP VIEWS\n"); }
                        if (!arguments.NoTables) { Output(db.Tables, output, scrp, "\n-- DROP TABLES\n"); }
                        if (!arguments.NoUserDefinedFunctions) { Output(db.UserDefinedFunctions, output, scrp, "\n-- DROP USER DEFINED FUNCTIONS\n"); }
                        if (!arguments.NoUserDefinedAggregates) { Output(db.UserDefinedAggregates, output, scrp, "\n-- DROP USER DEFINED AGGREGATES\n"); }
                        if (!arguments.NoUserDefinedTableTypes) { Output(db.UserDefinedTableTypes, output, scrp, "\n-- DROP USER DEFINED TABLE TYPES\n"); }
                        if (!arguments.NoUserDefinedDataTypes) { Output(db.UserDefinedDataTypes, output, scrp, "\n-- DROP USER DEFINED DATA TYPES\n"); }
                        if (!arguments.NoUserDefinedTypes) { Output(db.UserDefinedTypes, output, scrp, "\n-- DROP USER DEFINED TYPES\n"); }
                        if (!arguments.NoDbSchemas) { Output(db.Schemas, output, scrp, "\n-- DROP SCHEMAS\n"); }
                        if (!arguments.NoRoles) { Output(db.Roles, output, scrp, "\n-- DROP ROLES\n"); }
                        if (!arguments.NoUsers) { Output(db.Users, output, scrp, "\n-- DROP USERS\n"); }
                        scrp.Options.ScriptDrops = false;
                    }
                    if (!arguments.NoSchema || !arguments.NoData)
                    {
                        String header = "\n-- DATABASE\n";
                        if (!arguments.NoCreateDb) { Output(db, output, scrp, null, ref header); }
                        if (!arguments.NoUseDb) { output.WriteLine(String.Format("USE {0};{1}", db.Name, arguments.IncludeBatchSeparator ? "\nGO" : "")); }
                        if (!arguments.NoUsers) { Output(db.Users, output, scrp, "\n-- USERS\n"); }
                        if (!arguments.NoRoles) { Output(db.Roles, output, scrp, "\n-- ROLES\n"); }
                        if (!arguments.NoDbSchemas) { Output(db.Schemas, output, scrp, "\n-- SCHEMAS\n"); }
                        if (!arguments.NoUserDefinedTypes) { Output(db.UserDefinedTypes, output, scrp, "\n-- USER DEFINED TYPES\n"); }
                        if (!arguments.NoUserDefinedDataTypes) { Output(db.UserDefinedDataTypes, output, scrp, "\n-- USER DEFINED DATA TYPES\n"); }
                        if (!arguments.NoUserDefinedTableTypes) { Output(db.UserDefinedTableTypes, output, scrp, "\n-- USER DEFINED TABLE TYPES\n"); }
                        if (!arguments.NoUserDefinedAggregates) { Output(db.UserDefinedAggregates, output, scrp, "\n-- USER DEFINED AGGREGATES\n"); }
                        if (!arguments.NoUserDefinedFunctions) { Output(db.UserDefinedFunctions, output, scrp, "\n-- USER DEFINED FUNCTIONS\n"); }
                        if (!arguments.NoTables) { Output(db.Tables, output, scrp, "\n-- TABLES\n"); }
                        if (!arguments.NoViews) { Output(db.Views, output, scrp, "\n-- VIEWS\n"); }
                        if (!arguments.NoStoredProcedures) { Output(db.StoredProcedures, output, scrp, "\n-- STORED PROCEDURES\n"); }
                        if (!arguments.NoSynonyms) { Output(db.Synonyms, output, scrp, "\n-- SYNONYMS\n"); }
                        if (!arguments.NoTriggers) { Output(db.Triggers, output, scrp, "\n-- TRIGGERS\n"); }
                    }
                }
            }
            catch (IOException)
            {
                Console.Out.WriteLine(Resources.ErrIO);
            }
            output.Close();
        }
        private string GetDisplayVersionFromAssembly()
        {
            return Regex.Replace(Assembly.GetExecutingAssembly().GetName().Version.ToString(), "(\\.0)+", delegate(Match m) { return ""; });
        }
        private void ShowHelp()
        {
            Console.Out.Write(Resources.Help.Replace("{version}", GetDisplayVersionFromAssembly()).Replace("{usage}", Resources.Usage));
        }
        private void ShowUsage()
        {
            Console.Out.Write(Resources.Usage + "\n" + Resources.Usage_more);
        }
        private bool OutputAtEnd(SmoObjectBase o, string s)
        {
            return o is Table && s.Contains("\nALTER") && !s.StartsWith("INSERT");
        }
        private void Output(SmoCollectionBase coll, TextWriter tw, Scripter scrp, String header = null)
        {
            LinkedList<string> tableAlterings = new LinkedList<string>();
            // When dropping tables, table alterings (drop constraints) must be output first,
            // so we place the main table operations (drop) in a temporary writer (tmpWriter),
            // output the table alterings (drop constraint) to the main writer and then output
            // the temporary writer to the main writer.
            TextWriter tmpWriter = scrp.Options.ScriptDrops ? new StringWriter() : tw;
            String tmpHeader = scrp.Options.ScriptDrops ? null : header;
            foreach (NamedSmoObject o in coll)
            {
                if (!(o is DatabaseRole) || (!((DatabaseRole)o).IsFixedRole && o.Name != "public")) // Don't output fixed database roles neither the "public" database role
                {
                    Output(o, tmpWriter, scrp, tableAlterings, ref tmpHeader);
                }
            }
            if (!scrp.Options.ScriptDrops) { header = tmpHeader; }
            foreach (string s in tableAlterings)
            {
                if (header != null)
                {
                    tw.WriteLine(header);
                    header = null;
                }
                tw.WriteLine(s);
            }
            if (scrp.Options.ScriptDrops)
            {
                if (tmpWriter.ToString().Length > 0 && header != null)
                {
                    tw.WriteLine(header);
                }
                tw.Write(tmpWriter);
            }
        }
        private void Output(NamedSmoObject obj, TextWriter tw, Scripter scrp, LinkedList<string> outputAtEnd, ref String header)
        {
            if (
                (!obj.Properties.Contains("IsSystemObject") || !(bool)obj.Properties["IsSystemObject"].Value) || IncludeSysObject(obj)
                    &&
                IncludeObject(obj)
            )
            {
                // Don't include CLR objects (they can't be scripted)
                if (
                    obj.Discover().Count > 0 &&
                    obj.Discover()[0].GetType().GetProperty("ImplementationType") != null &&
                    obj.Discover()[0].GetType().GetProperty("ImplementationType").GetValue(obj.Discover()[0], null) is ImplementationType &&
                    (ImplementationType)obj.Discover()[0].GetType().GetProperty("ImplementationType").GetValue(obj.Discover()[0], null) == ImplementationType.SqlClr
                )
                { 
                    return;
                }

                bool hasContent = false;
                bool hasOutputAtEnd = false;
                foreach (string s in scrp.EnumScript(new Urn[] { obj.Urn }))
                {
                    if (outputAtEnd != null && OutputAtEnd(obj, s))
                    {
                        outputAtEnd.AddLast(s.TrimEnd() + ";");
                        hasOutputAtEnd = true;
                    }
                    else
                    {
                        if (header != null)
                        {
                            tw.WriteLine(header);
                            header = null;
                        }
                        tw.WriteLine(s.TrimEnd() + ";");
                        if ((s.Contains("CREATE TABLE") && obj.Properties.Contains("IsSystemObject") && ((bool)obj.Properties["IsSystemObject"].Value) && IncludeSysObject(obj)))
                        {
                            tw.WriteLine(MarkSystemObject(obj.Name));
                        }
                        hasContent = true;
                    }
                }
                if (hasContent && arguments.IncludeBatchSeparator)
                {
                    tw.WriteLine("GO");
                }
                if (hasOutputAtEnd && arguments.IncludeBatchSeparator)
                {
                    outputAtEnd.AddLast("GO");
                }
            }
        }

        private string MarkSystemObject(string p)
        {
            return String.Format(@"BEGIN TRY
                                    EXEC sp_MS_marksystemobject '{0}'
                                  END TRY
                                  BEGIN CATCH
                                  END CATCH;", p);
        }
        private bool IncludeSysObject(SmoObjectBase o)
        {
            return arguments.IncludeSystemObjects || (o is Table ? ((Table)o).Name == "sysdiagrams" : false) || (arguments.IncludeSystemTables && o is Table);
        }
        private bool IncludeObject(NamedSmoObject obj)
        {
            return arguments.DatabaseObjects == null || arguments.DatabaseObjects.Contains(obj.Name) || arguments.DatabaseObjects.Contains(obj.ToString()) || arguments.DatabaseObjects.Contains(obj.ToString().Replace("[", "").Replace("]", ""));
        }
        private bool ContainsObject(Database database, String objectName)
        {
            return database.Tables.Contains(objectName) ||
                database.Views.Contains(objectName) ||
                database.UserDefinedFunctions.Contains(objectName) ||
                database.StoredProcedures.Contains(objectName) ||
                database.Synonyms.Contains(objectName) ||
                database.Triggers.Contains(objectName);
        }
    }
}
