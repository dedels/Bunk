using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Bunk
{
    public class DocumentWithAttachments : Document
    {
        [JsonProperty("_attachments", NullValueHandling= NullValueHandling.Ignore)]
        public Dictionary<string,Attachment> Attachments { get; set; }
    }


    public class Attachment
    {
        [JsonProperty("content_type")]
        public string ContentType { get; set; }

        [JsonProperty("data")]
        public virtual string Data { get; set; }

        [JsonProperty("digest")]
        public string Digest { get; set; }
        public bool ShouldSerializeDigest() { return false; }

        [JsonProperty("encoded_length")]
        public int EncodedLength { get; set; }
        public bool ShouldSerializeEncodedLength() { return false; }

        [JsonProperty("encoding")]
        public string Encoding { get; set; }
        public bool ShouldSerializeEncoding() { return false; }

        [JsonProperty("length")]
        public int Length { get; set; }
        public bool ShouldSerializeLength() { return false; }

        [JsonProperty("revpos")]
        public int RevPos { get; set; }
        public bool ShouldSerializeRevPos() { return false; }

        [JsonProperty("stub")]
        public bool Stub { get; set; }
        public bool ShouldSerializeStub() { return false; }
                

        public virtual byte[] GetData()
        {
            if (this.Stub)
                throw new AttachmentException("Cannot get data from a stub response.  Use ?attachments=true or fetch the attachment directly.");

            return Convert.FromBase64String(this.Data);
        }

        public string GetDataString()
        {
            return System.Text.Encoding.UTF8.GetString(this.GetData());
        }

        public virtual void SetData(byte[] data)
        {
            this.Data = Convert.ToBase64String(data);
        }
        public void SetDataString(string data)
        {
            this.SetData(System.Text.Encoding.UTF8.GetBytes(data));
        }
    }

    public static class AttachmentHelper
    {

        public static async Task SetData(this Attachment att, System.IO.Stream stream, long length)
        {
            var data = new byte[length];

            int pos = 0, step = 1000;
            while (pos < length && await stream.ReadAsync(data, pos, Math.Min(step, (int)(length - pos))) > 0) { pos += step;  };
            
            att.SetData(data);
        }
    }


    public class DBAttachment
    {
        private DB db;
        private string id;
        private string rev;

        public DBAttachment(DB db, string id, string rev)
        {
            this.db = db;
            this.id = id;
            this.rev = rev;
        }

        public Task<Bunk.CouchBuiltins.OKDocument> Put(string attachment_name, string content_type, string data)
        {
            return this.Put(attachment_name, content_type, Encoding.UTF8.GetBytes(data));
        }
        
        public Task<Bunk.CouchBuiltins.OKDocument> Put(string attachment_name, string content_type, byte[] data)
        {
            if (this.rev == null) throw new BunkException("When putting an attachment, rev is required.");

            var url = this.db.couchUrl
                .Add(this.id, attachment_name)
                .QueryString("rev", this.rev)
                .ContentType(content_type);

            return url.withDB(this.db).PutContent(data);
        }

        public Task<Attachment> Get(string attachment_name)
        {
            var url = this.db.couchUrl
                .Add(this.id, attachment_name);

            if (this.rev!=null)
                url = url.QueryString("rev", this.rev);

            return url.withDB(this.db).GetContent();
        }
    }
}
