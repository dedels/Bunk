using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bunk
{
    public interface ICouchRepo
    {
        ConnectionConfig config { get; }
        CouchUrl couchUrl { get; }
        IHttpClient HttpClient { get; }

        Task<List<string>> AllDBs();
        Authentication Authentication();
        DB DB(string name);
        T Deserialize<T>(CouchResponse response);
        UsersMaintenance UserMaintenance();
        Task<List<string>> UUIDs(int count = 10);
    }
}