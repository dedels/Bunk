using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace BunkTest
{
    [TestClass]
    public class CouchConn
    {
        [TestMethod]
        [ExpectedException(typeof(Bunk.UnauthorizedException), "Unauthorized user should throw an exception")]
        public async Task UnauthorizedCreateDB()
        {
            var repo = Bunk.CouchRepo.Connect(
                new Bunk.ConnectionConfig(Config.Url().ToString(), "invalid$$", "bad")
            );
            var testdb = repo.DB("bunk-test");

            var ok = await testdb.CreateDB();
            Assert.IsFalse(ok.ok, "DB should not have been created");
        }
        
        [TestMethod]
        public async Task CreateAndDeleteDB()
        {
            var dbname = Rand.RandString("bunk-test");

            var repo = Bunk.CouchRepo.Connect(Config.Get());
            var testdb = repo.DB(dbname);

            var ok = await testdb.CreateDB();
            Assert.IsTrue(ok.ok, "DB {0} not created", dbname);

            var info = await testdb.DBInfo();
            Assert.IsNotNull(info, "Info not received from DB {0}", dbname);

            ok = await testdb.DeleteDB();
            Assert.IsTrue(ok.ok, "DB {0} not deleted", dbname);

        }
    }
}
