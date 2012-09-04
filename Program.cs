using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Helvartis.SQLServerDump.Properties;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Sdk.Sfc;
using Microsoft.SqlServer.Management.Smo;
using System.Security;

namespace Helvartis.SQLServerDump
{
    class Program
    {
        public const String PRODUCT_VERSION = "1.1";
        private SQLServerDumpArguments arguments;

        public static void Main(string[] args)
        {
            new Program().run(args);
        }

        public void run(string[] args)
        {
            if (args.Length == 0)
            {
                ShowUsage();
                return;
            }
            try {
                arguments = new SQLServerDumpArguments(args);
            } catch (SQLServerDumpArgumentsException ex) {
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
                else
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
                if (arguments.Username != null)
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
                Console.Error.WriteLine(ex.Message + (ex.InnerException != null ? ": "+ex.InnerException.Message : ""));
                return;
            }
            
            if (arguments.Databases.Length == 0)
            {
                LinkedList<string> dbs = new LinkedList<string>();
                foreach (Database db in server.Databases) {
                    if (arguments.IncludeSystemDatabases || !db.IsSystemObject)
                    {
                        dbs.AddLast(db.Name);
                    }
                }
                arguments.Databases = dbs.ToArray();
            } else {
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
                    ScriptSchema = !arguments.NoSchema,
                    ScriptData = !arguments.NoData,
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
                    String header = "-- DATABASE\n";
                    Output(db, output, scrp, null, ref header);
                    output.WriteLine(String.Format("USE {0};", db.Name));
                    if (!arguments.NoTables) { Output(db.Tables, output, scrp, "-- TABLES\n"); }
                    if (!arguments.NoViews) { Output(db.Views, output, scrp, "-- VIEWS\n"); }
                    if (!arguments.NoUserDefinedFunctions) { Output(db.UserDefinedFunctions, output, scrp, "-- USER DEFINED FUNCTIONS\n"); }
                    if (!arguments.NoStoredProcedures) { Output(db.StoredProcedures, output, scrp, "-- STORED PROCEDURES\n"); }
                    if (!arguments.NoSynonyms) { Output(db.Synonyms, output, scrp, "-- SYNONYMS\n"); }
                    if (!arguments.NoTriggers) { Output(db.Triggers, output, scrp, "-- TRIGGERS\n"); }
                }
            }
            catch (IOException)
            {
                Console.Out.WriteLine(Resources.ErrIO);
            }
            output.Close();
        }

        private void ShowHelp()
        {
            Console.Out.Write(Resources.Help.Replace("{version}", PRODUCT_VERSION).Replace("{usage}",Resources.Usage));
        }
        private void ShowUsage()
        {
            Console.Out.Write(Resources.Usage+"\n"+Resources.Usage_more);
        }
        private bool OutputAtEnd(SmoObjectBase o, string s)
        {
            return o is Table && s.Contains("\nALTER") && !s.StartsWith("INSERT");
        }
        private void Output(SmoCollectionBase coll, TextWriter tw ,Scripter scrp, String header = null)
        {
            LinkedList<string> tableAlterings = new LinkedList<string>();
            foreach (ScriptSchemaObjectBase o in coll)
            {
                Output(o, tw, scrp, tableAlterings, ref header);
            }
            foreach (string s in tableAlterings)
            {
                if (header != null)
                {
                    tw.WriteLine(header);
                }
                tw.WriteLine(s);
            }
        }
        private void Output(NamedSmoObject obj, TextWriter tw, Scripter scrp, LinkedList<string> outputAtEnd, ref String header)
        {
            if (
                (!(bool)obj.Properties["IsSystemObject"].Value || IncludeSysObject(obj))
                    &&
                IncludeObject(obj)
            )
            {
                foreach (string s in scrp.EnumScript(new Urn[] { obj.Urn }))
                {
                    if (outputAtEnd != null && OutputAtEnd(obj, s))
                    {
                        outputAtEnd.AddLast(s.TrimEnd() + ";");
                    }
                    else
                    {
                        if (header != null)
                        {
                            tw.WriteLine(header);
                            header = null;
                        }
                        tw.WriteLine(s.TrimEnd() + ";");
                    }
                }
            }
        }
        private bool IncludeSysObject(SmoObjectBase o)
        {
            return arguments.IncludeSystemObjects || (o is Table ? ((Table)o).Name == "sysdiagrams" : false);
        }
        private bool IncludeObject(NamedSmoObject obj)
        {
            return arguments.DatabaseObjects == null || arguments.DatabaseObjects.Contains(obj.Name) || arguments.DatabaseObjects.Contains(obj.ToString()) || arguments.DatabaseObjects.Contains(obj.ToString().Replace("[","").Replace("]",""));
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
