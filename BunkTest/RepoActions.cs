using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Bunk;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace BunkTest.RepoActions
{

    [TestClass]
    public class SingeleDoc :TempDBTest
    {

        [TestMethod]
        public async Task Repo_UUIDs()
        {
            var resp = await this.db.couchRepo.UUIDs(100);
            Assert.AreEqual(100, resp.Count, "100 uuids should be returned");

        }


        [TestMethod]
        public async Task Repo_AllDBs()
        {
            var resp = await this.db.couchRepo.AllDBs();
            Assert.IsTrue(resp.IndexOf(db.name) >= 0);

        }
    }
}
