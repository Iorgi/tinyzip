using System.IO.Compression;

namespace TinyZipper.Application.Core.ClientOptions
{
    public class ClientOptionsModel
    {
        public CompressionMode CompressionMode { get; }

        public string SourceFileName { get; }

        public string DestinationFileName { get; }

        public ClientOptionsModel(CompressionMode compressionMode, string sourceFileName, string destinationFileName)
        {
            CompressionMode = compressionMode;
            SourceFileName = sourceFileName;
            DestinationFileName = destinationFileName;
        }
    }
}