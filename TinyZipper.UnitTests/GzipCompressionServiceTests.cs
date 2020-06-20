using System;
using System.Linq;
using FluentAssertions;
using TinyZipper.Application.Compressing;
using Xunit;

namespace TinyZipper.UnitTests
{
    public class GzipCompressionServiceTests
    {
        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(100)]
        [InlineData(1000)]
        [InlineData(1024*1024)]
        public void Compress_Decompress_SameResult(int bytesCount)
        {
            // arrange
            var random = new Random();
            var service = new GzipCompressionService();
            var inputArray = new byte[bytesCount];
            random.NextBytes(inputArray);

            // act
            var compressed = service.Compress(inputArray);
            var decompressed = service.Decompress(compressed);

            // assert
            compressed.Should().NotEqual(decompressed);
            decompressed.Should().Equal(inputArray);
        }
    }
}