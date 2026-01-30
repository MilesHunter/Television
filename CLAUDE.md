# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a Unity 2022.3.62f2c1 project named "Television" - currently in early development with minimal content. The project uses Unity's 2D feature set and includes standard Unity packages for UI, Timeline, TextMeshPro, and testing.

## Project Structure

```
Television/
├── Assets/
│   ├── Art/           # Art assets (currently empty)
│   ├── Script/        # C# scripts (currently empty)
│   └── Scenes/        # Unity scenes (contains SampleScene.unity)
├── ProjectSettings/   # Unity project configuration
├── Packages/          # Unity package dependencies
└── Library/           # Unity-generated files (ignored by git)
```

## Development Commands

### Unity Editor
- Open the project by opening the `Television` folder in Unity Hub or Unity Editor
- The project is configured for Unity 2022.3.62f2c1

### Building
- Use Unity Editor's Build Settings (File > Build Settings) to configure and build
- No custom build scripts are currently configured

### Testing
- Unity Test Framework is included (`com.unity.test-framework@1.1.33`)
- Run tests via Unity Editor: Window > General > Test Runner
- No custom test scripts exist yet

### IDE Integration
- Visual Studio integration is configured (`.vsconfig` includes `Microsoft.VisualStudio.Workload.ManagedGame`)
- JetBrains Rider support is available via `com.unity.ide.rider@3.0.36`

## Unity Package Dependencies

Key packages included:
- **2D Feature Set** (`com.unity.feature.2d@2.0.1`) - 2D game development tools
- **UI System** (`com.unity.ugui@1.0.0`) - Unity's UI system
- **TextMeshPro** (`com.unity.textmeshpro@3.0.7`) - Advanced text rendering
- **Timeline** (`com.unity.timeline@1.7.7`) - Cinematic and gameplay sequences
- **Test Framework** (`com.unity.test-framework@1.1.33`) - Unit and integration testing
- **Visual Scripting** (`com.unity.visualscripting@1.9.4`) - Node-based scripting

## Development Notes

- This is a fresh Unity project with standard 2D setup
- No custom scripts, assets, or architecture patterns have been established yet
- The project follows Unity's default folder structure
- Git is configured with standard Unity `.gitignore` patterns

## Getting Started

1. Ensure Unity 2022.3.62f2c1 is installed
2. Open the project folder in Unity Hub or Unity Editor
3. The SampleScene is the default scene to start development
4. Create scripts in `Assets/Script/` and art assets in `Assets/Art/`