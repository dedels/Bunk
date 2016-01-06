using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Bunk;
using Bunk.Design;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using Bunk.DesignDoc;

namespace BunkTest.MapActionsTesting
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

    public class TestComposite
    {
        public List<ViewTestObj1> ListObj1 { get; } = new List<ViewTestObj1>();
        public List<ViewTestObj2> ListObj2 { get; } = new List<ViewTestObj2>();
    }

    public class ViewTestingDesignDoc : DesignDoc
    {
        public static readonly MapActions<TestComposite> testActions = MapActions<TestComposite>.Empty
            .Add<ViewTestObj1>("emit(5);", (tc, obj) => { tc.ListObj1.Add(obj); })
            .Add<ViewTestObj2>("emit(5);", (tc, obj) => { tc.ListObj2.Add(obj); });

        [View("o1")]
        public MapFunction<int, object> testMap { get; } = testActions.BuildMap<int, object>();
        public MapFunction<int, object,GenericDocument> testMapInclude { get; } 


        public ViewTestingDesignDoc(DB db) : base(db, Rand.RandString("testviewMapActions") ) 
        {
            this.testMapInclude = testMap.IncludeDocs<GenericDocument>();
        }
    }


    [TestClass]
    public class MapActions: TempDBTest
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
        public async Task MapActions_DoSimpleMap()
        {
            var newComposite = new TestComposite();
            await test_d.testMapInclude.GetKeys(5).ThroughFactory(ViewTestingDesignDoc.testActions, newComposite);

            Assert.IsNotNull(newComposite);
            Assert.IsNotNull(newComposite.ListObj1);
            Assert.IsTrue(newComposite.ListObj1.Count > 0);
            Assert.IsNotNull(newComposite.ListObj2);
            Assert.IsTrue(newComposite.ListObj2.Count > 0);
        }

    }
}
