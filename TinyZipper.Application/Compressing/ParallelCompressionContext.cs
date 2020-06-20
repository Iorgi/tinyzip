using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace TinyZipper.Application.Compressing
{
    public class ParallelCompressionContext
    {
        public IEnumerable<Thread> Threads { get; }
        public ConcurrentDictionary<int, byte[]> ResultsDictionary { get; }
        public CancellationTokenSource ExceptionSource { get; }
        public ManualResetEventSlim OutputOverflowEvent { get; }

        public ParallelCompressionContext(IEnumerable<Thread> threads,
            ConcurrentDictionary<int, byte[]> resultsDictionary, CancellationTokenSource exceptionSrc,
            ManualResetEventSlim outputOverflowEvent)
        {
            Threads = threads;
            ResultsDictionary = resultsDictionary;
            ExceptionSource = exceptionSrc;
            OutputOverflowEvent = outputOverflowEvent;
        }
    }
}