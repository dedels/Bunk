using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Bunk;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace BunkTest.Attachments
{
    [TestClass]
    public class SingeleDoc :TempDBTest
    {
        public class Tester : DocumentWithAttachments
        {
            public string val { get; set; }
        }


        public static string attachment_data = @"Standalone Attachments

CouchDB allows to create, change and delete attachments without touching the actual document.

You need to specify a MIME type using the Content-Type header. CouchDB will serve the attachment with the specified Content-Type when asked.

To create an attachment on an existing document";



        [TestMethod]
        public async Task Attachment_Upload()
        {
            var testobj = new Tester() { ID="tester_upload", val = "hello" };

            var resp = await this.db.Put(testobj);

            var att_resp = await this.db.Attachment(resp.ID, resp.REV).Put("test_attachment", "text/plain", attachment_data);
            Assert.IsNotNull(att_resp.ID, "ID should be returned from the attachment request");
            Assert.IsNotNull(att_resp.REV, "REV should be returned from the attachment request");

        }

        [TestMethod]
        public async Task Attachment_UploadMultipleBody()
        {
            var testobj = new Tester() { ID = "tester_uploadmulti", val = "hello" };
            testobj.Attachments = new Dictionary<string, Attachment>();
            testobj.Attachments["attach1"] = new Attachment() { ContentType = "text/plain" };
            testobj.Attachments["attach1"].SetDataString("1" + attachment_data);

            testobj.Attachments["attach2"] = new Attachment() { ContentType = "text/plain" };
            testobj.Attachments["attach2"].SetDataString("2" + attachment_data);

            var resp = await this.db.Put(testobj);

            var att_resp = await this.db.Attachment(resp.ID).Get("attach1");
            Assert.AreEqual(att_resp.Data, testobj.Attachments["attach1"].Data, "Should have retrieved the same data that was sent for attach1");

            att_resp = await this.db.Attachment(resp.ID).Get("attach2");
            Assert.AreEqual(att_resp.Data, testobj.Attachments["attach2"].Data, "Should have retrieved the same data that was sent for attach2");

        }

        [TestMethod]
        public async Task Attachment_RetreiveInOptions()
        {
            var testobj = new Tester() { ID = "tester_retrieve", val = "hello" };

            var resp = await this.db.Put(testobj);
            var att_resp = await this.db.Attachment(resp.ID, resp.REV).Put("test_attachment", "text/plain", attachment_data);

            var testobj_w_attach = await this.db.Options(attachments: true).Get<Tester>(att_resp.ID);
            Assert.IsFalse(testobj_w_attach.Attachments["test_attachment"].Stub, "stub should be false when querying with attachments");
            Assert.AreEqual(testobj_w_attach.Attachments["test_attachment"].ContentType, "text/plain", "Should have same content type as upload");
            Assert.IsNotNull(testobj_w_attach.Attachments["test_attachment"].Data, "Data should have been returned");
            Assert.AreEqual(testobj_w_attach.Attachments["test_attachment"].GetDataString(), attachment_data, "Data should be the same as what was uploaded");
        }
    }
}
