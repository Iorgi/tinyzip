using FluentAssertions;
using TinyZipper.Application.Compressing;
using TinyZipper.Application.UpstreamFormatting;
using Xunit;

namespace TinyZipper.UnitTests
{
    public class DataFormatServiceTests
    {
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(100)]
        [InlineData(255)]
        [InlineData(1024 * 1024)]
        [InlineData(1024 * 1024 * 1024)]
        public void Serialize_Deserialize_SameResult(int number)
        {
            // arrange
            var service = new DataFormatService();
            var bytes = new byte[number];

            // act
            var chunkMeta = service.GetChunkMeta(bytes);
            var deserialized = service.GetChunkLength(chunkMeta);

            // assert
            chunkMeta.Length.Should().Be(4);
            deserialized.Should().Be(number);
        }
    }
}