using System;
using System.IO.Compression;
using TinyZipper.Application.Core;
using TinyZipper.Application.Core.ClientOptions;
using TinyZipper.Application.Core.StatusUpdaters;

namespace TinyZipper.Application.Compressing
{
    public interface IOutcomeService
    {
        void CompletedSuccessfully(ClientOptionsModel clientOptions, DateTime startedTime);

        void Failed(ClientOptionsModel clientOptions);
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

        public void CompletedSuccessfully(ClientOptionsModel clientOptions, DateTime startedTime)
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

        public void Failed(ClientOptionsModel clientOptions)
        {
            _fileService.DeleteIfExistsSafe(clientOptions.DestinationFileName);
        }
    }
}