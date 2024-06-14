using CommandLine;
using MigrateYamlPipeline.Common;
using MigrateYamlPipeline.Migrate;

namespace MigrateYamlPipeline
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            // MigrateYamlPipeline.exe - y "C:\work_other\Test.yml" - c "C:\work_other\Test.json"
            await Parser.Default.ParseArguments<CommandOptions>(args).WithParsedAsync(ProcessCommandLines);
        }

        private static async Task ProcessCommandLines(CommandOptions command)
        {
            if (!command.YamlFilePath.EndsWith("yml", StringComparison.OrdinalIgnoreCase))
            {
                var yamlFiles = Directory.GetFiles(Path.GetDirectoryName(command.YamlFilePath), "*.yml", SearchOption.AllDirectories);
                foreach (var yamlFile in yamlFiles)
                {
                    MigrateTool batchMigrateTool = new MigrateTool(yamlFile, command.ClassicFilePath);
                    if (command.IsUpdateEnvironmentPermissions)
                    {
                        batchMigrateTool.AddMigrate(new UpdateEnvironmentPermissionsMigrate(new EnvironmentMigrateOptions()
                        {
                            Organization = command.Organization,
                            Project = command.Project,
                            PAT = command.PAT,
                            SourceEnvironment = command.CopyEnvironment
                        }));
                    }
                    await batchMigrateTool.Migrate(command.OutPutYamlFilePath);
                }
                return;
            }

            MigrateTool migrateTool = new MigrateTool(command.YamlFilePath, command.ClassicFilePath);

            if (command.IsMigrateVSTest)
            {
                migrateTool.AddMigrate(new VSTestMigrate(new VSTestMigrateOptions()
                {
                    TestPoolType = command.TestPoolType,
                    PipelineName = command.TestPipelineName,
                    PipelineArtifactName = command.TestArtifactName,
                    CustomPool = command.CustomPool,
                    ConnectedServiceName = command.ConnectedServiceName,
                    CloudTestTenant = command.CloudTestTenant,
                    ScheduleBuildRequesterAlias = command.ScheduleBuildRequesterAlias,
                    TestMapLocation = command.TestMapLocation
                }));
            }

            if (command.IsUpdateEnvironmentPermissions)
            {
                migrateTool.AddMigrate(new UpdateEnvironmentPermissionsMigrate(new EnvironmentMigrateOptions()
                {
                    Organization = command.Organization,
                    Project = command.Project,
                    PAT = command.PAT,
                    SourceEnvironment = command.CopyEnvironment
                }));
            }

            if (command.IsAddPreApproval)
            {
                migrateTool.AddMigrate(new AddPreApprovalMigrate(new AddPreApprovalMigrateOptions()
                {
                    Organization = command.Organization,
                    Project = command.Project,
                    PAT = command.PAT
                }));
            }

            migrateTool.AddMigrate<FixDependsOnMigrate>();
            migrateTool.AddMigrate<PathMigrate>();
            migrateTool.AddMigrate<BasicsMigrate>();

            await migrateTool.Migrate(command.OutPutYamlFilePath);
        }
    }
}
