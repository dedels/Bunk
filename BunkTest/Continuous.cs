using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Bunk;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace BunkTest.Continuous
{

    [TestClass]
    public class ContinuousTest :TempDBTest
    {

        [TestMethod]
        public async Task Continous_Bulk()
        {
            var responses=new List<string>();
            var ct = this.db.Continuous();

            var task = ct.Start((string line) =>
            {
                responses.Add(line);
                Console.WriteLine(line);
            });
            

            var bd = await this.db.BulkDocs(new BulkDocs(from i in Enumerable.Range(1, 100)
                                          select new GenericDocument() { ID = i.ToString(), TYPE = "test" }));

            await Task.Delay(5000);
            Assert.AreEqual(100, responses.Count, "Should have received 100 responses on the continuous feed for 100 docs uploaded");

            bd = await this.db.BulkDocs(new BulkDocs(from i in Enumerable.Range(101, 100)
                                                     select new GenericDocument() { ID = i.ToString(), TYPE = "test" }));

            await Task.Delay(5000);
            Assert.AreEqual(200, responses.Count, "Should have received a total of 200 responses on the continuous feed for next 100 docs uploaded");

            bd = await this.db.BulkDocs(new BulkDocs(from d in bd select new GenericDocument() {ID=d.ID, REV=d.REV, TYPE="test-updated"}));

            await Task.Delay(5000);
            Assert.AreEqual(300, responses.Count, "Should have received a total of 300 responses on the continuous feed for next 100 docs updated");

            ct.Stop();
            await task;
        }

        [TestMethod]
        public async Task Continous_Filter()
        {
            await Task.Delay(0);
        }
    }
}
