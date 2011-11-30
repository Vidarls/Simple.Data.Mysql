#Simple.Data.Mysql
Simple.Data.Mysql is an ADO provider for the Simple.Data framework by Mark Rendle.

##Mysql versions supported
The aim is to support Mysql from V 4.0 up to the latest V 5.5.
To achieve this the provider on the slightly ancient Mysql .net connector v 5.1.7 as it is the only version that supports that full range of versions. (http://dev.mysql.com/doc/refman/5.5/en/connector-net-versions.html)
So far there have been no issues caused by the old connector. Should there be any, I'd consider releasing differentiated versions of the provider.

##Installation
The simple path to get the package from Nuget (http://nuget.org/List/Packages/Simple.Data.Mysql) 
(PM> Install-Package Simple.Data.Mysql)
This will get all dependent libraries in addition to the Simple.Data.Mysql provider.

##Builing and testing
I've added a build script (using psake https://github.com/psake/psake) That automates a couple of things.
First, It enables anyone who wants to contribute to automatically download two versions of Mysql (4.0 and 5.5) as no-install packages that will be placed under /Tools. These databases will then be primed with the test data used in the project.
Second, it will build the project and run all tests on both servers.

###Useful shortcuts:
(Run in powershell, or use the package management console in Visual Studio)

\Build\psake.ps1 .\default.ps1 InitializeTestDatabases - This fetches the servers (if needed) and primes them with test data.

\Build\psake.ps1 .\default.ps1 -framework 4.0 - Builds project and runs all tests on both servers.

\Build\psake.ps1 .\default.ps1 Start_mysql_40 - Starts the 4.0 version of the Mysql Server

\Build\psake.ps1 .\default.ps1 Stop_mysql_40 - Stops the 4.0 version of the Mysql Server

\Build\psake.ps1 .\default.ps1 Start_mysql_55 - Starts the 5.5 version of the Mysql Server

\Build\psake.ps1 .\default.ps1 Stop_mysql_55 - Stops the 5.5 version of the Mysql Server

###Caveat
If a build fails, or the script otherwhise crashes, a MysqlServer might be left hanging. Use the commands above to stop them of kill the process (mysqld-nt.exe for 4.0 and mysqld.exe for 5.5)

##Implicit foreign key support
One of the key features of Simple.Data is the support for implicit foreign key queries. 
Thus you can do:

db.Customers.Find(db.Customers.Invoices.Paid == "N")

And the resulting SQL will be:

SELECT [Customers].* FROM [Customers] 
JOIN [Invoices] ON [Customers].[CustomerId] = [Invoices].[CustomerId] 
WHERE [Invoices].[Paid] = ?p1

If you've set up referential integrity between the tables.

The default storage engine in Mysql, the MyIsam engine does not support the foreign key constraints required for this feature to work.
To provide similar functionality I have implemented a naming conventions based foreign key support:
If a column in a table have the same name as the primary key column in another table, this column will be treated as the foreign key with regards to implicit foreign key queries. 
In the example above the provider would find that the Primary key of Customers have the same name as Invoices.CustomerId and perform the join as required.

At this point this is the only way to do implicit joins with the Mysql Provider, thus if you have an InnoDb database, you would still need to use the naming conventions to get support for this feature.

##Resources
* The Simple.Data project on Github: http://github.com/markrendle/Simple.Data
* Simple.Data wiki: http://github.com/markrendle/Simple.Data/wiki/Getting-started
=======
#Simple.Data.Mysql
Simple.Data.Mysql is an ADO provider for the Simple.Data framework by Mark Rendle.

##Mysql versions supported
The aim is to support Mysql from V 4.0 up to the latest V 5.5.
To achieve this the provider on the slightly ancient Mysql .net connector v 5.1.7 as it is the only version that supports that full range of versions. [Info on mysql.com](http://dev.mysql.com/doc/refman/5.5/en/connector-net-versions.html)
So far there have been no issues caused by the old connector. Should there be any, I'd consider releasing differentiated versions of the provider.

##Installation
The simple path to get the package from [Nuget](http://nuget.org/List/Packages/Simple.Data.Mysql) 
(PM> Install-Package Simple.Data.Mysql)

##Implicit foreign key support
One of the key features of Simple.Data is the support for implicit foreign key queries. 
Thus you can do:

db.Customers.Find(db.Customers.Invoices.Paid == "N")

And the resulting SQL will be:

SELECT [Customers].* FROM [Customers] 
JOIN [Invoices] ON [Customers].[CustomerId] = [Invoices].[CustomerId] 
WHERE [Invoices].[Paid] = ?p1

If you've set up referential integrity between the tables.

The default storage engine in Mysql, the MyIsam engine does not support the foreign key constraints required for this feature to work.
To provide similar functionality I have implemented a naming conventions based foreign key support:
If a column in a table have the same name as the primary key column in another table, this column will be treated as the foreign key with regards to implicit foreign key queries. 
In the example above the provider would find that the Primary key of Customers have the same name as Invoices.CustomerId and perform the join as required.

At this point this is the only way to do implicit joins with the Mysql Provider, thus if you have an InnoDb database, you would still need to use the naming conventions to get support for this feature.

##Resources
* The Simple.Data project on [Github](http://github.com/markrendle/Simple.Data)
* [Simple.Data wiki](http://github.com/markrendle/Simple.Data/wiki/Getting-started)
