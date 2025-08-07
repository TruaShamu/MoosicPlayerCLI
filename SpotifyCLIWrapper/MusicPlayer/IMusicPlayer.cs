public interface IMusicPlayer : IDisposable
{
    IReadOnlyList<AudioFile> CurrentPlaylist { get; }
    AudioFile CurrentTrack { get; }
    TimeSpan CurrentPosition { get; }
    TimeSpan TotalDuration { get; }
    bool IsPlaying { get; }
    bool IsLoopingCurrentTrack { get; }
    bool IsShuffling { get; }
    float Volume { get; }
    Subtitle? CurrentSubtitle { get; }
    Task LoadPlaylistFromDirectoryAsync(string directoryPath);
    void PlayCurrentTrack();
    void PlayTrackAtIndex(int index);
    void TogglePlayPause();
    void NextTrack();
    void PreviousTrack();
    void LoopCurrentTrack();
    void ShufflePlaylist();
    void SetVolume(float volume);
    void IncreaseVolume();
    void DecreaseVolume();
}