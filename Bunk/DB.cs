using Bunk.CouchBuiltins;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Bunk
{
    public class DB
    {
        private readonly Design.MapFunction<string, DocumentResponse> allDocs;
        public DB(CouchRepo couchRepo, string name)
        {
            this.couchRepo = couchRepo;
            this.name = name;
            this.couchUrl = this.couchRepo.couchUrl.DB(name);

            this.allDocs = new Design.AllDocs(this);
        }

        public CouchRepo couchRepo { get; private  set; }
        public string name { get; private  set; }
        public CouchUrl couchUrl { get;  set; }

        public async Task<OK> CreateDB()
        {
            var response = await couchRepo.HttpClient.Put(this.couchUrl);
            return couchRepo.Deserialize<OK>(response);
        }
        public async Task<OK> DeleteDB()
        {
            var response = await couchRepo.HttpClient.Delete(this.couchUrl);
            return couchRepo.Deserialize<OK>(response);
        }
        public async Task<DBInfo> DBInfo()
        {
            try
            {
                var response = await couchRepo.HttpClient.Get(this.couchUrl);
                var dbi = couchRepo.Deserialize<DBInfo>(response);
                dbi.exists = true;
                return dbi;
            }
            catch (NotFoundException ex) 
            {
                return new DBInfo() { exists = false };
            }
        }

        public DB Options(bool? attachments=null, string atts_since=null)
        {
            var new_db = new DB(this.couchRepo,this.name);
            if (attachments.HasValue)
                new_db.couchUrl = new_db.couchUrl.QueryString("attachments", attachments.Value );
            if (atts_since!=null)
                new_db.couchUrl = new_db.couchUrl.QueryString("atts_since", atts_since);

            return new_db;
        }

        public async Task<T> Get<T>(CouchUrl url)
        {
            var response = await couchRepo.HttpClient.Get(url);

            return couchRepo.Deserialize<T>(response);
        }
        public Task<T> Get<T>(string id)
        {
            var url = this.couchUrl.Add(id);
            return this.Get<T>(url);
        }

        public async Task<Microsoft.FSharp.Core.FSharpOption<T>> TryGet<T>(string id)
        {
            var url = this.couchUrl.Add(id);
            try
            {
                var response = await couchRepo.HttpClient.Get(url);

                return Microsoft.FSharp.Core.FSharpOption<T>.Some(
                    couchRepo.Deserialize<T>(response)
                    );
            }
            catch (NotFoundException ex) 
            {
                return Microsoft.FSharp.Core.FSharpOption<T>.None;
            }
        }


        public async Task<OKDocument> Put<T>(string id, T obj)
        {
            var url = this.couchUrl.Add(id);
            var response = await couchRepo.HttpClient.Put(url, couchRepo.SerializeToRequest(obj));
            return couchRepo.Deserialize<OKDocument>(response);
        }

        public Task<OKDocument> Put<T>(T obj) where T: IDocument
        {
            return this.Put<T>(obj.ID, obj);
        }

        internal async Task<T> Post<T>(CouchUrl couchUrl, Action<System.IO.Stream> action)
        {
            var response = await couchRepo.HttpClient.Post(couchUrl, action);
            return couchRepo.Deserialize<T>(response);
        }


        public async Task<OKDeleteDocument> Delete(string id, string rev)
        {
            var url = this.couchUrl
                .Add(id)
                .QueryString("rev", rev);
            var response = await couchRepo.HttpClient.Delete(url);
            return couchRepo.Deserialize<OKDeleteDocument>(response);
        }
        public Task<OKDeleteDocument> Delete<T>(T obj) where T : IDocument
        {
            return this.Delete(obj.ID, obj.REV);
        }

        public async Task<BulkDocsResponse> BulkDocs(BulkDocs bd)
        {
            var response = await couchRepo.HttpClient.Post(this.couchUrl.Add("_bulk_docs"), couchRepo.SerializeToRequest(bd));
            return couchRepo.Deserialize<BulkDocsResponse>(response);
        }

        public Design.MapFunction<string,DocumentResponse> AllDocs()
        {
            return this.allDocs;
        }


        /// <summary>
        /// Operate directly on an document's attachment without touching the document directly
        /// </summary>
        /// <returns></returns>
        public DBAttachment Attachment(string id, string rev=null)
        {
            return new DBAttachment(this, id, rev);
        }


        internal async Task<OKDocument> PutContent(byte[] data)
        {
            var cu = this.couchUrl.Filter((wr) => { wr.ContentLength = data.Length; return wr; });

            var resp = await this.couchRepo.HttpClient.Put(cu, (stream) =>
            {
                stream.Write(data, 0, data.Length);
            });
            return couchRepo.Deserialize<OKDocument>(resp);
        }

        internal async Task<Attachment> GetContent()
        {
            var resp = await this.couchRepo.HttpClient.Get(this.couchUrl);

            var result = new Attachment() { ContentType = resp.ContentType };
            await result.SetData(resp.Stream, resp.ContentLength);
            return result; 
        }


        public Continuous Continuous(int? timeout=null, int? heartbeat=null)
        {
            return new Continuous(this, timeout, heartbeat);
        }
    }
}
