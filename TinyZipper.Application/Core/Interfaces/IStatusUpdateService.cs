namespace TinyZipper.Application.Core.Interfaces
{
    public interface IStatusUpdateService
    {
        void Info(string message);

        void Error(string message);
    }
}