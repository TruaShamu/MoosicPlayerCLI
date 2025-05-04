// Playlist management
public interface IPlaylist
{
    IReadOnlyList<AudioFile> Files { get; }
    int CurrentIndex { get; }
    AudioFile CurrentFile { get; }
    void LoadFiles(IEnumerable<AudioFile> files);
    bool MoveNext();
    bool MovePrevious();
    void Reset();
    bool IsShuffling { get; }
    void ToggleShuffle();
}