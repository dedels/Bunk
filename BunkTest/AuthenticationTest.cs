using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Bunk;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using Bunk.Cloudant;

namespace BunkTest.Authentication
{
    [TestClass]
    public class AuthTest
    {
        [TestMethod]
        public async Task Auth_CookieLogin()
        {
            var cfg = new ConnectionConfig("http://onek.cloudant.com");
            var repo = CouchRepo.Connect(cfg);

            var aresp = await repo.Authentication().LoginSession("onek", "36633663");
            Assert.IsTrue((from s in aresp
                           where s.StartsWith("AuthSession")
                           select s).Any(), "AuthSession should have bene returned");
        }

        [TestMethod]
        public void Auth_UserPasswordExample()
        {
            //example from http://wiki.apache.org/couchdb/Security_Features_Overview
            
            var u = new Bunk.CouchBuiltins.User();
            u.SetPassword("mypassword", "mysalt");
            Assert.IsNotNull(u.PasswordSha, "Password sha should ahve been set");
            Assert.IsNull(u.Password, "Password should have been reset");
            Assert.AreEqual(u.Salt, "mysalt", "Salt should be the same as input");

            Assert.AreEqual("4f2c19f885eae0886b526fda968234874f51be34", u.PasswordSha, "Password should match the example from the couchdb wiki");

        }


        [TestMethod]
        public async Task Auth_AddUser()
        {
            var repo = CouchRepo.Connect(Config.Get());
            try
            {
                await repo.UserMaintenance().CreateDB();
                Console.WriteLine("Create _users db");
            }
            catch (BunkException ex) { }

            var new_user = new Bunk.CouchBuiltins.User(){ Name=Rand.RandString("auth_test_user"), Roles= new List<string>() {"test-role1", "test-role2"}};
            new_user.SetPassword("abc");

            Assert.IsTrue(new_user.ID.StartsWith("org.couchdb.user:"), "ID was not created properly {0}", new_user.ID);
            var new_user_resp = await repo.UserMaintenance().AddUser(new_user);
            Assert.IsTrue(new_user_resp.ok, "User ({0}) was not created properly - {1}", new_user_resp.ID, new_user_resp.reason);


            var new_user2 = await repo.UserMaintenance().GetUser(new_user.Name);
            Assert.AreEqual(new_user.Name, new_user2.Name, "Should have retrieved the same username as uploaded");
            Assert.IsNotNull(new_user2.PasswordSha, "Password should have been set by the server");
            Assert.IsNull(new_user.Password, "Password should have been removed by the server");


            var delresp = await repo.UserMaintenance().Delete(new_user2);
            Assert.IsTrue(delresp.ok, "Test user {0} should have been deleted", new_user2.ID);
        }


        [TestMethod]
        [ExpectedException(typeof(UnauthorizedException))]
        public async Task Auth_UserPasswordLoginFailed()
        {
            var u = new Bunk.CouchBuiltins.User();
            u.SetPassword("abc");
            u.Name = Rand.RandString("testuser");
            u.Roles = new List<string>() { "abc", "def" };

            var repo_ref = CouchRepo.Connect(Config.Get());
            var resp = await repo_ref.UserMaintenance().AddUser(u);

            var repo_testuser = CouchRepo.Connect(new ConnectionConfig(Config.Get().Uri.ToString(), u.Name, "aaaaaaaaa"));

            var session = await repo_testuser.Authentication().Session();
        }

        [TestMethod]
        public async Task Auth_UserPasswordLogin()
        {
            var u = new Bunk.CouchBuiltins.User();
            u.SetPassword("abc");
            u.Name = Rand.RandString("testuser");
            u.Roles = new List<string>() { "abc", "def", "_reader" };
            
            var repo_ref = CouchRepo.Connect(Config.Get());
            var resp = await repo_ref.UserMaintenance().AddUser(u);

            var repo_testuser = CouchRepo.Connect(new ConnectionConfig(Config.Get().Uri.ToString(), u.Name, "abc"));

            var session = await repo_testuser.Authentication().Session();
            Assert.AreEqual(u.Name, session.UserCtx.Name, "Session should return the user we just added");
            
            var u_from_server = await repo_testuser.UserMaintenance().GetUser(u.Name);
            Assert.AreEqual(u.Name, u_from_server.Name, "Should have received the same user from the server");

        }
    }
}
