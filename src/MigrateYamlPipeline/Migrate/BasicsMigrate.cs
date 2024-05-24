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
    /// Class for migrating basic YAML pipeline.
    /// </summary>
    public class BasicsMigrate : MigrateBase
    {
        private List<string> notJobStages = new List<string>();

        public override Task Migrate()
        {
            var templateNode = (YamlScalarNode)rootNode.Children["extends"]["template"];
            if (templateNode.Value.Contains("NonOfficial") && stages.Any(p => ProdEnv.Contains(p.GetEnvironment())))
            {
                // Update Yaml Template
                Console.WriteLine("Update Yaml Template: v2/OneBranch.Official.CrossPlat.yml@templates");
                templateNode.Value = "v2/OneBranch.Official.CrossPlat.yml@templates";
            }

            foreach (var stageNode in stages)
            {
                // Update the name of the stage node
                var stage = (YamlScalarNode)stageNode["stage"];
                var stageName = stage.Value;
                var env = stageNode.GetEnvironment();
                Console.WriteLine(stageName);
                var envPrefix = GetStagePrefix(env);
                if (!stageName.StartsWith(envPrefix))
                {
                    stage.Value = envPrefix + stageName;

                    // Update the name of the dependsOn node
                    stages.Where(a => a.GetDependsOn().Contains(stageName)).ToList().ForEach(b =>
                    {
                        var dependsOns = b.GetDependsOn();
                        b.UpdateDependsOn(dependsOns.Select(p => { return p == stageName ? stage.Value : p; }).ToList());
                    });
                }

                // Add ob_release_environment variables
                if (((YamlMappingNode)stageNode).Children.ContainsKey(new YamlScalarNode("variables")))
                {
                    var variables = (YamlSequenceNode)stageNode["variables"];
                    if (!variables.Children.Any(p => p["name"].ToString() == "ob_release_environment"))
                    {
                        var environmentVariable = new YamlMappingNode();
                        environmentVariable.Add("name", "ob_release_environment");
                        environmentVariable.Add("value", env);
                        variables.Children.Add(environmentVariable);
                    }
                }

                // Handle jobs
                if (((YamlMappingNode)stageNode).Children.ContainsKey(new YamlScalarNode("jobs")))
                {
                    var jobs = stageNode["jobs"];
                    var stageJobs = (YamlSequenceNode)jobs;

                    foreach (var jobNode in stageJobs.Children)
                    {
                        var jobProperties = (YamlMappingNode)jobNode;

                        if (jobProperties.Children.ContainsKey(new YamlScalarNode("steps")))
                        {
                            var stepsNode = (YamlSequenceNode)jobProperties.Children[new YamlScalarNode("steps")];

                            // Handle multiple Ev2 tasks in a single job
                            {
                                // Get the count of vsrm-ev2.vss-services-ev2.adm-release-task.ExpressV2Internal@1 tasks
                                int taskCount = 0;
                                List<YamlNode> taskList = new List<YamlNode>();
                                foreach (var stepNode in stepsNode.Children)
                                {
                                    if (((YamlMappingNode)stepNode).Children.ContainsKey("task"))
                                    {
                                        var taskDisplayName = ((YamlMappingNode)stepNode)["task"].ToString();
                                        if (taskDisplayName.Contains("vsrm-ev2.vss-services-ev2.adm-release-task.ExpressV2Internal@1"))
                                        {
                                            taskCount++;
                                            taskList.Add(stepNode);
                                        }
                                    }
                                }

                                // If the task count is greater than or equal to 2, split it into separate jobs
                                if (taskCount >= 2)
                                {
                                    for (int i = 0; i < taskList.Count; i++)
                                    {
                                        // Create a new job node
                                        var newJob1 = new YamlMappingNode();
                                        newJob1.Add("job", "ev2_job_" + i);
                                        newJob1.Add("displayName", "ev2 job " + i);
                                        if (i > 0)
                                        {
                                            newJob1.Add("dependsOn", "ev2_job_" + (i - 1));
                                        }
                                        var newpoolNode1 = new YamlMappingNode();
                                        newpoolNode1.Add("type", new YamlScalarNode("release"));
                                        newJob1.Add("pool", newpoolNode1);
                                        var newstepsNode1 = new YamlSequenceNode();
                                        var newdownloadStep1 = new YamlMappingNode();
                                        newdownloadStep1.Add("download", new YamlScalarNode("cdnrp-Buddy"));
                                        newstepsNode1.Add(newdownloadStep1);
                                        newstepsNode1.Add(taskList[i]);
                                        newJob1.Add("steps", newstepsNode1);

                                        stageJobs.Children.Add(newJob1);
                                    }

                                    // Remove the original job
                                    stageJobs.Children.Remove(jobNode);

                                    break;
                                }
                            }

                            // Handle multiple E2E tasks in a single job
                            {
                                // Get the count of CloudTestServerBuildTask@1 tasks
                                int taskCount = 0;
                                List<YamlNode> taskList = new List<YamlNode>();
                                foreach (var stepNode in stepsNode.Children)
                                {
                                    if (((YamlMappingNode)stepNode).Children.ContainsKey("task"))
                                    {
                                        var taskDisplayName = ((YamlMappingNode)stepNode)["task"].ToString();

                                        //var e2eTaskInputs = (YamlMappingNode)stepNode["inputs"];
                                        //if (!e2eTaskInputs.Children.ContainsKey("parserProperties"))
                                        //{
                                        //    e2eTaskInputs.Add("parserProperties", "worker:VsTestVersion=V150;VstsTestResultAttachmentUploadBehavior=Always;");
                                        //}

                                        if (taskDisplayName.Contains("CloudTestServerBuildTask@1"))
                                        {
                                            taskCount++;
                                            taskList.Add(stepNode);
                                        }
                                    }
                                }

                                // If the task count is greater than or equal to 2, split it into separate jobs
                                if (taskCount >= 2)
                                {
                                    for (int i = 0; i < taskList.Count; i++)
                                    {
                                        // Create a new job node
                                        var newJob1 = new YamlMappingNode();
                                        newJob1.Add("job", "cloudtest_" + i);
                                        newJob1.Add("displayName", "cloudtest " + i);
                                        if (i > 0)
                                        {
                                            newJob1.Add("dependsOn", "cloudtest_" + (i - 1));
                                        }
                                        newJob1.Add("timeoutInMinutes", "180");
                                        var newpoolNode1 = new YamlMappingNode();
                                        newpoolNode1.Add("type", new YamlScalarNode("server"));
                                        newJob1.Add("pool", newpoolNode1);
                                        var newstepsNode1 = new YamlSequenceNode();
                                        var newdownloadStep1 = new YamlMappingNode();
                                        newstepsNode1.Add(taskList[i]);
                                        newJob1.Add("steps", newstepsNode1);

                                        stageJobs.Children.Add(newJob1);
                                    }

                                    // Remove the original job
                                    stageJobs.Children.Remove(jobNode);

                                    break;
                                }
                            }
                        }
                    }
                }
                else
                {
                    notJobStages.Add(stage.Value);
                }
            }
            Console.WriteLine("\nNot Job Stages: ");
            Console.WriteLine(string.Join("\n", notJobStages));
            return Task.CompletedTask;
        }
    }
}
