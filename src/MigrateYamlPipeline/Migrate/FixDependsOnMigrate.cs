using MigrateYamlPipeline.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YamlDotNet.RepresentationModel;

namespace MigrateYamlPipeline.Migrate
{

    /// <summary>
    /// Class for fixing the dependencies in the YAML pipeline.
    /// </summary>
    public class FixDependsOnMigrate : MigrateBase
    {
        private List<string> AllStages = new List<string>();

        public override Task Migrate()
        {
            // Get all stage names
            AllStages = stages.Select(p => ((YamlScalarNode)p["stage"]).Value).ToList();

            foreach (var stageNode in stages)
            {
                var stage = (YamlScalarNode)stageNode["stage"];
                var stageName = stage.Value;
                var dependsOns = stageNode.GetDependsOn();
                var isUpdate = false;

                // If the dependent stage does not exist, find it in the classic pipeline
                if (dependsOns.Any(p => !AllStages.Contains(p)))
                {
                    List<string> newDependsOns = new List<string>();
                    var displayName = ((YamlScalarNode)stageNode["displayName"]).Value;
                    var conditions = classicStages.FirstOrDefault(p => p["Name"].ToString() == displayName)["Conditions"].AsArray();
                    conditions.ToList().ForEach(p =>
                    {
                        if (p["ConditionType"].ToString() == "2")
                        {
                            var condition = p["Name"].ToString();
                            var yamlStage = stages.FirstOrDefault(p => ((YamlScalarNode)p["displayName"]).Value == condition);
                            if (yamlStage != null)
                            {
                                newDependsOns.Add(((YamlScalarNode)yamlStage["stage"]).Value);
                            }
                        }
                    });

                    // Update the dependencies
                    stageNode.UpdateDependsOn(newDependsOns);
                }
            }
            return Task.CompletedTask;
        }
    }
}
