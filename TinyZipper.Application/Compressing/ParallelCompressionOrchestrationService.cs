using System;
using System.Collections.Generic;
using System.Threading;
using TinyZipper.Application.Core;
using TinyZipper.Application.Core.Interfaces;

namespace TinyZipper.Application.Compressing
{
    public class ParallelCompressionOrchestrationService : ICompressionOrchestrationService
    {
        private readonly IClientOptionsService _clientOptionsService;
        private readonly IParallelCompressionService _parallelCompressionService;
        private readonly ISourceReader _sourceReader;
        private readonly IDestinationWriter _destinationWriter;
        private readonly IOutcomeService _outcomeService;
        private readonly IThreadService _threadService;

        public ParallelCompressionOrchestrationService(
            IClientOptionsService clientOptionsService,
            IParallelCompressionService parallelCompressionService,
            ISourceReader sourceReader,
            IDestinationWriter destinationWriter,
            IOutcomeService outcomeService,
            IThreadService threadService)
        {
            _clientOptionsService = clientOptionsService;
            _parallelCompressionService = parallelCompressionService;
            _sourceReader = sourceReader;
            _destinationWriter = destinationWriter;
            _outcomeService = outcomeService;
            _threadService = threadService;
        }

        public bool Do(string[] args)
        {
            var startedTime = DateTime.Now;

            var clientOptions = _clientOptionsService.Map(args);

            var readContext = _sourceReader.Read(clientOptions.SourceFileName, clientOptions.CompressionMode);

            var parallelCompressionContext = _parallelCompressionService.Run(clientOptions.CompressionMode,
                Environment.ProcessorCount, readContext);

            var calculationWorkFinishedSrc = new CancellationTokenSource();

            var writeContext = _destinationWriter.Write(clientOptions.DestinationFileName, clientOptions.CompressionMode, 
                parallelCompressionContext, calculationWorkFinishedSrc.Token);

            _threadService.WaitThreadsCompletion(parallelCompressionContext.Threads);

            calculationWorkFinishedSrc.Cancel();

            writeContext.ProcessingThread.Join();

            return _outcomeService.Finalize(startedTime, clientOptions, readContext, parallelCompressionContext, writeContext);
        }
    }
}