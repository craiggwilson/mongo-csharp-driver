using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.Bson.Mapping.Configuration
{
    internal class FieldModelCollection : IEnumerable<FieldModel>
    {
        private readonly Dictionary<string, WeightedEntry> _values;

        public FieldModelCollection()
        {
            _values = new Dictionary<string, WeightedEntry>();
        }

        public FieldModel GetOrCreate(string name, Type type, int weight)
        {
            WeightedEntry entry;
            if (!_values.TryGetValue(name, out entry))
            {
                _values[name] = entry = new WeightedEntry();
                entry.Weights[weight] = new List<WeightedEntryType>
                {
                    WeightedEntryType.Added
                };
            }
            else
            {
                entry.Weights[weight].Add(WeightedEntryType.Added);
            }

            if (entry.FieldModel == null)
            {
                entry.FieldModel = new FieldModel(name, type);
            }

            return entry.FieldModel;
        }

        public void Remove(string name, int weight)
        {
            WeightedEntry entry;
            if (!_values.TryGetValue(name, out entry))
            {
                _values[name] = entry = new WeightedEntry();
                entry.Weights[weight] = new List<WeightedEntryType>
                {
                    WeightedEntryType.Removed
                };
            }
            else
            {
                entry.Weights[weight].Add(WeightedEntryType.Removed);
            }
        }

        private class WeightedEntry
        {
            public FieldModel FieldModel;
            public SortedDictionary<int, List<WeightedEntryType>> Weights;

            public WeightedEntry()
            {
                Weights = new SortedDictionary<int, List<WeightedEntryType>>();
            }
        }

        private enum WeightedEntryType
        {
            Added,
            Removed
        }

        public IEnumerator<FieldModel> GetEnumerator()
        {
            foreach (var entry in _values.Values)
            {
                if (entry.FieldModel == null)
                {
                    continue;
                }

                // we don't care about weights right now because we want to make sure
                // that even deleted fields get conventions applied in the circumstance
                // where a convention later on "adds" this field back at a higher weight.

                yield return entry.FieldModel;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}