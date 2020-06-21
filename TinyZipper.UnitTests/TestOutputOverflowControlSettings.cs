using TinyZipper.Application.Settings;

namespace TinyZipper.UnitTests
{
    public class TestOutputOverflowControlSettings : IOutputOverflowControlSettings
    {
        public int Ceiling { get; set; }
        public int Floor { get; set; }
    }
}