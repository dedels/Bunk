using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Bunk;
using System.Threading.Tasks;
using System.Linq;

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
    }
}
