using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MigrateYamlPipeline.Models
{
    public class EnvironmentCheckDto
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("type")]
        public EnvironmentCheckTypeDto Type { get; set; }
        [JsonProperty("resource")]
        public ResourceDto Resource { get; set; }
    }

    public class EnvironmentCheckTypeDto
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("Name")]
        public string Name { get; set; }
    }
}
