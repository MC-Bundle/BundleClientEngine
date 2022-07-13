using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bundle.Client.Cache
{
    public interface ICache
    {
        public T Get<T>(string key);
        public void Set<T>(string key, T value);
    }
}
