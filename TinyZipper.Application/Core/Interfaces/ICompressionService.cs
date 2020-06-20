namespace TinyZipper.Application.Core.Interfaces
{
    public interface ICompressionService
    {
        byte[] Compress(byte[] source);

        byte[] Decompress(byte[] source);
    }
}