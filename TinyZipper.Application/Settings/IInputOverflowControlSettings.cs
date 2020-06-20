namespace TinyZipper.Application.Settings
{
    public interface IInputOverflowControlSettings
    {
        int Ceiling { get; }

        int Floor { get; }
    }
}