/*
 * AssemblyIntegrityChecker.cs
 *
 * Purpose:
 *   Validates checksum of game assembly to detect tampering (e.g., via dnSpy).
 */

using UnityEngine;
using System.IO;
using System.Security.Cryptography;

public class AssemblyIntegrityChecker : MonoBehaviour
{
    [SerializeField] private AntiCheatConfig config;
    [SerializeField] private string expectedSHA256;

    private string TargetPath => Path.Combine(Application.dataPath, "../Managed/Assembly-CSharp.dll");

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        if (!File.Exists(TargetPath)) TriggerDetection("Assembly-CSharp.dll not found.");

        using var stream = File.OpenRead(TargetPath);
        using var sha = SHA256.Create();

        byte[] hash = sha.ComputeHash(stream);
        string hex = System.BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();

        if (hex != expectedSHA256.ToLowerInvariant())
            TriggerDetection("Assembly-CSharp.dll tampering detected.");
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
