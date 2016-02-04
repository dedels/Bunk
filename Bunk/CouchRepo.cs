using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bunk
{
    public class CouchRepo : ICouchRepo
    {
        private CouchRepo(ConnectionConfig config) { 
            this.config = config;
            this.couchUrl = CouchUrl.Create(this, null, null, null);

            //By default set application/json
            this.couchUrl = this.couchUrl.Filter((req) =>
            {
                req.ContentType = "application/json";
                return req;
            });
            
            foreach (var f in config.DefaultFilters)
            {
                this.couchUrl = this.couchUrl.Filter(f);
            }

        }
        public static CouchRepo Connect(ConnectionConfig config)
        {
            return new CouchRepo(config);
        }
        public static CouchRepo Connect(string url)
        {
            return Connect(new ConnectionConfig(url));
        }

        public DB DB(string name)
        {
            return new DB(this, name);
        }

        public IHttpClient HttpClient
        {
            get
            {
                return new HttpClient();
            }
        }

        public ConnectionConfig config { get; private  set; }
        public CouchUrl couchUrl { get; private  set; }

        private static JsonSerializer jsx = new JsonSerializer(); //lets assume this is threadsafe

        internal Action<System.IO.Stream> SerializeToRequest<T>(T obj)
        {
            //return JsonConvert.SerializeObject(obj);
            return (System.IO.Stream reqStream) =>
            {
                using (var outputStream = new JsonTextWriter(new System.IO.StreamWriter(reqStream)))
                {
                    jsx.Serialize(outputStream, obj);
                }
            };

        }

        internal T Deserialize<T>(string js_string)
        {
            return JsonConvert.DeserializeObject<T>(js_string);
        }

        public T Deserialize<T>(CouchResponse response)
        {
            using (var inputStream = new JsonTextReader(new System.IO.StreamReader(response.Stream)))
            {
                return jsx.Deserialize<T>(inputStream);
            }
        }


        public UsersMaintenance UserMaintenance()
        {
            return new UsersMaintenance(this);
        }

        public Authentication Authentication()
        {
            return new Authentication(this);
        }

        public async Task<List<string>> UUIDs(int count = 10)
        {
            var resp = await this.HttpClient.Get(this.couchUrl.Add("_uuids").QueryString("count", count));
            var uu= this.Deserialize<UUIDs>(resp);
            return (from u in uu.uuids select u).ToList();
        }

        public async Task<List<string>> AllDBs()
        {
            var resp = await this.HttpClient.Get(this.couchUrl.Add("_all_dbs"));
            return this.Deserialize<List<string>>(resp);
        }


    }


    public class UUIDs {
        public List<string> uuids;
    }

}
