# MainMenu Scene Configuration Guide

## Scene Hierarchy Structure

```
MainMenuScene
├── Canvas (UI)
│   ├── MainMenuPanel
│   │   ├── Background Image
│   │   ├── Title Text ("Filter Quest")
│   │   ├── StartButton
│   │   ├── LevelSelectButton
│   │   ├── SettingsButton
│   │   └── QuitButton
│   ├── LevelSelectPanel
│   │   ├── BackButton
│   │   ├── ScrollView
│   │   │   └── Content
│   │   │       └── LevelButtonContainer (Grid Layout)
│   │   └── Title Text ("Select Level")
│   ├── SettingsPanel
│   │   ├── BackButton
│   │   ├── Title Text ("Settings")
│   │   ├── Audio Settings
│   │   │   ├── Master Volume Slider
│   │   │   ├── Music Volume Slider
│   │   │   └── SFX Volume Slider
│   │   ├── Graphics Settings
│   │   │   ├── Fullscreen Toggle
│   │   │   └── Quality Dropdown
│   │   └── Controls Info
│   └── CreditsPanel
├── BackgroundManager
│   ├── Layer0_Sky (Sprite Renderer)
│   ├── Layer1_Mountains (Sprite Renderer)
│   ├── Layer2_Trees (Sprite Renderer)
│   └── Layer3_Foreground (Sprite Renderer)
├── GameManager
├── SaveManager
├── UIManager
├── AudioManager
└── Main Camera
```

## Component Configurations

### 1. Canvas Setup
```
Canvas Component:
- Render Mode: Screen Space - Overlay
- UI Scale Mode: Scale With Screen Size
- Reference Resolution: 1920x1080
- Screen Match Mode: Match Width Or Height
- Match: 0.5

Canvas Scaler:
- UI Scale Mode: Scale With Screen Size
- Reference Resolution: 1920x1080
```

### 2. UIManager Configuration
```csharp
UIManager Component Settings:

UI Panels:
[0] MainMenuPanel
    - Panel Name: "MainMenu"
    - Panel Object: MainMenuPanel GameObject
    - Start Active: true
    - Pause Game When Active: false
    - Toggle Key: None

[1] LevelSelectPanel
    - Panel Name: "LevelSelect"
    - Panel Object: LevelSelectPanel GameObject
    - Start Active: false
    - Pause Game When Active: false
    - Toggle Key: None

[2] SettingsPanel
    - Panel Name: "Settings"
    - Panel Object: SettingsPanel GameObject
    - Start Active: false
    - Pause Game When Active: false
    - Toggle Key: Escape

Menu Buttons:
- Start Button: StartButton GameObject
- Level Select Button: LevelSelectButton GameObject
- Settings Button: SettingsButton GameObject
- Quit Button: QuitButton GameObject

Settings UI:
- Master Volume Slider: MasterVolumeSlider GameObject
- Music Volume Slider: MusicVolumeSlider GameObject
- SFX Volume Slider: SFXVolumeSlider GameObject
- Fullscreen Toggle: FullscreenToggle GameObject
- Quality Dropdown: QualityDropdown GameObject

Level Selection:
- Level Button Container: LevelButtonContainer GameObject
- Level Button Prefab: LevelButtonPrefab (Create as prefab)
```

### 3. BackgroundManager Configuration
```csharp
BackgroundManager Component Settings:

Background Layers:
[0] Sky Layer
    - Layer Name: "Sky"
    - Layer Object: Layer0_Sky GameObject
    - Parallax Speed: 0.1f
    - Enable Parallax: true
    - Enable Animation: false
    - Affected By Filters: false

[1] Mountains Layer
    - Layer Name: "Mountains"
    - Layer Object: Layer1_Mountains GameObject
    - Parallax Speed: 0.3f
    - Enable Parallax: true
    - Enable Animation: false
    - Affected By Filters: false

[2] Trees Layer
    - Layer Name: "Trees"
    - Layer Object: Layer2_Trees GameObject
    - Parallax Speed: 0.6f
    - Enable Parallax: true
    - Enable Animation: true
    - Animation Speed: 0.5f
    - Animation Direction: (0.2, 0)
    - Affected By Filters: false

Parallax Settings:
- Parallax Target: Main Camera Transform
- Enable Global Parallax: true
- Layer Transition Speed: 2f
```

### 4. GameManager Configuration
```csharp
GameManager Component Settings:

Game Settings:
- Pause On Start: false
- Pause Key: Escape

Level Management:
- Level Scenes:
  [0] "MainMenu"
  [1] "Level01"
  [2] "Level02"
  [3] "Level03"
- Current Level Index: 0
```

### 5. Button Event Setup

#### StartButton OnClick Events:
```csharp
// Method 1: Direct scene load
UnityEngine.SceneManagement.SceneManager.LoadScene("Level01");

// Method 2: Through GameManager (Recommended)
GameManager.Instance.LoadLevel(1);
```

#### LevelSelectButton OnClick Events:
```csharp
UIManager.Instance.ShowPanel("LevelSelect");
```

#### SettingsButton OnClick Events:
```csharp
UIManager.Instance.ShowPanel("Settings");
```

#### QuitButton OnClick Events:
```csharp
Application.Quit();
```

## UI Styling Guidelines

### Color Scheme:
- Primary: #2E86AB (Blue)
- Secondary: #A23B72 (Purple)
- Accent: #F18F01 (Orange)
- Background: #C73E1D (Dark Red)
- Text: #FFFFFF (White)

### Button Styling:
```
Normal Color: #2E86AB
Highlighted Color: #A23B72
Pressed Color: #F18F01
Disabled Color: #666666
Color Multiplier: 1.0
Fade Duration: 0.1
```

### Text Styling:
```
Title Text:
- Font Size: 72
- Color: White
- Alignment: Center
- Font Style: Bold

Button Text:
- Font Size: 36
- Color: White
- Alignment: Center
- Font Style: Normal

Label Text:
- Font Size: 24
- Color: White
- Alignment: Left
- Font Style: Normal
```

## Audio Configuration

### AudioManager Setup (if using):
```csharp
Background Music:
- Clip: MainMenuMusic.mp3
- Volume: 0.7f
- Loop: true
- Play On Awake: true

UI Sound Effects:
- Button Click: ButtonClick.wav
- Button Hover: ButtonHover.wav
- Panel Open: PanelOpen.wav
- Panel Close: PanelClose.wav
```

## Scene Build Settings

### Build Index: 0
### Scene Name: "MainMenu"
### Platform Settings:
- Target Platform: PC, Mac & Linux Standalone
- Architecture: x86_64
- Scripting Backend: Mono