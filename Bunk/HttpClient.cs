using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Bunk
{
    public class CouchResponse
    {

        public Stream Stream { get; set; }
        public string ContentType { get; set; }
        public long ContentLength { get; set; }

        private CouchRepo couchRepo;
        public CouchResponse(CouchRepo couchRepo)
        {
            this.couchRepo = couchRepo;
        }

        public CookieCollection Cookies 
        { 
            get
            {
                return ((HttpWebResponse)this.Response).Cookies;
            } 
        }

        public WebResponse Response { get; set; }
    }

    public interface IHttpClient
    {
        Task<CouchResponse> Get(CouchUrl couchUrl);
        Task<CouchResponse> Put(CouchUrl couchUrl);
        Task<CouchResponse> Put(CouchUrl couchUrl, Action<Stream> writeData);
        Task<CouchResponse> Delete(CouchUrl couchUrl);
        Task<CouchResponse> Delete(CouchUrl couchUrl, Action<Stream> writeData);

        Task<CouchResponse> Post(CouchUrl couchUrl, Action<Stream> writeData);

    }

    public class HttpClient : IHttpClient
    {
        internal HttpWebRequest WR(CouchUrl c)
        {

            var req = WebRequest.Create(c.Uri);

            req = c.RunFilters(req);

            return (HttpWebRequest)req;
        }

        private CouchResponse MakeResponse(CouchRepo couchRepo, WebResponse resp)
        {
            return new CouchResponse(couchRepo)
            {
                Stream = resp.GetResponseStream(),
                ContentLength = resp.ContentLength,
                ContentType = resp.ContentType,
                Response = resp
            };
        }


        
        public async Task<CouchResponse> Get(CouchUrl couchUrl)
        {
            try
            {
                var req = WR(couchUrl);
                req.Method = "GET";
                var resp = await req.GetResponseAsync();

                return this.MakeResponse(couchUrl.couchRepo, resp);
            }
            catch (WebException ex)
            {
                ex.RethrowBunkException();
                throw;
            }
        }

        public Task<CouchResponse> Put(CouchUrl couchUrl)
        {
            return Put(couchUrl, null);
        }
        public async Task<CouchResponse> Put(CouchUrl couchUrl, Action<System.IO.Stream> writeData)
        {
            try
            {
                var req = WR(couchUrl);
                req.Method = "PUT";
                if (writeData != null)
                    writeData(req.GetRequestStream());
                var resp = await req.GetResponseAsync();

                return this.MakeResponse(couchUrl.couchRepo, resp);
            }
            catch (WebException ex)
            {
                ex.RethrowBunkException();
                throw;
            }
        }



        public async Task<CouchResponse> Post(CouchUrl couchUrl, Action<System.IO.Stream> writeData)
        {
            try
            {
                var req = WR(couchUrl);
                req.Method = "POST";
                if (writeData != null)
                    writeData(req.GetRequestStream());
                var resp = await req.GetResponseAsync();

                return this.MakeResponse(couchUrl.couchRepo, resp);
            }
            catch (WebException ex)
            {
                ex.RethrowBunkException();
                throw;
            }
        }


        public Task<CouchResponse> Delete(CouchUrl couchUrl)
        {
            return Delete(couchUrl, null);
        }
        public async Task<CouchResponse> Delete(CouchUrl couchUrl, Action<System.IO.Stream> writeData)
        {
            try
            {
                var req = WR(couchUrl);
                req.Method = "DELETE";
                if (writeData != null)
                    writeData(req.GetRequestStream());
                var resp = await req.GetResponseAsync();

                return this.MakeResponse(couchUrl.couchRepo, resp);
            }
            catch (WebException ex)
            {
                ex.RethrowBunkException();
                throw;
            }
        }

    }
}
