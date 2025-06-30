/*
 * SpeedHackDetector.cs
 *
 * Description:
 *   Detects unnatural manipulation of game time, such as speed hacks.
 *
 * How it Works:
 *   Compares Time.time and Time.realtimeSinceStartup.
 *   If the delta difference exceeds a defined threshold, flags cheating.
 */

using UnityEngine;
using System.Collections;

public class SpeedHackDetector : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private AntiCheatConfig config;
    [SerializeField] private float checkInterval = 5f;
    [SerializeField] private float allowedDesync = 0.5f;

    private float lastRealtime;
    private float lastGametime;

    private void Start()
    {
        if (config == null) return;
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
                TriggerDetection("Speed hack detected (time desync)");

            lastRealtime = Time.realtimeSinceStartup;
            lastGametime = Time.time;
        }
    }

    private void TriggerDetection(string message)
    {
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
