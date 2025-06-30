/*
 * SpeedHackDetector.cs
 * 
 * This script detects if the player is trying to manipulate game speed by comparing
 * real-world elapsed time vs game time elapsed.
 * Large discrepancies often indicate speed hacks or time cheats.
 * 
 * Usage:
 * - Attach this script to a GameObject early in the scene.
 * - Assign the AntiCheatConfig asset.
 * - Adjust checkInterval and allowedDesync if needed.
 * 
 * Notes:
 * - Does not detect every hack, but serves as a good first line of defense.
 */

using UnityEngine;
using System.Collections;

public class SpeedHackDetector : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private AntiCheatConfig config;
    [SerializeField, Tooltip("How often (seconds) to check for time desync.")] 
    private float checkInterval = 5f;

    [SerializeField, Tooltip("Allowed time difference in seconds before detection triggers.")]
    private float allowedDesync = 0.5f;

    private float lastRealtime;
    private float lastGametime;

    private void Start()
    {
        if (config == null)
        {
            Debug.LogError("SpeedHackDetector: Config asset is not assigned.");
            enabled = false;
            return;
        }

        if (!config.antiCheatEnabled)
        {
            enabled = false;
            return;
        }

        DontDestroyOnLoad(gameObject);
        lastRealtime = Time.realtimeSinceStartup;
        lastGametime = Time.time;

        StartCoroutine(CheckLoop());
    }

    private IEnumerator CheckLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(checkInterval);

            float realDelta = Time.realtimeSinceStartup - lastRealtime;
            float gameDelta = Time.time - lastGametime;

            if (Mathf.Abs(realDelta - gameDelta) > allowedDesync)
                TriggerDetection("Speed hack detected due to time desynchronization.");

            lastRealtime = Time.realtimeSinceStartup;
            lastGametime = Time.time;
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
