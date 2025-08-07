public class Playlist : IPlaylist
{
    private List<AudioFile> _tracklist = new();
    public IReadOnlyList<AudioFile> Files => _tracklist.AsReadOnly();

    private Stack<int> _historyIndices = new();         // Played
    private Queue<int> _futureIndices = new();          // Upcoming
    private HashSet<int> _playedIndices = new();        // Fast lookup
    private int? _currentIndex = null;

    private bool _isShuffling = false;
    public bool IsShuffling => _isShuffling;


    public AudioFile? CurrentFile =>
        _currentIndex.HasValue && _currentIndex.Value >= 0 && _currentIndex.Value < _tracklist.Count
            ? _tracklist[_currentIndex.Value]
            : null;

    public int CurrentIndex => _currentIndex ?? -1;

    public void LoadFiles(IEnumerable<AudioFile> files)
    {
        _tracklist = files.ToList();
        Reset();
    }

    public void Reset()
    {
        _historyIndices.Clear();
        _futureIndices.Clear();
        _playedIndices.Clear();

        for (int i = 0; i < _tracklist.Count; i++)
        {
            _futureIndices.Enqueue(i);
        }

        _currentIndex = null;
        _isShuffling = false;
    }

    public bool MoveNext()
    {
        if (_futureIndices.Count == 0)
            return false;

        if (_currentIndex.HasValue)
        {
            _historyIndices.Push(_currentIndex.Value);
        }

        _currentIndex = _futureIndices.Dequeue();
        _playedIndices.Add(_currentIndex.Value);
        return true;
    }

    public bool MovePrevious()
    {
        if (_historyIndices.Count == 0)
            return false;

        if (_currentIndex.HasValue)
        {
            _futureIndices = new Queue<int>(new[] { _currentIndex.Value }.Concat(_futureIndices));
        }

        _currentIndex = _historyIndices.Pop();
        return true;
    }

    public bool MoveToIndex(int index)
    {
        if (index < 0 || index >= _tracklist.Count)
            return false;

        // Add current track to history if we have one
        if (_currentIndex.HasValue)
        {
            _historyIndices.Push(_currentIndex.Value);
        }

        // Set new current index
        _currentIndex = index;
        
        // Add to played indices
        _playedIndices.Add(index);

        // Remove from future indices if it exists there
        var tempQueue = new Queue<int>();
        while (_futureIndices.Count > 0)
        {
            var item = _futureIndices.Dequeue();
            if (item != index)
            {
                tempQueue.Enqueue(item);
            }
        }
        _futureIndices = tempQueue;

        return true;
    }

    public void ToggleShuffle()
    {
        _isShuffling = !_isShuffling;

        var unplayed = Enumerable.Range(0, _tracklist.Count)
                                 .Where(i => !_playedIndices.Contains(i))
                                 .ToList();

        if (_isShuffling)
        {
            var rng = new Random();
            unplayed = unplayed.OrderBy(_ => rng.Next()).ToList();
        }

        _futureIndices = new Queue<int>(unplayed);
    }
}