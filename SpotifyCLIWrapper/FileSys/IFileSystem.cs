   public interface IFileSystem
    {
        bool DirectoryExists(string path);
        IEnumerable<string> GetFiles(string path, string searchPattern);
    }
