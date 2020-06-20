using System.IO;

namespace TinyZipper.Application.Core
{
    public interface IFileService
    {
        long GetFileSize(string filePath);

        void DeleteIfExistsSafe(string filePath);
    }

    public class FileService : IFileService
    {
        public long GetFileSize(string filePath)
        {
            var fileInfo = new FileInfo(filePath);
            return fileInfo.Length;
        }

        public void DeleteIfExistsSafe(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                    File.Delete(filePath);

            }
            catch
            {
                // safe
            }
        }
    }
}