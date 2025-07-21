// AXIOM.cs - Open Source Unity Anti-Cheat Runtime
// License: MIT
// Description: Centralized, modular, single-file anti-cheat system for Unity 6+
// Features: Assembly integrity, debugger detection, Mono injection, external process monitor,
// speedhack detection, stack trace check, method hash verification, memory decoys

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public sealed class AXIOM : MonoBehaviour
{
    private static AXIOM _instance;
    private static readonly object _lock = new object();
    private DateTime _lastRealTime;
    private float _lastGameTime;
    private List<IntPtr> _decoyPointers = new List<IntPtr>();
    private Dictionary<MethodInfo, string> _methodHashes = new Dictionary<MethodInfo, string>();

    // === CONFIGURATION ===
    public class Config
    {
        public bool CheckAssemblyIntegrity = true;
        public bool DetectDebugger = true;
        public bool DetectMonoInjection = true;
        public bool MonitorExternalProcesses = true;
        public bool DetectSpeedHack = true;
        public bool UseMemoryDecoys = true;
        public bool ValidateMethodHashes = true;
        public bool CheckStackTrace = true;

        public float SpeedHackThreshold = 0.5f;
        public List<string> KnownBadProcesses = new List<string> { "cheatengine", "ollydbg", "x64dbg", "dnspy", "processhacker" };
        public List<string> SuspiciousAssemblies = new List<string> { "dnlib", "mono.cecil", "xmono", "cheat" };
        public float ProcessScanInterval = 5f;
        public float SpeedHackScanInterval = 1f;
    }

    public static Config Settings = new Config();
    // === END CONFIGURATION ===

    void Awake()
    {
        lock (_lock)
        {
            if (_instance != null) Destroy(gameObject);
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    void Start()
    {
        if (Settings.CheckAssemblyIntegrity) VerifyAssemblies();
        if (Settings.DetectDebugger) CheckDebugger();
        if (Settings.DetectMonoInjection) CheckMonoInjection();
        if (Settings.ValidateMethodHashes) CacheMethodHashes();
        if (Settings.CheckStackTrace) InvokeRepeating(nameof(CheckStackConsistency), 3f, 10f);

        if (Settings.MonitorExternalProcesses)
            InvokeRepeating(nameof(ScanProcesses), 1f, Settings.ProcessScanInterval);

        if (Settings.DetectSpeedHack)
            InvokeRepeating(nameof(CheckSpeedHack), 1f, Settings.SpeedHackScanInterval);

        if (Settings.UseMemoryDecoys)
            CreateMemoryDecoys();
    }

    // === MODULE: ASSEMBLY INTEGRITY ===
    private void VerifyAssemblies()
    {
        string path = Assembly.GetExecutingAssembly().Location;
        using (var md5 = MD5.Create())
        {
            byte[] hash = md5.ComputeHash(File.ReadAllBytes(path));
            string hashString = BitConverter.ToString(hash).Replace("-", "");
            Debug.Log("AXIOM: Assembly MD5: " + hashString);
        }
    }

    // === MODULE: DEBUGGER DETECTION ===
    private void CheckDebugger()
    {
        if (Debugger.IsAttached || IsDebuggerPresent())
        {
            Debug.LogWarning("AXIOM: Debugger detected.");
            Application.Quit();
        }
    }

    [DllImport("kernel32.dll")]
    private static extern bool IsDebuggerPresent();

    // === MODULE: MONO INJECTION DETECTION ===
    private void CheckMonoInjection()
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach (var asm in assemblies)
        {
            string asmName = asm.GetName().Name.ToLower();
            if (Settings.SuspiciousAssemblies.Any(s => asmName.Contains(s)))
            {
                Debug.LogWarning("AXIOM: Suspicious assembly: " + asmName);
                Application.Quit();
            }
        }
    }

    // === MODULE: EXTERNAL PROCESS MONITORING ===
    private void ScanProcesses()
    {
        var processes = Process.GetProcesses();
        foreach (var proc in processes)
        {
            string name = proc.ProcessName.ToLower();
            if (Settings.KnownBadProcesses.Any(bad => name.Contains(bad)))
            {
                Debug.LogWarning("AXIOM: Blacklisted process: " + name);
                Application.Quit();
            }
        }
    }

    // === MODULE: SPEEDHACK DETECTION ===
    private void CheckSpeedHack()
    {
        float gameTimeDelta = Time.time - _lastGameTime;
        float realTimeDelta = (float)(DateTime.UtcNow - _lastRealTime).TotalSeconds;

        if (Mathf.Abs(gameTimeDelta - realTimeDelta) > Settings.SpeedHackThreshold)
        {
            Debug.LogWarning("AXIOM: Speedhack detected.");
            Application.Quit();
        }

        _lastRealTime = DateTime.UtcNow;
        _lastGameTime = Time.time;
    }

    // === MODULE: MEMORY DECOYS ===
    [DllImport("kernel32.dll", SetLastError = true)]
    static extern IntPtr VirtualAlloc(IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);

    private void CreateMemoryDecoys()
    {
        for (int i = 0; i < 5; i++)
        {
            IntPtr fake = VirtualAlloc(IntPtr.Zero, 4, 0x1000 | 0x2000, 0x40);
            Marshal.WriteInt32(fake, 1337);
            _decoyPointers.Add(fake);
        }
        Debug.Log("AXIOM: Decoys allocated.");
    }

    // === MODULE: METHOD HASHING ===
    private void CacheMethodHashes()
    {
        var methods = typeof(AXIOM).GetMethods(BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (var m in methods)
        {
            var body = m.GetMethodBody();
            if (body != null)
            {
                byte[] il = body.GetILAsByteArray();
                using (var sha = SHA256.Create())
                {
                    string hash = BitConverter.ToString(sha.ComputeHash(il)).Replace("-", "");
                    _methodHashes[m] = hash;
                }
            }
        }
        InvokeRepeating(nameof(ValidateMethodHashes), 10f, 30f);
    }

    private void ValidateMethodHashes()
    {
        foreach (var pair in _methodHashes)
        {
            var m = pair.Key;
            var expected = pair.Value;
            var body = m.GetMethodBody();
            if (body == null) continue;

            string current = BitConverter.ToString(SHA256.Create().ComputeHash(body.GetILAsByteArray())).Replace("-", "");
            if (current != expected)
            {
                Debug.LogWarning($"AXIOM: IL tampering in method: {m.Name}");
                Application.Quit();
            }
        }
    }

    // === MODULE: STACK TRACE CHECK ===
    private void CheckStackConsistency()
    {
        string stack = Environment.StackTrace.ToLower();
        if (stack.Contains("dnspy") || stack.Contains("debugger") || stack.Contains("mono.cecil"))
        {
            Debug.LogWarning("AXIOM: Suspicious stack trace detected.");
            Application.Quit();
        }
    }

    // === UTILITIES ===
    public static void Initialize()
    {
        if (_instance == null)
        {
            var go = new GameObject("AXIOM");
            _instance = go.AddComponent<AXIOM>();
        }
    }
}
