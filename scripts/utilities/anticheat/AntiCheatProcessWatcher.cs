/*
 * AntiCheatProcessWatcher.cs
 *
 * Purpose:
 *   Detects blacklisted processes and terminates game if detected.
 */

using UnityEngine;
using System.Diagnostics;
using System.Linq;
using System.Collections;

public class AntiCheatProcessWatcher : MonoBehaviour
{
    [SerializeField] private AntiCheatConfig config;
    [SerializeField] private float scanInterval = 5f;

    [SerializeField]
    private string[] blacklistedProcesses = new[]
    {
        "cheatengine", "dnspy", "ilspy", "x64dbg", "x32dbg", "ida", "ollydbg", "reclass", "processhacker"
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
        var processes = Process.GetProcesses();
        foreach (var proc in processes)
        {
            string name = proc.ProcessName.ToLowerInvariant();
            if (blacklistedProcesses.Any(bad => name.Contains(bad)))
                TriggerDetection($"Blacklisted process detected: {name}");
        }
    }

    private void TriggerDetection(string message)
    {
        if (config.logDetections) Debug.LogError(message);
        if (config.terminateOnDetection) Quit();
    }

    private void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
