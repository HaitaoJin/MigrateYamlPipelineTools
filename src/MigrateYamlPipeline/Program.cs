using CommandLine;
using MigrateYamlPipeline.Common;
using MigrateYamlPipeline.Migrate;

namespace MigrateYamlPipeline
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // MigrateYamlPipeline.exe -y "C:\work_other\Test.yml" -c "C:\work_other\Test.json"
            Parser.Default.ParseArguments<CommandOptions>(args).WithParsed(ProcessCommandLines);
        }

        private static void ProcessCommandLines(CommandOptions command)
        {
            MigrateTool migrateTool = new MigrateTool(command.YamlFilePath, command.ClassicFilePath);
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

            migrateTool.AddMigrate<FixDependsOnMigrate>();
            migrateTool.AddMigrate<PathMigrate>();
            migrateTool.AddMigrate<BasicsMigrate>();

            migrateTool.Migrate(command.OutPutYamlFilePath);
        }
    }
}
