using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bunk
{
    [JsonDictionary]
    public class GenericDocument : JObject, IDocument
    {
        [JsonProperty("_id")]
        public string ID
        {
            get
            {
                return this["_id"].ToString();
            }
            set
            {
                this["_id"] = value;
            }
        }
        
        [JsonProperty("_rev")]
        public string REV
        {
            get
            {
                return this["_rev"].ToString();
            }
            set
            {
                this["_rev"]=value;
            }
        }
        
        [JsonProperty("type")]
        public string TYPE
        {
            get
            {
                return this["type"].ToString();
            }
            set
            {
                this["type"]=value;
            }
        }


        public T As<T>()
        {
            return this.ToObject<T>();
        }

        public IDocument FromFactory(IDocumentTypeFactory df)
        {
            return (IDocument)(this.ToObject(df.GetNetTypeFor(this.TYPE)));
        }
    }

    public static class GenericDocumentHelper
    {
        public static IEnumerable<IDocument> FromFactory(this IEnumerable<GenericDocument> docs, IDocumentTypeFactory df)
        {
            return from d in docs
                   select d.FromFactory(df);
        }
    }


    public interface IDocumentTypeFactory 
    {
        Type GetNetTypeFor(string typename);
    }

    public class DocumentTypeMap : Dictionary<string,Type>, IDocumentTypeFactory
    {

        public Type GetNetTypeFor(string typename)
        {
            return this[typename];
        }
    }
}
