using System;
using System.Collections.Generic;

namespace FilterExample.Core
{
    public class ApplicationContext
    {
        public ApplicationContext()
        {
            _dictionary = new Dictionary<Type, object>();
        }

        private Dictionary<Type, object> _dictionary;

        public string RequestCheckSum { get; set; }

        public string CurrentCheckSum { get; set; }

        public string ResponseCheckSum { get; set; }

        public void AddObject<T>(T value)
        {
            AddObject(typeof(T), value);   
        }

        public void AddObject(Type type, object value)
        {
            if (_dictionary.ContainsKey(type))
            {
                _dictionary.Remove(type);
            }

            _dictionary.Add(type, value);
        }

        public T GetObject<T>()
        {
            T result = default(T);
            object existing;

            if (_dictionary.TryGetValue(typeof(T), out existing))
            {
                if (existing != null)
                {
                    result = (T)existing;
                }                
            }

            return result;
        }
    }
}
