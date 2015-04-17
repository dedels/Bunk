using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Bunk;
using Bunk.Design;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

namespace BunkTest.ViewTesting
{

    public class ViewTestObj1 : Document
    {
        public string value1;
        public string value2;
    }
        public class ViewTestObj2 : Document
    {
        public int value3;
        public int value4;
    }

    public class ViewTestingDesignDoc : DesignDoc
    {
        [View("o1")]
        public readonly MapFunction<string, string> o1 = new MapFunction<string, string>(
            @"function(doc){
                if(doc.type=='ViewTestObj1')
                    emit(doc._id, doc.value1+'/'+doc.value2);
            }");


        [View("o2")]
        public readonly MapFunction<int, string> o2 = new MapFunction<int,string>(
            @"function(doc){
                if(doc.type=='ViewTestObj2')
                    emit(doc.value3*doc.value4);
            }");

        [View]
        public readonly MapFunction<EmitKey, int> ArrView = new MapFunction<EmitKey, int>(
            @"function(doc){
                if(doc.type=='ViewTestObj2')
                    emit([100, 200, doc.value3, doc.value4], doc.value3);
            }");

        
        public ViewTestingDesignDoc(DB db) : base(db, Rand.RandString("testview") ) 
        {
        }
    }


    [TestClass]
    public class ViewTesting: TempDBTest
    {
        private ViewTestingDesignDoc test_d;
        private List<Document> objs;
        private BulkDocsResponse uploaded_objs;
        public override async Task ContinueInit()
        {
 	        this.test_d = new ViewTestingDesignDoc(this.db);
            var resp1 = await test_d.Upload();
            Assert.IsTrue(resp1.ok, "Design doc was not uploaded", resp1);

            this.objs = (
                from i in Enumerable.Range(0, 100)
                select i % 2 == 0 
                    ? (Document)new ViewTestObj1() { ID = "o1-" + i.ToString(), value1 = Rand.RandString(), value2 = i.ToString() }
                    : (Document)new ViewTestObj2() { ID = "o2-" + i.ToString(), value3 = i, value4 = i * 5 }
                    ).ToList();

            this.uploaded_objs = await this.db.BulkDocs(new BulkDocs(objs));
        }


        [TestMethod]
        public async Task ViewTest_GetByKeys()
        {
            var getkeys_resp = await this.db.AllDocs().GetKeys(this.uploaded_objs[75].ID, this.uploaded_objs[50].ID, this.uploaded_objs[25].ID);
            
            Assert.IsNotNull(getkeys_resp.Rows, "Rows should have been returend from view");
            Assert.AreEqual(3, getkeys_resp.Rows.Count(), "3 row should have been received according to total_rows");

            Assert.AreEqual(getkeys_resp.Rows[0].ID, this.uploaded_objs[75].ID, "First request object should be first returned object");

            var getkeys_resp_obj = await this.db.AllDocs().IncludeDocs<GenericDocument>().GetKeys(this.uploaded_objs[75].ID, this.uploaded_objs[50].ID, this.uploaded_objs[25].ID);

            Assert.IsNotNull(getkeys_resp_obj.Rows, "Rows should have been returend from view");
            Assert.AreEqual(3, getkeys_resp_obj.Rows.Count(), "3 row should have been received according to total_rows");
            Assert.IsNotNull(getkeys_resp_obj.Documents.ToList()[0], "A document should have been returned");
            Assert.AreEqual(getkeys_resp_obj.Documents.ToList()[1].ID, objs.ToList()[50].ID, "ID should match uploaded object");


            var get_o1_resp = await test_d.o1.IncludeDocs<ViewTestObj1>().Get();
            Assert.IsTrue((from r in get_o1_resp.Documents
                           select r).All(v => !String.IsNullOrEmpty(v.value2)), "all values should be filled");

            var get_o2_resp = await test_d.o2.IncludeDocs<ViewTestObj2>().Get();
            Assert.IsTrue(get_o2_resp.Documents.All(v => v.value4>0), 
                "all computed values should be greater than 0");

            Assert.IsTrue(get_o2_resp.Documents.All(v => v.value3 > 0 && v.value4 > 0), "Value3 and Value4 were not retrieved correctly");
        }

        [TestMethod]
        public async Task ViewTest_GetIntRange()
        {
            var view_result = await this.test_d.o2.Range(5, 845).IncludeDocs<ViewTestObj2>().Options(inclusive_end:false).Get();
            Assert.IsTrue(view_result.Rows.Count>0, "Rows should have been returned from the db");

            var num_5_845excl = (from o in this.objs
                                where o is ViewTestObj2
                                let o2 = o as ViewTestObj2
                                where o2.value3 * o2.value4 >= 5 && o2.value3 * o2.value4 < 845
                                select o2).Count();

            Assert.AreEqual(view_result.Rows.Count, num_5_845excl, "Should be {0} records returned when using exclusive", num_5_845excl);


            view_result = await this.test_d.o2.Range(5, 845).IncludeDocs<ViewTestObj2>().Options(inclusive_end:true).Get();
            Assert.IsTrue(view_result.Rows.Count > 0, "Rows should have been returned from the db");
            var num_5_845incl = ( from o in this.objs
                                  where o is ViewTestObj2
                                  let o2 = o as ViewTestObj2
                                  where o2.value3 * o2.value4 >= 5 && o2.value3 * o2.value4 <= 845
                                  select o2).Count();

            Assert.AreEqual(view_result.Rows.Count, num_5_845incl, "Should be {0} records returned when using exclusive", num_5_845incl);
        }

        [TestMethod]
        public async Task ViewTest_GetArrRange()
        {
            var view_result = await this.test_d.ArrView.Range(EmitKey.Array(100, 200), EmitKey.MaxArray(100, 200)).Get();
            Assert.IsTrue(view_result.Rows.Count > 0, "Rows should have been returned from the db");
            Assert.AreEqual(view_result.Rows.Count, 50, "50 rows should have been returned instead of {0}", view_result.Rows.Count);

            view_result = await this.test_d.ArrView.Range(EmitKey.Array(100, 200), EmitKey.Array(100, 200, 9, 45)).Options(inclusive_end: true).Get();
            Assert.AreEqual(view_result.Rows.Count, 5, "5 rows should have been returned instead of {0}", view_result.Rows.Count);

            view_result = await this.test_d.ArrView.Range(EmitKey.Array(100, 200), EmitKey.Array(100, 200, 9, 45)).Options(inclusive_end: false).Get();
            Assert.AreEqual(view_result.Rows.Count, 4, "4 rows should have been returned instead of {0}", view_result.Rows.Count);
        }

    }
}
