using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.Query.Structure.Parsing
{
    public class BufferedCharInputStream : AbstractBufferedInputStream<char>
    {
        private readonly TextReader _input;

        public BufferedCharInputStream(TextReader input, int blockSize)
            : base(blockSize)
        {
            if(input == null)
                throw new ArgumentNullException("input");

            _input = input;
        }

        public BufferedCharInputStream(string input, int blockSize)
            : this(new StringReader(input), blockSize)
        { }

        protected override char[] ReadInput(int count)
        {
            var ret = new char[count];
            if (_input.ReadBlock(ret, 0, count) == 0)
                throw new EndOfStreamException();

            return ret;
        }
    }
}
