using System.Threading;

namespace Generic.Collections
{
    /// <summary>
    /// Byte Buffer
    /// </summary>
    public class Buffer
    {
        private byte[] _buffer = new byte[0];
        private Mutex _mutex = new Mutex();

        /// <summary>
        /// Clears the buffer
        /// </summary>
        public void Clear()
        {
            _mutex.WaitOne();
            try
            {
                _buffer = new byte[0];
            }
            finally
            {
                _mutex.ReleaseMutex();
            }
        }

        /// <summary>
        /// Adds to the buffer (FIFO)
        /// </summary>
        /// <param name="Buffer"></param>
        public void Add(byte Buffer)
        {
            byte[] result = new byte[1];
            result[0] = Buffer;
            Add(result);
        }
        /// <summary>
        /// Adds to the buffer (FIFO)
        /// </summary>
        /// <param name="Buffer"></param>
        public void Add(byte[] Buffer)
        {
            _mutex.WaitOne();
            try
            {
                int _newLength = Buffer.Length + _buffer.Length;
                byte[] _newBuffer = new byte[_newLength];
                int _byteCount = 0;
                for (int i = 0; i < _buffer.Length; i++)
                {
                    _newBuffer[_byteCount] = _buffer[i];
                    _byteCount++;
                }
                for (int i = 0; i < Buffer.Length; i++)
                {
                    _newBuffer[_byteCount] = Buffer[i];
                    _byteCount++;
                }
                _buffer = _newBuffer;
            }
            finally
            {
                _mutex.ReleaseMutex();
            }
        }

        /// <summary>
        /// Retrieves the next byte (FIFO)
        /// </summary>
        /// <returns></returns>
        public byte[] Next()
        {
            return Next(1);
        }

        /// <summary>
        /// Retrieves the next set of bytes (FIFO)
        /// </summary>
        /// <param name="Length">The number of bytes to return</param>
        /// <returns></returns>
        public byte[] Next(int Length)
        {
            _mutex.WaitOne();
            byte[] _returnBuffer = new byte[0];
            try
            {
                if (Length > _buffer.Length)
                    Length = _buffer.Length;
                _returnBuffer = new byte[Length];
                int _newLength = _buffer.Length - Length;
                byte[] _newBuffer = new byte[_newLength];
                int _byteCount = 0;
                for (int i = 0; i < Length; i++)
                {
                    _returnBuffer[_byteCount] = _buffer[i];
                    _byteCount++;
                }
                _byteCount = 0;
                for (int i = Length; i < _buffer.Length; i++)
                {
                    _newBuffer[_byteCount] = _buffer[i];
                    _byteCount++;
                }
                _buffer = _newBuffer;
            }
            catch
            {
                _returnBuffer = new byte[0];
            }
            finally
            {
                _mutex.ReleaseMutex();
            }
            return _returnBuffer;
        }

        /// <summary>
        /// The current length of the buffer
        /// </summary>
        public int Length
        {
            get
            {
                _mutex.WaitOne();
                int _length = 0;
                try
                {
                    _length = _buffer.Length;
                }
                finally
                {
                    _mutex.ReleaseMutex();
                }
                return _length;
            }
        }

    }
}
