using System.Globalization;
using System.Text.RegularExpressions;

public class SrtSubtitleParser : ISubtitleParser
{
    private readonly IFileSystem _fileSystem;

    private static readonly Regex TimestampRegex = new Regex(
        @"(?<start>\d{2}:\d{2}:\d{2},\d{3})\s*-->\s*(?<end>\d{2}:\d{2}:\d{2},\d{3})",
        RegexOptions.Compiled);
    public SrtSubtitleParser(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    public SubtitleTrack Parse(string filePath)
    {
        var lines = _fileSystem.ReadAllText(filePath).Split('\n');
        var track = new SubtitleTrack(filePath);

        int i = 0;
        while (i < lines.Length)
        {
            while (i < lines.Length && string.IsNullOrWhiteSpace(lines[i]))
                i++;

            if (i >= lines.Length)
                break;

            i++;

            if (i >= lines.Length)
                break;

            var match = TimestampRegex.Match(lines[i]);
            if (!match.Success)
            {
                i++;
                continue;
            }

            var start = ParseTimestamp(match.Groups["start"].Value);
            var end = ParseTimestamp(match.Groups["end"].Value);

            i++;

            var textLines = new List<string>();
            while (i < lines.Length && !string.IsNullOrWhiteSpace(lines[i]))
            {
                textLines.Add(lines[i]);
                i++;
            }

            string text = string.Join("\n", textLines);
            track.AddSubtitle(new Subtitle(start, end, text));
        }

        return track;
    }

    private TimeSpan ParseTimestamp(string timestamp)
    {
        return TimeSpan.ParseExact(timestamp, @"hh\:mm\:ss\,fff", CultureInfo.InvariantCulture);
    }
}