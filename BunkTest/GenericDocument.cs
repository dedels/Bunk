using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Bunk;
using Bunk.Design;
using System.Threading.Tasks;
using System.Linq;

namespace BunkTest
{

    public class GDTest : Document
    {
        public string value1 { get; set; }
        public string value2 { get; set; }

        public static readonly BulkDocs GetRandTest26 = new BulkDocs(
                from i in Enumerable.Range(0, 26)
                let ia = "abcdefghijklmnopqrstuvwyxz"[i]
                select new GDTest() { value1 = i.ToString(), value2 = Rand.RandString(ia.ToString()), ID = Rand.RandString("bulktest"), 
                    TYPE=(i%2 ==0 ? "even":"odd" )
                }
            );
    }

    [TestClass]
    public class GenericDocumentTest : TempDBTest
    {
        private DocumentTypeMap tf;
        public GenericDocumentTest()
        {
            this.tf = new DocumentTypeMap();
            tf.Add("even", typeof(GDTest));
            tf.Add("odd", typeof(GDTest));
        }

        [TestMethod]
        public async Task GenericDocumentEvenOdd()
        {
            var bd = GDTest.GetRandTest26;
            bd.Documents = (from d in bd.Documents
                            orderby d.ID ascending
                            select d).ToList();

            var bd_resp = await this.db.BulkDocs(bd);

            var doc1 = await this.db.Get<GenericDocument>(bd_resp[0].ID);
            var doc_gd = doc1.FromFactory(this.tf) as GDTest;

            
            Assert.AreEqual(doc_gd.TYPE, bd.Documents[0].TYPE, "TYPE Should be the same for the first document");
            Assert.AreEqual(doc_gd.value1, (bd.Documents[0] as GDTest).value1, "value1 should be the same for both sent value and received value");
       
        }
    }
}
