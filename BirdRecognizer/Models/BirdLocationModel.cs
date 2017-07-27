using System;
using Newtonsoft.Json;

namespace BirdRecognizer
{
    public class BirdLocationModel
    {
		[JsonProperty(PropertyName = "Id")]
		public string ID { get; set; }

		[JsonProperty(PropertyName = "Longitude")]
        public float Longitude { get; set; }

		[JsonProperty(PropertyName = "Latitude")]
		public float Latitude { get; set; }

        [JsonProperty(PropertyName = "Bird")]
        public string Bird { get; set; }

        public string City { get; set; }
    }
}
