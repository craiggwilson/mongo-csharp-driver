using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson.Mapping.Configuration.Conventions;
using MongoDB.Bson.Serialization;

namespace MongoDB.Bson.Mapping.Configuration
{
    public class MappingConfiguration : IBsonSerializationProvider
    {
        private readonly ConcurrentDictionary<Type, ClassModel> _classModels;

        private readonly List<IConvention> _conventions;

        public MappingConfiguration()
        {
            _classModels = new ConcurrentDictionary<Type, ClassModel>();

            _conventions = new List<IConvention>
            {
                new PublicReadWritePropertyClassModelConvention()
            };
        }

        public ICollection<IConvention> Conventions
        {
            get { return _conventions; }
        }

        public MappingConfiguration Map<TClass>()
        {
            var model = new ClassModel(typeof(TClass));
            model.SetRunConventions(true, Weights.Default);
            _classModels.TryAdd(typeof(TClass), model);

            return this;
        }

        public MappingConfiguration Map<TClass>(Action<ClassModel<TClass>> mapper)
        {
            var model = _classModels.GetOrAdd(typeof(TClass), t => new ClassModel(t));
            var typedModel = new ClassModel<TClass>(model);

            mapper(typedModel);
            return this;
        }

        internal void ScanAssemblyOf<T>()
        {
            throw new NotImplementedException();
        }

        public IBsonSerializer GetSerializer(Type type)
        {
            // TODO: probably need a little locking here...
            ClassModel model;
            if (!_classModels.TryGetValue(type, out model))
            {
                // see if the type is annotated indicating is should be mapped...
                //if (type.HasAttribute(MapMe))
                //{
                //    model = new ClassModel(type);
                //}
            }

            if (model.RunConventions)
            {
                foreach (var convention in _conventions.OrderBy(x => x.Stage))
                {
                    convention.Apply(model);
                }
            }

            // return new ClassModelSerializer(model);
            throw new NotImplementedException();
        }
    }
}