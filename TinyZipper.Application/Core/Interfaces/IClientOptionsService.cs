using TinyZipper.Application.ClientOptions;

namespace TinyZipper.Application.Core.Interfaces
{
    public interface IClientOptionsService
    {
        ClientOptionsModel Map(string[] args);
    }
}