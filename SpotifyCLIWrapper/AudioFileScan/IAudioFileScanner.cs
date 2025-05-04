// Audio file scanner
public interface IAudioFileScanner
{
    IEnumerable<AudioFile> ScanDirectory(string directoryPath);
}