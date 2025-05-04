// Model for audio files.

public class AudioFile
{
    public string FilePath { get; }
    public string FileName { get; }

    // @TODO: Add optional properties for metadata.

    public AudioFile(string filePath, string fileName)
    {
        FilePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
        FileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
    }
}