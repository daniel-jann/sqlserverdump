# Project Description
SQL Server Dump (sqlserverdump) is a command line utility inspired by
mysqldump. It allows to dump SQL Server databases and their content as SQL
statements. It's written in C#.

The creator's initial aim was to be able to backup databases in a non-binary
format. Microsoft's backup tools create binary files which can be restored
pretty much only on a server having the same version as the one on which the
backup has been made. The goal with this project is to be as much as possible
independant of the server's version.

The project makes use of SQL Server Management Objects (SMO) in order to
generate the SQL statements. It will therefore require that library. If have
SQL Server 2008 installed you should be fine. If you have a lower version, you
will probably have to install the Shared Management Objects which you will
find in the [Microsoft® SQL Server® 2008 R2 SP1 Feature
Pack](http://www.microsoft.com/en-us/download/details.aspx?id=26728). That one
has several requirements. It will probably tell you to install the CLR Types
(SQLSysClrTypes) first. (Choose the files from the architecture that matches
yours: IA64 = Itanium, x64 = Windows 64 bits, x86 = Windows 32 bits)

Currently, only features of SQL Server up to 2008 are meant to be supported.
That is, it will probably work with newer versions but you won't be able to
dump the new features of these versions (things like SEQUENCE, ...)