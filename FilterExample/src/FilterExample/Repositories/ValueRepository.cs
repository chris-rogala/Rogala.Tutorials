using FilterExample.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FilterExample.Repositories
{
    public class ValueRepository : IRepository<Value>
    {
        private List<Value> _values;

        public ValueRepository()
        {
            _values = new List<Value>() { new Value() { Id = 12345, Content = "Some-thing" } };
        }

        public async Task<Value> CreateAsync(Value model)
        {
            model.Id = _values.Max(x => x.Id) + 1;

            _values.Add(model);

            return model;
        }

        public async Task<Value> FindAsync(params object[] keys)
        {
            int key = Convert.ToInt32(keys?.FirstOrDefault()?.ToString());
            return _values.FirstOrDefault(x => x.Id == key);
        }

        public async Task<IEnumerable<Value>> GetAsync()
        {
            return _values.AsEnumerable();
        }

        public async Task<Value> UpdateAsync(Value model, params object[] key)
        {
            Value result = null;
            var currentValue = await FindAsync(key);

            if (currentValue != null)
            {
                model.Id = currentValue.Id;
                _values.RemoveAt(_values.IndexOf(currentValue));
                _values.Add(model);

                result = model;
            }

            return result;
        }
    }
}
