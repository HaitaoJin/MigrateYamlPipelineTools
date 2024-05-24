using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MigrateYamlPipeline.Models
{
    public class ApprovalCheckDto
    {
        [JsonProperty("resource")]
        public ResourceDto Resource { get; set; }

        [JsonProperty("settings")]
        public ApprovalCheckSettingsDto Settings { get; set; }

        [JsonProperty("type")]
        public EnvironmentCheckTypeDto Type { get; set; }

        [JsonProperty("timeout")]
        public int Timeout { get; set; } = 43200;
    }

    public class ApprovalCheckSettingsDto 
    {
        [JsonProperty("approvers")]
        public List<ResourceDto> Approvers { get; set; }

        [JsonProperty("definitionRef")]
        public ResourceDto DefinitionRef { get; set; }

        [JsonProperty("blockedApprovers")]
        public List<ResourceDto> BlockedApprovers { get; set; }

        [JsonProperty("executionOrder")]
        public int ExecutionOrder { get; set; }

        [JsonProperty("instructions")]
        public string Instructions { get; set; }

        [JsonProperty("minRequiredApprovers")]
        public int MinRequiredApprovers { get; set; }

        [JsonProperty("requesterCannotBeApprover")]
        public bool RequesterCannotBeApprover { get; set; }
    }
}
