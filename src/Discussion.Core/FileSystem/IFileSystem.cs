using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discussion.Core.FileSystem
{
    public interface IFileSystem
    {
        Task<bool> FileExistsAsync(string path);
        
        Task<IList<IFile>> ListFilesAsync(string path);
        
        Task<IFile> GetFileAsync(string path);

        Task DeleteFileAsync(string path);

        Task<IFile> CreateFileAsync(string path);

        string GetDirectorySeparatorChar();
    }
}