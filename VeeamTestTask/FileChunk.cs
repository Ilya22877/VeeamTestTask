using System;

namespace VeeamTestTask
{
    public class FileChunk
    {
        public static FileChunk Empty { get; } = new(0, Array.Empty<byte>());

        public FileChunk(long number, byte[] bytes)
        {
            Number = number;
            Bytes = bytes;
        }

        public long Number { get; }

        public byte[] Bytes { get; }
    }
}