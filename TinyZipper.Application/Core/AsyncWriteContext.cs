using System.Threading;

namespace TinyZipper.Application.Core
{
    public class AsyncWriteContext
    {
        public Thread ProcessingThread { get; }

        public CancellationTokenSource ExceptionSource { get; }

        public AsyncWriteContext(Thread thread, CancellationTokenSource exceptionSource)
        {
            ProcessingThread = thread;
            ExceptionSource = exceptionSource;
        }
    }
}