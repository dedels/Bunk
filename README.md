# Bunk - a CouchDB Client

This is a library for connecting to CouchDB.  It currently supports basic authentication and parts of AuthSession cookie auth.  The design pattern used in this projects intends to have immutable objects.  Each configuration and change should return a new object.  This is so client code can have access to pre-configured calls as well as chain on new configurations (view params, etc).  

All web requests are done using HttpWebRequest Async methods, and JSON decoded using Newtonsoft Json.NET.

For example usage, take a look at BunkTest.  Basic respository and DB operations are supported as well as Views, BulkDocs, DesignDocs, Attachments and Continuous feeds.

TODO:
1.  Get library to work with ASP.NET so AuthSession can come from a user Cookie
2.  Cloudant specific support (indexes, creating users, permissions)
3.  More use of GenericDocument (a wrapped Json.NET JObject).
4.  Continuous feed including docs
5.  DesignDoc attachments
6.  DesignDoc show/list and update/rewrite
7.  Replication helper
8.  More http status code errors handled

