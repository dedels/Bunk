using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Bunk;

namespace BunkTest
{
    [TestClass]
    public class BuildDesignDoc
    {
        [TestMethod]
        public void DesignDoc_Build()
        {
            var db = CouchRepo.Connect(Config.Get()).DB("bunk-test");
            var dd = new TestD(db);

            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(dd));
        }
    }
}
