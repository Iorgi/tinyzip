using System;
using TinyZipper.Application.Core.Interfaces;

namespace TinyZipper.Application.UpstreamFormatting
{
    public class DataFormatService : IDataFormatService
    {
        public byte[] GetChunkMeta(byte[] chunk)
        {
            return BitConverter.GetBytes(chunk.Length);
        }

        public int GetChunkLength(byte[] chunkMetaBytes)
        {
            return BitConverter.ToInt32(chunkMetaBytes, 0);
        }
    }
}