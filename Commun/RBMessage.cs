using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commun
{
    public class RBMessage<T>
    {
        public int SequenceNumber { get; set; }
        public List<T> Models { get; set; } = new List<T>();
        public List<string> Deleted { get; set; } = new List<string>();

        public byte[] GetData()
        {
            var json = JsonConvert.SerializeObject(this);
            return Encoding.UTF8.GetBytes(json);
        }

        public static RBMessage<T>? GetObject(byte[] source)
        {
            var json = Encoding.UTF8.GetString(source);
            var obj = JsonConvert.DeserializeObject<RBMessage<T>>(json);
            return obj;
        }
    }
}
