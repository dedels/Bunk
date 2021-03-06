﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Bunk
{
    internal static class Exceptions
    {
        public static void RethrowBunkException<T>(this T ex) where T:WebException
        {
            var resp = ex.Response as HttpWebResponse;
            if (resp == null)
                throw ex;

            var msg = String.Empty;
            using (var resp_stream = new System.IO.StreamReader(resp.GetResponseStream()))
            {
                msg = resp_stream.ReadToEnd();
                if (String.IsNullOrEmpty(msg)) 
                    throw ex;
                var resp_j = JsonConvert.DeserializeObject<Bunk.CouchBuiltins.OK>(msg);

                msg = resp_j.reason ?? resp_j.error ?? ex.Message;
            }

            if (resp.StatusCode == HttpStatusCode.NotFound)
                throw new NotFoundException(msg, ex);
            else if (resp.StatusCode == HttpStatusCode.Unauthorized)
                throw new UnauthorizedException(msg, ex);
            else if (resp.StatusCode == HttpStatusCode.InternalServerError)
                throw new InternalServerException(msg, ex);
            else if (resp.StatusCode == HttpStatusCode.BadRequest)
                throw new BadRequestException(msg, ex);
            else if (resp.StatusCode == HttpStatusCode.PreconditionFailed)
                throw new PreconditionFailedException(msg, ex);
            else if (resp.StatusCode == HttpStatusCode.Forbidden)
                throw new ForbiddenException(msg, ex);
            else if (resp.StatusCode == HttpStatusCode.Conflict)
                throw new ConflictException(msg, ex);
            else
                throw ex;
        }
    }

    public class BunkException: Exception{
        internal BunkException(string message) :base(message)
        {}
        internal BunkException(string message, Exception ex) :base(message, ex)
        {}

    }

    public class ConflictException : BunkException
    {
        internal ConflictException(string msg, System.Net.WebException ex)
            : base(msg, ex)
        { }
    }
    public class NotFoundException : BunkException
    {
        internal NotFoundException(string msg, System.Net.WebException ex)
            : base(msg, ex)
        {    }
    }

    public class UnauthorizedException : BunkException
    {
        internal UnauthorizedException(string msg, System.Net.WebException ex) 
            : base(msg, ex)
        {    }
    
    }

    public class InternalServerException : BunkException
    {
        internal InternalServerException(string msg, System.Net.WebException ex)
            : base(msg, ex)
        {    }
    
    }

    public class BadRequestException : BunkException
    {
        internal BadRequestException(string msg, System.Net.WebException ex)
            : base(msg, ex)
        { }
    }

    public class AttachmentException : BunkException
    {
        internal AttachmentException(String msg) : base(msg) { }
    }

    public class PreconditionFailedException : BunkException
    {
        public PreconditionFailedException(string msg, Exception ex) : base(msg, ex) { }
    }

    public class ForbiddenException : BunkException
    {
        public ForbiddenException(string msg, Exception ex) : base(msg, ex) { }
    }
}
