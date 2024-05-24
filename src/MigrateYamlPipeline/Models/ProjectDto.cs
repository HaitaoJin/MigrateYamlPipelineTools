using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MigrateYamlPipeline.Models
{
    public class ProjectDto
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }
    }
}
