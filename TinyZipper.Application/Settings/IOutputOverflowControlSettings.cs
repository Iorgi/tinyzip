namespace TinyZipper.Application.Settings
{
    public interface IOutputOverflowControlSettings
    {
        int Ceiling { get; }

        int Floor { get; }
    }
}