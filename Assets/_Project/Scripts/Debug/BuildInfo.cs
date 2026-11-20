using System;
using System.IO;
using UnityEngine;

namespace Vchod.DebugTools
{
    /// <summary>
    /// Git hash of the running code. Editor reads .git directly; a player build reads
    /// the Resources/BuildInfo text that the build preprocessor writes.
    /// </summary>
    public static class BuildInfo
    {
        private static string _gitHash;

        public static string GitHash => _gitHash ??= ResolveGitHash();

        private static string ResolveGitHash()
        {
#if UNITY_EDITOR
            return ReadGitHash(Path.GetFullPath(Path.Combine(Application.dataPath, "..")));
#else
            var asset = Resources.Load<TextAsset>("BuildInfo");
            return asset != null ? asset.text.Trim() : "unknown";
#endif
        }

        public static string ReadGitHash(string repoRoot)
        {
            try
            {
                var gitDir = Path.Combine(repoRoot, ".git");
                var head = File.ReadAllText(Path.Combine(gitDir, "HEAD")).Trim();
                if (!head.StartsWith("ref: ", StringComparison.Ordinal)) return head;

                var refName = head.Substring(5);
                var refPath = Path.Combine(gitDir, refName.Replace('/', Path.DirectorySeparatorChar));
                if (File.Exists(refPath)) return File.ReadAllText(refPath).Trim();

                var packed = Path.Combine(gitDir, "packed-refs");
                if (File.Exists(packed))
                {
                    foreach (var line in File.ReadAllLines(packed))
                        if (line.EndsWith(" " + refName, StringComparison.Ordinal))
                            return line.Substring(0, line.IndexOf(' '));
                }
            }
            catch (Exception)
            {
                // A missing repo must never break a bug report.
            }
            return "unknown";
        }
    }
}
