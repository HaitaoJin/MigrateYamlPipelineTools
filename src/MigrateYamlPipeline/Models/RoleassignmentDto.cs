using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MigrateYamlPipeline.Models
{
    public class RoleassignmentDto
    {
        [JsonProperty("identity")]
        public IdentityDto Identity { get; set; }
        [JsonProperty("role")]
        public RoleDto Role { get; set; }
    }
}
