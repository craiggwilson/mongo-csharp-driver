using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.Bson.Mapping.Configuration
{
    internal class WeightedValue<T>
    {
        private readonly SortedDictionary<int, List<T>> _values;

        public WeightedValue()
        {
            _values = new SortedDictionary<int, List<T>>();
        }

        public WeightedValue(T value, int weigth)
            : this()
        {
            _values[weigth] = new List<T> { value };
        }

        public T Get()
        {
            if (_values.Count == 0)
            {
                return default(T);
            }

            return _values.First().Value.Last();
        }

        public void Set(T value, int weight)
        {
            _values[weight].Add(value);
        }
    }
}
