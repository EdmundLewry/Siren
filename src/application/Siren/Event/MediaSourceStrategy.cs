namespace PBS.Siren
{
    /*
    The Media Source strategy defines what Media will be played out for an event,
    from what point in the media and how much media is available to be played.

    The MediaSourceStrategy may relate to the whole Media Instance (the full duration) that it references
    or it may relate to a segment of that Media instance.
    */
    class MediaSourceStrategy : ISourceStrategy
    {
        public MediaInstance Instance { get; set; }
        
        //Will need a timecode for this. Currently in frames
        //SOM = Start of Media
        public int SOM { get; set; } //Currently in 25 FPS
        
        //Will need a timecode for this. Currently in frames
        //EOM = EOM of Media
        public int EOM { get; set; } //Currently in 25 FPS
        
        public MediaSourceStrategy()
        {

        }
    }
}