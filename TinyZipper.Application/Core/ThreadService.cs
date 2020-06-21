using System.Collections.Generic;
using System.Threading;

namespace TinyZipper.Application.Core
{
    public interface IThreadService
    {
        void WaitThreadsCompletion(IEnumerable<Thread> threads);
    }

    public class ThreadService : IThreadService
    {
        public void WaitThreadsCompletion(IEnumerable<Thread> threads)
        {
            foreach (var thread in threads)
                thread.Join();
        }
    }
}