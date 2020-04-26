using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;

namespace Racerr.Editor
{
    /// <summary>
    /// Used by the CI/CD pipeline to report the build status to the console and build the game.
    /// Pipeline will execute Unity from the command line and call BuildProject.
    /// Unity command line docs: https://docs.unity3d.com/Manual/CommandLineArguments.html.
    /// </summary>
    static class CICDPipeline
    {
        static readonly string EOL = Environment.NewLine;

        /// <summary>
        /// Custom parameters to configure the build. Parameters must start with a hyphen (-) and may be followed by a value (without hyphen). 
        /// Parameters without a value will be considered booleans (with a value of true).
        /// e.g. -profile SomeProfile -someBoolean -someValue exampleValue
        /// </summary>
        /// <param name="providedArguments">Key value pairs of arguments.</param>
        static void ParseCommandLineArguments(out Dictionary<string, string> providedArguments)
        {
            providedArguments = new Dictionary<string, string>();
            string[] args = Environment.GetCommandLineArgs();

            Console.WriteLine(
              $"{EOL}" +
              $"###########################{EOL}" +
              $"#    Parsing settings     #{EOL}" +
              $"###########################{EOL}" +
              $"{EOL}"
            );

            // Extract flags with optional values
            for (int current = 0, next = 1; current < args.Length; current++, next++)
            {
                // Parse flag
                bool isFlag = args[current].StartsWith("-");
                if (!isFlag) continue;
                string flag = args[current].TrimStart('-');

                // Parse optional value
                bool flagHasValue = next < args.Length && !args[next].StartsWith("-");
                string value = flagHasValue ? args[next].TrimStart('-') : "";

                // Assign
                Console.WriteLine($"Found flag \"{flag}\" with value \"{value}\".");
                providedArguments.Add(flag, value);
            }
        }

        /// <summary>
        /// Returns validated command line arguments. Validation ensures essential arguments
        /// are passed so the build does not fail.
        /// </summary>
        /// <returns>Updated command line arguments.</returns>
        static Dictionary<string, string> GetValidatedOptions()
        {
            ParseCommandLineArguments(out Dictionary<string, string> validatedOptions);

            if (!validatedOptions.TryGetValue("projectPath", out _))
            {
                Console.WriteLine("Missing argument -projectPath");
                EditorApplication.Exit(110);
            }

            if (!validatedOptions.TryGetValue("buildTarget", out string buildTarget))
            {
                Console.WriteLine("Missing argument -buildTarget");
                EditorApplication.Exit(120);
            }

            if (!Enum.IsDefined(typeof(BuildTarget), buildTarget))
            {
                EditorApplication.Exit(121);
            }

            if (!validatedOptions.TryGetValue("buildOptions", out string buildOptions) || !Enum.IsDefined(typeof(BuildOptions), buildOptions))
            {
                const string defaultBuildOptions = "None";
                Console.WriteLine($"Missing or invalid argument -buildOptions, defaulting to {defaultBuildOptions}.");
                validatedOptions.Add("buildOptions", defaultBuildOptions);
            }

            if (!validatedOptions.TryGetValue("customBuildPath", out _))
            {
                Console.WriteLine("Missing argument -customBuildPath");
                EditorApplication.Exit(130);
            }

            if (!validatedOptions.TryGetValue("customBuildName", out string customBuildName) || customBuildName == "")
            {
                const string defaultCustomBuildName = "TestBuild";
                Console.WriteLine($"Missing argument -customBuildName, defaulting to {defaultCustomBuildName}.");
                validatedOptions.Add("customBuildName", defaultCustomBuildName);
            }

            return validatedOptions;
        }

        /// <summary>
        /// Builds the project. Can only be executed by pipeline.
        /// </summary>
        static void BuildProject()
        {
            // Gather values from args
            Dictionary<string, string> options = GetValidatedOptions();

            // Gather values from project
            string[] scenes = EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(s => s.path).ToArray();

            // Define BuildPlayer Options
            BuildPlayerOptions buildOptions = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = options["customBuildPath"],
                target = (BuildTarget)Enum.Parse(typeof(BuildTarget), options["buildTarget"]),
                options = (BuildOptions)Enum.Parse(typeof(BuildOptions), options["buildOptions"])
            };

            // Perform build
            BuildReport buildReport = BuildPipeline.BuildPlayer(buildOptions);

            // Summary
            BuildSummary summary = buildReport.summary;
            ReportSummary(summary);

            // Result
            BuildResult result = summary.result;
            ExitWithResult(result);
        }

        /// <summary>
        /// Prints a summary of the build after it is done.
        /// </summary>
        /// <param name="summary">Build summary.</param>
        static void ReportSummary(BuildSummary summary)
        {
            Console.WriteLine(
              $"{EOL}" +
              $"###########################{EOL}" +
              $"#      Build results      #{EOL}" +
              $"###########################{EOL}" +
              $"{EOL}" +
              $"Duration: {summary.totalTime.ToString()}{EOL}" +
              $"Warnings: {summary.totalWarnings.ToString()}{EOL}" +
              $"Errors: {summary.totalErrors.ToString()}{EOL}" +
              $"Size: {summary.totalSize.ToString()} bytes{EOL}" +
              $"{EOL}"
            );
        }

        /// <summary>
        /// Prints the build result and exits with the correct code.
        /// </summary>
        /// <param name="result">Build result.</param>
        static void ExitWithResult(BuildResult result)
        {
            if (result == BuildResult.Succeeded)
            {
                Console.WriteLine("Build succeeded!");
                EditorApplication.Exit(0);
            }

            if (result == BuildResult.Failed)
            {
                Console.WriteLine("Build failed!");
                EditorApplication.Exit(101);
            }

            if (result == BuildResult.Cancelled)
            {
                Console.WriteLine("Build cancelled!");
                EditorApplication.Exit(102);
            }

            if (result == BuildResult.Unknown)
            {
                Console.WriteLine("Build result is unknown!");
                EditorApplication.Exit(103);
            }
        }
    }
}