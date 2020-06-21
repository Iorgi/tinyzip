using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using FluentAssertions;
using Moq;
using TinyZipper.Application.Compressing;
using TinyZipper.Application.Core;
using TinyZipper.Application.Core.Interfaces;
using TinyZipper.Application.Settings;
using Xunit;

namespace TinyZipper.UnitTests
{
    public class ParallelCompressionServiceTests
    {
        [Theory]
        [InlineData(CompressionMode.Compress)]
        [InlineData(CompressionMode.Decompress)]
        public void Run_ReturnsRightContext(CompressionMode compressionMode)
        {
            // arrange
            var threadsCount = 3;
            var compressionServiceMock = MockCompressionService(false);
            var statusUpdateServiceMock = new Mock<IStatusUpdateService>();
            var service = new ParallelCompressionService(compressionServiceMock.Object, statusUpdateServiceMock.Object, new DefaultSettings());
            var inputData = GenerateInputData(10);
            var readContext = GenerateReadContext(inputData, true, false);
            var threadService = new ThreadService();

            // act 
            var result = service.Run(compressionMode, threadsCount, readContext);
            threadService.WaitThreadsCompletion(result.Threads); // wait for completion

            // assert
            result.Threads.Count().Should().Be(threadsCount);
            result.ExceptionSource.IsCancellationRequested.Should().BeFalse();
            result.OutputOverflowEvent.IsSet.Should().BeTrue();
            result.ResultsDictionary.Count.Should().Be(inputData.Count);
            compressionServiceMock.Verify(x => x.Compress(It.IsAny<byte[]>()), Times.Exactly(compressionMode == CompressionMode.Compress ? inputData.Count : 0));
            compressionServiceMock.Verify(x => x.Decompress(It.IsAny<byte[]>()), Times.Exactly(compressionMode == CompressionMode.Decompress ? inputData.Count : 0));
        }

        [Theory]
        [InlineData(CompressionMode.Compress)]
        [InlineData(CompressionMode.Decompress)]
        public void Run_ExceptionInsideThread_ExceptionSourceCancelled(CompressionMode compressionMode)
        {
            // arrange
            var threadsCount = 3;
            var compressionServiceMock = MockCompressionService(true);
            var statusUpdateServiceMock = new Mock<IStatusUpdateService>();
            var service = new ParallelCompressionService(compressionServiceMock.Object, statusUpdateServiceMock.Object, new DefaultSettings());
            var inputData = GenerateInputData(10);
            var readContext = GenerateReadContext(inputData, true, false);
            var threadService = new ThreadService();

            // act 
            var result = service.Run(compressionMode, threadsCount, readContext);
            threadService.WaitThreadsCompletion(result.Threads); // wait for completion

            // assert
            result.ExceptionSource.IsCancellationRequested.Should().BeTrue();
        }

        private AsyncReadContext<QueueItem> GenerateReadContext(IEnumerable<QueueItem> inputData, bool readFinished, bool readException)
        {
            var queue = new ConcurrentQueue<QueueItem>(inputData);

            var readFinishedSrc = new CancellationTokenSource();
            if (readFinished) readFinishedSrc.Cancel();

            var readExceptionSrc = new CancellationTokenSource();
            if (readException) readExceptionSrc.Cancel();

            var emptyInputEvent = new ManualResetEventSlim(true);
            var inputOverflowEvent = new ManualResetEventSlim(true);

            return new AsyncReadContext<QueueItem>(queue, null,
                readFinishedSrc, readExceptionSrc, emptyInputEvent, inputOverflowEvent);
        }

        private Mock<ICompressionService> MockCompressionService(bool throwsException)
        {
            var mock = new Mock<ICompressionService>();
            var compressionSetup = mock.Setup(x => x.Compress(It.IsAny<byte[]>()));
            var decompressionSetup = mock.Setup(x => x.Decompress(It.IsAny<byte[]>()));
            if (throwsException)
            {
                compressionSetup.Throws<Exception>();
                decompressionSetup.Throws<Exception>();
                return mock;
            }

            compressionSetup.Returns((byte[] src) => src);
            decompressionSetup.Returns((byte[] src) => src);

            return mock;
        }

        private IReadOnlyCollection<QueueItem> GenerateInputData(int length)
        {
            var random = new Random();
            var collection = new List<QueueItem>(length);
            for (int i = 0; i < length; i++)
            {
                var buffer = new byte[10];
                random.NextBytes(buffer);
                collection.Add(new QueueItem(i + 1, buffer));
            }

            return collection;
        }
    }
}