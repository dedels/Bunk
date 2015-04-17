using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Bunk;
using Bunk.Design;
using System.Threading.Tasks;
using System.Linq;

namespace BunkTest
{

    public class BulkTestO : Document
    {
        public string value1 { get; set; }
        public string value2 { get; set; }

        public static readonly BulkDocs GetRandTest26 = new BulkDocs(
                from i in Enumerable.Range(0, 26)
                let ia = "abcdefghijklmnopqrstuvwyxz"[i]
                select new BulkTestO() { value1 = i.ToString(), value2 = Rand.RandString(ia.ToString()), ID = Rand.RandString("bulktest") }
            );
    }

    [TestClass]
    public class BulkDocsTest : TempDBTest
    {
        [TestMethod]
        public async Task UploadBulk()
        {
            var bd = BulkTestO.GetRandTest26;

            var bd_resp = await this.db.BulkDocs(bd);
            Assert.AreEqual(bd.Documents.Count, bd_resp.Count, "Should have received the same number of docs in bulk docs response");
            Assert.AreEqual(bd.Documents[0].ID, bd_resp[0].ID, "ID should be the same for every element pair");

            bd.Documents = (from d in bd.Documents
                           orderby d.ID
                           select d).ToList();

            await Task.Delay(100); // view doesn't return results immediately

            var alldocs = await this.db.AllDocs().Get();
            Assert.AreEqual(bd.Documents.Count, alldocs.Rows.Count, "Should have received same number of docs from all docs");

            var alldocs_include = await this.db.AllDocs().IncludeDocs<BulkTestO>().Get();
            Assert.AreEqual(bd.Documents.Count, alldocs_include.Rows.Count, "Should have received same number of docs from all docs include");
            Assert.AreEqual(alldocs_include.Rows[0].ID, alldocs.Rows[0].ID, "Same result should be in first record with and without include_docs");
            Assert.AreEqual(alldocs_include.Rows[0].Document.ID, bd.Documents[0].ID, "Check that alphabetically first result is same as alpha first bulk doc");


            //DELETE testing

            bd = new BulkDocs(from r in alldocs_include.Rows
                              let d = r.Document
                              select d.value1.StartsWith("2") && d.value1.Length>1 ? d.Delete() : d
                              );

            bd_resp = await this.db.BulkDocs(bd);
            Assert.AreEqual(bd.Documents.Count, bd_resp.Count, "Should have received the same number of docs in bulk docs response");

            await Task.Delay(100); // view doesn't return results immediately

            alldocs = await this.db.AllDocs().Get();
            Assert.AreEqual(20, alldocs.Rows.Count, "6 docs should have been deleted, leaving 20 non deleted for result set");


            //delete everything else
            bd = new BulkDocs(from r in bd_resp
                              select r.Delete());
            bd_resp = await this.db.BulkDocs(bd);
            await Task.Delay(100); // view doesn't return results immediately
            alldocs = await this.db.AllDocs().Get();
            Assert.AreEqual(0, alldocs.Rows.Count, "All docs should be deleted");

       
        }
    }
}
