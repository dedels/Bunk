using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Bunk;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using Bunk.Cloudant;
using Newtonsoft.Json.Linq;

namespace BunkTest.AuthenticationRoles
{
    [TestClass]
    public class AuthRolesTest :TempDBTest
    {

        [TestMethod]
        [ExpectedException(typeof(ForbiddenException), "Should have thrown an exception when putting a doc in with only reader access")]
        public async Task AuthRoles_ReaderFailsPut()
        {
            var u = new Bunk.CouchBuiltins.User();
            u.SetPassword("abc");
            u.Name = Rand.RandString("testuser");
            u.Roles = new List<string>() { "abc", "def", "_reader" };
            
            var resp = await this.db.couchRepo.UserMaintenance().AddUser(u);
            var repo_testuser = CouchRepo.Connect(new ConnectionConfig(Config.Get().Uri.ToString(), u.Name, "abc"));

            var test_doc = new GenericDocument() { ID = Rand.RandString("test"), TYPE = "dave-test" };
            test_doc["test_key"] = "hello";

            resp = await repo_testuser.DB(db.name).Put(test_doc);
            Assert.IsTrue(false, "Should have thrown an forbidden failure for readers");
        }

        [TestMethod]
        [ExpectedException(typeof(ForbiddenException), "Should have thrown an exception when reading a doc in with only writer access")]
        public async Task AuthRoles_WriterCanPut()
        {
            var u = new Bunk.CouchBuiltins.User();
            u.SetPassword("abc");
            u.Name = Rand.RandString("testuser");
            u.Roles = new List<string>() { "abc", "def", "_writer" };

            var resp = await this.db.couchRepo.UserMaintenance().AddUser(u);
            var repo_testuser = CouchRepo.Connect(new ConnectionConfig(Config.Get().Uri.ToString(), u.Name, "abc"));

            var test_doc = new GenericDocument() { ID = Rand.RandString("test"), TYPE = "dave-test" };
            test_doc["test_key"] = "hello";

            resp = await repo_testuser.DB(db.name).Put(test_doc);
            Assert.IsNotNull(resp.REV, "Writer should be able to put a document");

            test_doc = await repo_testuser.DB(db.name).Get<GenericDocument>(test_doc.ID);
            Assert.IsTrue(false, "Should have thrown an forbidden failure for writers");
        }
    }
}
