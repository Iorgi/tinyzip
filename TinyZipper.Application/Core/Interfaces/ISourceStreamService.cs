using System.IO;

namespace TinyZipper.Application.Core.Interfaces
{
    public interface ISourceStreamService
    {
        Stream OpenRead(string sourceUri);
    }
}