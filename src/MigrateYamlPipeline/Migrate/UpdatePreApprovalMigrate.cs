﻿using Microsoft.Cloud.MooncakeService.Common;
using MigrateYamlPipeline.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.RepresentationModel;

namespace MigrateYamlPipeline.Migrate
{
    public class UpdatePreApprovalMigrate : MigrateBase
    {
        private readonly UpdatePreApprovalMigrateOptions migrateOptions;
        private readonly DevOpsHttpClient devOpsHttpClient;

        public UpdatePreApprovalMigrate(UpdatePreApprovalMigrateOptions migrateOptions)
        {
            Requires.Argument("--org", migrateOptions.Organization).NotNullOrEmpty();
            Requires.Argument("--project", migrateOptions.Project).NotNullOrEmpty();
            Requires.Argument("--pat", migrateOptions.PAT).NotNullOrEmpty();

            this.migrateOptions = migrateOptions;
            this.devOpsHttpClient = new DevOpsHttpClient(migrateOptions.Organization, migrateOptions.Project, migrateOptions.PAT);
        }


        public override async Task Migrate()
        {
            foreach (var stageNode in stages)
            {
                // Env Update Pre Approval
                if (((YamlMappingNode)stageNode).Children.ContainsKey(new YamlScalarNode("variables")))
                {
                    var variables = stageNode["variables"];
                    var environmentJobName = variables.GetVariableValue("ob_deploymentjob_environment");

                    if (!string.IsNullOrWhiteSpace(environmentJobName))
                    {
                        var environment = devOpsHttpClient.GetEnvironmentsAsync(environmentJobName).Result.FirstOrDefault();
                        if (environment != null)
                        {
                            var approvalChecks = await devOpsHttpClient.GetApprovalCheckAsync(environment);
                            if (approvalChecks.Any(p => p.Type.Name == "Approval"))
                            {
                                var approvalCheck = approvalChecks.FirstOrDefault(p => p.Type.Name == "Approval");
                                await devOpsHttpClient.UpdatePreApprovalAsync(approvalCheck.Resource.Id, approvalCheck.Id, migrateOptions.RequesterCannotBeApprover);
                            }
                        }
                    }
                }
            }
        }
    }

    public class UpdatePreApprovalMigrateOptions
    {
        public string Organization { get; set; }

        public string Project { get; set; }

        public string PAT { get; set; }

        public bool RequesterCannotBeApprover { get; set; }
    }
}
