/*
 * DebuggerDetector.cs
 *
 * Description:
 *   Checks if the game is being run under a debugger (Visual Studio, dnSpy, etc).
 *
 * Setup:
 *   - Attach to any active GameObject in the scene.
 *   - Assign AntiCheatConfig in Inspector.
 */

using UnityEngine;
using System.Diagnostics;

public class DebuggerDetector : MonoBehaviour
{
    [SerializeField] private AntiCheatConfig config;

    private void Start()
    {
        if (config == null || !config.antiCheatEnabled) return;
        DontDestroyOnLoad(gameObject);

        if (Debugger.IsAttached || Debugger.IsLogging())
            TriggerDetection("Debugger detected.");
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
