using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gem
{
    public class PropertyBag
    {
        private Dictionary<String, Object> Properties = new Dictionary<string,object>();
        
        public bool HasProperty(String Name) { return Properties.ContainsKey(Name); }
        
        public void Upsert(String Name, Object Value) { Properties.Upsert(Name, Value); }
        
        public Object GetProperty(String Name)
        {
            if (HasProperty(Name)) return Properties[Name]; 
            else throw new InvalidOperationException();
        }

        public Object GetPropertyOrDefault(String Name)
        {
            if (HasProperty(Name)) return Properties[Name];
            else return null;
        }

        public bool TryGetProperty(String Name, out Object Value)
        {
            if (HasProperty(Name))
            {
                Value = Properties[Name];
                return true;
            }
            else
            {
                Value = null;
                return false;
            }
        }

        public bool TryGetPropertyAs<T>(String Name, out T Value)
        {
            if (HasProperty(Name) && Properties[Name] is T)
            {
                Value = (T)Properties[Name];
                return true;
            }
            else
            {
                Value = default(T);
                return false;
            }
        }

        public T GetPropertyAs<T>(String Name)
        {
            if (HasProperty(Name) && Properties[Name] is T) return (T)Properties[Name];
            else throw new InvalidOperationException();
        }

        public T GetPropertyAs<T>(String Name, Func<T> Default)
        {
            if (HasProperty(Name) && Properties[Name] is T) return (T)Properties[Name];
            else
            {
                Upsert(Name, Default());
                return (T)Properties[Name];
            }
        }

        public T GetPropertyAsOrDefault<T>(String Name)
        {
            if (HasProperty(Name) && Properties[Name] is T) return (T)Properties[Name];
            else return default(T);
        }

        public PropertyBag Clone()
        {
            var r = new PropertyBag();
            r.Properties = new Dictionary<string, object>(Properties);
            return r;
        }

        public static PropertyBag Create(params Object[] Properties)
        {
            if (Properties.Length % 2 != 0) throw new InvalidOperationException("Property bags must be created from key/value pairs.");
            var r = new PropertyBag();
            for (int i = 0; i < Properties.Length; i += 2)
                r.Upsert(Properties[i].ToString(), Properties[i + 1]);
            return r;
        }
    }
}
