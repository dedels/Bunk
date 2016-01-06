using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Bunk.CouchBuiltins
{

    public class OK
    {
        public bool ok { get; set; }
        public string error { get; set; }
        public string reason { get; set; }

        [OnDeserialized]
        internal void OnDeserialized(StreamingContext ctx){

        }

    }
    public class OKDeleteDocument : OK
    {
        [JsonProperty("rev")]
        public string REV { get; set; }
    }
    public class OKDocument : OK
    {
        [JsonProperty("id")]
        public string ID { get; set; }
        [JsonProperty("rev")]
        public string REV { get; set; }

        public void ThrowOnError()
        {
            if (!ok)
                throw new BunkException($"{this.error}: {this.reason}");
        }
    }

#pragma warning disable 169
    public class DBInfo
    {
        bool compact_running; 
        String db_name;
        int disk_format_version;
        int disk_size;
        int doc_count;
        int doc_del_count;
        int instance_start_time; //in micro-seconds
        int purge_seq;
        int update_seq;

        //custom
        public bool exists {get;set;}
    }
#pragma warning restore 169





    public class Session
    {
        public SessionInfo Info { get; set; }
        public bool OK { get; set; }
        public UserContext UserCtx { get; set; }
    }

    public class SessionInfo
    {
        [JsonProperty("authenticated")]
        public string Authenticated { get; set; }
        [JsonProperty("authentication_db")]
        public string AuthenticationDB { get; set; }
        [JsonProperty("authentication_handlers")]
        public List<string> AuthenticationHandlers { get; set; }
    }

    public class UserContext
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("roles")]
        public List<string> Roles { get; set; }
    }

    public class User : Document
    {
        private string _name = "";

        [JsonProperty("name")]
        public string Name {
            get { return this._name; }
            set {this._name=value.Replace("org.couchdb.user:", ""); } 
        }
        [JsonProperty("roles")]
        private List<string> _roles = null;
        public List<string> Roles { get { return this._roles ?? new List<string>(); } set { this._roles = value; } }
        [JsonProperty("password_sha")]
        public string PasswordSha { get; set; }
        [JsonProperty("password_scheme")]
        public string PasswordScheme { get; set; }
        [JsonProperty("password")]
        public string Password { get; set; }
        [JsonProperty("salt")]
        public string Salt { get; set; }

        [JsonProperty("type")]
        public override string TYPE { get { return "user"; } set { } }
        [JsonProperty("_id")]
        public override string ID { get { return "org.couchdb.user:"+this.Name; } set { } }

        public User GrantRoles(params string[] roles)
        {
            foreach (var r in roles)
                this.GrantRole(r);

            return this;
        }
        public User GrantRole(string role)
        {
            if (this.Roles == null)
            {
                this.Roles = new List<string>();
                this.Roles.Add(role);
                return this;
            }
            if (this.Roles.IndexOf(role) > 0)
                return this;

            this.Roles.Add(role);
            return this;
        }
        public User GrantReader()
        {
            return this.GrantRole("_reader");
        }

        public User GrantWriter()
        {
            return this.GrantRole("_writer");
        }
    }
}
