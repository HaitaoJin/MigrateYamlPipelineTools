using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.RepresentationModel;

namespace MigrateYamlPipeline.Common
{
    public static class MigrateExtensions
    {
        /// <summary>
        /// Replaces the "$(System.DefaultWorkingDirectory)" with "$(Pipeline.Workspace)" in the given string.
        /// </summary>
        /// <param name="str">The string to modify.</param>
        /// <returns>The modified string.</returns>
        public static string ToYamlPath(this string str)
        {
            return str.Replace("$(System.DefaultWorkingDirectory)", "$(Pipeline.Workspace)");
        }

        /// <summary>
        /// Replaces spaces and dashes in the given string with underscores.
        /// </summary>
        /// <param name="str">The string to modify.</param>
        /// <returns>The modified string.</returns>
        public static string ToStageName(this string str)
        {
            return str.Replace(" ", "_").Replace("-", "_");
        }

        /// <summary>
        /// Updates the "dependsOn" property of the YamlNode with the given stage names.
        /// </summary>
        /// <param name="stage">The YamlNode representing the stage.</param>
        /// <param name="stageNames">The list of stage names to set as dependencies.</param>
        public static void UpdateDependsOn(this YamlNode stage, List<string> stageNames)
        {
            if (((YamlMappingNode)stage).Children.ContainsKey(new YamlScalarNode("dependsOn")))
            {
                ((YamlMappingNode)stage).Children.Remove("dependsOn");
            }

            ((YamlMappingNode)stage).Children.Add("dependsOn", new YamlSequenceNode(stageNames.Select(p => new YamlScalarNode(p))));
        }

        /// <summary>
        /// Retrieves the list of dependencies from the "dependsOn" property of the YamlNode.
        /// </summary>
        /// <param name="stage">The YamlNode representing the stage.</param>
        /// <returns>The list of dependencies.</returns>
        public static List<string> GetDependsOn(this YamlNode stage)
        {
            List<string> dependsOnList = new List<string>();
            if (((YamlMappingNode)stage).Children.ContainsKey(new YamlScalarNode("dependsOn")))
            {
                var dependsOn = stage["dependsOn"];
                // Check the type of the dependsOn node
                if (dependsOn.NodeType == YamlNodeType.Scalar)
                {
                    // If it is a string type
                    var dependsOnNode = ((YamlScalarNode)dependsOn);
                    dependsOnList.Add(dependsOnNode.Value);
                }
                else if (dependsOn.NodeType == YamlNodeType.Sequence)
                {
                    // If it is a string array type
                    foreach (var item in ((YamlSequenceNode)dependsOn).Children)
                    {
                        var dependsOnNode = ((YamlScalarNode)item);
                        dependsOnList.Add(dependsOnNode.Value);
                    }
                }
            }
            return dependsOnList;
        }

        /// <summary>
        /// Retrieves the environment from the YamlNode.
        /// </summary>
        /// <param name="stage">The YamlNode representing the stage.</param>
        /// <returns>The environment.</returns>
        public static string GetEnvironment(this YamlNode stage)
        {
            // Get from variables
            if (((YamlMappingNode)stage).Children.ContainsKey(new YamlScalarNode("variables")))
            {
                var variables = stage["variables"];
                if (variables.NodeType == YamlNodeType.Sequence)
                {
                    var variablesNode = (YamlSequenceNode)stage["variables"];
                    if (variablesNode.Children.Any(p => p["name"].ToString() == "ob_release_environment"))
                    {
                        return variablesNode.Children.First(p => p["name"].ToString() == "ob_release_environment")["value"].ToString();
                    }
                }
                else if (variables.NodeType == YamlNodeType.Mapping)
                {
                    var variablesNode = (YamlMappingNode)stage["variables"];
                    if (variablesNode.Children.ContainsKey(new YamlScalarNode("ob_release_environment")))
                    {
                        return variablesNode[new YamlScalarNode("ob_release_environment")].ToString();
                    }
                }
            }

            // Get from ev2 test task
            if (((YamlMappingNode)stage).Children.ContainsKey(new YamlScalarNode("jobs")))
            {
                var jobs = stage["jobs"];
                var stageJobs = ((YamlSequenceNode)jobs);

                foreach (var jobNode in stageJobs.Children)
                {
                    var jobProperties = (YamlMappingNode)jobNode;

                    if (jobProperties.Children.ContainsKey(new YamlScalarNode("steps")))
                    {
                        var stepsNode = (YamlSequenceNode)jobProperties.Children[new YamlScalarNode("steps")];

                        foreach (var stepNode in stepsNode.Children)
                        {
                            if (((YamlMappingNode)stepNode).Children.ContainsKey("task"))
                            {
                                var taskDisplayName = ((YamlMappingNode)stepNode)["task"].ToString();
                                if (taskDisplayName.Contains("vsrm-ev2.vss-services-ev2.adm-release-task.ExpressV2Internal@1"))
                                {
                                    var taskInputs = (YamlMappingNode)stepNode["inputs"];
                                    if (taskInputs.Children.ContainsKey(new YamlScalarNode("ConnectedServiceName")))
                                    {
                                        return "Test";
                                    }
                                    if (taskInputs.Children.ContainsKey(new YamlScalarNode("ApprovalServiceEnvironment")))
                                    {
                                        return ((YamlScalarNode)taskInputs.Children["ApprovalServiceEnvironment"]).Value;
                                    }
                                }
                            }
                        }
                    }
                }

            }

            return "Test";
        }

        /// <summary>
        /// Retrieves the variableValue from the variables YamlNode.
        /// </summary>
        /// <param name="stage">The YamlNode representing the stage.</param>
        /// <returns>The environment.</returns>
        public static string GetVariableValue(this YamlNode variables, string variableName)
        {
            if (variables.NodeType == YamlNodeType.Sequence)
            {
                var variablesNode = (YamlSequenceNode)variables;
                if (variablesNode.Children.Any(p => p["name"].ToString() == variableName))
                {
                    return variablesNode.Children.First(p => p["name"].ToString() == variableName)["value"].ToString();

                }
            }
            else if (variables.NodeType == YamlNodeType.Mapping)
            {
                var variablesNode = (YamlMappingNode)variables;
                if (variablesNode.Children.ContainsKey(new YamlScalarNode(variableName)))
                {
                    return variablesNode[new YamlScalarNode(variableName)].ToString();
                }
            }

            return string.Empty;
        }

        public static bool StageDisplayNameEquals(this string classicName, string yamlName)
        {
            if (string.Equals(classicName, yamlName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (string.Equals(classicName.Replace(@"""", "").ToYamlPath(), yamlName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }
    }
}
