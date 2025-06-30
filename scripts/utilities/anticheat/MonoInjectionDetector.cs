/*
 * MonoInjectionDetector.cs
 *
 * Description:
 *   Detects unauthorized assemblies loaded at runtime (indicative of injection).
 *
 * Setup:
 *   - Add trusted assembly name prefixes to the allowlist.
 */

using UnityEngine;
using System;
using System.Linq;
using System.Reflection;

public class MonoInjectionDetector : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private AntiCheatConfig config;
    [SerializeField] private string[] allowedPrefixes =
    {
        "Assembly-CSharp", "Unity", "mscorlib", "System"
    };

    private void Start()
    {
        if (config == null) return;
        DontDestroyOnLoad(gameObject);

        foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            string name = asm.GetName().Name;
            if (!allowedPrefixes.Any(prefix => name.StartsWith(prefix)))
                TriggerDetection($"Injected or unknown assembly detected: {name}");
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
