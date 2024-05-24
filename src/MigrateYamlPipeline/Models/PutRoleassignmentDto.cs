using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MigrateYamlPipeline.Models
{
    public class PutRoleassignmentDto
    {
        [JsonProperty("userId")]
        public Guid UserId { get; set; }

        [JsonProperty("roleName")]
        public string RoleName { get; set; }
    }
}
