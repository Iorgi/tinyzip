﻿using System.IO.Compression;
using System.Threading;
using TinyZipper.Application.Compressing;

namespace TinyZipper.Application.Core.Interfaces
{
    public interface IDestinationWriter
    {
        AsyncWriteContext Write(string destinationUri, CompressionMode compressionMode,
            ParallelCompressionContext compressionContext, CancellationToken calculationsFinishedToken);
    }
}