// AXIOM.cs - Open Source Unity Anti-Cheat Runtime Framework
// Author: Mike
// License: MIT
// Version: 1.3

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
    private static readonly SHA256 _sha256 = SHA256.Create();
    private static readonly MD5 _md5 = MD5.Create();

    private DateTime _lastRealTime;
    private float _lastGameTime;

    private readonly List<IntPtr> _decoyPointers = new();
    private readonly Dictionary<MethodInfo, string> _methodHashes = new();

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

        public List<string> KnownBadProcesses = new()
        {
            "cheatengine", "ollydbg", "x64dbg", "dnspy", "processhacker"
        };

        public List<string> SuspiciousAssemblies = new()
        {
            "dnlib", "mono.cecil", "xmono", "cheat"
        };

        public float ProcessScanInterval = 5f;
        public float SpeedHackScanInterval = 1f;

        public string GameAssemblyNameHint = "YourGame";
        public bool QuitOnDetection = true;
    }

    public static readonly Config Settings = new();

    // === INIT ===
    public static void Initialize()
    {
        if (_instance != null) return;

        var obj = new GameObject(Settings.AntiCheatName);
        _instance = obj.AddComponent<AXIOM>();
        DontDestroyOnLoad(obj);
    }

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        if (Settings.CheckAssemblyIntegrity) VerifyAssemblies();
        if (Settings.DetectDebugger) CheckDebugger();
        if (Settings.DetectMonoInjection) CheckMonoInjection();
        if (Settings.ValidateMethodHashes) CacheMethodHashes();
        if (Settings.CheckStackTrace) InvokeRepeating(nameof(CheckStackConsistency), 3f, 10f);
        if (Settings.MonitorExternalProcesses) InvokeRepeating(nameof(ScanProcesses), 1f, Settings.ProcessScanInterval);
        if (Settings.DetectSpeedHack) InvokeRepeating(nameof(CheckSpeedHack), 1f, Settings.SpeedHackScanInterval);
        if (Settings.UseMemoryDecoys) CreateMemoryDecoys();

        _lastRealTime = DateTime.UtcNow;
        _lastGameTime = Time.time;
    }

    // === ASSEMBLY INTEGRITY ===
    private void VerifyAssemblies()
    {
        try
        {
            string path = Assembly.GetExecutingAssembly().Location;
            byte[] hash = _md5.ComputeHash(File.ReadAllBytes(path));
            string hashString = BitConverter.ToString(hash).Replace("-", "");
            Log("Assembly MD5: " + hashString);
        }
        catch (Exception ex)
        {
            Log("Assembly hash failed: " + ex.Message);
        }
    }

    // === DEBUGGER CHECK ===
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

    // === MONO INJECTION CHECK ===
    private void CheckMonoInjection()
    {
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            string asmName = asm.GetName().Name.ToLowerInvariant();
            if (!asmName.StartsWith("unity") &&
                !asmName.StartsWith("system") &&
                !asmName.StartsWith("mscorlib") &&
                !asm.Location.Contains(Settings.GameAssemblyNameHint, StringComparison.OrdinalIgnoreCase))
            {
                if (Settings.SuspiciousAssemblies.Any(s => asmName.Contains(s)))
                {
                    Log("Suspicious injected assembly: " + asmName);
                    Kill();
                }
            }
        }
    }

    // === EXTERNAL PROCESS CHECK ===
    private void ScanProcesses()
    {
        try
        {
            foreach (var proc in Process.GetProcesses())
            {
                string name = proc.ProcessName.ToLowerInvariant();
                foreach (var bad in Settings.KnownBadProcesses)
                {
                    if (name.Contains(bad))
                    {
                        Log("Blacklisted process: " + name);
                        Kill();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Log("Process scan failed: " + ex.Message);
        }
    }

    // === SPEEDHACK DETECTION ===
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

    // === MEMORY DECOYS ===
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr VirtualAlloc(IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);

    private void CreateMemoryDecoys()
    {
        const int decoyCount = 5;
        for (int i = 0; i < decoyCount; i++)
        {
            IntPtr ptr = VirtualAlloc(IntPtr.Zero, 4, 0x1000 | 0x2000, 0x40);
            if (ptr != IntPtr.Zero)
            {
                Marshal.WriteInt32(ptr, 1337);
                _decoyPointers.Add(ptr);
            }
        }
        Log("Memory decoys allocated.");
    }

    // === METHOD HASHING ===
    private void CacheMethodHashes()
    {
        var methods = typeof(AXIOM).GetMethods(BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (var m in methods)
        {
            var body = m.GetMethodBody();
            if (body == null) continue;

            byte[] il = body.GetILAsByteArray();
            string hash = BitConverter.ToString(_sha256.ComputeHash(il)).Replace("-", "");
            _methodHashes[m] = hash;
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

            byte[] currentIL = body.GetILAsByteArray();
            string actual = BitConverter.ToString(_sha256.ComputeHash(currentIL)).Replace("-", "");
            if (actual != expected)
            {
                Log("IL tampering detected: " + m.Name);
                Kill();
            }
        }
    }

    // === STACK TRACE MONITOR ===
    private void CheckStackConsistency()
    {
        string stack = Environment.StackTrace.ToLowerInvariant();
        if (stack.Contains("dnspy") || stack.Contains("debugger") || stack.Contains("mono.cecil"))
        {
            Log("Suspicious stack trace.");
            Kill();
        }
    }

    // === LOGGING & RESPONSE ===
    private void Log(string msg)
    {
        Debug.Log($"[{Settings.AntiCheatName}] {msg}");
    }

    private void Kill()
    {
        if (Settings.QuitOnDetection)
        {
            Log("Terminating game due to security violation.");
            Application.Quit();
        }
    }
}
