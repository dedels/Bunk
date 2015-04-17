using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Bunk
{
    public class Continuous
    {
        private DB db;
        private CouchUrl couchUrl;
        private System.Threading.CancellationTokenSource cancellationTokenSource;

        public Continuous(DB db, 
            int? timeout=null, 
            int? heartbeat=null,
            bool? conflicts = null,  
            bool? descending = null,  
            string filter = null,  
            bool? include_docs = null,  
            bool? attachments = null,  
            bool? att_encoding_info = null,  
            int? last_event_id = null,  
            int? limit = null,  
            string since = null,  
            string style = null,    
            string view = null)
        {
            this.db = db;
            this.couchUrl = this.db.couchUrl.Add("_changes").QueryString("feed", "continuous");
            this.cancellationTokenSource = new System.Threading.CancellationTokenSource();
            this.couchUrl = this.couchUrl.Filter((wr) => //Default to 30 second timeout
            {
                var tcp_timeout = (timeout.HasValue ? timeout.Value : 60000) / 2;
                ((HttpWebRequest)wr).ServicePoint.SetTcpKeepAlive(true, tcp_timeout, tcp_timeout);
                return wr;
            });


            if (timeout.HasValue){
                this.couchUrl = this.couchUrl.QueryString("timeout", timeout.Value);
            }
            if (heartbeat.HasValue)
                this.couchUrl = this.couchUrl.QueryString("heartbeat", heartbeat.Value);

            if (conflicts.HasValue)
                this.couchUrl = this.couchUrl.QueryString("conflicts", conflicts.Value);

            if (descending.HasValue)
                this.couchUrl = this.couchUrl.QueryString("descending", descending.Value);
            if (filter !=null)
                this.couchUrl = this.couchUrl.QueryString("filter", filter);
            if (include_docs.HasValue)
                this.couchUrl = this.couchUrl.QueryString("include_docs", include_docs.Value);
            if (attachments.HasValue)
                this.couchUrl = this.couchUrl.QueryString("attachments", attachments.Value);
            if (att_encoding_info.HasValue)
                this.couchUrl = this.couchUrl.QueryString("att_encoding_info", att_encoding_info.Value);
            if (last_event_id.HasValue)
                this.couchUrl = this.couchUrl.QueryString("last-event-id", last_event_id.Value);
            if (limit.HasValue)
                this.couchUrl = this.couchUrl.QueryString("limit", limit.Value);
            if (since!=null)
                this.couchUrl = this.couchUrl.QueryString("since",since);
            if (style!=null)
                this.couchUrl = this.couchUrl.QueryString("style", style);
            if (view!=null)
                this.couchUrl = this.couchUrl.QueryString("view", view);
        }

        public void Stop()
        {
            this.cancellationTokenSource.Cancel();
        }

        /// <summary>
        /// Given a view function, build up the view name for use as the view param to the continuous feed
        /// </summary>
        /// <param name="vf">ViewFunction that is attached to a design document</param>
        /// <returns>New Continuous object (non-started)</returns>
        public Continuous View(Design.ViewFunction vf)
        {
            var new_c = new Continuous(this.db);
            new_c.couchUrl = this.couchUrl;

            new_c.couchUrl = new_c.couchUrl.QueryString("view", string.Format("{0}/{1}", vf.dd.ID, vf.Name));
            return new_c;
        }

        public Task Start<T>(Action<T> action)
        {   //TODO: changes feed has a fixed format...
            return this.Start((string record) =>
            {
                T processed_value = this.couchUrl.couchRepo.Deserialize<T>(record);
                action(processed_value);
            });
        }
        public Task Start(Action<string> action)
        {

            return Task.Factory.StartNew(() =>
            {
                var http_client = (HttpClient)this.db.couchRepo.HttpClient;
                var wr = (HttpWebRequest)http_client.WR(this.couchUrl);
                wr.KeepAlive = true;
                wr.BeginGetResponse(BeginEndCallback, new ContinuousState(wr, action, this.cancellationTokenSource.Token));

            }, this.cancellationTokenSource.Token); //,TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        private void BeginEndCallback(IAsyncResult ar)
        {
            var state = (ContinuousState)ar.AsyncState;
            var request = state.wr;
            using (var response = request.EndGetResponse(ar))
            using (var reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8, true, 1024, true))
            {
                while (!reader.EndOfStream && !state.cancellationToken.IsCancellationRequested)
                {
                    var val = reader.ReadLine();
                    if(!string.IsNullOrEmpty(val))
                        state.action(val);
                }
            }
        }

    }

    class ContinuousState
    {

        public ContinuousState(HttpWebRequest wr, Action<string> action, System.Threading.CancellationToken cancellationToken)
        {
            this.wr = wr;
            this.action = action;
            this.cancellationToken = cancellationToken;
        }


        public HttpWebRequest wr { get; private set; }

        public Action<string> action { get; private set; }

        public System.Threading.CancellationToken cancellationToken { get; private set; }
    }
}
