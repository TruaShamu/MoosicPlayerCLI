// Wrapper for file system for testing / mocking purposes
public class FileSystem : IFileSystem
{
    public bool DirectoryExists(string path) => Directory.Exists(path);
    
    public IEnumerable<string> GetFiles(string path, string searchPattern) => 
        Directory.GetFiles(path, searchPattern);
}
