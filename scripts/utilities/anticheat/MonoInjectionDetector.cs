/*
 * MonoInjectionDetector.cs
 *
 * Purpose:
 *   Scans for unknown loaded assemblies. If unauthorized, triggers detection.
 */

using UnityEngine;
using System;
using System.Linq;
using System.Reflection;

public class MonoInjectionDetector : MonoBehaviour
{
    [SerializeField] private AntiCheatConfig config;
    [SerializeField]
    private string[] allowedAssemblyPrefixes = new[]
    {
        "Assembly-CSharp", "Unity", "mscorlib", "System"
    };

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        var loaded = AppDomain.CurrentDomain.GetAssemblies();

        foreach (var asm in loaded)
        {
            string name = asm.GetName().Name;
            if (!allowedAssemblyPrefixes.Any(prefix => name.StartsWith(prefix)))
                TriggerDetection($"Injected assembly: {name}");
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
