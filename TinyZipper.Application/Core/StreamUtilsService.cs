using System;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace TinyZipper.Application.Core
{
    public interface IStreamUtilsService
    {
        int GetChunkLength(Stream stream, CompressionMode compressionMode, int defaultChunkSize);

        byte[] GetNextChunk(Stream stream, int chunkLength);

        byte[] GetChunkMeta(byte[] chunk);

        int GetChunkLengthFromMeta(byte[] chunkMetaBytes);
    }

    public class StreamUtilsService : IStreamUtilsService
    {
        public int GetChunkLength(Stream stream, CompressionMode compressionMode, int defaultChunkSize)
        {
            if (compressionMode == CompressionMode.Compress)
                return defaultChunkSize;

            var chunkMetaBytesBuffer = new byte[4];
            var bytesRead = stream.Read(chunkMetaBytesBuffer, 0, chunkMetaBytesBuffer.Length);

            if (bytesRead == 0)
                return 0;

            return GetChunkLengthFromMeta(chunkMetaBytesBuffer);
        }

        public byte[] GetNextChunk(Stream stream, int chunkLength)
        {
            var buffer = new byte[chunkLength];
            var bytesRead = stream.Read(buffer, 0, buffer.Length);

            if (bytesRead == 0)
                return new byte[0];

            if (bytesRead < buffer.Length)
                buffer = buffer.Take(bytesRead).ToArray();

            return buffer;
        }

        public byte[] GetChunkMeta(byte[] chunk)
        {
            return BitConverter.GetBytes(chunk.Length);
        }

        public int GetChunkLengthFromMeta(byte[] chunkMetaBytes)
        {
            return BitConverter.ToInt32(chunkMetaBytes, 0);
        }
    }
}