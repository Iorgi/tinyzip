namespace TinyZipper.Application.Core.Interfaces
{
    public interface IDataFormatService
    {
        byte[] GetChunkMeta(byte[] chunk);
        int GetChunkLength(byte[] chunkMetaBytes);
    }
}