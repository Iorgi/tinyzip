using System;
using System.Collections.Concurrent;
using System.IO.Compression;
using System.Threading;
using TinyZipper.Application.Core;
using TinyZipper.Application.Core.Interfaces;
using TinyZipper.Application.Core.StatusUpdaters;
using TinyZipper.Application.Settings;

namespace TinyZipper.Application.Readers
{
    public class StreamToQueueReader : ISourceReader
    {
        private readonly IStreamUtilsService _streamUtilsService;
        private readonly IStatusUpdateService _statusUpdateService;
        private readonly ICompressionSettings _compressionSettings;
        private readonly ISourceStreamService _sourceService;

        public StreamToQueueReader(
            IStreamUtilsService streamUtilsService, 
            IStatusUpdateService statusUpdateService,
            ICompressionSettings compressionSettings,
            ISourceStreamService sourceService)
        {
            _streamUtilsService = streamUtilsService;
            _statusUpdateService = statusUpdateService;
            _compressionSettings = compressionSettings;
            _sourceService = sourceService;
        }

        public AsyncReadContext<QueueItem> Read(string sourceUri, CompressionMode compressionMode)
        {
            IProducerConsumerCollection<QueueItem> queue = new ConcurrentQueue<QueueItem>();
            var sourceStreamClosedSrc = new CancellationTokenSource();
            var exceptionSrc = new CancellationTokenSource();
            var emptyInputEvent = new ManualResetEventSlim(false);
            var inputOverflowEvent = new ManualResetEventSlim(true); 
            
            var readerThread = new Thread(() =>
            {
                try
                {
                    int order = 0;
                    using (var sourceStream = _sourceService.OpenRead(sourceUri))
                    {
                        while (true)
                        {
                            inputOverflowEvent.Wait();

                            var chunkLength = _streamUtilsService.GetChunkLength(sourceStream, compressionMode, _compressionSettings.ChunkSize);
                            if (chunkLength == 0)
                            {
                                sourceStreamClosedSrc.Cancel();
                                return;
                            }

                            var chunk = _streamUtilsService.GetNextChunk(sourceStream, chunkLength);
                            if (chunk.Length == 0)
                            {
                                sourceStreamClosedSrc.Cancel();
                                return;
                            }

                            // implementation of ConcurrentQueue never returns false, so just skip return value
                            queue.TryAdd(new QueueItem(++order, chunk));

                            emptyInputEvent.Set();
                        }
                    }
                }
                catch (Exception e)
                {
                    _statusUpdateService.Error(e.Message);
                    exceptionSrc.Cancel();
                }
            });

            readerThread.Start();

            return new AsyncReadContext<QueueItem>(queue, readerThread, sourceStreamClosedSrc, exceptionSrc, 
                emptyInputEvent, inputOverflowEvent);
        }
    }
}