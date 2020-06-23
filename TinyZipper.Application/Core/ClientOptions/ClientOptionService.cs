using System;
using System.IO.Compression;

namespace TinyZipper.Application.Core.ClientOptions
{
    public class ClientOptionsService : IClientOptionsService
    {
        private readonly string _example;

        public ClientOptionsService()
        {
            _example = $"Command template: tinyzipper.exe [{CompressionMode.Compress.ToString().ToLower()}|{CompressionMode.Decompress.ToString().ToLower()}]" +
                        " [\"source file name\"] [\"destination file name\"]";
        }

        public ClientOptionsModel Map(string[] args)
        {
            if (args == null || args.Length == 0)
                throw new ArgumentNullException(nameof(args));

            if (args.Length != 3)
                throw new ArgumentException($"Wrong number of arguments. {_example}");

            var compressionMode = ParseCompressionMode(args[0]);
            var sourceFileName = AssertSourceName(args[1]);
            var destinationFileName = AssertDestinationName(args[2]);

            return new ClientOptionsModel(compressionMode, sourceFileName, destinationFileName);
        }

        private CompressionMode ParseCompressionMode(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                throw new ArgumentException($"Missing compression mode. {_example}");

            if (!Enum.TryParse(input, true, out CompressionMode result))
                throw new ArgumentException($"Can't parse value [{input}] as compression mode. {_example}");

            return result;
        }

        private string AssertSourceName(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                throw new ArgumentException($"Source file name missed. {_example}");

            return input;
        }

        private string AssertDestinationName(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                throw new ArgumentException($"Destination file name missed. {_example}");

            return input;
        }
    }
}