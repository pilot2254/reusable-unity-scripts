// AXIOM.cs - Open Source Unity Anti-Cheat Runtime
// License: MIT
// Description: Centralized, modular, single-file anti-cheat system for Unity 6+
// Features: Integrity check, debugger detection, Mono injection detection, process monitoring, speedhack detection, memory decoys

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

    // === CONFIGURATION BLOCK ===
    public class Config
    {
        public bool CheckAssemblyIntegrity = true;
        public bool DetectDebugger = true;
        public bool DetectMonoInjection = true;
        public bool MonitorExternalProcesses = true;
        public bool DetectSpeedHack = true;
        public bool UseMemoryDecoys = true;

        public float SpeedHackThreshold = 0.5f; // if Time.realtimeSinceStartup - lastRealTime diverges too much from Time.time
        public List<string> KnownBadProcesses = new List<string> { "cheatengine", "ollydbg", "x64dbg", "dnspy", "processhacker" };
        public float ProcessScanInterval = 5f; // in seconds
        public float SpeedHackScanInterval = 1f; // in seconds
    }

    public static Config Settings = new Config();
    // ================== END CONFIGURATION ==================

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
        // To extend: store known hashes in config and compare
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
            if (!asm.FullName.StartsWith("System") &&
                !asm.FullName.StartsWith("Unity") &&
                !asm.FullName.StartsWith("mscorlib") &&
                !asm.Location.Contains("YourGame")) // adjust if needed
            {
                Debug.LogWarning("AXIOM: Suspicious assembly injected: " + asm.FullName);
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
                Debug.LogWarning($"AXIOM: Detected blacklisted process: {name}");
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
            IntPtr fake = VirtualAlloc(IntPtr.Zero, 4, 0x1000 | 0x2000, 0x40); // MEM_COMMIT | MEM_RESERVE, PAGE_EXECUTE_READWRITE
            Marshal.WriteInt32(fake, 1337); // fake value
            _decoyPointers.Add(fake);
        }
        Debug.Log("AXIOM: Memory decoys allocated.");
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
