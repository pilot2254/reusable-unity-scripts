/*
 * anticheat -> ProcessWatcher.cs
 * 
 * Purpose:
 *   Monitors running system processes during runtime. If any known cheating/debugging tools 
 *   (e.g., Cheat Engine, dnSpy, IDA, x64dbg) are detected, the game is forcibly shut down.
 *
 * Config:
 *   - scanInterval: Interval (in seconds) between scan cycles.
 *   - blacklistedProcesses: List of lowercase substrings of process names to block.
 *
 * Deps:
 *   - UnityEngine (for MonoBehaviour, Coroutine, Application lifecycle)
 *   - System.Diagnostics (to enumerate system processes)
 *   - System.Linq (for list searching)
 *   - System.Collections (for Coroutine IEnumerator)
 *
 * Usage:
 *   1. Attach this script to a persistent GameObject in your first scene.
 *   2. Ensure the GameObject is not destroyed on load (handled in this script).
 *   3. To customize detection list, modify the `blacklistedProcesses` array.
 *   4. Optionally expose settings to Unity Inspector via [SerializeField].
 *
 * Example:
 *   Create empty GameObject in scene -> Attach ProcessWatcher.cs
 *   -> On game runtime, if user opens "cheatengine.exe", game terminates.
 */

using System;
using System.Diagnostics;
using System.Linq;
using System.Collections;
using UnityEngine;

public class ProcessWatcher : MonoBehaviour
{
    [Tooltip("Time between process scans in seconds.")]
    [SerializeField]
    private float scanInterval = 5f;

    [Tooltip("List of substrings (lowercase) to detect in running process names.")]
    [SerializeField]
    private string[] blacklistedProcesses = new[]
    {
        "cheatengine", "cheat engine",
        "dnspy", "dnspy-netcore",
        "ilspy", "ilspycmd",
        "x64dbg", "x32dbg",
        "ida", "ida64", "ida32",
        "ollydbg", "reclass",
        "processhacker"
    };

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        StartCoroutine(ScanLoop());
    }

    private IEnumerator ScanLoop()
    {
        while (true)
        {
            ScanProcesses();
            yield return new WaitForSeconds(scanInterval);
        }
    }

    private void ScanProcesses()
    {
        try
        {
            var processes = Process.GetProcesses();

            foreach (var proc in processes)
            {
                string name = proc.ProcessName.ToLowerInvariant();

                if (blacklistedProcesses.Any(bad => name.Contains(bad)))
                {
                    KillGame($"Detected unauthorized process: {name}");
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"AntiCheat scan failed: {ex.Message}");
        }
    }

    private void KillGame(string reason)
    {
        Debug.LogError($"AntiCheat triggered. Reason: {reason}");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
