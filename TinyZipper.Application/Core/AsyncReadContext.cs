using System.Collections.Concurrent;
using System.Threading;

namespace TinyZipper.Application.Core
{
    public class AsyncReadContext<T>
    {
        public IProducerConsumerCollection<T> Queue { get; }

        public Thread ProcessingThread { get; }

        public CancellationTokenSource ReadFinishedSource { get; }

        public CancellationTokenSource ExceptionSource { get; }

        public ManualResetEventSlim EmptyInputEvent { get; }

        public ManualResetEventSlim InputOverflowEvent { get; }

        public AsyncReadContext(IProducerConsumerCollection<T> queue, Thread thread,
            CancellationTokenSource readFinishedSource, CancellationTokenSource exceptionSource,
            ManualResetEventSlim emptyInputEvent, ManualResetEventSlim inputOverflowEvent)
        {
            Queue = queue;
            ProcessingThread = thread;
            ReadFinishedSource = readFinishedSource;
            ExceptionSource = exceptionSource;
            EmptyInputEvent = emptyInputEvent;
            InputOverflowEvent = inputOverflowEvent;
        }
    }
}