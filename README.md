# tinyzip

Simple console application to compress data in efficient manner.
Uses .net GZipStream under the hood. https://docs.microsoft.com/en-us/dotnet/api/system.io.compression.gzipstream?view=netcore-3.1

## Performance 

To compare performance were used files from https://cs.fit.edu/~mmahoney/compression/textdata.html

enwik9 file (10^9 bytes):

|Process           |Compressed (bytes)|Ratio (%)|Elapsed (seconds)|
|------------------|------------------|---------|-----------------|
|tinyzip (Parallel)|328,440,035       |33       |7                |
|tinyzip (Simple)  |327,323,092       |33       |35               |
|7Zip (Fast)       |336,979,190       |34       |37               | 

enwik8 file (10^8 bytes):

|Process           |Compressed (bytes)|Ratio (%)|Elapsed (seconds)|
|------------------|------------------|---------|-----------------|
|tinyzip (Parallel)|37,055,733        |37       |0.8              |
|tinyzip (Simple)  |36,948,688        |37       |4                |
|7Zip (Fast)       |37,907,740        |38       |4                | 

## How to use

tinyzip.exe [compress|decompress] ["source file name"] ["destination file name"]
Directory for the destination file should be created manually.

## What can compress/dicompress 

Now tool implements file to file compression and decompression.
But you can easily change it just implementing `ISourceStreamService` for source data and `IDestinationStreamService` for resulting data.
These service are about working with `Stream`. But if you want read and write dtat using something else instead of `Stream` you can implement `ISourceReader` and 'IDestinationWriter'.
Current implementation of `ISourceReader` reads stream to end and signals about it to complete process, but you can write own implementation to read forever.
So there might be a lot of options what to read and where to write (for example, read form web end point, compress and save to cloud storage).
Also there is ability to override how to interact with caller. Currently tinizyp just writes messages to Console, but you can write your own implementation of `IStatusUpdateService` to send some web requests.
Also you can change compression algorithm just implementing `ICompressionService`.
If you want change entire logic, implenment `ICompressionOrchestrationService`, whic now has two implementations: `SimpleCompressionOrchestrationService` (tinyzip (Simple)) and `ParallelCompressionOrchestrationService` (tinyzip (Parallel)).
