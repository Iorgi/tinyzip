using TinyZipper.Application.Settings;

namespace TinyZipper.UnitTests
{
    public class TestCompressionSettings : ICompressionSettings
    {
        public int ChunkSize { get; set; }
    }
}