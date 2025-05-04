namespace ConsoleMediaPlayer.Tests;

using Xunit;
using System.Collections.Generic;

public class PlaylistTests
{
    private static AudioFile[] GetTestFiles() =>
        new[] {
            new AudioFile("1.mp3", "Track 1"),
            new AudioFile("2.mp3", "Track 2"),
            new AudioFile("3.mp3", "Track 3"),
            new AudioFile("4.mp3", "Track 4"),
            new AudioFile("5.mp3", "Track 5"),
        };

    [Fact]
    // Test that LoadFiles initializes the playlist correctly
    public void LoadFiles_InitializesStateCorrectly()
    {
        var playlist = new Playlist();
        playlist.LoadFiles(GetTestFiles());

        Assert.Equal(5, playlist.Files.Count);
        Assert.Equal(-1, playlist.CurrentIndex);
    }

    [Fact]
    // Test that MoveNext advances to the next track after initialization
    public void MoveNext_AdvancesToFirstTrack()
    {
        var playlist = new Playlist();
        playlist.LoadFiles(GetTestFiles());

        var result = playlist.MoveNext();

        Assert.True(result);
        Assert.Equal(0, playlist.CurrentIndex);
        Assert.Equal("Track 1", playlist.CurrentFile?.FileName);
    }

    [Fact]
    // Test that MovePrevious returns false after initialization
    public void MovePrevious_BeforeAnyMoveNext_ReturnsFalse()
    {
        var playlist = new Playlist();
        playlist.LoadFiles(GetTestFiles());

        var result = playlist.MovePrevious();

        Assert.False(result);
        Assert.Equal(-1, playlist.CurrentIndex);
    }

    [Fact]
    // Test that movenext and moveprevious go back to track 0.
    public void MoveNext_ThenPrevious_ReturnsToPreviousTrack()
    {
        var playlist = new Playlist();
        playlist.LoadFiles(GetTestFiles());

        playlist.MoveNext(); // 0
        Assert.Equal(0, playlist.CurrentIndex);
        playlist.MoveNext(); // 1
        Assert.Equal(1, playlist.CurrentIndex);
        var result = playlist.MovePrevious();

        Assert.True(result);
        Assert.Equal(0, playlist.CurrentIndex);
    }

    [Fact]
    public void ToggleShuffle_ShufflesUnplayedTracksOnly()
    {
        var playlist = new Playlist();
        playlist.LoadFiles(GetTestFiles());

        playlist.MoveNext(); // Play 0
        playlist.MoveNext(); // Play 1

        playlist.ToggleShuffle();

        var shuffledOrder = new List<int>();
        while (playlist.MoveNext())
        {
            shuffledOrder.Add(playlist.CurrentIndex);
        }

        Assert.DoesNotContain(0, shuffledOrder);
        Assert.DoesNotContain(1, shuffledOrder);
        Assert.Equal(3, shuffledOrder.Count); // 5 total - 2 played = 3
    }

    [Fact]
    public void ToggleShuffleOff_RevertsToOriginalOrderForUnplayed()
    {
        var playlist = new Playlist();
        playlist.LoadFiles(GetTestFiles());

        playlist.MoveNext(); // 0
        playlist.ToggleShuffle();
        var firstShuffle = playlist.CurrentIndex;

        playlist.MoveNext(); // next shuffled
        playlist.MoveNext(); // next shuffled

        playlist.ToggleShuffle(); // back to linear

        var remaining = new List<int>();
        while (playlist.MoveNext())
        {
            remaining.Add(playlist.CurrentIndex);
        }

        var expectedUnplayed = new List<int> { 1, 2, 3, 4 };
        expectedUnplayed.Remove(0); // played
        expectedUnplayed.Remove(firstShuffle);
        foreach (var i in playlist.Files)
        {
            if (remaining.Contains(playlist.Files.ToList().IndexOf(i)))
                Assert.Contains(playlist.Files.ToList().IndexOf(i), expectedUnplayed);
        }
    }

    [Fact]
    public void Reset_ClearsState()
    {
        var playlist = new Playlist();
        playlist.LoadFiles(GetTestFiles());

        playlist.MoveNext(); // 0
        playlist.MoveNext(); // 1
        playlist.ToggleShuffle();

        playlist.Reset();

        Assert.Equal(-1, playlist.CurrentIndex);
        Assert.Equal(5, playlist.Files.Count);
        Assert.True(playlist.MoveNext());
        Assert.Equal(0, playlist.CurrentIndex);
    }
}
