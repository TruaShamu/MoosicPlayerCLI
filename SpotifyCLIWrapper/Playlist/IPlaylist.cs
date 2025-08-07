// Playlist management
public interface IPlaylist
{
    IReadOnlyList<AudioFile> Files { get; }
    int CurrentIndex { get; }
    AudioFile CurrentFile { get; }
    void LoadFiles(IEnumerable<AudioFile> files);
    bool MoveNext();
    bool MovePrevious();
    bool MoveToIndex(int index);
    void Reset();
    bool IsShuffling { get; }
    void ToggleShuffle();
}