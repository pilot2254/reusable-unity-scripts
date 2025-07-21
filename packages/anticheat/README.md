# AXIOM - Unity Anti-Cheat Core

AXIOM is a lightweight, open-source anti-cheat system designed for Unity 6 games.  
It is implemented as a single C# file (`AXIOM.cs`) to simplify integration and maintenance.  

> AXIOM - a statement that is taken to be true, to serve as a premise or starting point for further reasoning and arguments.

## Features

- **Assembly Integrity Verification**  
  Detects runtime tampering by hashing loaded assemblies.

- **Debugger Detection**  
  Identifies attached debuggers using both managed and native checks.

- **Mono Injection Detection**  
  Scans loaded assemblies for suspicious injected code.

- **External Process Monitoring**  
  Detects known cheat tools and debuggers running on the system.

- **Speedhack Detection**  
  Monitors game time scale against real time to detect time manipulation.

- **Memory Decoys**  
  Allocates fake memory regions to confuse memory scanners and cheat tools.

- **Method IL Hashing**  
  Validates internal method bytecode to detect in-memory tampering.

- **Stack Trace Monitoring**  
  Checks for suspicious stack traces indicating debugging or hooking.

- **Centralized Configuration**  
  All detection features and parameters are runtime configurable.

## Installation

1. Add `AXIOM.cs` to your Unity project's `Scripts` folder.
2. Initialize the anti-cheat early in your game startup code:

```csharp
AXIOM.Settings.AntiCheatName = "YourGameAntiCheat";
AXIOM.Initialize();
````

3. Optionally, adjust configuration parameters in `AXIOM.Settings` before initialization.

## Configuration

The anti-cheat behavior is controlled through the static `AXIOM.Settings` class.
Example configurable parameters:

* `AntiCheatName` (string): Name used in logs and object names. Default: "AXIOM"
* `CheckAssemblyIntegrity` (bool): Enable assembly tampering check.
* `DetectDebugger` (bool): Enable debugger detection.
* `DetectMonoInjection` (bool): Enable injected assembly detection.
* `MonitorExternalProcesses` (bool): Enable scanning for known cheat processes.
* `DetectSpeedHack` (bool): Enable speedhack detection.
* `UseMemoryDecoys` (bool): Enable allocation of decoy memory.
* `ValidateMethodHashes` (bool): Enable runtime IL code integrity.
* `CheckStackTrace` (bool): Enable suspicious stack trace detection.
* `KnownBadProcesses` (List<string>): List of blacklisted process name substrings.
* `SpeedHackThreshold` (float): Allowed time difference before detection.
* `QuitOnDetection` (bool): Whether to quit application on detection.

Modify these values before calling `AXIOM.Initialize()`.

## How It Works

AXIOM runs multiple detection mechanisms simultaneously using Unity lifecycle methods:

* Assembly hashes ensure core code is unmodified.
* Debugger presence is detected through native calls and managed APIs.
* Injected assemblies are detected by scanning all loaded assemblies for suspicious names.
* External processes are periodically scanned for known cheat/debugger tool names.
* Time discrepancies between Unity's time and system time reveal speedhacks.
* Decoy memory allocations make memory scanning harder.
* Runtime IL hashing ensures method bodies are not altered in memory.
* Stack traces are inspected for known debugging/hooking tool signatures.

On detection of any tampering, AXIOM can optionally terminate the game to prevent further abuse.

## Limitations

* Designed for Unity 6 and above.
* Not a complete solution against advanced kernel-level cheats or hardware hacks.
* Intended as a starting point for open-source anti-cheat development.
* Should be combined with server-side validation for online games.

## License

MIT License. See `LICENSE` file for details.

## Contributing

Contributions are welcome. Please follow these guidelines:

* Use clear and modular code.
* Write meaningful commit messages.
* Provide tests for new features or bug fixes.
* Avoid dependencies on external libraries.

## Contact

No official support. For inquiries or contributions, please use the GitHub repository issues and pull requests.