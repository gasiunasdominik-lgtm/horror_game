using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Vchod.EditorTools
{
    /// <summary>Stamps the git hash into Resources so bug reports from builds know their code version.</summary>
    public sealed class BuildInfoPreprocessor : IPreprocessBuildWithReport
    {
        public const string AssetPath = "Assets/_Project/Resources/BuildInfo.txt";

        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            var root = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            Directory.CreateDirectory(Path.GetDirectoryName(Path.Combine(root, AssetPath)));
            File.WriteAllText(Path.Combine(root, AssetPath), ReadGitHash(root));
            AssetDatabase.ImportAsset(AssetPath);
        }

        private static string ReadGitHash(string root)
        {
            try
            {
                var git = Path.Combine(root, ".git");
                var head = File.ReadAllText(Path.Combine(git, "HEAD")).Trim();
                if (!head.StartsWith("ref: ")) return head;
                var refPath = Path.Combine(git, head.Substring(5));
                return File.Exists(refPath) ? File.ReadAllText(refPath).Trim() : "unknown";
            }
            catch (IOException)
            {
                return "unknown";
            }
        }
    }
}
