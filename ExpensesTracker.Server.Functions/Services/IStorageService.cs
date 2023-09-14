using System.IO;
using System.Threading.Tasks;

namespace ExpensesTracker.Server.Functions.Services
{
    public interface IStorageService
    {
        /// <summary>
        /// Save a file to a storage source and return its full url.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        Task<string> SaveFileAsync(Stream stream, string fileName);

        /// <summary>
        /// Delete a file by using its full path.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        Task DeleteFileAsync(string filePath);
    }
}
