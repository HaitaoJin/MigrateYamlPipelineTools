using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MigrateYamlPipeline.Models
{
    public class IdentityDto
    {
        [JsonProperty("displayName")]
        public string DisplayName { get; set; }
        [JsonProperty("uniqueName")]
        public string UniqueName { get; set; }
        [JsonProperty("id")]
        public Guid Id { get; set; } 
    }
}
