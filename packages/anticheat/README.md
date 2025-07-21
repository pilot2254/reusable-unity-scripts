# AXIOM - AntiCheat Utilities for your Unity games.

A set of scripts designed to detect and prevent common cheating techniques in Unity games. This anti-cheat system aims to enhance game security by monitoring and mitigating unauthorized modifications and debugging attempts.

> AXIOM - a statement that is taken to be true, to serve as a premise or starting point for further reasoning and arguments.

## Features

- **AssemblyIntegrityChecker**: Verifies the integrity of loaded assemblies to detect tampering.
- **DebuggerDetector**: Detects attached debuggers to prevent runtime debugging.
- **MonoInjectionDetector**: Identifies attempts to inject code into the Mono runtime.
- **ProcessWatcher**: Monitors suspicious external processes.
- **SpeedHackDetector**: Detects manipulation of the game's time scale or speed hacks.
- **AntiCheatConfig**: Central configuration for anti-cheat behavior.

## Usage

1. Import all anti-cheat scripts into your Unity project.
2. Add the relevant components to a persistent GameObject early in your game's lifecycle.
3. Configure settings in `AntiCheatConfig` to customize detection and response behaviors.
4. Monitor console logs or hook into events for detected cheat attempts.

## Notes

- Designed primarily for Unity 6; compatibility with other versions may require adjustments.
- This anti-cheat system does not guarantee full protection but raises the bar against common cheating methods.
- Regular updates and tuning are recommended to adapt to new cheating techniques.

## License

MIT License

## Contact

Discord: @michal.flaska or @pilot2254
Or simply [open an issue](https://github.com/pilot2254/reusable-unity-scripts/issues).