// Build Profile Scene 목록으로 Quest용 APK를 Builds/ 아래에 만드는 배치 빌드 진입점
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace GatePassVR.Editor
{
    public static class QuestBuild
    {
        const string OutputPath = "Builds/GatePassVR.apk";

        // 사용: Unity -batchmode -quit -buildTarget Android -executeMethod GatePassVR.Editor.QuestBuild.BuildAndroid
        public static void BuildAndroid()
        {
            var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = OutputPath,
                target = BuildTarget.Android,
                options = BuildOptions.None
            };

            Debug.Log($"[QuestBuild] scenes: {string.Join(", ", scenes)}");
            BuildReport report = BuildPipeline.BuildPlayer(options);
            Debug.Log($"[QuestBuild] result: {report.summary.result}, size: {report.summary.totalSize} bytes, errors: {report.summary.totalErrors}");

            if (Application.isBatchMode)
                EditorApplication.Exit(report.summary.result == BuildResult.Succeeded ? 0 : 1);
        }
    }
}
