﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Helvartis.SqlServerDump.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "15.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Helvartis.SqLServerDump.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to database &apos;{0}&apos; doesn&apos;t exist.
        /// </summary>
        internal static string ErrDatabaseNonExistent {
            get {
                return ResourceManager.GetString("ErrDatabaseNonExistent", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to An I/O error has occured while writing to the output (if using Result file, was the media disconnected during write ?).
        /// </summary>
        internal static string ErrIO {
            get {
                return ResourceManager.GetString("ErrIO", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to There is more than one local instance. Please use --server-name to select the instance to use..
        /// </summary>
        internal static string ErrMoreThanOneLocalInstance {
            get {
                return ResourceManager.GetString("ErrMoreThanOneLocalInstance", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to There is no local instance running.
        /// </summary>
        internal static string ErrNoLocalInstance {
            get {
                return ResourceManager.GetString("ErrNoLocalInstance", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to object &apos;{0}&apos; doesn&apos;t exist in database &apos;{1}&apos;.
        /// </summary>
        internal static string ErrObjectNonExistentInDatabase {
            get {
                return ResourceManager.GetString("ErrObjectNonExistentInDatabase", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Result file argument is empty..
        /// </summary>
        internal static string ErrResultFileArgumentExceptionEmpty {
            get {
                return ResourceManager.GetString("ErrResultFileArgumentExceptionEmpty", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Result file contains the name of a system device (com1, com2, ...).
        /// </summary>
        internal static string ErrResultFileArgumentExceptionSystemDevice {
            get {
                return ResourceManager.GetString("ErrResultFileArgumentExceptionSystemDevice", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Result file&apos;s directory not found..
        /// </summary>
        internal static string ErrResultFileDirectoryNotFoundException {
            get {
                return ResourceManager.GetString("ErrResultFileDirectoryNotFoundException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Result file includes an incorrect or invalid syntax for file name, directory name, or volume label syntax..
        /// </summary>
        internal static string ErrResultFileIOException {
            get {
                return ResourceManager.GetString("ErrResultFileIOException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Result file path or filename is too long..
        /// </summary>
        internal static string ErrResultFilePathTooLongException {
            get {
                return ResourceManager.GetString("ErrResultFilePathTooLongException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to This software doesn&apos;t have permission to access the result file (try executing it as a user having more privileges).
        /// </summary>
        internal static string ErrResultFileSecurityException {
            get {
                return ResourceManager.GetString("ErrResultFileSecurityException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Access to result file is not authorized..
        /// </summary>
        internal static string ErrResultFileUnauthorizedAccessException {
            get {
                return ResourceManager.GetString("ErrResultFileUnauthorizedAccessException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to You must specify a database.
        /// </summary>
        internal static string ErrUsageDatabaseRequired {
            get {
                return ResourceManager.GetString("ErrUsageDatabaseRequired", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to You cannot use the connection-string option with any of the options: server-name, sql-engine, username or password..
        /// </summary>
        internal static string ErrUsageOptionsConnectionStringIncompatibility {
            get {
                return ResourceManager.GetString("ErrUsageOptionsConnectionStringIncompatibility", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to You cannot use the databases option with the all-databases option..
        /// </summary>
        internal static string ErrUsageOptionsDatabaseAndAllDatabaseIncompatibles {
            get {
                return ResourceManager.GetString("ErrUsageOptionsDatabaseAndAllDatabaseIncompatibles", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to You cannot use the drop-db option with the drop-objects option..
        /// </summary>
        internal static string ErrUsageOptionsDropDbAndDropObjectsIncompatibles {
            get {
                return ResourceManager.GetString("ErrUsageOptionsDropDbAndDropObjectsIncompatibles", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to You may use the drop-db option with the no-create-db option only with both the no-schema and no-data options.
        /// </summary>
        internal static string ErrUsageOptionsDropDbWithNoCreateDbOnlyWhenNoSchemaAndNoData {
            get {
                return ResourceManager.GetString("ErrUsageOptionsDropDbWithNoCreateDbOnlyWhenNoSchemaAndNoData", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to You must use the no-data option when using the no-schema option with either the drop-db or drop-objects option..
        /// </summary>
        internal static string ErrUsageOptionsDropWithDataAndNoSchemaIncompatibles {
            get {
                return ResourceManager.GetString("ErrUsageOptionsDropWithDataAndNoSchemaIncompatibles", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to You cannot use both options no-schema and no-data at the same time, except when using the drop-db or drop-objects options..
        /// </summary>
        internal static string ErrUsageOptionsNoSchemaAndNoDataIncompatibles {
            get {
                return ResourceManager.GetString("ErrUsageOptionsNoSchemaAndNoDataIncompatibles", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to You cannot use the password option when there is already a password in the connection string.
        /// </summary>
        internal static string ErrUsagePasswordAlreadyInConnectionString {
            get {
                return ResourceManager.GetString("ErrUsagePasswordAlreadyInConnectionString", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unknown option(s) : {0}.
        /// </summary>
        internal static string ErrUsageUnknownOptions {
            get {
                return ResourceManager.GetString("ErrUsageUnknownOptions", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to sqlserverdump version {version}
        ///By Daniel Jann and other contributors, inspired by the mysqldump utility by
        ///Igor Romanenko, Monty, Jani &amp; Sinisa.
        ///This sofware relies on SQL Server Management Objects (SMO) in order to generate
        ///the SQL statements.
        ///This software comes with ABSOLUTELY NO WARRANTY. This is free software,
        ///and you are welcome to modify and redistribute it under the GPL license.
        ///
        ///When dumping data containing non-ascii base characters, use the --result-file
        ///rather than dumping the command l [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string Help {
            get {
                return ResourceManager.GetString("Help", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Usage: sqlserverdump [OPTIONS] database [OBJECT1 [OBJECT2 [...]]]
        ///OR     sqlserverdump [OPTIONS] --databases [OPTIONS] DB1 [DB2 [DB3 [...]]]
        ///OR     sqlserverdump [OPTIONS] --all-databases [OPTIONS]
        ///OR (*) sqlserverdump [OPTIONS] [OBJECT1 [OBJECT2 [...]]]
        ///
        ///(*) when using a connection string specifying a database.
        /// </summary>
        internal static string Usage {
            get {
                return ResourceManager.GetString("Usage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to For more options, use sqlserverdump --help.
        /// </summary>
        internal static string Usage_more {
            get {
                return ResourceManager.GetString("Usage_more", resourceCulture);
            }
        }
    }
}