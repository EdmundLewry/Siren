using System;

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
    public class MediaInstance
    {
        public int? Id { get; set; }
        public string Name { get; set; } 

        public TimeSpan Duration { get; set; }
        public string FilePath { get; set;  }
        public FileType InstanceFileType { get; set;  }

        public MediaInstance(string instanceName, TimeSpan duration, string instanceFilePath = "", FileType type = FileType.TEXT)
        {
            Id = null;
            Name = instanceName;
            Duration = duration;
            FilePath = instanceFilePath;
            InstanceFileType = type;
        }
    }
}