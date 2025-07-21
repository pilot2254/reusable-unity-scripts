/*
 * AntiCheatConfig.cs
 * 
 * This script holds all the main configuration settings for the anti-cheat system.
 * You can enable or disable the entire anti-cheat functionality here.
 * It also controls what happens when cheating is detected—whether the game shuts down or just logs the event.
 * 
 * To use:
 * 1. Create an instance of this ScriptableObject via the Unity Editor (Assets > Create > AntiCheat > Config).
 * 2. Adjust the toggles to suit your needs.
 * 3. Assign this config asset to all anti-cheat components.
 * 
 * Dependencies:
 * - None.
 * 
 * Example:
 * antiCheatConfig.antiCheatEnabled = true;
 * antiCheatConfig.terminateOnDetection = true;
 */

using UnityEngine;

[CreateAssetMenu(fileName = "AntiCheatConfig", menuName = "AntiCheat/Config")]
public class AntiCheatConfig : ScriptableObject
{
    [Header("Global Toggle")]
    [Tooltip("Turn this off to disable all anti-cheat checks.")]
    public bool antiCheatEnabled = true;

    [Header("Detection Behavior")]
    [Tooltip("If true, the game will exit immediately upon detecting cheating.")]
    public bool terminateOnDetection = true;

    [Tooltip("If true, detection events will be logged to the Unity console.")]
    public bool logDetections = true;
}
