using SharedMemory;

namespace ReadySolution
{
    internal class SharedMemoryManager(string name, int size) : IDisposable
    {
        private readonly SharedArray<byte> _sharedArray = new(name, size);
        private readonly int _size = size;

        public async Task WriteAsync(byte[] data)
        {
            if (data.Length > _size)
                throw new ArgumentException("Размер данных превышает размер выделенной памяти");

            await Task.Run(() =>
            {
                for (int i = 0; i < data.Length; i++)
                    _sharedArray[i] = data[i];
            });
        }

        public async Task<byte[]> ReadAsync(int sizeForRead)
        {
            if (sizeForRead > _size)
                throw new ArgumentException("Размер для чтения больше размера для выделенной памяти");

            return await Task.Run(() =>
            {
                byte[] data = new byte[sizeForRead];
                for (int i = 0; i < sizeForRead; i++)
                    data[i] = _sharedArray[i];

                return data;
            });
        }


        public void Dispose()
        {
            _sharedArray.Dispose();
        }
    }
}
