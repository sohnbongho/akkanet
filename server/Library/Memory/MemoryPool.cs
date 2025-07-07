using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable CS8600 // null 리터럴 또는 가능한 null 값을 null을 허용하지 않는 형식으로 변환하는 중입니다.

namespace Library.Memory
{
    public class PacketMemoryPool<T> : IDisposable where T : new()
    {    
        private readonly ConcurrentBag<T> _pool;
        private bool _disposed = false;

        public PacketMemoryPool()
        {
            _pool = new ConcurrentBag<T>();
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                // 풀에 있는 모든 객체를 해제합니다.

                while (_pool.TryTake(out T buffer))
                {
                    if (buffer is IDisposable disposableBuffer)
                    {
                        disposableBuffer.Dispose();
                        disposableBuffer = null;
                    }
                }

                _disposed = true;
            }
        }

        public T Rent()
        {
            if (_pool.TryTake(out T buffer))
            {
                return buffer;
            }

            // 풀에 사용 가능한 버퍼가 없으면 새로운 버퍼를 생성합니다.
            return new T();
        }

        public void Return(T buffer)
        {            
            _pool.Add(buffer);
        }
    }
}
#pragma warning restore CS8600 // null 리터럴 또는 가능한 null 값을 null을 허용하지 않는 형식으로 변환하는 중입니다.