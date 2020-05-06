using CBS.Siren.Time;
using System;
using System.Diagnostics.CodeAnalysis;

namespace CBS.Siren
{
    /*
    The Media Source strategy defines what Media will be played out for an event,
    from what point in the media and how much media is available to be played.

    The MediaSourceStrategy may relate to the whole Media Instance (the full duration) that it references
    or it may relate to a segment of that Media instance.
    */
    public class MediaSourceStrategy : ISourceStrategy
    {
        public MediaInstance Instance { get; }
        
        //SOM = Start of Media
        public TimeSpan SOM { get; set; }
        
        //EOM = EOM of Media
        public TimeSpan EOM { get; set; }

        public string StrategyType => "mediaSource";

        public MediaSourceStrategy(MediaInstance instance, TimeSpan som, TimeSpan eom)
        {
            Instance = instance;
            SOM = som;
            EOM = eom;
        }

        public MediaSourceStrategy(ISourceStrategy other)
        {
            if (other is MediaSourceStrategy mediaStrategy)
            {
                Instance = mediaStrategy.Instance;
                SOM = mediaStrategy.SOM;
                EOM = mediaStrategy.EOM;
                return;
            }

            throw new ArgumentException("Failed to construct source strategy. Given strategy was not the same type", "other");
        }


        public override string ToString()
        {
            return "MediaSourceStrategy:" +
            $"\nMedia Name:  {Instance.Name}" +
            $"\nSOM: {SOM.ToTimecodeString()}" +
            $"\nEOM: {EOM.ToTimecodeString()}";
        }

        public virtual bool Equals([AllowNull] ISourceStrategy other)
        {
            return other is MediaSourceStrategy sourceStrategy &&
                Instance.Equals(sourceStrategy.Instance) &&
                SOM == sourceStrategy.SOM &&
                EOM == sourceStrategy.EOM;
        }

        public TimeSpan GetDuration()
        {
            long durationFrames = Math.Min(Instance.Duration.TotalFrames(), EOM.TotalFrames()) - SOM.TotalFrames();
            return TimeSpanExtensions.FromFrames(durationFrames);
        }

        public object BuildStrategyData()
        {
            var strategyData = new
            {
                type = StrategyType,
                mediaInstance = new
                {
                    Instance.Name,
                    Duration = Instance.Duration.ToTimecodeString(),
                    Instance.FilePath,
                    Instance.InstanceFileType
                },
                som = SOM.ToTimecodeString(),
                eom = EOM.ToTimecodeString()
            };

            return strategyData;
        }


    }
}