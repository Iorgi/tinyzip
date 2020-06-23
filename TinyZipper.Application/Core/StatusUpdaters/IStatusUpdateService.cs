namespace TinyZipper.Application.Core.StatusUpdaters
{
    public interface IStatusUpdateService
    {
        void Info(string message);

        void Error(string message);
    }
}