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
    public bool IsShuffling => _playlist.IsShuffling;

    public Subtitle CurrentSubtitle => _currentSubtitle;

    private Timer _subtitleTimer;
    private Subtitle? _currentSubtitle;

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
            
            if (CurrentTrack.HasSubtitles)
            {
                StartSubtitleTracking();
            }
            else
            {
                StopSubtitleTracking();
                _currentSubtitle = null;
            }
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
    /// </summary>
    public void ShufflePlaylist()
    {
        _playlist.ToggleShuffle();
    }

    private void StartSubtitleTracking()
    {
        StopSubtitleTracking();
        
        _subtitleTimer = new Timer(UpdateCurrentSubtitle, null, 0, 100);
    }

    private void StopSubtitleTracking()
    {
        if (_subtitleTimer != null)
        {
            _subtitleTimer.Dispose();
            _subtitleTimer = null;
        }
    }

    private void UpdateCurrentSubtitle(object state)
    {
        if (CurrentTrack?.HasSubtitles == true && IsPlaying)
        {
            // Get current position
            TimeSpan position = CurrentPosition;
            
            // Find the subtitle that should be active at this position
            Subtitle subtitle = CurrentTrack.Subtitles.GetActiveSubtitleAt(position);
            
            // Only update if there's a change (to avoid unnecessary UI updates)
            if (_currentSubtitle != subtitle)
            {
                _currentSubtitle = subtitle;
            }
        }
    }
}