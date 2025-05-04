namespace ConsoleMediaPlayer.Tests;

using Xunit;

public class PlaylistTests
{
    [Fact]
    // GPT wrote this unit test scaffolding.
    // @TODO: Add more unit tests for TDD.
    public void MoveNext_WhenAtEnd_ReturnsFalse()
    {
        // Arrange
        var playlist = new Playlist();
        var files = new[] { new AudioFile("a.mp3", "a"), new AudioFile("b.mp3", "b") };
        playlist.LoadFiles(files);

        // Act
        playlist.MoveNext(); // Index 0 → 1
        bool result = playlist.MoveNext(); // Index 1 → 2 (out of bounds)

        // Assert
        Assert.False(result);
        Assert.Equal(1, playlist.CurrentIndex); // Stays at last valid index
    }
}