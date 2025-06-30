using System;
using System.Diagnostics;
using System.Linq;
using System.Collections;
using UnityEngine;

public class ProcessWatcher : MonoBehaviour
{
    private readonly string[] blacklistedProcesses = new[]
    {
        "cheatengine", "cheat engine",
        "dnspy", "dnspy-netcore",
        "ilspy", "ilspycmd",
        "x64dbg", "x32dbg",
        "ida", "ida64", "ida32",
        "ollydbg", "reclass",
        "processhacker"
    };

    public float scanInterval = 5f;

    void Start()
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
                    KillGame("Detected unauthorized process: " + name);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning("AntiCheat scan failed: " + ex.Message);
        }
    }

    private void KillGame(string reason)
    {
        Debug.LogError("AntiCheat triggered. Reason: " + reason);
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
