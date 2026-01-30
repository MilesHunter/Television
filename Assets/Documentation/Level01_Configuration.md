# Level01 Scene Configuration Guide

## Scene Hierarchy Structure

```
Level01Scene
├── Canvas (UI)
│   ├── HUD
│   │   ├── TopBar
│   │   │   ├── LevelText
│   │   │   ├── ScoreText
│   │   │   ├── TimeText
│   │   │   └── PauseButton
│   │   ├── BottomBar
│   │   │   ├── FilterCountText
│   │   │   ├── HealthBar
│   │   │   └── ControlsHint
│   │   └── FilterIndicator (Shows current held filter)
│   ├── PauseMenu
│   │   ├── Background Panel
│   │   ├── Title Text ("Paused")
│   │   ├── ResumeButton
│   │   ├── RestartButton
│   │   ├── SettingsButton
│   │   └── MainMenuButton
│   ├── GameOverScreen
│   │   ├── Background Panel
│   │   ├── Title Text ("Game Over")
│   │   ├── FinalScoreText
│   │   ├── RestartButton
│   │   └── MainMenuButton
│   └── VictoryScreen
│       ├── Background Panel
│       ├── Title Text ("Level Complete!")
│       ├── StatsPanel
│       │   ├── TimeText
│       │   ├── ScoreText
│       │   └── FiltersUsedText
│       ├── NextLevelButton
│       └── MainMenuButton
├── Level Environment
│   ├── Ground
│   │   ├── Platform_01 (Sprite + Collider)
│   │   ├── Platform_02 (Sprite + Collider)
│   │   ├── Platform_03 (Sprite + Collider)
│   │   └── MovingPlatform (Sprite + Collider + Animation)
│   ├── Walls
│   │   ├── LeftWall (Sprite + Collider)
│   │   ├── RightWall (Sprite + Collider)
│   │   └── Ceiling (Sprite + Collider)
│   ├── Hazards
│   │   ├── Spikes_01 (Sprite + Collider + Damage)
│   │   └── Lava_Pool (Sprite + Collider + Damage)
│   └── Decorations
│       ├── Tree_01 (Sprite)
│       ├── Rock_01 (Sprite)
│       └── Grass_Patches (Sprite)
├── Filter System
│   ├── FilterSystemManager
│   ├── Filters
│   │   ├── RedFilter_01 (FilterController + Sprite + Collider)
│   │   ├── GreenFilter_01 (FilterController + Sprite + Collider)
│   │   ├── BlueFilter_01 (FilterController + Sprite + Collider)
│   │   └── RedFilter_02 (FilterController + Sprite + Collider)
│   └── RevealableObjects
│       ├── HiddenPlatform_Red (RevealableObject + Sprite + Collider)
│       ├── HiddenPlatform_Green (RevealableObject + Sprite + Collider)
│       ├── HiddenPlatform_Mixed (RevealableObject + Sprite + Collider)
│       ├── SecretDoor_Blue (RevealableObject + Sprite + Collider)
│       └── TreasureChest (RevealableObject + Sprite + Collider)
├── Player
│   ├── PlayerSprite (Sprite Renderer)
│   ├── PlayerCollider (Capsule Collider 2D)
│   ├── PlayerRigidbody (Rigidbody 2D)
│   ├── GroundCheck (Empty GameObject)
│   └── PlayerController Script
├── BackgroundManager
│   ├── Layer0_Sky (Sprite Renderer)
│   ├── Layer1_Mountains (Sprite Renderer)
│   ├── Layer2_Clouds (Sprite Renderer)
│   └── Layer3_Trees (Sprite Renderer)
├── Collectibles
│   ├── Coin_01 (Sprite + Collider + Animation)
│   ├── Coin_02 (Sprite + Collider + Animation)
│   ├── PowerUp_01 (Sprite + Collider + Animation)
│   └── Key_01 (Sprite + Collider + Animation)
├── Audio
│   ├── BackgroundMusic (Audio Source)
│   ├── AmbientSounds (Audio Source)
│   └── SFXManager (Audio Sources Pool)
├── Lighting
│   ├── DirectionalLight (Main Light)
│   ├── PointLight_01 (Accent Light)
│   └── AreaLight_01 (Ambient Light)
├── GameManager
├── SaveManager
├── UIManager
└── Main Camera
    ├── CameraController (Follow Player)
    └── CinemachineVirtualCamera (Optional)
```

## Detailed Component Configurations

### 1. Player Setup

#### PlayerController Configuration:
```csharp
PlayerController Component Settings:

Movement Settings:
- Move Speed: 8.0f
- Jump Force: 15.0f

Ground Detection:
- Ground Check: GroundCheck Transform
- Ground Check Radius: 0.3f
- Ground Layer Mask: Ground (Layer 8)

Item Pickup:
- Pickup Range: 2.5f
- Item Tag: "Filter"
- Held Item Offset: (0, 1.0f, 0)
```

#### Player GameObject Setup:
```
Player GameObject:
- Position: (0, 2, 0)
- Scale: (1, 1, 1)

Components:
- Transform
- Sprite Renderer (Player Sprite)
- Rigidbody2D:
  * Mass: 1
  * Linear Drag: 0
  * Angular Drag: 0.05
  * Gravity Scale: 3
  * Freeze Rotation Z: true
- Capsule Collider 2D:
  * Size: (0.8, 1.6)
  * Offset: (0, 0)
- PlayerController Script

GroundCheck Child Object:
- Position: (0, -0.8, 0)
- No components needed (just Transform)
```

### 2. Filter System Setup

#### FilterSystemManager Configuration:
```csharp
FilterSystemManager Component Settings:

System Settings:
- Grid Cell Size: 2.0f
- Max Filter Range: 10.0f
- Update Frequency: 0.1f
- Enable Debug Visualization: true (for development)

Performance Settings:
- Use Spatial Grid: true
- Cache Results: true
- Max Objects Per Frame: 50

Debug Settings:
- Show Filter Ranges: true
- Show Grid: true
- Show Object States: true
```

#### Filter Examples:

##### RedFilter_01 Configuration:
```csharp
FilterController Component Settings:

Filter Settings:
- Filter Type: Red
- Filter Range: 5.0f
- Filter Shape: Circle
- Can Be Picked Up: true

Visual Settings:
- Filter Color: (1, 0.2, 0.2, 0.7) // Semi-transparent red
- Range Indicator Color: (1, 0, 0, 0.3)
- Show Range When Held: true

GameObject Setup:
- Position: (5, 1, 0)
- Sprite: RedFilterSprite
- Collider: Circle Collider 2D (Radius: 0.5, Is Trigger: true)
- Layer: Filter (Layer 9)
- Tag: "Filter"
```

##### GreenFilter_01 Configuration:
```csharp
FilterController Component Settings:

Filter Settings:
- Filter Type: Green
- Filter Range: 4.0f
- Filter Shape: Circle
- Can Be Picked Up: true

Visual Settings:
- Filter Color: (0.2, 1, 0.2, 0.7) // Semi-transparent green
- Range Indicator Color: (0, 1, 0, 0.3)
- Show Range When Held: true

GameObject Setup:
- Position: (12, 3, 0)
- Sprite: GreenFilterSprite
- Collider: Circle Collider 2D (Radius: 0.5, Is Trigger: true)
- Layer: Filter (Layer 9)
- Tag: "Filter"
```

#### Revealable Objects Examples:

##### HiddenPlatform_Red Configuration:
```csharp
RevealableObject Component Settings:

Visibility Settings:
- Required Filters: Red
- Require All Filters: true
- Default Visible: false

Transition Settings:
- Transition Speed: 3.0f
- Transition Curve: EaseInOut
- Fade Material: true
- Disable Collider When Hidden: true

GameObject Setup:
- Position: (8, 4, 0)
- Sprite: PlatformSprite
- Collider: Box Collider 2D (Size: (3, 0.5))
- Layer: Platform (Layer 10)
- Initial Alpha: 0 (invisible)
```

##### HiddenPlatform_Mixed Configuration:
```csharp
RevealableObject Component Settings:

Visibility Settings:
- Required Filters: Red | Green (Both required)
- Require All Filters: true
- Default Visible: false

Transition Settings:
- Transition Speed: 2.0f
- Transition Curve: EaseInOut
- Fade Material: true
- Disable Collider When Hidden: true

GameObject Setup:
- Position: (15, 6, 0)
- Sprite: SpecialPlatformSprite
- Collider: Box Collider 2D (Size: (4, 0.5))
- Layer: Platform (Layer 10)
- Initial Alpha: 0 (invisible)
```

##### SecretDoor_Blue Configuration:
```csharp
RevealableObject Component Settings:

Visibility Settings:
- Required Filters: Blue
- Require All Filters: true
- Default Visible: false

Transition Settings:
- Transition Speed: 1.5f
- Transition Curve: EaseInOut
- Fade Material: false
- Disable Collider When Hidden: true
- Custom Reveal Animation: SlideUp

GameObject Setup:
- Position: (20, 2, 0)
- Sprite: DoorSprite
- Collider: Box Collider 2D (Size: (1, 3))
- Layer: Interactive (Layer 11)
- Initial Position: (20, -1, 0) // Hidden below ground
```

### 3. Level Environment Setup

#### Platform Configuration:
```
Platform_01:
- Position: (0, 0, 0)
- Scale: (6, 1, 1)
- Sprite: GroundPlatformSprite
- Box Collider 2D: Size (6, 1)
- Layer: Ground (Layer 8)

Platform_02:
- Position: (10, 2, 0)
- Scale: (4, 1, 1)
- Sprite: GroundPlatformSprite
- Box Collider 2D: Size (4, 1)
- Layer: Ground (Layer 8)

MovingPlatform:
- Position: (18, 3, 0)
- Scale: (3, 1, 1)
- Sprite: MovingPlatformSprite
- Box Collider 2D: Size (3, 1)
- Layer: Ground (Layer 8)
- Animation: Move between (18, 3, 0) and (18, 7, 0) over 3 seconds
```

### 4. BackgroundManager Configuration

```csharp
BackgroundManager Component Settings:

Background Layers:
[0] Sky Layer
    - Layer Name: "Sky"
    - Layer Object: Layer0_Sky
    - Parallax Speed: 0.0f (Static)
    - Enable Parallax: false
    - Affected By Filters: false

[1] Mountains Layer
    - Layer Name: "Mountains"
    - Layer Object: Layer1_Mountains
    - Parallax Speed: 0.2f
    - Enable Parallax: true
    - Affected By Filters: false

[2] Clouds Layer
    - Layer Name: "Clouds"
    - Layer Object: Layer2_Clouds
    - Parallax Speed: 0.5f
    - Enable Parallax: true
    - Enable Animation: true
    - Animation Speed: 0.3f
    - Animation Direction: (1, 0)
    - Affected By Filters: false

[3] Trees Layer
    - Layer Name: "Trees"
    - Layer Object: Layer3_Trees
    - Parallax Speed: 0.8f
    - Enable Parallax: true
    - Affected By Filters: true
    - Required Filters: Green
    - Require All Filters: false

Parallax Settings:
- Parallax Target: Main Camera
- Enable Global Parallax: true
- Layer Transition Speed: 2.0f
```

### 5. UIManager Configuration

```csharp
UIManager Component Settings:

UI Panels:
[0] HUD
    - Panel Name: "HUD"
    - Panel Object: HUD GameObject
    - Start Active: true
    - Pause Game When Active: false

[1] PauseMenu
    - Panel Name: "PauseMenu"
    - Panel Object: PauseMenu GameObject
    - Start Active: false
    - Pause Game When Active: true
    - Toggle Key: Escape

[2] GameOverScreen
    - Panel Name: "GameOver"
    - Panel Object: GameOverScreen GameObject
    - Start Active: false
    - Pause Game When Active: true

[3] VictoryScreen
    - Panel Name: "Victory"
    - Panel Object: VictoryScreen GameObject
    - Start Active: false
    - Pause Game When Active: true

HUD Elements:
- Level Text: LevelText GameObject
- Score Text: ScoreText GameObject
- Time Text: TimeText GameObject
- Health Bar: HealthBar Slider GameObject
- Filter Count Text: FilterCountText GameObject

Menu Buttons:
- Pause Button: PauseButton GameObject
- Resume Button: ResumeButton GameObject
- Restart Button: RestartButton GameObject
- Main Menu Button: MainMenuButton GameObject
- Next Level Button: NextLevelButton GameObject
```

### 6. Camera Setup

#### Main Camera Configuration:
```
Main Camera:
- Position: (0, 2, -10)
- Projection: Orthographic
- Size: 8
- Clipping Planes: Near 0.3, Far 1000
- Culling Mask: Everything except UI
- Background: Solid Color (Sky Blue)

Camera Follow Script (Optional):
- Target: Player Transform
- Follow Speed: 5.0f
- Look Ahead Distance: 2.0f
- Vertical Offset: 1.0f
- Boundary Limits:
  * Left: -5
  * Right: 25
  * Bottom: 0
  * Top: 15
```

### 7. Audio Configuration

```csharp
Background Music:
- Audio Clip: Level01_BGM.mp3
- Volume: 0.6f
- Loop: true
- Play On Awake: true

Ambient Sounds:
- Audio Clip: Forest_Ambience.mp3
- Volume: 0.3f
- Loop: true
- Play On Awake: true

SFX Examples:
- Jump Sound: Jump.wav
- Land Sound: Land.wav
- Filter Pickup: FilterPickup.wav
- Filter Drop: FilterDrop.wav
- Platform Reveal: PlatformReveal.wav
- Coin Collect: CoinCollect.wav
```

### 8. Collectibles Setup

#### Coin Configuration:
```
Coin_01:
- Position: (3, 2, 0)
- Sprite: CoinSprite
- Circle Collider 2D: Radius 0.5, Is Trigger: true
- Animation: Rotate 360° over 2 seconds, loop
- Layer: Collectible (Layer 12)
- Tag: "Coin"
- Value: 100 points

PowerUp_01:
- Position: (16, 4, 0)
- Sprite: PowerUpSprite
- Circle Collider 2D: Radius 0.7, Is Trigger: true
- Animation: Float up/down 0.5 units over 1.5 seconds
- Layer: Collectible (Layer 12)
- Tag: "PowerUp"
- Effect: Double jump ability for 30 seconds
```

## Level Design Guidelines

### Filter Puzzle Examples:

1. **Basic Red Filter Puzzle:**
   - Place RedFilter_01 at start of level
   - Create HiddenPlatform_Red that requires red filter to cross gap
   - Teaches basic filter mechanics

2. **Filter Combination Puzzle:**
   - Player needs both Red and Green filters
   - HiddenPlatform_Mixed only appears with both filters
   - Requires filter management and planning

3. **Filter Exchange Puzzle:**
   - Multiple filters available but can only hold one
   - Strategic placement requires dropping and picking up different filters
   - Tests player understanding of filter system

### Difficulty Progression:
- **Early Level:** Simple single-filter reveals
- **Mid Level:** Filter combinations and exchanges
- **Late Level:** Complex multi-filter sequences with timing

## Layer Setup

```
Layer Configuration:
0: Default
1: TransparentFX
2: Ignore Raycast
3: (Unused)
4: Water
5: UI
6: (Unused)
7: (Unused)
8: Ground
9: Filter
10: Platform
11: Interactive
12: Collectible
13: Player
14: Enemy
15: Background
```

## Physics Settings

```
Physics2D Settings:
- Gravity: (0, -20)
- Default Material: None
- Velocity Iterations: 8
- Position Iterations: 3
- Velocity Threshold: 1
- Max Linear Correction: 0.2
- Max Angular Correction: 8
- Max Translation Speed: 100
- Max Rotation Speed: 360
```

## Build Settings

### Scene Build Index: 1
### Scene Name: "Level01"
### Required Prefabs:
- FilterEffectPrefab (for visual effects)
- ParticleSystemPrefab (for pickup effects)
- UIButtonPrefab (for dynamic UI elements)