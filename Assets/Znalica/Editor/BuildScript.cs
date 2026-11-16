using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;

/// <summary>Batch build entry point named in AGENTS.md: BuildScript.Build.</summary>
public static class BuildScript
{
    private const string MainScene = "Assets/Znalica/Scenes/Main.unity";
    private const string OutputPath = "Builds/Windows/Znalica.exe";

    public static void Build()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(OutputPath));

        var options = new BuildPlayerOptions
        {
            scenes = new[] { MainScene },
            locationPathName = OutputPath,
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None,
        };

        BuildSummary summary = BuildPipeline.BuildPlayer(options).summary;
        if (summary.result != BuildResult.Succeeded)
        {
            throw new Exception("Build failed: " + summary.result + ", " + summary.totalErrors + " error(s).");
        }
    }
}
