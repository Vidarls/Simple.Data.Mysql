#Simple.Data.Mysql
Simple.Data.Mysql is an ADO provider for the Simple.Data framework by Mark Rendle.

##Mysql versions supported
The aim is to support Mysql from V 4.0 up to the latest V 5.5.
To achieve this the provider on the slightly ancient Mysql .net connector v 5.1.7 as it is the only version that supports that full range of versions. [Info on mysql.com] (http://dev.mysql.com/doc/refman/5.5/en/connector-net-versions.html)
So far there have been no issues caused by the old connector. Should there be any, I'd consider releasing differentiated versions of the provider.

##A little notice on the choice of license 
I've chosen to distribute the MysqlConnector assembly bundled with the provider to avoid having to download it separately, staying true to the Simple of simple.data. This requires that I license the provider with the same license as the Mysql connector, which is licensed under the GPL 2 license.
The core Simple.Data and the Simple.Data.Ado is licensed under the MIT license.  If the Mysql connector assembly was available on Nuget (or some other simple way) I would have gone with the MIT license, as I was no longer needed to distribute the Mysql property. 

##Installation
The simple path to get the package from [Nuget] (http://nuget.org/List/Packages/Simple.Data.Mysql) 
(PM> Install-Package Simple.Data.Mysql)
This will get all dependent libraries in addition to the Simple.Data.Mysql provider.

##A little note on deployment with regards to the Mysql connector
A positive side effect of the Mysql connector assembly being distributed with the package is that the resulting application is XCopy deployable.  The standard download from Mysql is a nasty Msi that registers the connector in the GAC, which effectively prevents any form of XCopy deployment.

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
* The Simple.Data project on [Github] (http://github.com/markrendle/Simple.Data)
* [Simple.Data wiki] (http://github.com/markrendle/Simple.Data/wiki/Getting-started)
