using System.IO;
using TinyZipper.Application.Core.Interfaces;

namespace TinyZipper.Application.Writers
{
    public class FileDestinationService : IDestinationStreamService
    {
        public Stream OpenWrite(string destinationUri)
        {
            return new FileStream(destinationUri, FileMode.Create, FileAccess.Write, FileShare.Write);
        }
    }
}