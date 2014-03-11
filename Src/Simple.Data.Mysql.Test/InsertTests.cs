using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Simple.Data.Ado;

namespace Simple.Data.Mysql.Test
{
    [TestFixture]
    public class InsertTests
    {
        private static readonly string ConnectionString =
           "server=localhost;user=root;database=SimpleDataTest;";

        private static readonly dynamic Db = Database.Opener.OpenConnection(ConnectionString);

        [Test]
        public void Inserted_items_can_be_found()
        {
            Db.empty_table.DeleteAll();
            Db.empty_table.Insert(new {Somevalue = "thevalue"});
            var inserted = Db.empty_table.All().Single();

            Assert.AreEqual("thevalue", inserted.Somevalue);
        }

        [Test]
        public void Ineserted_item_with_autoincrementid_are_returned_on_insert()
        {
            var inserted = Db.empty_table.Insert(new {Somevalue = "a value"});
            Assert.NotNull(inserted);
            Assert.GreaterOrEqual(inserted.Id, 1);
            Assert.AreEqual("a value", inserted.Somevalue);
        }

        [Test]
        public void By_default_autoincrement_columns_are_not_inserted_to()
        //note: try reinitializing the testdatabases if this test fails.
        {
            Db.empty_table.DeleteAll();
            Db.empty_table.Insert(new {Id = 99999, Somevalue = "thenewvalue"});
            var inserted = Db.empty_table.All().Single();
            Assert.AreNotEqual(9999, inserted.Id);
        }

        [Test]
        public void With_the_insertidentity_option_the_autoincrement_column_should_be_inserted_to()
        {
            var db = Db.WithOptions(new AdoOptions(identityInsert: true, commandTimeout: 30));
            db.empty_table.DeleteAll();
            db.empty_table.Insert(new {id = 88888, Somevalue = "This is really not needed, is it?"});
            var inserted = db.empty_table.All().Single();
            Assert.AreEqual(88888, inserted.Id);
        }

        [Test]
        public void With_the_insertidentity_option_the_autoincrement_column_should_autoincrement_when_no_value_is_provided()
        {
            var db = Db.WithOptions(new AdoOptions(identityInsert: true, commandTimeout: 30));
            db.empty_table.DeleteAll();
            db.empty_table.Insert(new {Somevalue = "humty dumpty"});
            var inserted = db.empty_table.All().Single();
            Assert.GreaterOrEqual(inserted.Id, 1);
            db.empty_table.Insert(new {Somevalue = "sing a long now kids"});
            var inserted2 = db.empty_table.FindById(inserted.Id + 1);
            Assert.NotNull(inserted2);
            Assert.AreEqual("sing a long now kids", inserted2.Somevalue);
        }

        [Test]
        public void With_the_insertidentity_option_the_inserted_row_will_not_be_returned()
        {
            var db = Db.WithOptions(new AdoOptions(identityInsert: true, commandTimeout: 30));
            db.empty_table.DeleteAll();
            var toBeInserted = new {Id = 99997, Somevalue = "running out of ideas here"};
            var inserted = db.empty_table.Insert(toBeInserted);
            Assert.Null(inserted);
        }
    }
}
