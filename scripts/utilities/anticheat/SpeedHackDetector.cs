/*
 * SpeedHackDetector.cs
 *
 * Purpose:
 *   Detects Time.time/Time.realtimeSinceStartup desync. Flags possible speedhacks.
 */

using UnityEngine;
using System.Collections;

public class SpeedHackDetector : MonoBehaviour
{
    [SerializeField] private AntiCheatConfig config;
    [SerializeField] private float checkInterval = 5f;
    [SerializeField] private float maxDeltaDesync = 0.5f;

    private float lastReal, lastGame;

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        lastReal = Time.realtimeSinceStartup;
        lastGame = Time.time;
        StartCoroutine(DetectLoop());
    }

    private IEnumerator DetectLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(checkInterval);

            float deltaReal = Time.realtimeSinceStartup - lastReal;
            float deltaGame = Time.time - lastGame;

            if (Mathf.Abs(deltaReal - deltaGame) > maxDeltaDesync)
                TriggerDetection($"Speed hack detected. Real:{deltaReal:F2}, Game:{deltaGame:F2}");

            lastReal = Time.realtimeSinceStartup;
            lastGame = Time.time;
        }
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
