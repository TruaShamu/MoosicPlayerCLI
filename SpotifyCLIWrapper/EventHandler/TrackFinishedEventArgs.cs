public class TrackFinishedEventArgs : EventArgs
{
    public string FilePath { get; }
    
    public TrackFinishedEventArgs(string filePath)
    {
        FilePath = filePath;
    }
}