using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bunk.Design
{
    public abstract class ViewFunction :ICloneable, IEmbedDesign
    {
        internal DesignDoc dd;
        public string Name { get; private set; }
        internal virtual CouchUrl viewUrl { get; set; }

        internal ViewFunction() { }

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        public abstract JObject GetJO();

        public void Setup(DesignDoc dd, string element_name)
        {
            this.Name = element_name;
            this.dd= dd;

            this.viewUrl = this.dd.viewUrl.Add("_view", this.Name);
        }
    }

    public static class ViewFunctionHelpers
    {
        public static string TrueFalse(bool arg){
            return arg ? "true" : "false";
        }

        internal static T QueryString<T>(this T vf, string key, string value) where T: ViewFunction
        {
            var new_vf = (T)vf.Clone();
            new_vf.viewUrl=new_vf.viewUrl.QueryString(key, value);
            return new_vf;
        }

        /// <summary>
        /// All options:
        ///key	key-value
        ///keys	array of key-values
        ///startkey	key-value
        ///startkey_docid	document id
        ///endkey	key-value
        ///endkey_docid	document id
        ///limit	number of docs
        ///stale	ok / update_after
        ///descending	true / false
        ///skip	number of docs
        ///group	true
        ///group_level	number
        ///reduce	true / false
        ///include_docs	true / false
        ///inclusive_end	true / false
        ///update_seq	true / false 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="vf"></param>
        /// <param name="include_docs"></param>
        /// <returns></returns>
        public static T Options<T>(this T vf, bool? include_docs = null, bool? update_seq=null, bool? inclusive_end=null,
            int? skip=null, bool? descending=null, bool? stale=null, int? limit=null, string key=null) where T:ViewFunction
        {
            var new_vf = vf;
            
            if (!String.IsNullOrEmpty(key)) new_vf = new_vf.QueryString("key", key);
            if (limit.HasValue) new_vf = new_vf.QueryString("limit", limit.Value.ToString());
            if (stale.HasValue && stale.Value) new_vf = new_vf.QueryString("stale", "ok");
            if (descending.HasValue) new_vf = new_vf.QueryString("descending", TrueFalse(descending.Value));
            if (skip.HasValue) new_vf = new_vf.QueryString("skip", skip.Value.ToString());
            //if (group.HasValue) new_vf = new_vf.QueryString("group", group.Value);
            //if (group_level.HasValue) new_vf = new_vf.QueryString("group_level", group_level.Value);
            //if (reduce.HasValue) new_vf = new_vf.QueryString("reduce", reduce.Value);
            if (include_docs.HasValue) new_vf = new_vf.QueryString("include_docs", TrueFalse(include_docs.Value));
            if (inclusive_end.HasValue) new_vf = new_vf.QueryString("inclusive_end", TrueFalse(inclusive_end.Value));
            if (update_seq.HasValue) new_vf = new_vf.QueryString("update_seq ", TrueFalse(update_seq.Value));

            return new_vf;
        }

        public static MapFunction<EmitType, ObjType> Range<EmitType, ObjType>(this MapFunction<EmitType, ObjType> vf, EmitType startkey, bool? descending=null) 
        {
            var new_vf = vf
                .QueryString("startkey", startkey.ToString());

            if (descending.HasValue)
                new_vf = new_vf.QueryString("descending", TrueFalse(descending.HasValue));
            return new_vf;
        }
        public static MapFunction<EmitType, ObjType> Range<EmitType, ObjType>(this MapFunction<EmitType, ObjType> vf, EmitType startkey, EmitType endkey, bool? descending = null) 
        {
            var new_vf = vf
                .QueryString("startkey", startkey.ToString())
                .QueryString("endkey", endkey.ToString());

            if (descending.HasValue)
                new_vf = new_vf.QueryString("descending", TrueFalse(descending.HasValue));
            return new_vf;
        }

        public static T Limit<T>(this T vf, int limit) where T : ViewFunction
        {
            return vf.QueryString("limit", limit.ToString());
        }

        public static MapFunction<EmitType, ObjType> Paginate<EmitType, ObjType>(this MapFunction<EmitType, ObjType> vf, 
            EmitType startkey, string startkey_docid, int? limit = null)
        {
            var new_vf = vf
                .QueryString("startkey", startkey.ToString())
                .QueryString("startkey_docid",startkey_docid);
            if (limit.HasValue)
                new_vf = new_vf.QueryString("limit", limit.Value.ToString());
            return new_vf;
        }
    }


    public class MapFunction<EmitType, ObjType> : ViewFunction 
    {
        public string mapText { get; private set; }

        public MapFunction(string text) : base()
        {
            this.mapText = text; 
        }

        public override JObject GetJO()
        {
            return new JObject { { "map", this.mapText } };
        }




        public MapFunction<EmitType, ObjType, DocType> IncludeDocs<DocType>()
        {
            var new_vf = new MapFunction<EmitType, ObjType, DocType>(this.Options(include_docs:true));
            new_vf.dd = this.dd;
            return new_vf;
        }



        #region Invoke actions!
        public virtual Task<ViewResults<EmitType, ObjType>> Get()
        {
            return this.dd.GetView<ViewResults<EmitType, ObjType>>(this);
        }


        public Task<ViewResults<EmitType, ObjType>> GetKeys(params EmitType[] keys)
        {
            return this.GetKeys((IEnumerable<EmitType>)keys);
        }
        public Task<ViewResults<EmitType, ObjType>> GetKeys(IEnumerable<EmitType> keylist)
        {
            var keys = new KeyList<EmitType>(keylist);
            return this.dd.PostView<ViewResults<EmitType, ObjType>>(this, keys);
        }
        #endregion
    }


    public class MapFunction<EmitType, ObjType, DocType> : MapFunction<EmitType, ObjType>
    {
        private MapFunction<EmitType, ObjType> mapFunction;

        public MapFunction(MapFunction<EmitType, ObjType> mapFunction) :base(null)
        {
            this.mapFunction = mapFunction;
        }

        /// <summary>
        /// Careful!  This contains a mapfunction, is not exactly a map function
        /// </summary>
        internal override CouchUrl viewUrl {
            get {return this.mapFunction.viewUrl; } 
            set {this.mapFunction.viewUrl=value;} 
        }


        #region Invoke actions!
        public new Task<ViewResults<EmitType, ObjType, DocType>> Get()
        {
            return this.mapFunction.dd.GetView<ViewResults<EmitType, ObjType, DocType>>(this.mapFunction);
        }
        
        public new Task<ViewResults<EmitType, ObjType, DocType>> GetKeys(params EmitType[] keys)
        {
            return this.GetKeys((IEnumerable<EmitType>)keys);
        }
        public new Task<ViewResults<EmitType, ObjType, DocType>> GetKeys(IEnumerable<EmitType> keylist)
        {
            var keys = new KeyList<EmitType>(keylist);
            return this.dd.PostView<ViewResults<EmitType, ObjType, DocType>>(this.mapFunction, keys);
        }
        #endregion
    }


    public class AllDocs : MapFunction<string, DocumentResponse>
    {
        public AllDocs(DB db) : base(null)
        {
            this.viewUrl = db.couchUrl.Add("_all_docs");
            this.dd = new DesignDoc(db); //dummy;
        }
        public override Task<ViewResults<string, DocumentResponse>> Get()
        {
            return this.dd.GetView<ViewResults<string, DocumentResponse>>(this);
        }
    }


}
