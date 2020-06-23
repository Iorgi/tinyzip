using System;
using System.IO.Compression;
using FluentAssertions;
using TinyZipper.Application.Core.ClientOptions;
using Xunit;

namespace TinyZipper.UnitTests
{
    public class ClientOptionsServiceTests
    {
        private readonly IClientOptionsService _serviceUnderTest;

        public ClientOptionsServiceTests()
        {
            _serviceUnderTest = new ClientOptionsService();
        }

        [Fact]
        public void Map_NullArgs_ThrowsException()
        {
            // act
            var invoking = _serviceUnderTest.Invoking(s => s.Map(null));

            // assert
            invoking.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Map_EmptyArgs_ThrowsException()
        {
            // arrange
            var args = new string[0];

            // act
            var invoking = _serviceUnderTest.Invoking(s => s.Map(args));

            // assert
            invoking.Should().Throw<ArgumentNullException>();
        }

        [Theory]
        [InlineData("compres", "file.txt", "file.zip")] // wrong mode string
        [InlineData(null, "file.txt", "file.zip")]      // null mode string
        [InlineData("", "file.txt", "file.zip")]        // empty mode string
        [InlineData("   ", "file.txt", "file.zip")]     // white spaces mode string
        [InlineData("compress", "", "file.zip")]        // empty source
        [InlineData("decompress", null, "file.zip")]    // null source
        [InlineData("decompress", " ", "file.zip")]     // white space source
        [InlineData("decompress", "file.txt", null)]    // null destination
        [InlineData("decompress", "file.txt", "")]      // empty destination
        [InlineData("decompress", "file.txt", " ")]     // white space destination
        public void Map_Arguments_Throws(string first, string second, string third)
        {
            // arrange
            var args = new [] { first, second, third };

            // act
            var invoking = _serviceUnderTest.Invoking(s => s.Map(args));

            // assert
            invoking.Should().Throw<ArgumentException>();
        }

        [Theory]
        [InlineData("compress", "file.txt", "file.zip", CompressionMode.Compress)]
        [InlineData("Compress", "c:/folder/file.txt", "file.zip", CompressionMode.Compress)]
        [InlineData("COMPRESS", "c:/folder 2/file.txt", "d:/folder 3/file.zip", CompressionMode.Compress)]
        [InlineData("CoMPrESs", "file", ".zip", CompressionMode.Compress)]
        [InlineData("Decompress", "file.txt", "file.zip", CompressionMode.Decompress)]
        [InlineData("decompress", "file.txt", "file.zip", CompressionMode.Decompress)]
        public void Map_Arguments_ReturnsObject(string first, string second, string third, CompressionMode expectedCompressionMode)
        {
            // arrange
            var args = new [] { first, second, third };

            // act
            var model = _serviceUnderTest.Map(args);

            // assert
            model.Should().NotBeNull();
            model.CompressionMode.Should().Be(expectedCompressionMode);
            model.SourceFileName.Should().Be(second);
            model.DestinationFileName.Should().Be(third);
        }
    }
}
