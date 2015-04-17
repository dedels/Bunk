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

        public async Task<List<string>> LoginSession(string username, string password)
        {
            var auth_url = this.couchRepo.couchUrl
                .Add("_session")
                .ContentType("application/x-www-form-urlencoded");

            var resp = await this.couchRepo.HttpClient.Post(auth_url, (stream) =>
            {
                using(var sw = new System.IO.StreamWriter(stream))
                    sw.Write(string.Format("name={0}&password={1}", username, password));
            });

            if (resp.Cookies==null) return new List<String>();
            else return resp.Cookies.ToList();
        }

    }
}
