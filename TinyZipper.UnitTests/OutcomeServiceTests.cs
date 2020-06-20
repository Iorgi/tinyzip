using System;
using System.IO.Compression;
using System.Threading;
using FluentAssertions;
using Moq;
using TinyZipper.Application.ClientOptions;
using TinyZipper.Application.Compressing;
using TinyZipper.Application.Core;
using TinyZipper.Application.Core.Interfaces;
using Xunit;

namespace TinyZipper.UnitTests
{
    public class OutcomeServiceTests
    {
        [Theory]
        [InlineData(false, false, false, true)]
        [InlineData(true, false, false, false)]
        [InlineData(false, true, false, false)]
        [InlineData(false, false, true, false)]
        [InlineData(true, true, true, false)]
        public void Finalize_ExceptionSourcesSet(bool readException, bool compressException, bool writeException, bool expectedResult)
        {
            // arrange
            var readExceptionSrc = new CancellationTokenSource();
            var compressExceptionSrc = new CancellationTokenSource();
            var writeExceptionSrc = new CancellationTokenSource();

            if (readException) readExceptionSrc.Cancel();
            if (compressException) compressExceptionSrc.Cancel();
            if (writeException) writeExceptionSrc.Cancel();

            var statusUpdateServiceMock = new Mock<IStatusUpdateService>();
            var fileServiceMock = new Mock<IFileService>();
            fileServiceMock.Setup(x => x.GetFileSize(It.IsAny<string>())).Returns(0);
            var service = new OutcomeService(statusUpdateServiceMock.Object, fileServiceMock.Object);

            var clientOptions = new ClientOptionsModel(CompressionMode.Compress, "", "");
            var readContext = new AsyncReadContext<QueueItem>(null, null, null, readExceptionSrc, null, null);
            var compressionContext = new ParallelCompressionContext(null, null, compressExceptionSrc, null);
            var writeContext = new AsyncWriteContext(null, writeExceptionSrc);

            // act
            var actualResult = service.Finalize(DateTime.Now, clientOptions,  readContext, compressionContext, writeContext);

            // assert
            actualResult.Should().Be(expectedResult);
        }
    }
}