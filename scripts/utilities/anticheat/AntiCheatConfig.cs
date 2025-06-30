/*
 * AntiCheatConfig.cs
 *
 * Purpose:
 *   Global settings for all anti-cheat modules.
 *
 * Usage:
 *   Configure `terminateOnDetection` to toggle game shutdown.
 *   Adjust logging behavior.
 */

using UnityEngine;

[CreateAssetMenu(fileName = "AntiCheatConfig", menuName = "AntiCheat/Config", order = 0)]
public class AntiCheatConfig : ScriptableObject
{
    [Tooltip("Terminate game when a threat is detected.")]
    public bool terminateOnDetection = true;

    [Tooltip("Log anti-cheat detections to Unity console.")]
    public bool logDetections = true;
}
