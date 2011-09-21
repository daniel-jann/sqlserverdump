using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommandLine.Utility;

namespace Helvartis.SQLServerDump
{
    class SQLServerDumpArguments : Arguments
    {
        private string[] _databases = null; // null means ALL
        private string[] _databaseObjects = null; // null means ALL
        private bool _showHelp = false;
        private bool _wrongOptions = false;
        private string _resultFile = null;
        private string _serverName = null;
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

        }
    }
}
