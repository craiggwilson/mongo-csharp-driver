using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using MongoDB.Driver.Linq.Translators;
using Xunit;

namespace TestCSharp1777
{
    public class Test
    {
        public enum E { A, B }

        public class C
        {
            public int Id;
            [BsonRepresentation(BsonType.String)]
            public E? P;
        }

        [Fact]
        public void Weirdness()
        {
            var persistentIds = (IEnumerable<E>)(new E[] { E.A, E.B });
            var pIds = persistentIds.Select(x => x as E?);
            var result = Project(x => pIds.Contains(x.P));
        }

        private RenderedProjectionDefinition<TResult> Project<TResult>(Expression<Func<C, TResult>> projector)
        {
            return Project(projector, null);
        }

        private RenderedProjectionDefinition<TResult> Project<TResult>(Expression<Func<C, TResult>> projector, ExpressionTranslationOptions translationOptions)
        {
            var serializer = BsonSerializer.SerializerRegistry.GetSerializer<C>();
            return AggregateProjectTranslator.Translate(projector, serializer, BsonSerializer.SerializerRegistry, translationOptions);
        }
    }
}
