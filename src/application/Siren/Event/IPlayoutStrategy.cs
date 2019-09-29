namespace PBS.Siren
{
    /*
    A playout strategy is a piece of logic and series of data that defines where in the layout
    of a Channel output the event should be played.

    Strategies could include Primary (Base) Video; the main visual data displayed to screen, it could
    relate to Graphics (what layer, position, and dimensions to use to display the event contents), it could
    relate to audio or subtitles potentially.
    */
    interface IPlayoutStrategy
    {
        
    }
}