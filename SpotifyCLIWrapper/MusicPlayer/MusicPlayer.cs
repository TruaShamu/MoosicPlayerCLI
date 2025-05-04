public class MusicPlayer : IMusicPlayer
{
    private readonly IAudioFileScanner _audioFileScanner;
    private readonly IAudioPlayer _audioPlayer;
    private readonly IPlaylist _playlist;

    public IReadOnlyList<AudioFile> CurrentPlaylist => _playlist.Files;
    public AudioFile CurrentTrack => _playlist.CurrentFile;
    public TimeSpan CurrentPosition => _audioPlayer.CurrentPosition;
    public TimeSpan TotalDuration => _audioPlayer.TotalDuration;
    public bool IsPlaying => _audioPlayer.IsPlaying;
    private bool _isLoopingCurrentTrack;
    public bool IsLoopingCurrentTrack => _isLoopingCurrentTrack;
    private bool _isShuffling;
    public bool IsShuffling => _isShuffling;

    public MusicPlayer(
        IAudioFileScanner audioFileScanner,
        IAudioPlayer audioPlayer,
        IPlaylist playlist)
    {
        _audioFileScanner = audioFileScanner ?? throw new ArgumentNullException(nameof(audioFileScanner));
        _audioPlayer = audioPlayer ?? throw new ArgumentNullException(nameof(audioPlayer));
        _playlist = playlist ?? throw new ArgumentNullException(nameof(playlist));
        _audioPlayer.TrackFinished += AudioPlayer_TrackFinished;
    }

    public Task LoadPlaylistFromDirectoryAsync(string directoryPath)
    {
        return Task.Run(() =>
        {
            try
            {
                var audioFiles = _audioFileScanner.ScanDirectory(directoryPath);
                _playlist.LoadFiles(audioFiles);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to load playlist", ex);
            }
        });
    }


    public void PlayCurrentTrack()
    {
        if (CurrentTrack != null)
        {
            _audioPlayer.Play(CurrentTrack.FilePath);
        }
    }

    public void TogglePlayPause()
    {
        if (_audioPlayer.IsPlaying)
        {
            _audioPlayer.Pause();
        }
        else
        {
            if (CurrentTrack != null)
            {
                _audioPlayer.Resume();
            }
        }
    }

    public void NextTrack()
    {
        if (_playlist.MoveNext())
        {
            PlayCurrentTrack();
        }
    }

    public void PreviousTrack()
    {
        if (_playlist.MovePrevious())
        {
            PlayCurrentTrack();
        }
    }

    public void Dispose()
    {
        _audioPlayer.TrackFinished -= AudioPlayer_TrackFinished;
        _audioPlayer.Dispose();
    }

    public void LoopCurrentTrack()
    {
        _isLoopingCurrentTrack = !_isLoopingCurrentTrack;
    }

    private void AudioPlayer_TrackFinished(object? sender, TrackFinishedEventArgs e)
    {
        if (_isLoopingCurrentTrack)
        {
            PlayCurrentTrack();
        } else
        {
            NextTrack();
        }
    }

    /// <summary>
    ///  Shuffling stub
    /// </summary>
    public void ShufflePlaylist()
    {
        _isShuffling = !_isShuffling;
    }
}