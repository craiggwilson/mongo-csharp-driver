using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.Query.Language.Parsing
{
    public interface IInputStream<T>
    {
        bool IsMarked { get; }

        int Position { get; }

        T[] ClearMark();

        T Consume();

        T Consume(int count);

        T LA(int count);

        void Mark();
    }
}
