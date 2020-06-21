using System.IO;
using TinyZipper.Application.Core.Interfaces;
using TinyZipper.Application.Settings;

namespace TinyZipper.Application.Readers
{
    public class FileSourceService : ISourceStreamService
    {
        private readonly ICompressionSettings _compressionSettings;

        public FileSourceService(ICompressionSettings compressionSettings)
        {
            _compressionSettings = compressionSettings;
        }

        public Stream OpenRead(string sourceUri)
        {
            return new FileStream(sourceUri, FileMode.Open, FileAccess.Read, FileShare.Read,
                _compressionSettings.ChunkSize, FileOptions.SequentialScan);
        }
    }
}