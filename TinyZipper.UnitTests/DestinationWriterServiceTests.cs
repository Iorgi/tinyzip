using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading;
using FluentAssertions;
using Moq;
using TinyZipper.Application.Compressing;
using TinyZipper.Application.Core;
using TinyZipper.Application.Core.Interfaces;
using TinyZipper.Application.Writers;
using Xunit;

namespace TinyZipper.UnitTests
{
    public class DestinationStreamWriterTests
    {
        [Fact]
        public void Write_CompressMode_ItemsWrittenToStream()
        {
            // arrange
            var outputStream = new FakeStream();
            var service = PrepareDestinationWriter(outputStream, false);
            var expectedResult = new byte[]
            {
                5, 0, 0, 0,     // size of first block is 5 bytes
                1, 2, 3, 4, 5,  // first data block
                4, 0, 0, 0,     // size of second block is 4 bytes
                6, 7, 8, 9      // second data block
            };
            var pieces = new List<KeyValuePair<int, byte[]>>
            {
                new KeyValuePair<int, byte[]>(1, new byte[] {1, 2, 3, 4, 5}),
                new KeyValuePair<int, byte[]>(2, new byte[] {6, 7, 8, 9})
            };
            var compressionResultsPieces = new ConcurrentDictionary<int, byte[]>(pieces);
            var compressionContext = new ParallelCompressionContext(null, compressionResultsPieces,
                new CancellationTokenSource(), new ManualResetEventSlim(true));
            var calculationsFinishedSrc = new CancellationTokenSource();
            calculationsFinishedSrc.Cancel();

            // act
            var result = service.Write("", CompressionMode.Compress, compressionContext, calculationsFinishedSrc.Token);
            result.ProcessingThread.Join();

            // assert
            result.ExceptionSource.IsCancellationRequested.Should().BeFalse();
            outputStream.Length.Should().Be(expectedResult.Length);
            outputStream.ToArray().Should().Equal(expectedResult);

            outputStream.Close();
            outputStream.Dispose();
        }

        [Fact]
        public void Write_DecompressMode_ItemsWrittenToStream()
        {
            // arrange
            var expectedResult = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 };
            var pieces = new List<KeyValuePair<int, byte[]>>
            {
                new KeyValuePair<int, byte[]>(1, new byte[] {1, 2, 3}),
                new KeyValuePair<int, byte[]>(2, new byte[] {4, 5}),
                new KeyValuePair<int, byte[]>(3, new byte[] {6, 7, 8, 9, 0})
            };
            var outputStream = new FakeStream();
            var service = PrepareDestinationWriter(outputStream, false);
            
            var compressionResultsPieces = new ConcurrentDictionary<int, byte[]>(pieces);
            var compressionContext = new ParallelCompressionContext(null, compressionResultsPieces,
                new CancellationTokenSource(), new ManualResetEventSlim(true));
            var calculationsFinishedSrc = new CancellationTokenSource();
            calculationsFinishedSrc.Cancel();

            // act
            var result = service.Write("", CompressionMode.Decompress, compressionContext, calculationsFinishedSrc.Token);
            result.ProcessingThread.Join();

            // assert
            result.ExceptionSource.IsCancellationRequested.Should().BeFalse();
            outputStream.Length.Should().Be(expectedResult.Length);
            outputStream.ToArray().Should().Equal(expectedResult);

            outputStream.Close();
            outputStream.Dispose();
        }

        [Fact]
        public void Write_ExceptionInsideThread_ExceptionSourceCancelled()
        {
            // arrange
            var destinationStreamServiceMock = new Mock<IDestinationStreamService>();
            destinationStreamServiceMock.Setup(x => x.OpenWrite(It.IsAny<string>()))
                .Throws<Exception>();
            var service = PrepareDestinationWriter(null, true);
            var compressionContext = new ParallelCompressionContext(null, null,
                new CancellationTokenSource(), new ManualResetEventSlim(true));
            var calculationsFinishedSrc = new CancellationTokenSource();
            calculationsFinishedSrc.Cancel();

            // act
            var result = service.Write("", CompressionMode.Decompress, compressionContext, calculationsFinishedSrc.Token);
            result.ProcessingThread.Join();

            // assert
            result.ExceptionSource.IsCancellationRequested.Should().BeTrue();
        }

        private IDestinationWriter PrepareDestinationWriter(Stream outputStream, bool throwException)
        {
            var statusUpdateServiceMock = new Mock<IStatusUpdateService>();
            var testSettings = new TestOutputOverflowControlSettings { Ceiling = 100, Floor = 100 };

            var destinationStreamServiceMock = new Mock<IDestinationStreamService>();
            var setup = destinationStreamServiceMock.Setup(x => x.OpenWrite(It.IsAny<string>()));
            if (throwException) setup.Throws<Exception>();
            else setup.Returns(outputStream);

            var service = new DestinationStreamWriter(new StreamUtilsService(), statusUpdateServiceMock.Object,
                testSettings, destinationStreamServiceMock.Object);

            return service;
        }
    }
}