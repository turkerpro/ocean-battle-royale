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

        [MenuItem("Build/Android")]
        public static void BuildAndroid()
        {
            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = SCENES,
                locationPathName = "Build/OceanBattleRoyale.aab",
                target = BuildTarget.Android,
                options = BuildOptions.None
            };

            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel21;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel33;
            PlayerSettings.applicationIdentifier = "com.oceanbattleroyale.game";
            PlayerSettings.productName = "Ocean Battle Royale";

            EditorUserBuildSettings.buildAppBundle = true;
            EditorUserBuildSettings.androidBuildSubtarget = MobileTextureSubtarget.ASTC;

            QualitySettings.SetQualityLevel(2, true);
            GraphicsSettings.defaultRenderPipeline = null;

            BuildReport report = BuildPipeline.BuildPlayer(options);
            LogBuildResult(report);
        }

        [MenuItem("Build/WebGL")]
        public static void BuildWebGL()
        {
            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = SCENES,
                locationPathName = "Build/WebGL",
                target = BuildTarget.WebGL,
                options = BuildOptions.None
            };

            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
            PlayerSettings.WebGL.nameFilesAsHashes = true;
            PlayerSettings.WebGL.dataCaching = true;
            PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.None;
            PlayerSettings.stripEngineCode = true;
            PlayerSettings.SetManagedStrippingLevel(BuildTargetGroup.WebGL, ManagedStrippingLevel.High);

            QualitySettings.SetQualityLevel(1, true);

            BuildReport report = BuildPipeline.BuildPlayer(options);
            LogBuildResult(report);
        }

        [MenuItem("Build/Windows")]
        public static void BuildWindows()
        {
            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = SCENES,
                locationPathName = "Build/Windows/OceanBattleRoyale.exe",
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.IL2CPP);
            PlayerSettings.stripEngineCode = true;
            PlayerSettings.SetManagedStrippingLevel(BuildTargetGroup.Standalone, ManagedStrippingLevel.High);

            QualitySettings.SetQualityLevel(3, true);

            BuildReport report = BuildPipeline.BuildPlayer(options);
            LogBuildResult(report);
        }

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
                        {
                            Debug.LogError($"[{step.name}] {message.content}");
                        }
                    }
                }
                throw new System.Exception($"Build failed: {report.summary.result}");
            }
        }
    }
}
