using System.IO;

namespace TinyZipper.Application.Core.Interfaces
{
    public interface IDestinationStreamService
    {
        Stream OpenWrite(string destinationUri);
    }
}