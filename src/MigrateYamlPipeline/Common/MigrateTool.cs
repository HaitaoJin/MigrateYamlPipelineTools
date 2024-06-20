using MigrateYamlPipeline.Migrate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using YamlDotNet.RepresentationModel;

namespace MigrateYamlPipeline.Common
{
    public class MigrateTool
    {
        private List<MigrateBase> migrate = new List<MigrateBase>();
        private YamlStream yaml;
        private YamlMappingNode rootNode;
        private JsonNode classicPipeline;
        private string newYamlFile;

        /// <summary>
        /// Initializes a new instance of the <see cref="MigrateTool"/> class.
        /// </summary>
        /// <param name="yamlFilePath">The path of the YAML file to migrate.</param>
        /// <param name="classicFilePath">The path of the classic pipeline file.</param>
        public MigrateTool(string yamlFilePath, string? classicFilePath = null)
        {
            var reader = new StreamReader(yamlFilePath);
            yaml = new YamlStream();
            yaml.Load(reader);
            rootNode = (YamlMappingNode)yaml.Documents[0].RootNode;

            if (!string.IsNullOrWhiteSpace(classicFilePath))
            {
                classicPipeline = JsonNode.Parse(File.ReadAllText(classicFilePath));
            }

            newYamlFile = yamlFilePath.Replace(".yml", $"_new{DateTime.Now.ToString("MMddHHmmss")}.yml");
        }

        /// <summary>
        /// Adds a migration instance to the list of migrations.
        /// </summary>
        /// <param name="migrate">The migration instance to add.</param>
        public void AddMigrate(MigrateBase migrate)
        {
            this.migrate.Add(migrate);
        }

        /// <summary>
        /// Adds a new instance of a migration type to the list of migrations.
        /// </summary>
        /// <typeparam name="T">The type of the migration to add.</typeparam>
        public void AddMigrate<T>()
            where T : MigrateBase, new()
        {
            this.migrate.Add(new T());
        }

        /// <summary>
        /// Migrates the YAML file to the new format.
        /// </summary>
        /// <param name="newYamlFilePath">The path of the new YAML file to create.</param>
        public async Task Migrate(string newYamlFilePath)
        {
            if (!string.IsNullOrWhiteSpace(newYamlFilePath))
            {
                newYamlFile = newYamlFilePath;
            }

            foreach (var m in migrate)
            {
                m.Load(rootNode, classicPipeline, newYamlFile);

                Console.WriteLine($"========= Begin Migrate: {m.GetType().Name} =========");
                await m.Migrate();
                Console.WriteLine($"========= End Migrate: {m.GetType().Name} =========\n");
            }

            if (!string.Equals(newYamlFile, "None", StringComparison.OrdinalIgnoreCase))
            {
                using (var writer = new StreamWriter(newYamlFile))
                {
                    yaml.Save(writer, assignAnchors: false);
                }
            }
        }

        /// <summary>
        /// Migrates the YAML file to the new format using the default new YAML file path.
        /// </summary>
        public void Migrate()
        {
            Migrate(newYamlFile);
        }

    }
}
