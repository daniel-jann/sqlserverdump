using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommandLine.Utility;
using System.IO;

namespace Helvartis.SQLServerDump
{
    class SQLServerDumpArguments : Arguments
    {
        private string[] _databases = null; // null means ALL
        private string[] _databaseObjects = null; // null means ALL
        private bool _showHelp = false;
        private bool _wrongOptions = false;
        private string _password = null;
        private string _resultFile = null;
        private string _serverName = null;
        private string _username = null;
        private bool _includeSystemDatabases = false;
        private bool _includeSystemObjects = false;

        public bool WrongOptions
        {
            get { return _wrongOptions; }
        }
        public string[] Databases
        {
            get { return _databases; }
            set { _databases = value; }
        }
        public bool IncludeSystemDatabases
        {
            get { return _includeSystemDatabases; }
            set { _includeSystemDatabases = value; }
        }
        public bool IncludeSystemObjects
        {
            get { return _includeSystemObjects; }
            set { _includeSystemObjects = value; }
        }
        public string Password
        {
            get { return _password; }
            set { _password = value; }
        }
        public string ResultFile
        {
            get { return _resultFile; }
            set { _resultFile = value; }
        }
        public string ServerName
        {
            get { return _serverName; }
            set { _serverName = value; }
        }
        public bool ShowHelp
        {
            get { return _showHelp; }
        }
        public string Username
        {
            get { return _username; }
            set { _username = value; }
        }
        public string[] DatabaseObjects
        {
            get { return _databaseObjects; }
            set { _databaseObjects = value; }
        }

        public SQLServerDumpArguments(string[] args) : base(args, true)
        {
            // Show help ?
            if (ContainsKey("help") || ContainsKey("?"))
            {
                _showHelp = true;
                return;
            }

            // Which database(s) and table(s)
            if (ContainsKey("databases") && ContainsKey("all-databases"))
            {
                _wrongOptions = true;
                throw new SQLServerDumpArgumentsException("You cannot use the databases option with the all-databases option.");
            }
            if (ContainsKey("databases"))
            {
                _databases = OrphanValues.ToArray();
                if (_databases.Length == 0)
                {
                    _wrongOptions = true;
                }
            }
            else if (ContainsKey("all-databases"))
            {
                _databases = new string[0];
            }
            else
            {
                if (OrphanValues.Count < 1)
                {
                    _wrongOptions = true;
                    throw new SQLServerDumpArgumentsException("You must specify a database");
                }
                _databases = new string[1] { OrphanValues.First.Value };
                if (OrphanValues.Count > 1)
                {
                    _databaseObjects = OrphanValues.Skip(1).ToArray();
                }
            }

            if (ContainsKey("server-name")) { _serverName = this["server-name"]; }
            if (ContainsKey("system-databases")) { _includeSystemDatabases = true; }
            if (ContainsKey("result-file")) { _resultFile = this["result-file"]; }
            if (ContainsKey("system-objects")) { _includeSystemObjects = true; }
            if (ContainsKey("username")) { _username = this["username"]; }
            if (ContainsKey("password")) {
                _password = this["password"];
                if (_password == "true") // Password value not included in the command,
                {                        // let's read it from stdin without echoing it.
                    _password = "";
                    ConsoleKeyInfo keyInfo;
                    do
                    {
                        keyInfo = Console.ReadKey(true);
                        if (keyInfo.Key != ConsoleKey.Enter && keyInfo.KeyChar != '\0') {
                            _password += keyInfo.KeyChar;
                        }
                    } while (keyInfo == null || keyInfo.Key != ConsoleKey.Enter);
                }
            }
        }
    }
}
