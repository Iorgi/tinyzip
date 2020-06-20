namespace TinyZipper.Application.Settings
{
    public class DefaultSettings : IInputOverflowControlSettings, IOutputOverflowControlSettings, ICompressionSettings
    {
        int IInputOverflowControlSettings.Ceiling { get; } = 100;

        int IInputOverflowControlSettings.Floor { get; } = 50;

        int IOutputOverflowControlSettings.Ceiling { get; } = 100;

        int IOutputOverflowControlSettings.Floor { get; } = 50;

        int ICompressionSettings.ChunkSize { get; } = 1024 * 1024;
    }
}