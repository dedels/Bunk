using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bunk
{
    public interface IDocument
    {
        string ID { get; set; }
        string REV { get; set; }
        string TYPE { get; set; }
    }

    public class Document :IDocument, ICloneable
    {
        [JsonProperty("_id")]
        public string ID
        {
            get;
            set;
        }

        [JsonProperty("_rev", NullValueHandling = NullValueHandling.Ignore)]
        public string REV
        {
            get;
            set;
        }

        private string _type=null;
        [JsonProperty("type")]
        public string TYPE
        {
            get { return this._type = (this._type ?? this.GetType().Name); }
            set { this._type = value; } 
        }

        [JsonProperty("_deleted", DefaultValueHandling=DefaultValueHandling.Ignore)]
        public bool Deleted
        {
            get;
            internal set;
        }


        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }

    public static class DocumentHelper
    {
        public static T Delete<T>(this T obj) where T:Document
        {
            var n = (T)obj.Clone();
            n.Deleted = true;
            return n;
        }

    }


    public class DocumentResponse
    {
        [JsonProperty("id")]
        public string ID { get; set; }
        [JsonProperty("rev")]
        public string REV { get; set; }

        public Document Delete()
        {
            return new Document() { ID = this.ID, REV = this.REV, Deleted = true };
        }
    }


}
