using System.IO.Compression;

namespace TinyZipper.Application.Core.Interfaces
{
    public interface ISourceReader
    {
        AsyncReadContext<QueueItem> Read(string sourceUri, CompressionMode compressionMode);
    }
}