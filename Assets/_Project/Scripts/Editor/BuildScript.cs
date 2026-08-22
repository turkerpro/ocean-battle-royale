using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace OceanBattleRoyale.Editor
{
    public static class BuildScript
    {
        private const string SCENES_PATH = "Assets/_Project/Scenes/";
        private static readonly string[] SCENES = new[]
        {
            SCENES_PATH + "MainMenu.unity",
            SCENES_PATH + "Prototype.unity"
        };

        private static void LogBuildResult(BuildReport report)
        {
            if (report.summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"Build succeeded: {report.summary.outputPath} ({report.summary.totalSize} bytes)");
            }
            else
            {
                Debug.LogError($"Build failed: {report.summary.result}");
                foreach (var step in report.steps)
                {
                    foreach (var message in step.messages)
                    {
                        if (message.type == LogType.Error)
                            Debug.LogError($"[{step.name}] {message.content}");
                    }
                }
                throw new System.Exception($"Build failed: {report.summary.result}");
            }
        }

        [MenuItem("Build/WebGL")]
        public static void BuildWebGL()
        {
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
            PlayerSettings.WebGL.nameFilesAsHashes = true;
            PlayerSettings.WebGL.dataCaching = true;
            PlayerSettings.WebGL.debugSymbolMode = WebGLDebugSymbolMode.Off;
            PlayerSettings.stripEngineCode = true;
            PlayerSettings.SetManagedStrippingLevel(BuildTargetGroup.WebGL, ManagedStrippingLevel.Low);
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.WebGL, ScriptingImplementation.IL2CPP);

            QualitySettings.SetQualityLevel(1, true);

            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = SCENES,
                locationPathName = "build/WebGL",
                target = BuildTarget.WebGL,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            LogBuildResult(report);
        }

        [MenuItem("Build/Android")]
        public static void BuildAndroid()
        {
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel22;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel34;
            PlayerSettings.applicationIdentifier = "com.oceanbattleroyale.game";
            PlayerSettings.productName = "Ocean Battle Royale";
            PlayerSettings.stripEngineCode = true;
            PlayerSettings.SetManagedStrippingLevel(BuildTargetGroup.Android, ManagedStrippingLevel.Low);

            EditorUserBuildSettings.buildAppBundle = true;

            QualitySettings.SetQualityLevel(2, true);

            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = SCENES,
                locationPathName = "build/OceanBattleRoyale.aab",
                target = BuildTarget.Android,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            LogBuildResult(report);
        }

        [MenuItem("Build/Windows")]
        public static void BuildWindows()
        {
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.IL2CPP);
            PlayerSettings.stripEngineCode = true;
            PlayerSettings.SetManagedStrippingLevel(BuildTargetGroup.Standalone, ManagedStrippingLevel.Low);

            QualitySettings.SetQualityLevel(3, true);

            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = SCENES,
                locationPathName = "build/Windows/OceanBattleRoyale.exe",
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            LogBuildResult(report);
        }
    }
}
