using System;
using System.IO.Compression;
using TinyZipper.Application.ClientOptions;
using TinyZipper.Application.Core;
using TinyZipper.Application.Core.Interfaces;

namespace TinyZipper.Application.Compressing
{
    public interface IOutcomeService
    {
        bool Finalize(in DateTime startedTime, ClientOptionsModel clientOptions, AsyncReadContext<QueueItem> readContext, ParallelCompressionContext compressionContext, AsyncWriteContext writeContext);
    }

    public class OutcomeService : IOutcomeService
    {
        private readonly IStatusUpdateService _statusUpdateService;
        private readonly IFileService _fileService;

        public OutcomeService(
            IStatusUpdateService statusUpdateService,
            IFileService fileService)
        {
            _statusUpdateService = statusUpdateService;
            _fileService = fileService;
        }

        public bool Finalize(in DateTime startedTime, ClientOptionsModel clientOptions, AsyncReadContext<QueueItem> readContext,
            ParallelCompressionContext compressionContext, AsyncWriteContext writeContext)
        {
            var isErrorOccured = readContext.ExceptionSource.IsCancellationRequested ||
                                 compressionContext.ExceptionSource.IsCancellationRequested ||
                                 writeContext.ExceptionSource.IsCancellationRequested;

            if (isErrorOccured)
                TidyUp(clientOptions);
            else
                SendCompletionMessage(clientOptions, startedTime);

            return !isErrorOccured;
        }

        private void SendCompletionMessage(ClientOptionsModel clientOptions, DateTime startedTime)
        {
            var elapsed = (DateTime.Now - startedTime).TotalMilliseconds;

            var srcSize = _fileService.GetFileSize(clientOptions.SourceFileName);
            var dstSize = _fileService.GetFileSize(clientOptions.DestinationFileName);

            decimal compressionRatio = 0;

            if (srcSize != 0 && dstSize != 0)
            {
                compressionRatio = clientOptions.CompressionMode == CompressionMode.Compress
                    ? (decimal)dstSize / srcSize * 100
                    : (decimal)srcSize / dstSize * 100;
            }

            var message =
                $"{clientOptions.CompressionMode} completed. Elapsed: {elapsed}ms. Source file size {srcSize} bytes. Destination file size {dstSize} bytes." +
                $" Compression ratio {(int)compressionRatio}%";

            _statusUpdateService.Info(message);
        }

        private void TidyUp(ClientOptionsModel clientOptions)
        {
            _fileService.DeleteIfExistsSafe(clientOptions.DestinationFileName);
        }
    }
}