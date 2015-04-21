using Bunk.CouchBuiltins;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bunk.Cloudant
{
    public static class Security
    {
        public async static Task<OK> EnableUsersDB(this UsersMaintenance maint, string for_db, CloudantSecurity cs = null)
        {
            if(cs==null)
                cs= new CloudantSecurity(){CouchDBAuthOnly=true};
            var resp = await maint.couchRepo.HttpClient.Put(maint.couchRepo.couchUrl.Add(for_db, "_security"));
            return maint.couchRepo.Deserialize<OK>(resp);
        }
    }

    public class CloudantSecurity
    {
        [JsonProperty("couch_auth_only")]
        public bool CouchDBAuthOnly;
        [JsonProperty("members")]
        public CloudantSecurityMembers Members { get; set; }
        [JsonProperty("admins")]
        public List<string> Admins { get; set; }
    }

    public class CloudantSecurityMembers
    {
        [JsonProperty("names")]
        public List<string> Names { get; set; }
        [JsonProperty("roles")]
        public List<string> Roles {get;set;}
    }
}
