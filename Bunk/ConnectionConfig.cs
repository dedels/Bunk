using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bunk
{
    public class ConnectionConfig
    {
        public ConnectionConfig(string url, string username=null, string password=null, bool use_proxy=true)
        {
            this.Uri = new Uri(url);
            this.DefaultFilters = new List<CouchFilter>();

            if (!string.IsNullOrEmpty(username))
                this.DefaultFilters.Add((req) =>
                {
                    string _auth = string.Format("{0}:{1}", username, password);
                    string _enc = Convert.ToBase64String(Encoding.ASCII.GetBytes(_auth));
                    string _cred = string.Format("{0} {1}", "Basic", _enc);
                    req.Headers[System.Net.HttpRequestHeader.Authorization] = _cred;
                    return req;
                });


            if (!use_proxy)
                this.DefaultFilters.Add((req) =>
                {
                    req.Proxy = null; 
                    return req;
                });
        }

        public Uri Uri { get; private set; }
        public List<CouchFilter> DefaultFilters {get; private set; }
    }

}
