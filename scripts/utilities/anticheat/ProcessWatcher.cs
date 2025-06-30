/*
 * ProcessWatcher.cs
 *
 * Description:
 *   Periodically scans running processes for known cheat/debug tools.
 *   Triggers detection if any blacklisted tool is found.
 *
 * Setup:
 *   - Assign a reference to an AntiCheatConfig asset.
 *   - Adjust scanInterval and blacklist as needed.
 */

using UnityEngine;
using System.Diagnostics;
using System.Linq;
using System.Collections;

public class ProcessWatcher : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private AntiCheatConfig config;
    [SerializeField] private float scanInterval = 5f;

    [Tooltip("Process names to detect (case-insensitive).")]
    [SerializeField] private string[] blacklistedProcesses =
    {
        "cheatengine", "dnspy", "ilspy", "x64dbg", "x32dbg", "ida", "ollydbg", "reclass", "processhacker"
    };

    private void Start()
    {
        if (config == null || !config.antiCheatEnabled) return;
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
        if (config.logDetections) Debug.LogError(message);
        if (config.terminateOnDetection) TerminateGame();
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
