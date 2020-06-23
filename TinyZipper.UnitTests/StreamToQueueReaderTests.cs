using System;
using System.IO;
using System.IO.Compression;
using FluentAssertions;
using Moq;
using TinyZipper.Application.Core;
using TinyZipper.Application.Core.Interfaces;
using TinyZipper.Application.Core.StatusUpdaters;
using TinyZipper.Application.Readers;
using Xunit;

namespace TinyZipper.UnitTests
{
    public class StreamToQueueReaderTests
    {
        [Theory]
        [InlineData(0, 10, 0)]
        [InlineData(5, 10, 1)]
        [InlineData(20, 5, 4)]
        [InlineData(19, 5, 4)]
        [InlineData(21, 5, 5)]
        public void Read_CompressMode_ChunksAddedToQueue(int srcStreamLength, int chunkSize, int expectedQueueItemsCount)
        {
            // arrange
            var random = new Random();
            var statusUpdateServiceMock = new Mock<IStatusUpdateService>();
            var testSettings = new TestCompressionSettings { ChunkSize = chunkSize };
            var bytes = new byte[srcStreamLength];
            random.NextBytes(bytes);
            var memoryStream = new MemoryStream(bytes);
            var sourceServiceMock = new Mock<ISourceStreamService>();
            sourceServiceMock.Setup(x => x.OpenRead(It.IsAny<string>()))
                .Returns(memoryStream);
            var service = new StreamToQueueReader(new StreamUtilsService(), statusUpdateServiceMock.Object, testSettings, sourceServiceMock.Object);

            // act
            var result = service.Read("", CompressionMode.Compress);
            result.ProcessingThread.Join(); // wait completion
            memoryStream.Close();
            memoryStream.Dispose();

            // assert
            result.Queue.Count.Should().Be(expectedQueueItemsCount);
            result.ReadFinishedSource.IsCancellationRequested.Should().BeTrue();
            result.ExceptionSource.IsCancellationRequested.Should().BeFalse();
        }

        [Fact]
        public void Read_DecompressMode_ChunksAddedToQueue()
        {
            // arrange
            var statusUpdateServiceMock = new Mock<IStatusUpdateService>();
            var testSettings = new TestCompressionSettings { ChunkSize = 10 };
            var bytes = new byte[]
            {
                5, 0, 0, 0,     // size of first block is 5 bytes
                1, 2, 3, 4, 5,  // first data block
                4, 0, 0, 0,     // size of second block is 4 bytes
                6, 7, 8, 9      // second data block
            };
            var memoryStream = new MemoryStream(bytes);
            var sourceServiceMock = new Mock<ISourceStreamService>();
            sourceServiceMock.Setup(x => x.OpenRead(It.IsAny<string>()))
                .Returns(memoryStream);
            var service = new StreamToQueueReader(new StreamUtilsService(), statusUpdateServiceMock.Object, testSettings, sourceServiceMock.Object);

            // act
            var result = service.Read("", CompressionMode.Decompress);
            result.ProcessingThread.Join(); // wait completion
            memoryStream.Close();
            memoryStream.Dispose();

            // assert
            result.Queue.Count.Should().Be(2);
            result.ReadFinishedSource.IsCancellationRequested.Should().BeTrue();
            result.ExceptionSource.IsCancellationRequested.Should().BeFalse();
            result.Queue.TryTake(out QueueItem first);
            result.Queue.TryTake(out QueueItem second);
            first.Order.Should().Be(1);
            first.Data.Should().Equal(new [] {1, 2, 3, 4, 5});
            second.Order.Should().Be(2);
            second.Data.Should().Equal(new[] { 6, 7, 8, 9 });

        }

        [Fact]
        public void Read_ExceptionInsideThread_ExceptionSrcSet()
        {
            // arrange
            var statusUpdateServiceMock = new Mock<IStatusUpdateService>();
            var sourceServiceMock = new Mock<ISourceStreamService>();
            sourceServiceMock.Setup(x => x.OpenRead(It.IsAny<string>()))
                .Throws<Exception>();
            var service = new StreamToQueueReader(null, statusUpdateServiceMock.Object, null, sourceServiceMock.Object);

            // act
            var result = service.Read("", CompressionMode.Decompress);
            result.ProcessingThread.Join(); // wait completion

            // assert
            result.Queue.Count.Should().Be(0);
            result.ExceptionSource.IsCancellationRequested.Should().BeTrue();
        }
    }
}
