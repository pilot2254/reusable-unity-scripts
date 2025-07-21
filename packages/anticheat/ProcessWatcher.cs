/*
 * ProcessWatcher.cs
 * 
 * This component regularly scans running processes on the player's machine
 * and looks for known cheating or debugging tools like Cheat Engine, dnSpy, IDA, etc.
 * If it finds any blacklisted processes, it triggers the configured detection response.
 * 
 * Setup:
 * - Assign your AntiCheatConfig asset to the 'config' field.
 * - Optionally adjust the scan interval (default is 5 seconds).
 * - You can customize the list of blacklisted process names if needed.
 * 
 * Notes:
 * - This script uses System.Diagnostics.Process, which works on Windows standalone builds.
 * - It requires appropriate permissions to enumerate processes.
 * 
 * Usage example:
 * Attach this script to a GameObject in your initial scene.
 */

using UnityEngine;
using System.Diagnostics;
using System.Linq;
using System.Collections;

public class ProcessWatcher : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private AntiCheatConfig config;
    [SerializeField, Tooltip("How often (in seconds) to scan for blacklisted processes.")] 
    private float scanInterval = 5f;

    [Tooltip("Process names to watch for (case-insensitive).")]
    [SerializeField] private string[] blacklistedProcesses =
    {
        "cheatengine", "dnspy", "ilspy", "x64dbg", "x32dbg", "ida", "ollydbg", "reclass", "processhacker"
    };

    private void Start()
    {
        if (config == null)
        {
            Debug.LogError("ProcessWatcher: Config asset is not assigned.");
            enabled = false;
            return;
        }

        if (!config.antiCheatEnabled)
        {
            enabled = false;
            return;
        }

        DontDestroyOnLoad(gameObject);
        StartCoroutine(ScanLoop());
    }

    private IEnumerator ScanLoop()
    {
        while (true)
        {
            DetectBlacklistedProcesses();
            yield return new WaitForSeconds(scanInterval);
        }
    }

    private void DetectBlacklistedProcesses()
    {
        foreach (var process in Process.GetProcesses())
        {
            string name = process.ProcessName.ToLowerInvariant();
            if (blacklistedProcesses.Any(b => name.Contains(b)))
            {
                TriggerDetection($"Blacklisted process detected: {name}");
                break;
            }
        }
    }

    private void TriggerDetection(string message)
    {
        if (!config.antiCheatEnabled) return;

        if (config.logDetections)
            Debug.LogError("[AntiCheat] " + message);

        if (config.terminateOnDetection)
            TerminateGame();
    }

    private void TerminateGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
