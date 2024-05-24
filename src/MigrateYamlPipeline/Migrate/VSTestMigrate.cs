using Newtonsoft.Json;
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
    /// Class for migrating VSTest tasks in YAML pipeline.
    /// </summary>
    public class VSTestMigrate : MigrateBase
    {
        private VSTestMigrateOptions migrateOptions;
        private HashSet<string> cloudTestTestJob = new HashSet<string>();

        public VSTestMigrate(VSTestMigrateOptions migrateOptions)
        {
            this.migrateOptions = migrateOptions;
        }

        public override Task Migrate()
        {
            foreach (var classicStage in classicStages)
            {
                string classicStageName = classicStage["Name"].ToString();
                var stage = stages.FirstOrDefault(p => ((YamlScalarNode)p["displayName"]).Value == classicStageName);
                if (stage != null && classicStage["DeployPhases"].AsArray().Any(p => p["WorkflowTasks"].AsArray().Any(task => task["TaskId"].ToString() == "ef087383-ee5e-42c7-9a53-ab56c98420f9")))
                {
                    if (migrateOptions.TestPoolType == MigrateTestPoolType.CustomPool)
                    {
                        MigrateToCustomPool(stage, classicStage);
                    }
                    else
                    {
                        MigrateToCloudTest(stage, classicStage);
                    }
                }
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// Migrate to CloudTest
        /// </summary>
        /// <param name="stage">yaml stage</param>
        /// <param name="classicStage">classic stage</param>
        private void MigrateToCloudTest(YamlNode stage, JsonNode classicStage)
        {
            var jobsNode = new YamlSequenceNode();
            foreach (var classicJob in classicStage["DeployPhases"].AsArray())
            {
                int index = 1;
                string preJobName = "";
                foreach (var classicTask in classicJob["WorkflowTasks"].AsArray())
                {
                    if (classicTask["TaskId"].ToString() != "ef087383-ee5e-42c7-9a53-ab56c98420f9")
                    {
                        continue;
                    }

                    var jobName = classicTask["Name"].ToString().ToStageName() + "_" + index;
                    var newJob = new YamlMappingNode();
                    newJob.Add("job", jobName);
                    newJob.Add("displayName", classicTask["Name"].ToString());
                    newJob.Add("timeoutInMinutes", "180");
                    if (index > 1)
                    {
                        newJob.Add("dependsOn", preJobName);
                    }

                    var poolNode = new YamlMappingNode();
                    poolNode.Add("type", new YamlScalarNode("server"));
                    newJob.Add("pool", poolNode);

                    var newstepsNode = new YamlSequenceNode();

                    var taskNode = new YamlMappingNode();
                    taskNode.Add("task", new YamlScalarNode("CloudTestServerBuildTask@2"));

                    var inputsNode = new YamlMappingNode();

                    string assemblies = classicTask["Inputs"]["testAssemblyVer2"].ToString().Split("\\").Last();
                    string filtercriteria = classicTask["Inputs"]["testFiltercriteria"].ToString().Replace("&", "&amp;");
                    string settingsFile = Path.GetFileName(classicTask["Inputs"]["runSettingsFile"].ToString());
                    var tags = $"Assemblies={assemblies};Filtercriteria={filtercriteria};SettingsFile={settingsFile}";
                    inputsNode.Add("connectedServiceName", migrateOptions.ConnectedServiceName);
                    inputsNode.Add("cloudTestTenant", migrateOptions.CloudTestTenant);
                    inputsNode.Add("testMapLocation", migrateOptions.TestMapLocation);
                    inputsNode.Add("pipelineArtifactName", migrateOptions.PipelineArtifactName);
                    inputsNode.Add("pipelineArtifactBuildUrl", $"$(System.TaskDefinitionsUri)$(System.TeamProject)/_build/results?buildId=$(resources.pipeline.{migrateOptions.PipelineName}.runID)");
                    inputsNode.Add("buildDropArtifactName", "");
                    inputsNode.Add("scheduleBuildRequesterAlias", migrateOptions.ScheduleBuildRequesterAlias);
                    inputsNode.Add("tags", tags);
                    inputsNode.Add("displayName", migrateOptions.SessionName(classicJob, classicTask, migrateOptions));
                    inputsNode.Add("cacheEnabled", "false");
                    inputsNode.Add("sessionTimeout", "1440");
                    inputsNode.Add("parserProperties", "worker:VsTestVersion=V150;VstsTestResultAttachmentUploadBehavior=Always;");
                    inputsNode.Add("failFast", "false");
                    taskNode.Add("inputs", inputsNode);

                    newstepsNode.Add(taskNode);
                    newJob.Add("steps", newstepsNode);
                    jobsNode.Add(newJob);
                    index++;
                    preJobName = jobName;


                    cloudTestTestJob.Add($@"<TestJob Name=""{assemblies.Replace(".dll", "").Replace(".", "-")}_{settingsFile.Split(".").First()}"" Type=""SingleBox"" Tags=""{tags}"" TimeoutMins=""1000"">
          <Execution Type=""MsTest"" Path=""[WorkingDirectory]{classicTask["Inputs"]["searchFolder"].ToString().Split(migrateOptions.PipelineArtifactName).Last().Replace("/", "\\")}\{classicTask["Inputs"]["testAssemblyVer2"].ToString().Split("\\").Last()}"" Args=""/Settings:[WorkingDirectory]{classicTask["Inputs"]["runSettingsFile"].ToString().Split(migrateOptions.PipelineArtifactName).Last().Replace("/", "\\")} /Logger:trx /TestCaseFilter:{classicTask["Inputs"]["testFiltercriteria"].ToString().Replace("&", "&amp;")}"" />
        </TestJob>");
                    Console.WriteLine("add cloudtest job: " + jobName);
                }
            }

            if (jobsNode.Count() > 0)
            {
                ((YamlMappingNode)stage).Add("jobs", jobsNode);
                File.WriteAllText(Path.Combine(Path.GetDirectoryName(newYamlFile), "cloudtest.xml"), string.Join("\n", cloudTestTestJob));
            }
        }

        /// <summary>
        /// Migrate to Custom Pool
        /// </summary>
        /// <param name="stage">yaml stage</param>
        /// <param name="classicStage">classic stage</param>
        private void MigrateToCustomPool(YamlNode stage, JsonNode classicStage)
        {
            foreach (var classicJob in classicStage["DeployPhases"].AsArray())
            {
                var newJob = new YamlMappingNode();
                newJob.Add("job", classicJob["Name"].ToString().ToStageName());
                newJob.Add("displayName", classicJob["Name"].ToString());
                newJob.Add("timeoutInMinutes", "180");

                var poolNode = new YamlMappingNode();
                poolNode.Add("type", new YamlScalarNode("windows"));
                poolNode.Add("isCustom", new YamlScalarNode("true"));
                poolNode.Add("name", new YamlScalarNode(migrateOptions.CustomPool));
                newJob.Add("pool", poolNode);

                var newstepsNode = new YamlSequenceNode();

                var downloadNode = new YamlMappingNode();
                downloadNode.Add("download", new YamlScalarNode(migrateOptions.PipelineName));
                downloadNode.Add("artifact", new YamlScalarNode(migrateOptions.PipelineArtifactName));
                newstepsNode.Add(downloadNode);

                foreach (var classicTask in classicJob["WorkflowTasks"].AsArray())
                {
                    if (classicTask["TaskId"].ToString() != "ef087383-ee5e-42c7-9a53-ab56c98420f9")
                    {
                        continue;
                    }

                    var taskNode = new YamlMappingNode();
                    taskNode.Add("task", new YamlScalarNode("VSTest@3"));
                    taskNode.Add("displayName", classicTask["Name"].ToString());

                    var inputsNode = new YamlMappingNode();
                    inputsNode.Add("testSelector", new YamlScalarNode(classicTask["Inputs"]["testSelector"].ToString()));
                    inputsNode.Add("searchFolder", new YamlScalarNode(classicTask["Inputs"]["searchFolder"].ToString().ToYamlPath()));
                    inputsNode.Add("testFiltercriteria", new YamlScalarNode(classicTask["Inputs"]["testFiltercriteria"].ToString()));
                    inputsNode.Add("runSettingsFile", new YamlScalarNode(classicTask["Inputs"]["runSettingsFile"].ToString().ToYamlPath()));
                    inputsNode.Add("runInParallel", new YamlScalarNode(classicTask["Inputs"]["runInParallel"].ToString()));
                    inputsNode.Add("codeCoverageEnabled", new YamlScalarNode(classicTask["Inputs"]["codeCoverageEnabled"].ToString()));
                    inputsNode.Add("testAssemblyVer2", new YamlScalarNode(classicTask["Inputs"]["testAssemblyVer2"].ToString()));
                    taskNode.Add("inputs", inputsNode);

                    newstepsNode.Add(taskNode);

                    Console.WriteLine("add vstest tast: " + classicTask["Name"].ToString());
                }

                newJob.Add("steps", newstepsNode);

                var jobsNode = new YamlSequenceNode();
                jobsNode.Add(newJob);
                ((YamlMappingNode)stage).Add("jobs", jobsNode);
            }
        }
    }

    public class VSTestMigrateOptions
    {
        public MigrateTestPoolType TestPoolType { get; set; }
        public string PipelineName { get; set; }
        public string PipelineArtifactName { get; set; }

        #region CustomPool
        public string CustomPool { get; set; }
        #endregion

        #region CloudTest
        public string ConnectedServiceName { get; set; }
        public string CloudTestTenant { get; set; }
        public string TestMapLocation { get; set; }
        public string ScheduleBuildRequesterAlias { get; set; }

        public Func<JsonNode, JsonNode, VSTestMigrateOptions, string> SessionName = (classicJob, classicTask, options) => { return $"CloudTest_{classicTask["Name"].ToString().ToStageName()}_$(resources.pipeline.{options.PipelineName}.runName)"; };
        #endregion
    }

    public enum MigrateTestPoolType
    {
        CloudTest,
        CustomPool
    }
}
