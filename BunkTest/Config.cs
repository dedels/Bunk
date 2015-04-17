using Bunk;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BunkTest
{
    class Config
    {
        private static ConnectionConfig cfg;
     
        public static ConnectionConfig Get()
        {
            if (cfg == null)
            {
                var config = new Dictionary<string, string>();

                using(var ini_fs = System.IO.File.OpenText("../../TestSettings.ini")){
                    while (!ini_fs.EndOfStream)
                    {
                        var line = ini_fs.ReadLine();
                        var parts = line.Split(new char[] { '=' }, 2);
                        if (parts.Length < 2) continue;
                        config[parts[0].Trim()] = parts[1].Trim(); ;
                    }
                }

                cfg = new ConnectionConfig(config["url"], config["username"], config["password"]);
            }
            return cfg;
        }

    }

    class Rand
    {
        private static Random rng = new Random();
        private static readonly string ALPHA = "abcdefghijklmnopqrstuvwxyz0123456789";
        public static string RandString(string base_string=null)
        {
            var sb=new StringBuilder();
            if (!string.IsNullOrEmpty(base_string))
            {
                sb.Append(base_string);
                sb.Append("-");
            }

            var i = rng.Next();
            do
            {
                sb.Append(ALPHA[i % ALPHA.Length]);
                i /= ALPHA.Length;
            } while (i > 0);
            return sb.ToString();
        }
    }

    [TestClass]
    class TestRand
    {
        [TestMethod]
        public void TestRandString()
        {
            var s1 = Rand.RandString();
            var s2 = Rand.RandString();
            Assert.AreNotEqual(s1, s2, "Two random strings should not be equal");
        }
    }
}
