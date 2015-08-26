using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.Query.Language.Parsing
{
    public abstract class AbstractBufferedInputStream<T> : IInputStream<T>
    {
        private readonly int _blockSize;
        private readonly List<T> _buffer;
        private readonly Stack<int> _marks;
        private int _bufferPosition;
        private int _position;

        public bool IsMarked
        {
            get { return (_marks.Count > 0); }
        }

        public int Position
        {
            get { return _position; }
        }

        protected AbstractBufferedInputStream(int blockSize)
        {
            if (blockSize <= 0)
                throw new ArgumentOutOfRangeException("blockSize", "blockSize must be greater than 0");

            _buffer = new List<T>();
            _blockSize = blockSize;
            _bufferPosition = 0;
            _position = 0;
            _marks = new Stack<int>();
        }

        public T[] ClearMark()
        {
            T[] ret;
            if (IsMarked)
            {
                int lastMark = _marks.Pop();
                ret = _buffer.GetRange(lastMark, _bufferPosition - lastMark).ToArray();
            }
            else
            {
                ret = new T[0];
            }
            if (!IsMarked)
            {
                _buffer.RemoveRange(0, _bufferPosition);
                _bufferPosition = 0;
            }
            return ret;
        }

        public T Consume()
        {
            if (_bufferPosition >= _buffer.Count)
                ReadToBuffer();

            var ret = _buffer[_bufferPosition];
            _position++;
            if (!IsMarked)
            {
                _buffer.RemoveAt(0);
                return ret;
            }

            _bufferPosition++;
            return ret;
        }

        public T Consume(int count)
        {
            var ret = LA(0);
            while (count-- > 0)
                ret = Consume();

            return ret;
        }

        public T LA(int count)
        {
            while ((_bufferPosition + count) >= _buffer.Count)
                ReadToBuffer();

            return _buffer[_bufferPosition + count];
        }

        public void Mark()
        {
            _marks.Push(_bufferPosition);
        }

        protected abstract T[] ReadInput(int count);

        private void ReadToBuffer()
        {
            _buffer.AddRange(ReadInput(_blockSize));
        }
    }
}
