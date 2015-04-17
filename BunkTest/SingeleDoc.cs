using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Bunk;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace BunkTest
{
    [TestClass]
    public class TempDBTest
    {
        protected DB db;

        public async Task<Bunk.CouchBuiltinResponses.OK> RecreateDB()
        {
            var dbi = await this.db.DBInfo();
            if (dbi.exists == true)
                await this.db.DeleteDB();

            return await this.db.CreateDB();
        }

        [TestInitialize]
        public void Init()
        {
            this.db = CouchRepo.Connect(Config.Get()).DB("bunk-test");
            RecreateDB().Wait();

            var ci = this.ContinueInit();
            if(ci!=null) ci.Wait();
        }

        [TestCleanup]
        public void Cleanup()
        {
            db.DeleteDB().Wait();

            var cc = this.ContinueCleanup();
            if(cc!=null) cc.Wait();
        }

        public virtual Task ContinueInit() { return null;  }
        public virtual Task ContinueCleanup() { return null; }
    }

    [TestClass]
    public class SingeleDoc :TempDBTest
    {
        public class Tester
        {
            public string teststring { get; set; }
            public int testint { get; set; }
            public List<string> stringlist { get; set; }
        }

        [TestMethod]
        public async Task PutGetDelete()
        {
            var testobj = new Tester() { teststring = "hello", testint = 123, stringlist = new List<string>() { "a", "b", "c" } };

            var resp = await db.Put(Rand.RandString("test"), testobj);
            Assert.AreNotEqual(resp.ID, string.Empty, "ID must be returned by put");

            var testobj2 = await db.Get<Tester>(resp.ID);
            Assert.AreEqual(testobj.teststring, testobj2.teststring, "Obj retrieved should be equal to saved obj");
            Assert.AreEqual(testobj.testint, testobj2.testint, "Obj retrieved should be equal to saved obj");
            Assert.AreEqual(testobj.stringlist.Count, testobj2.stringlist.Count, "Obj retrieved should be equal to saved obj");

            var del_resp = await db.Delete(resp.ID, resp.REV);
            Assert.IsTrue(del_resp.ok, "Object was deleted correctly");

        }

        public class TesterID : Document
        {
            public string teststring { get; set; }
        }

        [TestMethod]
        public async Task PutGetDeleteWithID()
        {
            var testobj = new TesterID() { ID=Rand.RandString(), teststring = "hello" };

            var resp = await db.Put(testobj);
            Assert.AreNotEqual(resp.ID, string.Empty, "ID must be returned by put");

            var testobj2 = await db.Get<TesterID>(resp.ID);
            Assert.AreEqual(testobj.teststring, testobj2.teststring, "Obj retrieved should be equal to saved obj");
            Assert.AreEqual(testobj.ID, testobj2.ID, "Obj retrieved should be equal to saved obj");

            var del_resp = await db.Delete(resp.ID, resp.REV);
            Assert.IsTrue(del_resp.ok, "Object was deleted correctly");

        }

        [TestMethod]
        public async Task TryGetTest()
        {
            var testobj = new TesterID() { ID = Rand.RandString(), teststring = "hello" };

            var testobj2 = await db.TryGet<TesterID>(testobj.ID);
            Assert.IsFalse(testobj2.IsSome(), "TryGet should have returned false");
            Assert.IsTrue(testobj2.IsNone(), "Should have returned isNone");


        }
    }
}
