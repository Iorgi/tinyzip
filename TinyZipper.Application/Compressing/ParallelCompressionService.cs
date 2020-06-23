using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using TinyZipper.Application.Core;
using TinyZipper.Application.Core.Interfaces;
using TinyZipper.Application.Core.StatusUpdaters;
using TinyZipper.Application.Settings;

namespace TinyZipper.Application.Compressing
{
    public interface IParallelCompressionService
    {
        ParallelCompressionContext Run(CompressionMode compressionMode, int threadsCount, AsyncReadContext<QueueItem> asyncReadContext);
    }

    public class ParallelCompressionService : IParallelCompressionService
    {
        private readonly ICompressionService _compressionService;
        private readonly IStatusUpdateService _statusUpdateService;
        private readonly IInputOverflowControlSettings _inputOverflowControlSettings;

        public ParallelCompressionService(
            ICompressionService compressionService,
            IStatusUpdateService statusUpdateService, 
            IInputOverflowControlSettings inputOverflowControlSettings)
        {
            _compressionService = compressionService;
            _statusUpdateService = statusUpdateService;
            _inputOverflowControlSettings = inputOverflowControlSettings;
        }

        public ParallelCompressionContext Run(CompressionMode compressionMode, int threadsCount, AsyncReadContext<QueueItem> asyncReadContext)
        {
            var threads = new List<Thread>(threadsCount);
            var resultsDictionary = new ConcurrentDictionary<int, byte[]>();
            var exceptionSrc = new CancellationTokenSource();
            var tokens = new [] { asyncReadContext.ReadFinishedSource.Token, asyncReadContext.ExceptionSource.Token, exceptionSrc.Token };
            var outputOverflowEvent = new ManualResetEventSlim(true);

            for (int i = 0; i < threadsCount; i++)
            {
                var thread = new Thread(() => CompressChunk(compressionMode, asyncReadContext, resultsDictionary, 
                    exceptionSrc, tokens.ToArray(), outputOverflowEvent));
                threads.Add(thread);
                thread.Start();
            }

            return new ParallelCompressionContext(threads, resultsDictionary, exceptionSrc, outputOverflowEvent);
        }

        private void CompressChunk(
            CompressionMode compressionMode, AsyncReadContext<QueueItem> asyncReadContext,
            ConcurrentDictionary<int, byte[]> resultPieces, CancellationTokenSource exceptionSrc, 
            CancellationToken[] cancellationTokens,
            ManualResetEventSlim outputOverflowEvent)
        {
            try
            {
                while (!cancellationTokens.Any(ct => ct.IsCancellationRequested) || asyncReadContext.Queue.Count > 0)
                {
                    outputOverflowEvent.Wait();

                    if (!asyncReadContext.Queue.TryTake(out var queueItem))
                    {
                        asyncReadContext.EmptyInputEvent.Reset();
                        asyncReadContext.EmptyInputEvent.Wait(TimeSpan.FromMilliseconds(10));
                        continue;
                    }

                    byte[] data;
                    switch (compressionMode)
                    {
                        case CompressionMode.Compress:
                            data = _compressionService.Compress(queueItem.Data);
                            break;
                        case CompressionMode.Decompress:
                            data = _compressionService.Decompress(queueItem.Data);
                            break;
                        default:
                            throw new ApplicationException($"Managing of compression mode '{compressionMode}' not implemented'");
                    }
                    resultPieces.AddOrUpdate(queueItem.Order, data, (i1, byteArray) => byteArray);

                    ControlInputOverflow(asyncReadContext.Queue, asyncReadContext.InputOverflowEvent);
                }
            }
            catch (Exception e)
            {
                _statusUpdateService.Error(e.Message);
                exceptionSrc.Cancel();
            }
        }

        private void ControlInputOverflow(IProducerConsumerCollection<QueueItem> queue, ManualResetEventSlim inputOverflowEvent)
        {
            if (queue.Count >= _inputOverflowControlSettings.Ceiling)
                inputOverflowEvent.Reset();
            else if (queue.Count <= _inputOverflowControlSettings.Floor && !inputOverflowEvent.IsSet)
                inputOverflowEvent.Set();
        }
    }
}