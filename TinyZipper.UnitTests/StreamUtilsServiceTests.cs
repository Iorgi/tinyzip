using System;
using System.IO;
using System.IO.Compression;
using FluentAssertions;
using TinyZipper.Application.Core;
using Xunit;

namespace TinyZipper.UnitTests
{
    public class StreamUtilsServiceTests
    {
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(100)]
        [InlineData(255)]
        [InlineData(1024 * 1024)]
        [InlineData(1024 * 1024 * 1024)]
        public void ChunkMeta_Serialize_Deserialize_SameResult(int number)
        {
            // arrange
            var service = new StreamUtilsService();
            var bytes = new byte[number];

            // act
            var chunkMeta = service.GetChunkMeta(bytes);
            var deserialized = service.GetChunkLengthFromMeta(chunkMeta);

            // assert
            chunkMeta.Length.Should().Be(4);
            deserialized.Should().Be(number);
        }

        [Fact]
        public void Decompress_ReadingStream()
        {
            // arrange
            var service = new StreamUtilsService();
            var bytes = new byte[]
            {
                5, 0, 0, 0,     // size of first block is 5 bytes
                1, 2, 3, 4, 5,  // first data block
                4, 0, 0, 0,     // size of second block is 4 bytes
                6, 7, 8, 9      // second data block
            };
            var stream = new MemoryStream(bytes);
            var compressionMode = CompressionMode.Decompress;

            // act
            var firstBlockSize = service.GetChunkLength(stream, compressionMode, 10);
            var firstBlockData = service.GetNextChunk(stream, firstBlockSize);
            var secondBlockSize = service.GetChunkLength(stream, compressionMode, 10);
            var secondBlockData = service.GetNextChunk(stream, secondBlockSize);
            var thirdBlockSize = service.GetChunkLength(stream, compressionMode, 10);

            // assert
            firstBlockSize.Should().Be(5);
            firstBlockData.Should().Equal(new [] {1, 2, 3, 4, 5});
            secondBlockSize.Should().Be(4);
            secondBlockData.Should().Equal(new [] { 6, 7, 8, 9 });
            thirdBlockSize.Should().Be(0);
        }

        [Fact]
        public void Compress_ReadingStream()
        {
            // arrange
            var service = new StreamUtilsService();
            var bytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 };
            var stream = new MemoryStream(bytes);
            var defaultChunkSize = 5;
            var compressionMode = CompressionMode.Compress;

            // act
            var firstBlockSize = service.GetChunkLength(stream, compressionMode, defaultChunkSize);
            var firstBlockData = service.GetNextChunk(stream, firstBlockSize);
            var secondBlockSize = service.GetChunkLength(stream, compressionMode, defaultChunkSize);
            var secondBlockData = service.GetNextChunk(stream, secondBlockSize);
            var thirdBlockSize = service.GetChunkLength(stream, compressionMode, defaultChunkSize);
            var thirdBlockData = service.GetNextChunk(stream, thirdBlockSize);

            // assert
            firstBlockSize.Should().Be(defaultChunkSize);
            firstBlockData.Should().Equal(new [] { 1, 2, 3, 4, 5 });
            secondBlockSize.Should().Be(defaultChunkSize);
            secondBlockData.Should().Equal(new [] { 6, 7, 8, 9, 0 });
            thirdBlockSize.Should().Be(defaultChunkSize);
            thirdBlockData.Should().Equal();
        }
    }
}