using System;
using System.IO;
using System.IO.Compression;
using TinyZipper.Application.Core.ClientOptions;
using TinyZipper.Application.Core.Interfaces;

namespace TinyZipper.Application.Compressing
{
    // Implemented just to compare performance
    public class SimpleCompressionOrchestrationService : ICompressionOrchestrationService
    {
        private readonly IClientOptionsService _clientOptionsService;
        private readonly IOutcomeService _outcomeService;

        public SimpleCompressionOrchestrationService(
            IClientOptionsService clientOptionsService,
            IOutcomeService outcomeService)
        {
            _clientOptionsService = clientOptionsService;
            _outcomeService = outcomeService;
        }

        public bool Do(string[] args)
        {
            var startedTime = DateTime.Now;
            var clientOptions = _clientOptionsService.Map(args);

            try
            {
                if (clientOptions.CompressionMode == CompressionMode.Compress)
                    Compress(clientOptions);
                else
                    Decompress(clientOptions);

                _outcomeService.CompletedSuccessfully(clientOptions, startedTime);
            }
            catch (Exception)
            {
                _outcomeService.Failed(clientOptions);
                throw;
            }

            return true;
        }

        // code from examples on https://docs.microsoft.com/en-us/dotnet/api/system.io.compression.gzipstream?view=netcore-3.1
        // just adopted to one file
        public static void Compress(ClientOptionsModel clientOptions)
        {
            using (FileStream originalFileStream = File.OpenRead(clientOptions.SourceFileName))
            {

                using (FileStream compressedFileStream = File.Create(clientOptions.DestinationFileName))
                {
                    using (GZipStream compressionStream = new GZipStream(compressedFileStream,
                        CompressionMode.Compress))
                    {
                        originalFileStream.CopyTo(compressionStream);
                    }
                }
            }
        }

        public static void Decompress(ClientOptionsModel clientOptions)
        {
            using (FileStream originalFileStream = File.OpenRead(clientOptions.SourceFileName))
            {
                using (FileStream decompressedFileStream = File.Create(clientOptions.DestinationFileName))
                {
                    using (GZipStream decompressionStream = new GZipStream(originalFileStream, CompressionMode.Decompress))
                    {
                        decompressionStream.CopyTo(decompressedFileStream);
                    }
                }
            }
        }
    }
}