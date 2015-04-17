using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bunk.Design
{
    /// <summary>
    /// Use string/int etc if the key is simple.  Use EmitKey if it is composite
    /// </summary>
    [JsonArray]
    public class EmitKey : JArray
    {
        public EmitKey() : base() { }
        private EmitKey(IEnumerable objs) : base(objs)
        {}

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static EmitKey Array(params object[] p)
        {
            return new EmitKey(p);
        }

        /// <summary>
        /// Return a list appending {}
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static EmitKey MaxArray(params object[] p)
        {
            var ek = new EmitKey(p);
            ek.Add(new JObject());
            return ek;
        }
    }
}
