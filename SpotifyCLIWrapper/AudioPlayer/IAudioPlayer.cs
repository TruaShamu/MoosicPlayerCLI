// Audio player abstraction
public interface IAudioPlayer : IDisposable
{
    TimeSpan CurrentPosition { get; }
    TimeSpan TotalDuration { get; }
    bool IsPlaying { get; }
    float Volume { get; }
    void Play(string filePath);
    void Pause();
    void Resume();
    void Stop();
    void SetVolume(float volume);
    void IncreaseVolume(float step = 0.1f);
    void DecreaseVolume(float step = 0.1f);
    event EventHandler<TrackFinishedEventArgs> TrackFinished;
}