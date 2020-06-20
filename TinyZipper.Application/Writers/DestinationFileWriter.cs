using System;
using System.Collections.Concurrent;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using TinyZipper.Application.Compressing;
using TinyZipper.Application.Core;
using TinyZipper.Application.Core.Interfaces;
using TinyZipper.Application.Settings;

namespace TinyZipper.Application.Writers
{
    public class DestinationFileWriter : IDestinationWriter
    {
        private readonly IDataFormatService _dataFormatService;
        private readonly IStatusUpdateService _statusUpdateService;
        private readonly IOutputOverflowControlSettings _outputOverflowControlSettings;

        public DestinationFileWriter(
            IDataFormatService dataFormatService,
            IStatusUpdateService statusUpdateService,
            IOutputOverflowControlSettings outputOverflowControlSettings)
        {
            _dataFormatService = dataFormatService;
            _statusUpdateService = statusUpdateService;
            _outputOverflowControlSettings = outputOverflowControlSettings;
        }

        public AsyncWriteContext Write(string destinationFileName, CompressionMode compressionMode,
            ParallelCompressionContext compressionContext, CancellationToken calculationsFinishedToken)
        {
            var exceptionSource = new CancellationTokenSource();
            var tokens = new[] { calculationsFinishedToken, compressionContext.ExceptionSource.Token };

            var thread = new Thread(() =>
            {
                WriteFile(destinationFileName, compressionMode, compressionContext.ResultsDictionary, tokens, exceptionSource, compressionContext.OutputOverflowEvent);
            });

            thread.Start();

            return new AsyncWriteContext(thread, exceptionSource);
        }

        private void WriteFile(string fileName, CompressionMode compressionMode, ConcurrentDictionary<int, byte[]> resultPieces, 
            CancellationToken[] cancellationTokens, CancellationTokenSource exceptionSrc, ManualResetEventSlim outputOverflowEvent)
        {
            try
            {
                var index = 1;
                using (var outputFileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Write))
                {
                    while (true)
                    {
                        if (cancellationTokens.Any(ct => ct.IsCancellationRequested) && resultPieces.Count == 0)
                            return;

                        if (resultPieces.TryRemove(index, out var dataBytes))
                        {
                            WriteMeta(compressionMode, dataBytes, outputFileStream);

                            outputFileStream.Write(dataBytes);
                            index++;
                            continue;
                        }

                        ControlOutputOverflow(resultPieces, outputOverflowEvent);

                        // Actually I don't know why, but with this small sleep interval performance much better
                        // (for enwik9 (10^9 bytes) without this line ~8000ms, with it ~7150ms)
                        // (for enwik8 (10^8 bytes) without this line ~920ms, with it ~850ms)
                        // need additional investigation here
                        Thread.Sleep(1);
                    }
                }
            }
            catch (Exception e)
            {
                _statusUpdateService.Error(e.Message);
                exceptionSrc.Cancel();
            }
        }

        private void ControlOutputOverflow(ConcurrentDictionary<int, byte[]> resultPieces, ManualResetEventSlim overflowControlEvent)
        {
            if (resultPieces.Count >= _outputOverflowControlSettings.Ceiling)
                overflowControlEvent.Reset();
            else if (resultPieces.Count <= _outputOverflowControlSettings.Floor && !overflowControlEvent.IsSet)
                overflowControlEvent.Set();
        }

        private void WriteMeta(CompressionMode compressionMode, byte[] dataBytes, Stream outputFileStream)
        {
            if (compressionMode != CompressionMode.Compress) return;

            var chunkMeta = _dataFormatService.GetChunkMeta(dataBytes);
            outputFileStream.Write(chunkMeta);
        }
    }
}