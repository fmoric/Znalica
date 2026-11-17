using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using Znalica.Editor;

/// <summary>Batch build entry point named in AGENTS.md: BuildScript.Build.</summary>
public static class BuildScript
{
    private const string MainScene = "Assets/Znalica/Scenes/Main.unity";
    private const string OutputPath = "Builds/Windows/Znalica.exe";

    public static void Build()
    {
        // A broken Hr.json or a clip that is not an .ogg is logged by the builder but does not fail the build;
        // the EditMode completeness test is the guard.
        VoiceLibraryBuilder.Rebuild();

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
