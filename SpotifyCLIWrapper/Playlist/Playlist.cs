public class Playlist : IPlaylist
{
    private List<AudioFile> _files = new List<AudioFile>();
    
    private IReadOnlyList<AudioFile> _readOnlyFiles;
    public IReadOnlyList<AudioFile> Files => _readOnlyFiles;

    public int CurrentIndex { get; private set; } = -1;
    
    public AudioFile CurrentFile => 
        CurrentIndex >= 0 && CurrentIndex < _files.Count ? _files[CurrentIndex] : null;

    public void LoadFiles(IEnumerable<AudioFile> files)
    {
        _files = files?.ToList() ?? throw new ArgumentNullException(nameof(files));
        _readOnlyFiles = _files.AsReadOnly();
        Reset();
    }

    public bool MoveNext()
    {
        if (_files.Count == 0)
            return false;
            
        if (CurrentIndex < _files.Count - 1)
        {
            CurrentIndex++;
            return true;
        }
        
        return false;
    }

    public bool MovePrevious()
    {
        if (_files.Count == 0)
            return false;
            
        if (CurrentIndex > 0)
        {
            CurrentIndex--;
            return true;
        }
        
        return false;
    }

    public void Reset()
    {
        CurrentIndex = _files.Count > 0 ? 0 : -1;
    }
}