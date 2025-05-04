namespace ConsoleMediaPlayer.Tests;
using System;
using System.IO;
using Xunit;

public class SrtSubtitleParserTests
{
    [Fact]
    public void Parse_ValidSrtFile_ReturnsCorrectSubtitles()
    {
        string srtContent = string.Join("\n", new[]
        {
            "1",
            "00:00:01,000 --> 00:00:03,000",
            "Hello, world!",
            "",
            "2",
            "00:00:04,000 --> 00:00:06,000",
            "This is a test.",
            "",
            "3",
            "00:00:07,500 --> 00:00:08,500",
            "Multiline",
            "subtitle text."
        });

        string tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, srtContent);

        var parser = new SrtSubtitleParser();

        var track = parser.Parse(tempFile);

        Assert.Equal(tempFile, track.FilePath);
        Assert.Equal(3, track.Subtitles.Count);

        Assert.Equal("Hello, world!", track.Subtitles[0].Text);
        Assert.Equal(TimeSpan.FromSeconds(1), track.Subtitles[0].StartTime);
        Assert.Equal(TimeSpan.FromSeconds(3), track.Subtitles[0].EndTime);

        Assert.Equal("This is a test.", track.Subtitles[1].Text);

        Assert.Equal("Multiline\nsubtitle text.", track.Subtitles[2].Text);

        File.Delete(tempFile);
    }
}
