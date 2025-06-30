/*
 * DebuggerDetector.cs
 *
 * Purpose:
 *   Uses .NET Debugger class to detect if the game is being debugged.
 */

using UnityEngine;
using System.Diagnostics;

public class DebuggerDetector : MonoBehaviour
{
    [SerializeField] private AntiCheatConfig config;

    private void Start()
    {
        DontDestroyOnLoad(gameObject);

        if (Debugger.IsAttached || Debugger.IsLogging())
            TriggerDetection("Debugger attached.");
    }

    private void TriggerDetection(string msg)
    {
        if (config.logDetections) Debug.LogError(msg);
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
