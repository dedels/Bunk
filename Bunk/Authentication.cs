using Bunk.CouchBuiltins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bunk
{
    public class Authentication
    {
        private CouchRepo couchRepo;

        public Authentication(CouchRepo couchRepo)
        {
            this.couchRepo = couchRepo;
        }

        #region "Session methods"
        private string urlEncode(string str)
        {
            return str.Replace("%", "%25").Replace(".", "%2E").Replace("&", "%26").Replace("?", "?3F");
        }
        public async Task<System.Net.CookieCollection> LoginSession(string username, string password)
        {
            var auth_url = this.couchRepo.couchUrl
                .Add("_session")
                .ContentType("application/x-www-form-urlencoded")
                .Filter(wr => {
                    ((System.Net.HttpWebRequest)wr).CookieContainer = new System.Net.CookieContainer();
                    return wr;
                });

            var resp = await this.couchRepo.HttpClient.Post(auth_url, (stream) =>
            {
                
                using(var sw = new System.IO.StreamWriter(stream))
                    sw.Write(string.Format("name={0}&password={1}", urlEncode(username), urlEncode(password)));
            });

            return resp.Cookies;
        }

        public async Task<Session> Session()
        {
            var resp = await this.couchRepo.HttpClient.Get(this.couchRepo.couchUrl.Add("_session"));
            return couchRepo.Deserialize<Session>(resp);
        }

        public async Task LogoutSession()
        {
            await this.couchRepo.HttpClient.Delete(this.couchRepo.couchUrl.Add("_session"));
        }
        #endregion

    }

}
