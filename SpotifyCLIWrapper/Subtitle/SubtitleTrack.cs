public class SubtitleTrack
{
    private readonly List<Subtitle> _subtitles = new List<Subtitle>();
    
    public IReadOnlyList<Subtitle> Subtitles => _subtitles.AsReadOnly();
    public string FilePath { get; }
    
    public SubtitleTrack(string filePath)
    {
        FilePath = filePath;
    }

    public void AddSubtitle(Subtitle subtitle)
    {
        _subtitles.Add(subtitle);
    }
    
    public Subtitle GetActiveSubtitleAt(TimeSpan position)
    {
        return _subtitles.FirstOrDefault(s => s.IsActiveAt(position));
    }
}