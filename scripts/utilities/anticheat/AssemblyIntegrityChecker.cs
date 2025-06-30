/*
 * AssemblyIntegrityChecker.cs
 *
 * Description:
 *   Verifies the SHA-256 hash of the Assembly-CSharp.dll file to detect tampering.
 *
 * Setup:
 *   - Generate a SHA-256 hash of the Assembly-CSharp.dll file at build time.
 *   - Assign the expected hash to the script.
 */

using UnityEngine;
using System.IO;
using System.Security.Cryptography;

public class AssemblyIntegrityChecker : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private AntiCheatConfig config;
    [SerializeField] private string expectedSHA256;

    private void Start()
    {
        if (config == null || !config.antiCheatEnabled || string.IsNullOrEmpty(expectedSHA256)) return;
        DontDestroyOnLoad(gameObject);

        string path = Path.Combine(Application.dataPath, "../Managed/Assembly-CSharp.dll");
        if (!File.Exists(path))
        {
            TriggerDetection("Assembly-CSharp.dll not found.");
            return;
        }

        string hash = ComputeSHA256(path);
        if (!hash.Equals(expectedSHA256.ToLowerInvariant()))
            TriggerDetection("Assembly-CSharp.dll integrity check failed.");
    }

    private string ComputeSHA256(string filePath)
    {
        using FileStream stream = File.OpenRead(filePath);
        using SHA256 sha = SHA256.Create();
        byte[] hashBytes = sha.ComputeHash(stream);
        return System.BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
    }

    private void TriggerDetection(string message)
    {
        if (!config.antiCheatEnabled) return;
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
