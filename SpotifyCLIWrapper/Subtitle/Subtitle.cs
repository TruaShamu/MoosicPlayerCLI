public class Subtitle
{
    public TimeSpan StartTime { get; }
    public TimeSpan EndTime { get; }
    public string Text { get; }
    
    public Subtitle(TimeSpan startTime, TimeSpan endTime, string text)
    {
        StartTime = startTime;
        EndTime = endTime;
        Text = text ?? string.Empty;
    }
    
    public bool IsActiveAt(TimeSpan position)
    {
        return position >= StartTime && position <= EndTime;
    }
}