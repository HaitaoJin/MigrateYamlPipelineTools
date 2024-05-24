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
    /// Represents a class for migrating paths in a YAML pipeline.
    /// </summary>
    public class PathMigrate : MigrateBase
    {
        public override Task Migrate()
        {
            rootNode.AllNodes.OfType<YamlScalarNode>().Where(p => p.Value.Contains("$(System.DefaultWorkingDirectory)")).ToList().ForEach(p =>
            {
                Console.WriteLine($"Update Path: {p.Value} -> {p.Value.Replace("$(System.DefaultWorkingDirectory)", "$(Pipeline.Workspace)")}");
                p.Value = p.Value.Replace("$(System.DefaultWorkingDirectory)", "$(Pipeline.Workspace)");
            });

            rootNode.AllNodes.OfType<YamlScalarNode>().Where(p => p.Value.Contains("$(RELEASE.ARTIFACTS")).ToList().ForEach(p =>
            {
                Console.WriteLine($"Update Path: {p.Value} -> {p.Value.Replace("$(RELEASE.ARTIFACTS", "$(resources.pipeline")}");
                p.Value = p.Value.Replace("$(RELEASE.ARTIFACTS", "$(resources.pipeline");
            });

            return Task.CompletedTask;
        }
    }
}
