using Microsoft.Cloud.MooncakeService.Common;
using MigrateYamlPipeline.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.RepresentationModel;

namespace MigrateYamlPipeline.Migrate
{
    public class UpdateEnvironmentPermissionsMigrate : MigrateBase
    {
        private readonly EnvironmentMigrateOptions migrateOptions;
        private readonly DevOpsHttpClient devOpsHttpClient;

        public UpdateEnvironmentPermissionsMigrate(EnvironmentMigrateOptions migrateOptions)
        {
            Requires.Argument("--org", migrateOptions.Organization).NotNullOrEmpty();
            Requires.Argument("--project", migrateOptions.Project).NotNullOrEmpty();
            Requires.Argument("--pat", migrateOptions.PAT).NotNullOrEmpty();
            Requires.Argument("--copyenv", migrateOptions.SourceEnvironment).NotNullOrEmpty();

            this.migrateOptions = migrateOptions;
            this.devOpsHttpClient = new DevOpsHttpClient(migrateOptions.Organization, migrateOptions.Project, migrateOptions.PAT);
        }

        public override async Task Migrate()
        {
            var sourceEnvironment = (await devOpsHttpClient.GetEnvironmentsAsync(migrateOptions.SourceEnvironment)).FirstOrDefault();
            foreach (var stageNode in stages)
            {
                // Update ob_deploymentjob_environment Role
                if (((YamlMappingNode)stageNode).Children.ContainsKey(new YamlScalarNode("variables")))
                {
                    var variables = (YamlSequenceNode)stageNode["variables"];
                    if (variables.Children.Any(p => p["name"].ToString() == "ob_deploymentjob_environment"))
                    {
                        var environmentJobName = variables.Children.First(p => p["name"].ToString() == "ob_deploymentjob_environment")["value"].ToString();
                        var targetEnvironment = devOpsHttpClient.GetEnvironmentsAsync(environmentJobName).Result.FirstOrDefault();
                        if (targetEnvironment != null)
                        {
                            await devOpsHttpClient.CopyRoleassignmentsAsync(sourceEnvironment, targetEnvironment);
                        }
                    }
                }
            }
        }
    }

    public class EnvironmentMigrateOptions
    {
        public string Organization { get; set; }

        public string Project { get; set; }

        public string PAT { get; set; }

        public string SourceEnvironment { get; set; }
    }
}
