public class AudioFileScanner : IAudioFileScanner
{
    private readonly IFileSystem _fileSystem;
    private readonly string[] _supportedExtensions = { ".mp3", ".wav", ".ogg", ".flac" };

    public AudioFileScanner(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
    }

    /// <summary>
    /// Scans the specified directory and returns all supported audio files.
    /// </summary>
    /// <param name="directoryPath">The full path of the directory to scan.</param>
    /// <returns>A list of audio files found in the directory.</returns>
    /// <exception cref="ArgumentException">Thrown if the directory path is null or whitespace.</exception>
    /// <exception cref="DirectoryNotFoundException">Thrown if the specified directory does not exist.</exception>
    public IEnumerable<AudioFile> ScanDirectory(string directoryPath)
    {
        if (string.IsNullOrWhiteSpace(directoryPath))
            throw new ArgumentException("Directory path cannot be empty.", nameof(directoryPath));

        if (!_fileSystem.DirectoryExists(directoryPath))
            throw new DirectoryNotFoundException($"Directory not found: {directoryPath}");

        var audioFiles = new List<AudioFile>();
        var allFiles = _fileSystem.GetFiles(directoryPath, "*");
        foreach (var file in allFiles)
        {
            if (_supportedExtensions.Contains(Path.GetExtension(file), StringComparer.OrdinalIgnoreCase))
            {
                var fileName = Path.GetFileName(file);
                audioFiles.Add(new AudioFile(file, fileName));
            }
        }
        return audioFiles;
    }
}