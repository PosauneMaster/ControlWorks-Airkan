using System.Collections.Generic;

using Newtonsoft.Json;

namespace ControlWorks.Services.Rest.Models
{
    public class AlarmCollection
    {
        private readonly List<Alarm> _alarms;

        [JsonProperty(PropertyName = "alarms")]
        public Alarm[] Alarms => _alarms.ToArray();

        public AlarmCollection()
        {
            _alarms = new List<Alarm>();

        }

        public AlarmCollection(IEnumerable<Alarm> alarms)
        {
            _alarms = new List<Alarm>(alarms);
        }

        public void AddAlarm(Alarm alarm)
        {
            _alarms.Add(alarm);
        }
    }

    public class Alarm
    {
        [JsonProperty(PropertyName = "id")]
        public int Id { get; set; }
        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        public Alarm()
        {
        }

        public Alarm(string description)
        {
            Description = description;
        }
        public Alarm(int id, string description)
        {
            Id = id;
            Description = description;
        }
    }
}
