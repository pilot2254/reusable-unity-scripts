/*
 * DebuggerDetector.cs
 * 
 * Detects if a debugger is attached to the game process at startup.
 * If a debugger is found, it triggers the configured anti-cheat response.
 * 
 * Usage:
 * - Attach to an early initialization GameObject.
 * - Assign AntiCheatConfig.
 * 
 * Limitations:
 * - Only detects debugger attachment at start.
 * - Does not continuously monitor for debugger attach/detach.
 */

using UnityEngine;
using System.Diagnostics;

public class DebuggerDetector : MonoBehaviour
{
    [SerializeField] private AntiCheatConfig config;

    private void Start()
    {
        if (config == null)
        {
            Debug.LogError("DebuggerDetector: Config asset is not assigned.");
            enabled = false;
            return;
        }

        if (!config.antiCheatEnabled)
        {
            enabled = false;
            return;
        }

        DontDestroyOnLoad(gameObject);

        if (Debugger.IsAttached || Debugger.IsLogging())
            TriggerDetection("Debugger detected.");
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
