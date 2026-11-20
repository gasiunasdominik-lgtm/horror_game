using System;
using System.Collections.Generic;
using UnityEngine;

namespace Vchod.DebugTools
{
    /// <summary>Keeps the last console lines in memory so a bug report can include them.</summary>
    public static class LogBuffer
    {
        public const int Capacity = 300;

        private static readonly Queue<string> Lines = new(Capacity + 1);
        private static readonly object Gate = new();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Install()
        {
            lock (Gate) Lines.Clear();
            Application.logMessageReceivedThreaded -= OnLog;
            Application.logMessageReceivedThreaded += OnLog;
        }

        private static void OnLog(string condition, string stackTrace, LogType type)
        {
            lock (Gate)
            {
                Add($"[{DateTime.Now:HH:mm:ss.fff}] [{type}] {condition}");
                if (type is LogType.Error or LogType.Exception or LogType.Assert && !string.IsNullOrEmpty(stackTrace))
                {
                    foreach (var line in stackTrace.Split('\n'))
                        if (line.Length > 0) Add("    " + line.TrimEnd());
                }
            }
        }

        private static void Add(string line)
        {
            Lines.Enqueue(line);
            while (Lines.Count > Capacity) Lines.Dequeue();
        }

        public static string[] Snapshot()
        {
            lock (Gate) return Lines.ToArray();
        }
    }
}
