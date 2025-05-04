public interface IMusicPlayer : IDisposable
{
    IReadOnlyList<AudioFile> CurrentPlaylist { get; }
    AudioFile CurrentTrack { get; }
    TimeSpan CurrentPosition { get; }
    TimeSpan TotalDuration { get; }
    bool IsPlaying { get; }
    bool IsLoopingCurrentTrack { get; }
    Task LoadPlaylistFromDirectoryAsync(string directoryPath);
    void PlayCurrentTrack();
    void TogglePlayPause();
    void NextTrack();
    void PreviousTrack();
    void LoopCurrentTrack();
}