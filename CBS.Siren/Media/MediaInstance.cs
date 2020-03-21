namespace CBS.Siren
{
    //Should this be some abstraction for video, graphics, audio, and subtitle file types?
    public enum FileType
    {
        TEXT,
        MOV,
        IMAGE_SEQUENCE
    }
    /*
    A Media Instance represents 1 or more files that relate to a single coherent
    piece of media that can be scheduled for playout in a list
     */
    public struct MediaInstance
    {
        public string Name { get; } 

        //This will need to become a timecode
        public int Duration { get; } //currently in number of frames (assuming 25FPS)
        public string FilePath { get; }
        public FileType InstanceFileType { get; }

        public MediaInstance(string instanceName = "", int totalDurationInFrames = 0, string instanceFilePath = "", FileType type = FileType.TEXT)
        {
            Name = instanceName;
            Duration = totalDurationInFrames;
            FilePath = instanceFilePath;
            InstanceFileType = type;
        }        
    }
}