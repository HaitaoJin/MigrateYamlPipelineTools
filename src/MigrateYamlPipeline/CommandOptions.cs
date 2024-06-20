using CommandLine;
using MigrateYamlPipeline.Migrate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MigrateYamlPipeline
{
    public class CommandOptions
    {
        [Option('y', "yaml", Required = true, HelpText = "Yaml Pipeline File Path")]
        public string YamlFilePath { get; set; }

        [Option('c', "classic", Required = true, HelpText = "Classic Pipeline File Path")]
        public string ClassicFilePath { get; set; }

        [Option('o', "output", Required = false, HelpText = "OutPut Yaml File Path [None/FilePath]")]
        public string OutPutYamlFilePath { get; set; }

        [Option("org", Required = false, HelpText = "Organization")]
        public string Organization { get; set; }
        [Option("project", Required = false, HelpText = "Organization")]
        public string Project { get; set; }
        [Option("pat", Required = false, HelpText = "Organization")]
        public string PAT { get; set; }

        [Option("addpreapproval", Required = false, HelpText = "Is Add Pre Approval", Default = false, Group = "Add Pre Approval Option")]
        public bool IsAddPreApproval { get; set; }

        #region Update Environment Permissions
        [Option("envpermissions", Required = false, HelpText = "Is Update Environment Permissions", Default = false, Group = "Update Environment Permissions Option")]
        public bool IsUpdateEnvironmentPermissions { get; set; }
        [Option("copyenv", Required = false, HelpText = "Copy Permissions Environment", Group = "Update Environment Permissions Option")]
        public string CopyEnvironment { get; set; }
        #endregion

        #region VSTest
        [Option("vstest", Required = false, HelpText = "Is Migrate VSTest", Default = false, Group = "VSTest Migrate Option")]
        public bool IsMigrateVSTest { get; set; }

        [Option("testpool", Required = false, HelpText = "Test Pool Type [CloudTest/CustomPool]", Default = MigrateTestPoolType.CloudTest, Group = "VSTest Migrate Option")]
        public MigrateTestPoolType TestPoolType { get; set; }

        [Option("testpipeline", Required = false, HelpText = "Test Pipeline Name", Default = "cdnrp-Buddy", Group = "VSTest Migrate Option")]
        public string TestPipelineName { get; set; }

        [Option("testartifact", Required = false, HelpText = "Test Artifact Name", Default = "drop_build_test", Group = "VSTest Migrate Option")]
        public string TestArtifactName { get; set; }

        [Option("custompool", Required = false, HelpText = "Custom Pool", Default = "cdnrp-agentpool", Group = "VSTest Migrate Option")]
        public string CustomPool { get; set; }

        [Option("cloudtest-connected", Required = false, HelpText = "Cloud Test Connected Service Name", Default = "CloudTest-Prod", Group = "VSTest Migrate Option")]
        public string ConnectedServiceName { get; set; }

        [Option("cloudtest-tenant", Required = false, HelpText = "Cloud Test Tenant", Default = "cdncloudtesttenant", Group = "VSTest Migrate Option")]
        public string CloudTestTenant { get; set; }

        [Option("cloudtest-alias", Required = false, HelpText = "Cloud Test Schedule Build Requester Alias", Default = "cdnrp", Group = "VSTest Migrate Option")]
        public string ScheduleBuildRequesterAlias { get; set; }

        [Option("cloudtest-maplocation", Required = false, HelpText = "Cloud Test Test Map Location", Default = @"[BuildRoot]\CloudTests\TestMap.E2E.xml", Group = "VSTest Migrate Option")]
        public string TestMapLocation { get; set; }
        #endregion
    }
}
