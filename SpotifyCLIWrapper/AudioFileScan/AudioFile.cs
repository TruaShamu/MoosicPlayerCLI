// Model for audio files.

public class AudioFile
{
    public string FilePath { get; }
    public string FileName { get; }
    public SubtitleTrack Subtitles { get; private set; }
    public bool HasSubtitles => Subtitles != null;

    // @TODO: Add optional properties for metadata.

    public AudioFile(string filePath, string fileName)
    {
        FilePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
        FileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
    }

    public void AttachSubtitles(SubtitleTrack subtitles)
    {
        Subtitles = subtitles;
    }
}