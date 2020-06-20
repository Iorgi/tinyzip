using System;
using System.Collections.Concurrent;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using TinyZipper.Application.Core;
using TinyZipper.Application.Core.Interfaces;
using TinyZipper.Application.Settings;

namespace TinyZipper.Application.Readers
{
    public class FileToQueueReader : ISourceReader
    {
        private readonly IDataFormatService _dataFormatService;
        private readonly IStatusUpdateService _statusUpdateService;
        private readonly ICompressionSettings _compressionSettings;

        public FileToQueueReader(
            IDataFormatService dataFormatService, 
            IStatusUpdateService statusUpdateService,
            ICompressionSettings compressionSettings)
        {
            _dataFormatService = dataFormatService;
            _statusUpdateService = statusUpdateService;
            _compressionSettings = compressionSettings;
        }

        public AsyncReadContext<QueueItem> Read(string sourceUrl, CompressionMode compressionMode)
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
                    using (var fileStream = new FileStream(sourceUrl, FileMode.Open, FileAccess.Read, FileShare.Read, _compressionSettings.ChunkSize, FileOptions.SequentialScan))
                    {
                        while (true)
                        {
                            inputOverflowEvent.Wait();

                            var chunkLength = GetChunkLength(fileStream, compressionMode);
                            if (chunkLength == 0)
                            {
                                sourceStreamClosedSrc.Cancel();
                                return;
                            }

                            var chunk = GetNextChunk(fileStream, chunkLength);
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
        
        private int GetChunkLength(Stream stream, CompressionMode compressionMode)
        {
            if (compressionMode == CompressionMode.Compress)
                return _compressionSettings.ChunkSize;

            var chunkMetaBytesBuffer = new byte[4];
            var bytesRead = stream.Read(chunkMetaBytesBuffer, 0, chunkMetaBytesBuffer.Length);

            if (bytesRead == 0)
                return 0;

            return _dataFormatService.GetChunkLength(chunkMetaBytesBuffer);
        }

        private byte[] GetNextChunk(Stream stream, int chunkLength)
        {
            var buffer = new byte[chunkLength];
            var bytesRead = stream.Read(buffer, 0, buffer.Length);

            if (bytesRead == 0)
                return new byte[0];

            if (bytesRead < buffer.Length)
                buffer = buffer.Take(bytesRead).ToArray();

            return buffer;
        }
    }
}