using Bunk.CouchBuiltins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bunk
{
    public class UsersMaintenance : DB
    {
        public UsersMaintenance(CouchRepo couchRepo) : base(couchRepo, "_users")
        {}
        
        public Task<User> GetUser(string name){
            var u = new User() { Name = name };

            return this.Get<User>(u.ID);
        }

        public Task<OKDocument> AddUser(User user)
        {
            return this.Put(user);
        }
    }
}
