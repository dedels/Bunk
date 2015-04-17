using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.FSharp.Collections;

namespace Bunk
{
    public class CouchUrl
    {
        public CouchUrl(){ }

        internal static CouchUrl Create(CouchRepo couchRepo, FSharpList<string> path, FSharpMap<string,string> queryString, FSharpList<CouchFilter> filters)
        {
            return new CouchUrl() { 
                couchRepo = couchRepo , 
                path = path ?? ListModule.Empty<string>(),
                queryString = queryString ?? MapModule.Empty<string,string>(),
                filters = filters ?? ListModule.Empty<CouchFilter>()
            };
        }
        public CouchRepo couchRepo { get; private set; }
        private FSharpList<string> path { get; set; }
        private FSharpList<CouchFilter> filters { get; set; }
        private FSharpMap<string, string> queryString { get; set; }


        internal CouchUrl DB(string db)
        {

            var path = ListModule.OfSeq<string>(new string[] { db });

            if (this.path.Length > 0) //replace just the first of the list
            {
                path = ListModule.Append(path, ListModule.Tail(this.path));
            }

            return Create(this.couchRepo, path, this.queryString, this.filters);
        }


        internal CouchUrl Add(params string[] collection)
        {
            return this.AddRange(collection);
        }
        internal CouchUrl AddRange(IEnumerable<string> collection)
        {
            return Create(this.couchRepo, ListModule.Append<string>(this.path, ListModule.OfSeq(collection)), this.queryString, this.filters);
        }




        public Uri Uri { 
            get {
                var pstr = String.Join("/", this.path);
                var qstr = String.Join("&",
                    from kv in this.queryString
                    select kv.Key+"="+CouchUrlEscape.Escape(kv.Value ?? String.Empty)
                );
                if (!String.IsNullOrEmpty(qstr))
                    pstr = pstr + "?" + qstr;
                return new Uri(this.couchRepo.config.Uri, pstr);
            }
        }

        internal CouchUrl QueryString(string key, string value)
        {
            return Create(this.couchRepo, this.path, this.queryString.Add(key, value), this.filters);
        }

        internal DB withDB(DB dB)
        {
            var new_db = new DB(dB.couchRepo, "ZZZZtempZZZZZ");
            new_db.couchUrl = this;

            return new_db;
        }

        internal System.Net.WebRequest RunFilters(System.Net.WebRequest req)
        {
            foreach (var f in this.filters)
                req = f(req);
            return req;
        }

        public CouchUrl Filter(CouchFilter f)
        {
            var fl = ListModule.OfSeq(new CouchFilter[] {f});
            fl = ListModule.Append<CouchFilter>(this.filters, fl);

            return Create(this.couchRepo, this.path, this.queryString, fl);
        }

        public CouchUrl ContentType(string content_type)
        {
            return this.Filter((wr) =>
            {
                wr.ContentType = content_type;
                return wr;
            });
        }

        internal CouchUrl QueryString(string key, bool value)
        {
            return this.QueryString(key, value ? "true" : "false");
        }

        internal CouchUrl QueryString(string key, int value)
        {
            return this.QueryString(key, value.ToString());
        }
    }

    public delegate System.Net.WebRequest CouchFilter(System.Net.WebRequest wr);

    /*
    public class CouchFilter 
    {
        private Func<System.Net.WebRequest, System.Net.WebRequest> func;
        public CouchFilter(Func<System.Net.WebRequest, System.Net.WebRequest> f)
        {
            this.func = f;
        }
        public System.Net.WebRequest Run(System.Net.WebRequest wr)
        {
            return this.func(wr);
        }

        public static implicit operator CouchFilter(Func<System.Net.WebRequest, System.Net.WebRequest> f)
        {
            return new CouchFilter(f);
        }
    }*/


    public class CouchUrlEscape
    {
        static Dictionary<char, string> Lookup = new Dictionary<char, string>(){
            { '[', "%5b" },
            { ']', "%5d" },
            { ',', "%2c" },
            { '?', "%3f" },
            { '&', "%26" },
            { '=', "%3d" },
            { '{', "%7b" },
            { '}', "%7d" },
        };

        /// <summary>
        /// Escape with url escape.  Dont use HTTPUtility.UrlEscape in System.Web
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static string Escape(string val)
        {
            var output = new StringBuilder();

            foreach(var c in val.ToCharArray())
            {
                string replaceval;
                if (Lookup.TryGetValue(c, out replaceval))
                    output.Append(replaceval);
                else
                    output.Append(c);
            }
            return output.ToString();
        }
    }
}
