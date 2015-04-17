using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Bunk;
using Bunk.Design;
using System.Threading.Tasks;

namespace BunkTest
{

    public class TestObj : Document {}
    public class TestD : DesignDoc
    {
        [View("aaaaaaaaaaaaaaaa")]
        public readonly MapFunction<string, string> to = new MapFunction<string, string>(
            @"function(doc){
                if(doc.type=='testobject')
                    emit(doc._id, doc.value1);
            }");


        [View]
        public readonly MapFunction<string, string> allTestObjs = new MapFunction<string,string>(
            @"function(doc){
                if(doc.type=='testobject')
                    emit(doc._id, doc.value1);
            }");

        public readonly MapFunction<string, string, testobject> allTestObjsInclude;

        public TestD(DB db) : base(db, Rand.RandString("testd") ) 
        {
            this.allTestObjsInclude = this.allTestObjs.IncludeDocs<testobject>();
        }
    }

    public class testobject : Document
    {
        public string value1 { get; set; }
        public string value2 { get; set; }
    }

    [TestClass]
    public class TestingDesignDoc : TempDBTest
    {
        [TestMethod]
        public async Task UploadChangesRevision()
        {
            var test_d = new TestD(this.db);
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(test_d));

            var resp1 = await test_d.Upload();
            Assert.IsTrue(resp1.ok, "Design doc was not uploaded", resp1);

            var resp2 = await test_d.Upload();
            Assert.IsTrue(resp2.ok, "Design doc was not uploaded second try", resp2);

            Assert.AreNotEqual(resp1.REV, resp2.REV, "Design doc should have been revised");
        }

        [TestMethod]
        public async Task UploadAndQuery()
        {
            var test_d = new TestD(this.db);
            var resp1 = await test_d.Upload();
            Assert.IsTrue(resp1.ok, "Design doc was not uploaded", resp1);

            var test_object = new testobject() { value1 = "dave", value2 = "abcd", ID = Rand.RandString("testobject") };
            await this.db.Put(test_object);

            var view_response = await test_d.allTestObjs.Get();
            Assert.AreEqual(1, view_response.TotalRows, "1 row should have been received according to total_rows");
            Assert.IsNotNull(view_response.Rows, "Rows should have been returend from view");
            Assert.AreEqual(1, view_response.Rows.Count, "1 row should be in the resultset");

            Assert.AreEqual(test_object.ID, view_response.Rows[0].ID, "Should have received the same object from the view");
        }

        [TestMethod]
        public async Task QueryIncludeDocs()
        {
            var test_d = new TestD(this.db);
            var resp1 = await test_d.Upload();
            Assert.IsTrue(resp1.ok, "Design doc was not uploaded", resp1);

            var test_object = new testobject() { value1 = "dave", value2 = "abcd", ID = Rand.RandString("testobject") };
            await this.db.Put(test_object);

            var includedocs_response = await test_d.allTestObjsInclude.Get();
            Assert.AreEqual(1, includedocs_response.Rows.Count, "1 row should be in the resultset");
            Assert.AreEqual(includedocs_response.Rows[0].Document.value1, test_object.value1, "Same value should be returned as was sent");

        }
    }
}
