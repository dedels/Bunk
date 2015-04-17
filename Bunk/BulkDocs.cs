using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bunk
{
    public class BulkDocs
    {

        public BulkDocs(IEnumerable<IDocument> docs)
        {
            this.Documents = new List<IDocument>(docs);
        }


        [JsonProperty("docs")]
        public List<IDocument> Documents { get; set; }

    }

    public class BulkDocsResponse : List<BulkDocumentResponse>
    {

    }

    public class BulkDocumentResponse : DocumentResponse
    {
        [JsonProperty("error")]
        public string Error {get;set;}
        [JsonProperty("reason")]
        public string Reason { get; set; }
    }


}
