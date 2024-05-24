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
    /// Base class for pipeline migration.
    /// </summary>
    public abstract class MigrateBase
    {
        protected YamlMappingNode rootNode;
        protected List<YamlNode> stages;
        protected JsonNode classicPipeline;
        protected JsonArray classicStages;
        protected string newYamlFile;

        /// <summary>
        /// Loads the necessary data for migration.
        /// </summary>
        /// <param name="rootNode">The root node of the YAML file.</param>
        /// <param name="classicPipeline">The classic pipeline in JSON format.</param>
        /// <param name="newYamlFile">The path of the new YAML file.</param>
        public void Load(YamlMappingNode rootNode, JsonNode classicPipeline, string newYamlFile)
        {
            this.classicPipeline = classicPipeline;
            this.rootNode = rootNode;
            this.newYamlFile = newYamlFile;
            stages = ((YamlSequenceNode)rootNode.Children["extends"]["parameters"]["stages"]).Children.ToList();
            classicStages = classicPipeline["ReleaseDefinition"]["Environments"].AsArray();
        }

        /// <summary>
        /// Gets the stage prefix based on the environment.
        /// </summary>
        /// <param name="env">The environment name.</param>
        /// <returns>The stage prefix.</returns>
        protected string GetStagePrefix(string env)
        {
            switch (env)
            {
                case "Test":
                    return "Test_";
                case "PPE":
                    return "PPE_";
                case "Production":
                    return "Prod_";
                case "Mooncake":
                    return "MC_";
                case "Fairfax":
                    return "FF_";
                case "USNat":
                    return "USNat_";
                case "USSec":
                    return "USSec_";
                default:
                    return "Test_";
            }
        }

        protected List<string> AllStagePrefix = new List<string> { "Test_", "PPE_", "Prod_", "MC_", "USNat_", "USSec_", "FF_" };

        protected List<string> ProdEnv = new List<string> { "Production", "Mooncake", "USNat", "USSec", "Fairfax" };

        protected List<string> TestEnv = new List<string> { "Test", "PPE" };

        public abstract Task Migrate();
    }
}
