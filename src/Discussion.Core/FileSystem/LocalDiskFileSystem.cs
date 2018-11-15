using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Discussion.Core.FileSystem
{
    public class LocalDiskFileSystem : IFileSystem
    {
        private readonly string _storagePath;

        public LocalDiskFileSystem(string storagePath)
        {
            _storagePath = storagePath;
        }


       public Task<bool> FileExistsAsync(string path)
        {
            return Task.FromResult(File.Exists(MapStorage(path)));
        }

        public async Task<IList<IFile>> ListFilesAsync(string path)
        {
            return await Task.Factory.StartNew(() =>
            {
                var dir = MapStorage(path);
                return new DirectoryInfo(dir)
                    .GetFiles()
                    .Select(f => (IFile)(new LocalDiskFile(f, _storagePath)))
                    .ToList();
            });
        }

        public async Task<IFile> GetFileAsync(string path)
        {
            return await Task.Factory.StartNew(() =>
            {
                var fileInfo = new FileInfo(MapStorage(path));
                if (!fileInfo.Exists)
                {
                    throw new FileNotFoundException("File does not exist", path);
                }

                return CreateFileObject(fileInfo.FullName);
            });
        }

        public async Task DeleteFileAsync(string path)
        {
            await Task.Factory.StartNew(() =>
            {
                var fullPath = MapStorage(path);
                File.Delete(fullPath);
            });
        }

        public async Task<IFile> CreateFileAsync(string path)
        {   
            return await Task.Factory.StartNew(() =>
            {
                var fullPath = MapStorage(path);
                var dir = new FileInfo(fullPath).DirectoryName;
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                
                File.Create(fullPath).Dispose();
                return CreateFileObject(fullPath);
            });
        }

        private LocalDiskFile CreateFileObject(string path)
        {
            return new LocalDiskFile(new FileInfo(path), _storagePath);
        }

        public string GetDirectorySeparatorChar()
        {
            return PathSeparator;
        }

        public bool SupportGeneratingPublicUrl => false;


        private string MapStorage(string path) {
            string mappedPath = string.IsNullOrEmpty(path) ? _storagePath : Path.Combine(_storagePath, path);
            var normalizedPath = Path.GetFullPath(mappedPath);
            if (!normalizedPath.StartsWith(_storagePath, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"Path '{normalizedPath}' is out of storage path '{_storagePath}'");
            }

            return normalizedPath;
        }
        
        
        static readonly string PathSeparator = Path.DirectorySeparatorChar.ToString();
        
        
        private class LocalDiskFile : IFile
        {
            private readonly FileInfo _fileInfo;
            private readonly int _basePathLength;

            public LocalDiskFile(FileInfo fileInfo, string basePath)
            {
                _fileInfo = fileInfo;
                _basePathLength = basePath.Length;
                if (!basePath.EndsWith(PathSeparator))
                {
                    _basePathLength += PathSeparator.Length;
                }
            }

            #region Implementation of IFile

            public string GetPath()
            {
                var fullPath = _fileInfo.FullName;
                return fullPath.Substring(_basePathLength);
            }

            public string GetName()
            {
                return _fileInfo.Name;
            }

            public long GetSize()
            {
                return _fileInfo.Length;
            }

            public async Task<Stream> OpenReadAsync()
            {
                return await Task.Factory.StartNew(() => File.OpenRead(_fileInfo.FullName));
            }

            public async Task<Stream> OpenWriteAsync()
            {
                return await Task.Factory.StartNew(() => File.OpenWrite(_fileInfo.FullName));
            }

            public Task<string> GetPublicUrlAsync(TimeSpan timeout)
            {
                throw new NotImplementedException();
            }

            #endregion
        }

    }
}


