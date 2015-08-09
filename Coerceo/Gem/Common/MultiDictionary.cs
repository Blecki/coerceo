using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gem
{
    /// <summary>
    /// Associates a Key with multiple Values
    /// </summary>
    /// <typeparam name="KEY">The type of the Key</typeparam>
    /// <typeparam name="VALUE">The type of the Values</typeparam>
    public class MultiDictionary<KEY, VALUE> : IEnumerable<VALUE>
    {
        private Dictionary<KEY, List<VALUE>> _data = new Dictionary<KEY, List<VALUE>>();
        
        private List<VALUE> getComponentList(KEY t)
        {
            if (!_data.ContainsKey(t)) _data.Add(t, new List<VALUE>());
            return _data[t];
        }

        public bool ContainsKey(KEY t) { return _data.ContainsKey(t); }

        public List<VALUE> this[KEY t] { get { return _data[t]; } }

        public void Add(KEY t, VALUE c)
        {
            getComponentList(t).Add(c);
        }

        public void Remove(KEY t)
        {
            if (_data.ContainsKey(t)) _data.Remove(t);
        }

        public IEnumerator<VALUE> GetEnumerator()
        {
            foreach (var entity in _data)
                foreach (var item in entity.Value)
                    yield return item;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            foreach (var entity in _data)
                foreach (var item in entity.Value)
                    yield return item;
        }
    }
}
