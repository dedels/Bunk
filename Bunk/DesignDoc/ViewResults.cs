using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bunk.Design
{
    public class ViewResults<EmitType, ObjType>
    {
        [JsonProperty("total_rows")]
        public int TotalRows { get; set; }
        [JsonProperty("offset")]
        public int Offset { get; set; }

        [JsonProperty("rows")]
        public List<ViewResult<EmitType, ObjType>> Rows { get; set; }
    }

    public class ViewResults<EmitType, ObjType, DocType>
    {
        [JsonProperty("total_rows")]
        public int TotalRows { get; set; }
        [JsonProperty("offset")]
        public int Offset { get; set; }

        [JsonProperty("rows")]
        public List<ViewResult<EmitType, ObjType, DocType>> Rows { get; set; }


        public IEnumerable<DocType> Documents
        {
            get
            {
                return from r in this.Rows
                       select r.Document;
            }
        }
    }

    public class ViewResult<EmitType, ObjType>
    {
        [JsonProperty("id")]
        public string ID { get; set; }
        [JsonProperty("key")]
        public EmitType Key { get; set; }

        [JsonProperty("value")]
        public ObjType Value { get; set; }
    }

    public class ViewResult<EmitType, ObjType, DocType>
    {
        [JsonProperty("id")]
        public string ID { get; set; }
        [JsonProperty("key")]
        public EmitType Key { get; set; }

        [JsonProperty("value")]
        public ObjType Value { get; set; }

        [JsonProperty("doc")]
        public DocType Document { get; set; }


    }


    [JsonObject]
    public class KeyList<T>
    {
        [JsonProperty("keys")]
        public List<T> Keys{get;set;}

        public KeyList() { this.Keys = new List<T>(); }
        public KeyList(IEnumerable<T> keys)
        {
            this.Keys = new List<T>(keys);
        }
    }
}
