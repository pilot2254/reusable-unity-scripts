/*
 * MonoInjectionDetector.cs
 * 
 * This script checks all loaded assemblies in the current AppDomain to detect
 * any unauthorized or injected assemblies that do not match allowed prefixes.
 * It helps catch common injection tools or dynamically loaded cheat code.
 * 
 * Setup:
 * - Assign your AntiCheatConfig asset.
 * - Adjust allowedPrefixes array if needed.
 * 
 * Limitations:
 * - May need updating if legitimate third-party assemblies are added.
 */

using UnityEngine;
using System;
using System.Linq;
using System.Reflection;

public class MonoInjectionDetector : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private AntiCheatConfig config;

    [Tooltip("Allowed assembly name prefixes that are considered safe.")]
    [SerializeField] private string[] allowedPrefixes =
    {
        "Assembly-CSharp", "Unity", "mscorlib", "System"
    };

    private void Start()
    {
        if (config == null)
        {
            Debug.LogError("MonoInjectionDetector: Config asset is not assigned.");
            enabled = false;
            return;
        }

        if (!config.antiCheatEnabled)
        {
            enabled = false;
            return;
        }

        DontDestroyOnLoad(gameObject);

        foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            string asmName = asm.GetName().Name;
            if (!allowedPrefixes.Any(prefix => asmName.StartsWith(prefix)))
            {
                TriggerDetection($"Injected or unknown assembly detected: {asmName}");
            }
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
