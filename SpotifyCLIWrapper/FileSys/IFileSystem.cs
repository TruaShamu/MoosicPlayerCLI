public interface IFileSystem
{
    bool DirectoryExists(string path);
    bool FileExists(string path);
    IEnumerable<string> GetFiles(string path, string searchPattern);
    string ReadAllText(string path);
}