using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Vchod.DebugTools
{
    /// <summary>
    /// F8 saves BugReports/&lt;date-time&gt;/ with screenshot.png, log.txt, state.json and
    /// note.txt, then offers a one-line note (Enter saves, Esc skips).
    /// </summary>
    public sealed class BugReporter : MonoBehaviour
    {
        public const string FolderName = "BugReports";

        private bool _busy;
        private bool _noteOpen;
        private string _note = string.Empty;
        private string _currentDir;

        public static string RootDirectory =>
            Path.GetFullPath(Path.Combine(Application.dataPath, "..", FolderName));

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null || _busy || _noteOpen) return;
            if (keyboard.f8Key.wasPressedThisFrame) StartCoroutine(Capture());
        }

        private IEnumerator Capture()
        {
            _busy = true;
            var snapshot = DebugContext.Instance.Capture();
            yield return new WaitForEndOfFrame();

            string dir = null;
            try
            {
                dir = CreateReportDirectory();
                var shot = ScreenCapture.CaptureScreenshotAsTexture();
                File.WriteAllBytes(Path.Combine(dir, "screenshot.png"), shot.EncodeToPNG());
                Destroy(shot);
                File.WriteAllLines(Path.Combine(dir, "log.txt"), LogBuffer.Snapshot());
                File.WriteAllText(Path.Combine(dir, "state.json"), JsonUtility.ToJson(snapshot, true));
                File.WriteAllText(Path.Combine(dir, "note.txt"), string.Empty);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"Bug report failed: {e.Message}");
                _busy = false;
                yield break;
            }

            _currentDir = dir;
            DebugContext.Instance.Notify($"Bug report saved: {FolderName}/{Path.GetFileName(dir)}");
            OpenNote();
            _busy = false;
        }

        private static string CreateReportDirectory()
        {
            var name = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            var dir = Path.Combine(RootDirectory, name);
            for (var i = 2; Directory.Exists(dir); i++) dir = Path.Combine(RootDirectory, $"{name}_{i}");
            Directory.CreateDirectory(dir);
            return dir;
        }

        private void OpenNote()
        {
            _note = string.Empty;
            _noteOpen = true;
            DebugContext.Instance.SetInputBlocked(this, true);
        }

        private void CloseNote(bool save)
        {
            if (save && _note.Trim().Length > 0)
                File.WriteAllText(Path.Combine(_currentDir, "note.txt"), _note.Trim() + Environment.NewLine);
            _noteOpen = false;
            DebugContext.Instance.SetInputBlocked(this, false);
        }

        private void OnGUI()
        {
            if (!_noteOpen) return;

            var e = Event.current;
            if (e.type == EventType.KeyDown)
            {
                if (e.keyCode is KeyCode.Return or KeyCode.KeypadEnter) { CloseNote(true); e.Use(); return; }
                if (e.keyCode == KeyCode.Escape) { CloseNote(false); e.Use(); return; }
            }

            var rect = new Rect((Screen.width - 520) * 0.5f, Screen.height * 0.62f, 520, 70);
            GUILayout.BeginArea(rect, GUI.skin.box);
            GUILayout.Label("Bug note (Enter = save, Esc = skip):");
            GUI.SetNextControlName("BugNote");
            _note = GUILayout.TextField(_note, 300);
            GUI.FocusControl("BugNote");
            GUILayout.EndArea();
        }
    }
}
