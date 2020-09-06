using System;
using System.IO;

namespace Spi
{
    public class BufferEspecial
    {
        readonly TextReader _reader;
        readonly int BUFSIZE;
        readonly char[] _buf;
        int _bufLen;
        int _startIdx;
        int _readIdx;

        public BufferEspecial(TextReader rdr, int buffersize = 4096)
        {
            _reader = rdr;
            BUFSIZE = buffersize;
            _buf = new char[BUFSIZE];
            _bufLen = _reader.ReadBlock(_buf);
            _startIdx = 0;
            _readIdx = 0;
        }
        public bool Read(ref char c)
        {
            int idxToRead = _startIdx + _readIdx;
            if (idxToRead < _bufLen)
            {
                c = _buf[idxToRead];
                ++_readIdx;
                return true;
            }

            if ( HandleReadBehindBuffer() )
            {
                c = _buf[0];
                return true;
            }

            return false;
        }
        public void SetStartIdx()
        {
            _startIdx += _readIdx;
            _readIdx = 0;
        }
        public bool EOF()
        {
            return
                   (_bufLen < BUFSIZE)
                && (_startIdx + _readIdx >= _bufLen);
        }
        public ReadOnlySpan<char> GetSpan(int start, int length)
        {
            return _buf.AsSpan(_startIdx + start, length);
        }
        private bool HandleReadBehindBuffer()
        {
            if ( _bufLen < BUFSIZE )
            {
                return false;
            }

            if (_startIdx == 0)
            {
                throw new Exception($"cannot read more chars to buffer. startIdx: 0, bufLen: {_bufLen}");
            }

            int numberCharsRead = ShiftBufferAndRefill();
            if (numberCharsRead == 0)
            {
                return false;
            }

            _startIdx = 0;
            _readIdx = 1;


            return true;
        }
        private int ShiftBufferAndRefill()
        {
            //
            // move the actual record down the array
            //
            int charsToMoveDown = _bufLen - _startIdx;
            if (charsToMoveDown > 0)
            {
                Array.Copy(
                    sourceArray:        _buf,
                    sourceIndex:        _startIdx,
                    destinationArray:   _buf,
                    destinationIndex:   0,
                    length:             charsToMoveDown);
            }
            //
            // fill the rest of the array
            //
            int charsToRead     = BUFSIZE - charsToMoveDown;
            int numberCharsRead = _reader.ReadBlock(_buf, charsToMoveDown, charsToRead);

            _bufLen = charsToMoveDown + numberCharsRead;

            return numberCharsRead;
        }
    }
}
