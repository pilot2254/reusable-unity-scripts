/*
 * AssemblyIntegrityChecker.cs
 * 
 * Checks the integrity of the game's core managed assembly (Assembly-CSharp.dll)
 * by computing its SHA256 hash and comparing it to a known valid hash.
 * This helps detect tampering or unauthorized code injection.
 * 
 * Usage:
 * - Set the 'expectedSHA256' field to your known good assembly hash.
 * - Attach this script early in your game initialization.
 * - Assign the AntiCheatConfig asset.
 * 
 * Limitations:
 * - This method only works on standalone builds where the assembly file is accessible.
 * - Must update the hash after every legitimate build.
 */

using UnityEngine;
using System.IO;
using System.Security.Cryptography;

public class AssemblyIntegrityChecker : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private AntiCheatConfig config;

    [Tooltip("Expected SHA256 hash of Assembly-CSharp.dll in lowercase hex.")]
    [SerializeField] private string expectedSHA256;

    private void Start()
    {
        if (config == null)
        {
            Debug.LogError("AssemblyIntegrityChecker: Config asset is not assigned.");
            enabled = false;
            return;
        }

        if (!config.antiCheatEnabled || string.IsNullOrEmpty(expectedSHA256))
        {
            enabled = false;
            return;
        }

        DontDestroyOnLoad(gameObject);

        string assemblyPath = Path.Combine(Application.dataPath, "../Managed/Assembly-CSharp.dll");
        if (!File.Exists(assemblyPath))
        {
            TriggerDetection("Assembly-CSharp.dll not found at expected location.");
            return;
        }

        string actualHash = ComputeSHA256(assemblyPath);
        if (!actualHash.Equals(expectedSHA256))
        {
            TriggerDetection($"Assembly integrity check failed. Expected: {expectedSHA256}, Found: {actualHash}");
        }
    }

    private string ComputeSHA256(string filePath)
    {
        using (FileStream stream = File.OpenRead(filePath))
        using (SHA256 sha = SHA256.Create())
        {
            byte[] hashBytes = sha.ComputeHash(stream);
            return System.BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
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
