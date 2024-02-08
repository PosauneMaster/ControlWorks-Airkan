using Newtonsoft.Json;

namespace ControlWorks.Services.Rest.Models
{
    public class HealthCheckItem
    {
        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }
        [JsonProperty(PropertyName = "active recipes")]
        public int ActiveRecipes { get; set; }
        [JsonProperty(PropertyName = "active alarms")]
        public int ActiveAlarms { get; set; }

        public HealthCheckItem()
        {
        }

        public HealthCheckItem(string status, int activeRecipes, int activeAlarms)
        {
            Status = status;
            ActiveRecipes = activeRecipes;
            ActiveAlarms = activeAlarms;
        }


    }
}
