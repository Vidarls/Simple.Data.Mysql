using System.Collections.Generic;
using NUnit.Framework;
using Simple.Data.Ado;
using Simple.Data.Mysql.Mysql40;
using System.Linq;

namespace Simple.Data.Mysql.Mysql40Test
{
    /// <summary>
    /// Summary description for FindTests
    /// </summary>
    [TestFixture]
    public class FindTests
    {
        private static readonly string ConnectionString =
           "server=localhost;user=root;database=SimpleDataTest;";

        [TestFixtureSetUp]
        public void DeleteAlice()
        {
            var db = Database.Opener.OpenConnection(ConnectionString);
            db.Users.DeleteByName("Alice");
        }

        [Test]
        public void TestProviderWithConnectionString()
        {
            var provider = new ProviderHelper().GetProviderByConnectionString(ConnectionString);
            Assert.IsInstanceOf(typeof(Mysql40ConnectionProvider), provider);
        }

        [Test]
        public void TestFindById()
        {
            var db = Database.Opener.OpenConnection(ConnectionString);
            var user = db.Users.FindById(1);
            Assert.AreEqual(1, user.Id);
        }

        [Test]
        public void TestAll()
        {
            var db = Database.Opener.OpenConnection(ConnectionString);
            var all = new List<object>(db.Users.All().Cast<dynamic>());
            Assert.IsNotEmpty(all);
        }

        [Test]
        public void TestImplicitCast()
        {
            var db = Database.Opener.OpenConnection(ConnectionString);
            User user = db.Users.FindById(1);
            Assert.AreEqual(1, user.Id);
        }

        [Test]
        public void TestImplicitEnumerableCast()
        {
            var db = Database.Opener.OpenConnection(ConnectionString);
            foreach (User user in db.Users.All())
            {
                Assert.IsNotNull(user);
            }
        }

        [Test]
        public void TestInsert()
        {
            var db = Database.Opener.OpenConnection(ConnectionString);

            var user = db.Users.Insert(Name: "Alice", Password: "foo", Age: 29);

            Assert.IsNotNull(user);
            Assert.AreEqual("Alice", user.Name);
            Assert.AreEqual("foo", user.Password);
            Assert.AreEqual(29, user.Age);
        }

        [Test]
        public void TestTake()
        {
            var db = Database.OpenConnection(ConnectionString);
            
            var users = db.Users.All().Take(2);
            
            Assert.AreEqual(2,users.ToList().Count);
        }

        [Test]
        public void TestSkipTakeOrderBy()
        {
            var db = Database.OpenConnection(ConnectionString);

            IEnumerable<dynamic> users = db.Users.All().OrderBy(db.Users.Id).Skip(1).Take(1).ToList();
            Assert.AreEqual("Steve", users.First().Name);
        }
    }
}
