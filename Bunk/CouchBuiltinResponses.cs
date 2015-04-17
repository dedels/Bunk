using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Bunk.CouchBuiltinResponses
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
    }

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


    public class CouchException : Exception
    {
        private string error;
        private string reason;
        public CouchException(string error, string reason)
        {
            this.error = error; this.reason = reason;
        }

        public override string ToString()
        {
            return string.Format("CouchException-{0}: {1}", error, reason);
        }

    }
}
