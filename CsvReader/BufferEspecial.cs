using System;
using System.IO;
using System.Runtime.CompilerServices;

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
        int _shiftedDownCount;
        long _copiedDownChars;

        public BufferEspecial(TextReader rdr, int buffersize = 4096)
        {
            _reader = rdr;
            BUFSIZE = buffersize;
            _buf = new char[BUFSIZE];
            _bufLen = _reader.ReadBlock(_buf);
            _startIdx = 0;
            _readIdx = 0;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ReadNextIdx(ref char c)
        {
            int idxToRead = _startIdx + _readIdx;
            ++_readIdx;

            if (idxToRead < _bufLen)
            {
                c = _buf[idxToRead];
                return true;
            }

            if ( HandleReadBehindBuffer() )
            {
                idxToRead = _startIdx + _readIdx - 1;
                c = _buf[idxToRead];
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
        public int LastReadIdx()
        {
            return _readIdx - 1;
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
                ++_shiftedDownCount;
                _copiedDownChars += charsToMoveDown;
            }
            //
            // fill the rest of the array
            //
            int charsToRead     = BUFSIZE - charsToMoveDown;
            int numberCharsRead = _reader.ReadBlock(_buf, charsToMoveDown, charsToRead);

            _bufLen = charsToMoveDown + numberCharsRead;

            return numberCharsRead;
        }
        public void PrintDebugInfo()
        {
            Console.WriteLine(
                  $"BUFSIZE\t\t{BUFSIZE}"
                + $"\nbuflen\t\t{_bufLen}"
                + $"\nstartIdx\t{_startIdx}"
                + $"\nreadIdx\t\t{_readIdx}"
                + $"\nshiftedDown\t{_shiftedDownCount}"
                + $"\ncopiedDownChars\t{_copiedDownChars}"
                + $"\nbuf\t\t[{_buf.AsSpan(_startIdx, _readIdx).ToString()}]");
        }

    }
}
