using Bunk;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bunk.Design
{
    [JsonObject(MemberSerialization.OptIn)]
    public class DesignDoc : Document
    {
        [JsonProperty]
        public string language { get; set; }

        private DB db { get; set; }
        internal CouchUrl viewUrl { get; private set; }

        public DesignDoc(DB db, string name = null)
        {
            this.db = db;
            this.ID = name ?? this.TYPE;

            this.viewUrl = this.db.couchUrl.Add("_design", this.ID);

            this.SetupEmbedded();
        }


        public async Task<CouchBuiltins.OKDocument> Upload()
        {
            var new_dd = (DesignDoc)this.MemberwiseClone();

            var old_dd = await this.db.TryGet<Document>(this.DESIGN_ID);
            if (old_dd.IsSome())
                new_dd.REV = old_dd.Value.REV;

            var resp = await db.Put<DesignDoc>(this.DESIGN_ID, new_dd);
            return resp;
        }


        [JsonProperty("views")]
        internal JObject Views
        {
            get
            {
                return new JObject { from e in this.Embedded
                                     let view_attrib = e.ba as ViewAttribute
                                     where view_attrib !=null
                                     select new JProperty( e.in_design.Name, e.in_design.GetJO())
                };
            }
        }

        [JsonIgnore]
        private string DESIGN_ID
        {
            get
            {
                var id = this.ID;
                if (id == null)
                    id = this.TYPE;
                if (!id.StartsWith("_design/"))
                    id = "_design/" + id;
                return id;
            }
        }
        [System.Runtime.Serialization.OnSerializing]
        internal void OnSerializingMethod(System.Runtime.Serialization.StreamingContext context)
        {
            this.ID = this.DESIGN_ID;
            this.TYPE = "design_doc";
            this.language = "javascript";
        }




        /// <summary>
        /// VIEW RETRIEVAL
        /// </summary>
        /// <param name="mapFunction"></param>
        /// <returns></returns>
        internal Task<T> GetView<T>(ViewFunction viewFcn)
        {
            return this.db.Get<T>(viewFcn.viewUrl);
        }

        internal Task<T> PostView<T>(ViewFunction viewFcn, object post_data)
        {
            return this.db.Post<T>(viewFcn.viewUrl, this.db.couchRepo.SerializeToRequest(post_data));
        }

        internal IEnumerable<Embedded> Embedded
        {
            get
            {
                foreach (var mem in this.GetType().GetMembers(
                    System.Reflection.BindingFlags.Public 
                    | System.Reflection.BindingFlags.Instance 
                    | System.Reflection.BindingFlags.GetField 
                    | System.Reflection.BindingFlags.GetProperty))
                {
                    foreach (var c in mem.GetCustomAttributes(true))
                    {
                        var ca = c as BunkAttribute;
                        if (ca == null) continue;
                        IEmbedDesign pembed = null;

                        var prop = mem as System.Reflection.PropertyInfo;
                        if (prop != null)
                            pembed = prop.GetValue(this) as IEmbedDesign;
                        else
                        {
                            var field = mem as System.Reflection.FieldInfo;
                            if (field != null) pembed = field.GetValue(this) as IEmbedDesign;
                        }
                        if (pembed == null) continue;

                        yield return new Embedded(pembed, ca, mem);
                    }
                 
                }
            }
        }

        /// <summary>
        /// Go through embedded properties looking for BunkAttributes
        /// </summary>
        private void SetupEmbedded()
        {
            foreach (var e in this.Embedded)
            {
                e.Setup(this);
            }
        }
    }


    internal class Embedded
    {
        public Embedded(IEmbedDesign in_design, BunkAttribute ba, System.Reflection.MemberInfo prop)
        {
            this.in_design = in_design;
            this.ba = ba;
            this.prop = prop;
        }

        public IEmbedDesign in_design { get; private set; }

        public BunkAttribute ba { get; private set; }

        public System.Reflection.MemberInfo prop { get; set; }

        internal void Setup(DesignDoc designDoc)
        {
            this.in_design.Setup(designDoc, this.ba.Name ?? this.prop.Name);
        }
    }
    public interface IEmbedDesign
    {
        JObject GetJO();
        string Name { get; }

        void Setup(DesignDoc dd, string element_name);
    }

}
