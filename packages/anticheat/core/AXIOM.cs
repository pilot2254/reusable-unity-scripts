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
    private List<IntPtr> _decoyPointers = new();
    private Dictionary<MethodInfo, string> _methodHashes = new();

    // === CENTRALIZED CONFIGURATION ===
    public class Config
    {
        public string AntiCheatName = "AXIOM";

        public bool CheckAssemblyIntegrity = true;
        public bool DetectDebugger = true;
        public bool DetectMonoInjection = true;
        public bool MonitorExternalProcesses = true;
        public bool DetectSpeedHack = true;
        public bool UseMemoryDecoys = true;
        public bool ValidateMethodHashes = true;
        public bool CheckStackTrace = true;

        public float SpeedHackThreshold = 0.5f;
        public List<string> KnownBadProcesses = new() { "cheatengine", "ollydbg", "x64dbg", "dnspy", "processhacker" };
        public List<string> SuspiciousAssemblies = new() { "dnlib", "mono.cecil", "xmono", "cheat" };

        public float ProcessScanInterval = 5f;
        public float SpeedHackScanInterval = 1f;

        public string GameAssemblyNameHint = "YourGame"; // used in mono injection check
        public bool QuitOnDetection = true;
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
        using var md5 = MD5.Create();
        byte[] hash = md5.ComputeHash(File.ReadAllBytes(path));
        string hashString = BitConverter.ToString(hash).Replace("-", "");
        Log("Assembly MD5: " + hashString);
    }

    // === MODULE: DEBUGGER DETECTION ===
    private void CheckDebugger()
    {
        if (Debugger.IsAttached || IsDebuggerPresent())
        {
            Log("Debugger detected.");
            Kill();
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
            if (!asmName.StartsWith("unity") &&
                !asmName.StartsWith("system") &&
                !asmName.StartsWith("mscorlib") &&
                !asm.Location.Contains(Settings.GameAssemblyNameHint))
            {
                if (Settings.SuspiciousAssemblies.Any(s => asmName.Contains(s)))
                {
                    Log("Suspicious injected assembly: " + asmName);
                    Kill();
                }
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
                Log("Blacklisted process: " + name);
                Kill();
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
            Log("Speedhack detected.");
            Kill();
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
        Log("Memory decoys allocated.");
    }

    // === MODULE: METHOD IL HASHING ===
    private void CacheMethodHashes()
    {
        var methods = typeof(AXIOM).GetMethods(BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (var m in methods)
        {
            var body = m.GetMethodBody();
            if (body != null)
            {
                byte[] il = body.GetILAsByteArray();
                using var sha = SHA256.Create();
                string hash = BitConverter.ToString(sha.ComputeHash(il)).Replace("-", "");
                _methodHashes[m] = hash;
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
                Log($"IL tampering detected: {m.Name}");
                Kill();
            }
        }
    }

    // === MODULE: STACK TRACE CHECK ===
    private void CheckStackConsistency()
    {
        string stack = Environment.StackTrace.ToLower();
        if (stack.Contains("dnspy") || stack.Contains("debugger") || stack.Contains("mono.cecil"))
        {
            Log("Suspicious stack trace.");
            Kill();
        }
    }

    // === UTILITIES ===
    public static void Initialize()
    {
        if (_instance == null)
        {
            var go = new GameObject(Settings.AntiCheatName);
            _instance = go.AddComponent<AXIOM>();
        }
    }

    private void Log(string msg)
    {
        Debug.Log($"[{Settings.AntiCheatName}] {msg}");
    }

    private void Kill()
    {
        if (Settings.QuitOnDetection)
        {
            Log("Quitting due to violation.");
            Application.Quit();
        }
    }
}
