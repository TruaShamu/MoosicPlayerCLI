using NAudio.Wave;

public class NAudioPlayer : IAudioPlayer
{
    private IWavePlayer? _waveOutDevice;
    private AudioFileReader? _audioFileReader;
    private string? _currentFilePath; // File path of the currently playing audio file
    private bool _isPaused;
    private bool _isManuallyStopped; // Flag to indicate if the player was manually stopped
    public event EventHandler<TrackFinishedEventArgs> TrackFinished;
    public TimeSpan CurrentPosition => _audioFileReader?.CurrentTime ?? TimeSpan.Zero;
    public TimeSpan TotalDuration => _audioFileReader?.TotalTime ?? TimeSpan.Zero;
    public bool IsPlaying => _waveOutDevice != null && _waveOutDevice.PlaybackState == PlaybackState.Playing;
    private float _volume = 1.0f;
    public float Volume => _volume;

    /// <summary>
    /// Starts playback of the specified audio file.
    /// If the same file is paused, it resumes playback instead.
    /// </summary>
    /// <param name="filePath">The full path to the audio file to play.</param>
    /// <exception cref="ArgumentException">Thrown when the file path is null, empty, or whitespace.</exception>
    public void Play(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be empty.", nameof(filePath));

        if (_currentFilePath == filePath && _isPaused)
        {
            Resume();
            return;
        }

        Stop(); // Stop any existing playback and dispose of resources

        Console.WriteLine("Starting playback of: " + filePath);

        // Initialize new playback
        _currentFilePath = filePath;
        _audioFileReader = new AudioFileReader(filePath);
        _waveOutDevice = new WaveOutEvent();
        _waveOutDevice.Init(_audioFileReader);
        _waveOutDevice.Volume = _volume;
        _waveOutDevice.Play();
        _isManuallyStopped = false;
        _waveOutDevice.PlaybackStopped += WaveOutDevice_PlaybackStopped;
        _isPaused = false;
    }

    /// <summary>
    /// Pauses the current playback if it is playing.
    /// </summary>
    public void Pause()
    {
        if (_waveOutDevice?.PlaybackState == PlaybackState.Playing)
        {
            _waveOutDevice.Pause();
            _isPaused = true;
        }
    }

    /// <summary>
    /// Resumes playback if it is currently paused.
    /// </summary>
    public void Resume()
    {
        if (_waveOutDevice?.PlaybackState == PlaybackState.Paused)
        {
            _waveOutDevice.Play();
            _isPaused = false;
        }
    }

    /// <summary>
    /// Stops playback and disposes of the audio resources.
    /// </summary>
    public void Stop()
    {
        _isManuallyStopped = true;
        try
        {
            if (_waveOutDevice != null)
            {
                _waveOutDevice.PlaybackStopped -= WaveOutDevice_PlaybackStopped;
            }
            _waveOutDevice?.Stop();
            _waveOutDevice?.Dispose();
            _audioFileReader?.Dispose();
        }
        catch (Exception ex)
        {
            // Handle potential issues (logging, etc.)
            Console.WriteLine($"Error stopping or disposing: {ex.Message}");
        }

        // Reset state
        _waveOutDevice = null;
        _audioFileReader = null;
        _currentFilePath = null;
        _isPaused = false;
    }

    private void WaveOutDevice_PlaybackStopped(object? sender, StoppedEventArgs e)
    {
        if (!_isManuallyStopped && _currentFilePath != null)
        {
            // If it wasn't manually stopped, then it reached the end naturally
            string finishedFilePath = _currentFilePath;
            Task.Run(() => TrackFinished?.Invoke(this, new TrackFinishedEventArgs(finishedFilePath)));
        }
        
        // Reset for next time
        _isManuallyStopped = false;
    }

    public void SetVolume(float volume)
    {
        _volume = Math.Clamp(volume, 0f, 1f);
        if (_waveOutDevice != null)
        {
            _waveOutDevice.Volume = volume;
        }
    }

    public void IncreaseVolume(float step = 0.1f)
    {
        SetVolume(_volume + step);
    }
    
    public void DecreaseVolume(float step = 0.1f)
    {
        SetVolume(_volume - step);
    }

    public void Dispose()
    {
        Stop();
    }
}