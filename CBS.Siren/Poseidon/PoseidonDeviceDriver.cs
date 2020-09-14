using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using CBS.Siren.Time;


namespace CBS.Siren.Device.Poseidon
{
    [Serializable]
    public class PoseidonTime
    {
        public int hour {get; set;}
        public int min {get; set;}
        public int sec {get; set;}
        public int frame {get; set;}
    }

    [Serializable]
    public class JobParameters
    {
        public String filename {get; set;}
        public int inPoint {get; set;}
        public int outPoint {get; set;}
    }

    [Serializable]
    public class SeqJobStart
    {
        public String type {get; set;}
        public PoseidonTime startTime {get; set;}
    }

    [Serializable]
    public class EventData
    {
        public int id {get; set;}
        [JsonPropertyName("startTime")]
        public SeqJobStart start {get; set;}
        public PoseidonTime stopTime {get; set;}

        [JsonPropertyName("params")]
        public JobParameters jobParameters {get; set;}
    }

    public class PoseidonDeviceDriver : IDeviceDriver
    {        
        private ILogger Logger { get; set; }
        private readonly IHttpClientFactory _clientFactory;
        public event EventHandler<DeviceListEvent> OnEventCued = delegate { };     
        public event EventHandler<DeviceListEvent> OnEventStarted = delegate { }; 
        public event EventHandler<DeviceListEvent> OnEventEnded = delegate { }; 

        public event EventHandler<DeviceListEvent> OnCueError = delegate { }; 
        public event EventHandler<DeviceListEvent> OnStartError = delegate { }; 
        public event EventHandler<DeviceListEvent> OnEndError = delegate { };

        public PoseidonDeviceDriver(ILogger logger, IHttpClientFactory httpFactory)
        {
            Logger = logger;
            _clientFactory = httpFactory;
        }

        public async Task CueEvent(DeviceListEvent Event)
        {
            EventData eData = new EventData();
            eData.id = Event.Id;
            eData.jobParameters = new JobParameters();
            eData.jobParameters.filename = JsonDocument.Parse(Event.EventData).RootElement.GetProperty("source").GetProperty("strategyData").GetProperty("mediaInstance").GetProperty("filePath").ToString();
            eData.jobParameters.inPoint = 0;
            eData.jobParameters.outPoint = 30 * 25; // TODO calculate duration/EOM;

            eData.start = new SeqJobStart();
            eData.start.type = new String("Fixed");
            var start = DateTimeExtensions.FromTimecodeString(JsonDocument.Parse(Event.EventData).RootElement.GetProperty("timing").GetProperty("startTime").GetString());
            eData.start.startTime = new PoseidonTime();
            eData.start.startTime.hour = start.Hour;
            eData.start.startTime.min = start.Minute;
            eData.start.startTime.sec = start.Second;
            eData.start.startTime.frame = 0;

            eData.stopTime = new PoseidonTime();
            var end =  DateTimeExtensions.FromTimecodeString(JsonDocument.Parse(Event.EventData).RootElement.GetProperty("timing").GetProperty("endTime").GetString());
            eData.stopTime.hour = end.Hour;
            eData.stopTime.min = end.Minute; 
            eData.stopTime.sec = end.Second;
            eData.stopTime.frame = 0;

            var json = new StringContent(JsonSerializer.Serialize(eData), Encoding.UTF8, "application/json");
            var hmm = await json.ReadAsStringAsync();
            var client = _clientFactory.CreateClient();
            Logger.LogInformation($"JSON: {hmm}");
            var request = await client.PostAsync("http://localhost:45687/JobProcessor", json);

            Logger.LogInformation($"Event Cued: {Event.ToString()}");
            Event.EventState.CurrentStatus = DeviceListEventState.Status.CUED;
            OnEventCued?.Invoke(this, Event);
        }

        public async Task StartEvent(DeviceListEvent Event)
        {
            await Task.Delay(5);
            Logger.LogInformation($"Event Started: {Event.ToString()}");
            Event.EventState.CurrentStatus = DeviceListEventState.Status.PLAYING;
            OnEventStarted?.Invoke(this, Event);
        }

        public async Task EndEvent(DeviceListEvent Event)
        {
            await Task.Delay(5);
            Logger.LogInformation($"Event Ended: {Event.ToString()}");
            Event.EventState.CurrentStatus = DeviceListEventState.Status.PLAYED;
            OnEventEnded?.Invoke(this, Event);
        }

         #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {

                }

                disposedValue = true;
            }
        }        

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}