namespace TinyZipper.Application.Core.ClientOptions
{
    public interface IClientOptionsService
    {
        ClientOptionsModel Map(string[] args);
    }
}