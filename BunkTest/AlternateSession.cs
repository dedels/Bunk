using Bunk.Cloudant;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BunkTest
{
    public class TestSessionConfig : Bunk.ConnectionConfig
    {
        public TestSessionConfig(Uri url, Bunk.CouchFilter cf) : base(url)
        {
            this.DefaultFilters.Add(cf);
        }
    }


    [TestClass]
    public class AlternateSession :TempDBTest
    {


        public Bunk.CouchRepo UnauthenticatedRepo => Bunk.CouchRepo.Connect(
            new TestSessionConfig(Config.Url(), (wr) =>
            {
                return wr;
            }));

        [TestMethod]
        public async Task AlternateSession_NewUserCreate()
        {
            var u = new Bunk.CouchBuiltins.User();
            u.Name = Rand.RandString("testuser");
            u.SetPassword("abc");
            u.Roles = new List<string> { "_reader", "_writer" };

            try
            {
                var resp = await this.db.couchRepo.UserMaintenance().AddUser(u);
                u.ID = resp.ID;
                u.REV = resp.REV;

                var cookies = await UnauthenticatedRepo.Authentication().LoginSession(u.Name, "abc");
                Assert.IsNotNull(cookies);

                var userAuthenticatedRepo = Bunk.CouchRepo.Connect(
                    new TestSessionConfig(Config.Get().Uri, (wr) =>
                    {
                        var hwr = (System.Net.HttpWebRequest)wr;
                        hwr.CookieContainer = new System.Net.CookieContainer();
                        hwr.CookieContainer.Add(cookies);
                        return wr;
                    }));
                var putresp = await userAuthenticatedRepo.DB(this.db.name).Put("test string!!", new Dictionary<string, string> { { "abc", "def" } });

                Assert.IsTrue(putresp.ok);
            }
            finally
            {
                await this.db.couchRepo.UserMaintenance().Delete(u);
            }
        }

        [TestMethod]
        public async Task AlternateSession_UserCantWrite()
        {
            var u = new Bunk.CouchBuiltins.User();
            u.Name = Rand.RandString("testuser");
            u.SetPassword("abc");
            u.Roles = new List<string> { "_reader"};

            try
            {
                var resp = await this.db.couchRepo.UserMaintenance().AddUser(u);
                u.ID = resp.ID;
                u.REV = resp.REV;

                var cookies = await UnauthenticatedRepo.Authentication().LoginSession(u.Name, "abc");
                Assert.IsNotNull(cookies);

                var userAuthenticatedRepo = Bunk.CouchRepo.Connect(
                    new TestSessionConfig(Config.Get().Uri, (wr) =>
                    {
                        var hwr = (System.Net.HttpWebRequest)wr;
                        hwr.CookieContainer = new System.Net.CookieContainer();
                        hwr.CookieContainer.Add(cookies);
                        return wr;
                    }));
                try {
                    var putresp = await userAuthenticatedRepo.DB(this.db.name).Put("test string!!", new Dictionary<string, string> { { "abc", "def" } });
                    Assert.IsTrue(false, "This request should have failed.");
                }
                catch (Exception ex)
                {
                    Assert.IsTrue(ex is Bunk.ForbiddenException);
                    Assert.IsTrue(ex.Message.IndexOf("_writer access is required for this request") > -1);
                }
            }
            finally
            {
                await this.db.couchRepo.UserMaintenance().Delete(u);
            }
        }
    }
}
