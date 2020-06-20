using System.IO;
using System.IO.Compression;
using TinyZipper.Application.Core.Interfaces;

namespace TinyZipper.Application.Compressing
{
    public class GzipCompressionService : ICompressionService
    {
        public byte[] Compress(byte[] source)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var gzip = new GZipStream(memoryStream, CompressionMode.Compress))
                    gzip.Write(source, 0, source.Length);
                return memoryStream.ToArray();
            }
        }

        public byte[] Decompress(byte[] source)
        {
            using (var toDecompressStream = new MemoryStream(source))
            {
                using (var decompressedStream = new MemoryStream())
                {
                    using (var gzipStream = new GZipStream(toDecompressStream, CompressionMode.Decompress))
                        gzipStream.CopyTo(decompressedStream);
                    return decompressedStream.ToArray();
                }
            }
        }
    }
}