namespace Library.messages
{
    public class SessionPacket : IDisposable
    {
        public Byte[]? Buffer = null;

        public ushort Length => _length;
        public const int MaxBufferBytes = 32;  // 최대 패킷 수
        public const ushort UShortSize = sizeof(ushort);

        private ushort _length { get; set; } = 0;
        private ushort _bufferLength { get; set; } = 0;

        public void Init()
        {
            if (null == Buffer)
            {
                _bufferLength = MaxBufferBytes;
                _length = _bufferLength;
                Buffer = new Byte[_bufferLength + 1];
            }

            if (_length > 0)
            {
                // 기존에 메모리를 날리자
                Array.Clear(Buffer, 0, _length);
            }
            _length = 0;
        }
        public void SetLength(ushort length)
        {
            if (null == Buffer || length > _bufferLength)
            {
                _bufferLength = length;
                Buffer = new Byte[_bufferLength + 1];
            }
            _length = length;
        }

        public void Dispose()
        {
            Buffer = null;
            _bufferLength = 0;
            _length = 0;
        }
    }
}
