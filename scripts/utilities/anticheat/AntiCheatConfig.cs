/*
 * AntiCheatConfig.cs
 *
 * Description:
 *   Central configuration for all anti-cheat scripts. Controls global behavior.
 *
 * Setup:
 *   1. Right-click in the Unity Project window → Create → AntiCheat → Config.
 *   2. Assign the generated AntiCheatConfig asset to each anti-cheat script.
 *
 * Fields:
 *   - terminateOnDetection: If true, the game will shut down on detection.
 *   - logDetections: If true, detection events are logged to the console.
 */

using UnityEngine;

[CreateAssetMenu(fileName = "AntiCheatConfig", menuName = "AntiCheat/Config")]
public class AntiCheatConfig : ScriptableObject
{
    [Header("Detection Response")]
    [Tooltip("Immediately shut down the game when a cheat is detected.")]
    public bool terminateOnDetection = true;

    [Tooltip("Log detection events to the Unity console.")]
    public bool logDetections = true;
}
