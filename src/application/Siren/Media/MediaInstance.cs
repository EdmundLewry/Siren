using System;

namespace PBS.Siren
{
    //Should this be some abstraction for video, graphics, audio, and subtitle file types?
    enum FileType
    {
        MOV,
        IMAGE_SEQUENCE
    }
    /*
    A Media Instance represents 1 or more files that relate to a single coherent
    piece of media that can be scheduled for playout in a list
     */
    class MediaInstance
    {
        public String Name { get; set; } 

        //This will need to become a timecode
        public int Duration { get; set; } //currently in number of frames (assuming 25FPS)
        public String FilePath { get; set; }
        public FileType InstanceFileType { get; set; }
        
    }
}