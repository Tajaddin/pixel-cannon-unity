# 🎮 Pixel Cannon Game

A mobile puzzle game where players use colored cannons to fill in pixel art pictures.

## 📋 Table of Contents
1. [Game Overview](#game-overview)
2. [Installation](#installation)
3. [Project Setup](#project-setup)
4. [How to Run](#how-to-run)
5. [Creating Levels](#creating-levels)
6. [Monetization Setup](#monetization-setup)
7. [Publishing to App Store](#publishing-to-app-store)

---

## 🎯 Game Overview

**Core Gameplay:**
- Players see a grayscale pixel art image
- Colored cannons appear at the bottom of the screen
- Players tap/drag cannons to cannon slots (4-5 slots)
- When slots are full, cannons fire and fill matching colored pixels
- Complete the picture to win!

**Difficulty Levels:**
- Easy: 1-2 colors, simple shapes
- Medium: 3-4 colors, moderate complexity
- Hard: 5-6 colors, detailed images
- Super Hard: 6+ colors, complex artwork

---

## 💻 Installation

### Step 1: Install Unity

1. Go to https://unity.com/download
2. Download **Unity Hub**
3. Install Unity Hub and sign in/create account
4. In Unity Hub, go to **Installs** → **Install Editor**
5. Select **Unity 2022.3 LTS** (or newer LTS version)
6. Check these modules:
   - ✅ iOS Build Support (for iPhone)
   - ✅ Android Build Support (optional)
   - ✅ Visual Studio / VS Code

### Step 2: Install Additional Tools (for iOS)

**On Mac:**
1. Install Xcode from App Store
2. Open Xcode → Preferences → Locations → Set Command Line Tools
3. Create an Apple Developer account ($99/year for publishing)

**On Windows:**
- You'll need a Mac or Mac VM for final iOS builds
- Can develop and test in Unity on Windows

---

## 🚀 Project Setup

### Step 1: Create New Unity Project

1. Open Unity Hub
2. Click **New Project**
3. Select **2D (Core)** template
4. Name it "PixelCannonGame"
5. Click **Create Project**

### Step 2: Import Game Scripts

1. In Unity, go to **Assets** folder in Project window
2. Right-click → **Create** → **Folder** → name it "Scripts"
3. Copy ALL `.cs` files from this package into the Scripts folder:
   - GameManager.cs
   - PixelGrid.cs
   - Cannon.cs (includes CannonManager)
   - LevelData.cs
   - UIManager.cs
   - MainMenu.cs
   - AudioManager.cs
   - AdManager.cs
   - SceneSetup.cs

### Step 3: Import Resources

1. Create folder: Assets → Create → Folder → "Resources"
2. Inside Resources, create folder "Levels"
3. Inside Levels, create folders: "Easy", "Medium", "Hard", "SuperHard"
4. Copy the sample JSON level files into Easy folder

### Step 4: Setup Game Scene

**Method 1: Automatic Setup (Recommended)**
1. In Unity, go to File → New Scene
2. Save it as "GameScene" in Assets/Scenes
3. Create empty GameObject: Right-click in Hierarchy → Create Empty
4. Name it "SceneSetup"
5. Drag SceneSetup.cs onto it
6. In Inspector, click **"Setup Game Scene"** button
7. Delete the SceneSetup object after setup

**Method 2: Manual Setup**
1. Create these GameObjects:
   - GameManager (attach GameManager.cs)
   - PixelGrid (attach PixelGrid.cs)
   - CannonManager (attach Cannon.cs → CannonManager component)
   - Canvas with UIManager
   - AudioManager (attach AudioManager.cs)

### Step 5: Setup Main Menu Scene

1. File → New Scene
2. Save as "MainMenu" in Assets/Scenes
3. Create empty GameObject, add SceneSetup.cs
4. Click **"Setup Main Menu Scene"**
5. Delete SceneSetup object

### Step 6: Configure Build Settings

1. File → Build Settings
2. Add both scenes (drag from Assets/Scenes):
   - MainMenu (set as index 0)
   - GameScene (set as index 1)
3. Select **iOS** platform
4. Click **Switch Platform**

---

## ▶️ How to Run

### In Unity Editor (Testing)

1. Open GameScene
2. Click the **Play** button (▶️) at top of Unity
3. Game will run in Game view
4. Click cannons to select them
5. When 5 cannons are in slots, they fire automatically

### On iPhone (Device Testing)

1. Connect iPhone via USB
2. File → Build Settings → iOS → Build
3. Choose a folder for Xcode project
4. Open the generated `.xcodeproj` in Xcode
5. Select your iPhone as target device
6. Click Run (▶️) in Xcode
7. Trust the developer certificate on your iPhone if prompted

---

## 🎨 Creating Levels

### Level JSON Structure

```json
{
    "levelId": 0,
    "levelName": "Level Name",
    "difficulty": 0,
    "width": 20,
    "height": 20,
    "optimalMoves": 5,
    "availableColors": [1, 2, 3],
    "pixelData": [
        {"x": 0, "y": 0, "color": 1},
        {"x": 1, "y": 0, "color": 2}
    ]
}
```

### Color Codes

| Code | Color  |
|------|--------|
| 0    | Empty  |
| 1    | Red    |
| 2    | Blue   |
| 3    | Green  |
| 4    | Yellow |
| 5    | Purple |
| 6    | Orange |
| 7    | Pink   |
| 8    | Cyan   |
| 9    | White  |
| 10   | Black  |

### Difficulty Values
- 0 = Easy
- 1 = Medium
- 2 = Hard
- 3 = SuperHard

### Level Generator Tool (Python Script)

Create a file `level_generator.py`:

```python
import json
from PIL import Image
import os

def image_to_level(image_path, level_id, difficulty, output_path):
    img = Image.open(image_path).convert('RGBA')
    width, height = img.size
    
    # Color mapping
    color_map = {
        (255, 0, 0): 1,      # Red
        (0, 0, 255): 2,      # Blue
        (0, 255, 0): 3,      # Green
        (255, 255, 0): 4,    # Yellow
        (128, 0, 128): 5,    # Purple
        (255, 128, 0): 6,    # Orange
        (255, 192, 203): 7,  # Pink
        (0, 255, 255): 8,    # Cyan
        (255, 255, 255): 9,  # White
        (0, 0, 0): 10        # Black
    }
    
    pixels = []
    used_colors = set()
    
    for y in range(height):
        for x in range(width):
            r, g, b, a = img.getpixel((x, y))
            if a > 128:  # Not transparent
                color = find_closest_color((r, g, b), color_map)
                if color:
                    pixels.append({"x": x, "y": height - 1 - y, "color": color})
                    used_colors.add(color)
    
    level = {
        "levelId": level_id,
        "levelName": f"Level {level_id + 1}",
        "difficulty": difficulty,
        "width": width,
        "height": height,
        "optimalMoves": len(pixels) // 20 + len(used_colors),
        "availableColors": list(used_colors),
        "pixelData": pixels
    }
    
    with open(output_path, 'w') as f:
        json.dump(level, f, indent=2)
    
    print(f"Created level with {len(pixels)} pixels and {len(used_colors)} colors")

def find_closest_color(rgb, color_map):
    min_dist = float('inf')
    closest = None
    
    for ref_color, code in color_map.items():
        dist = sum((a - b) ** 2 for a, b in zip(rgb, ref_color))
        if dist < min_dist:
            min_dist = dist
            closest = code
    
    return closest if min_dist < 50000 else None

# Usage
# image_to_level("heart.png", 0, 0, "Assets/Resources/Levels/Easy/0.json")
```

---

## 💰 Monetization Setup

### Option 1: Unity Ads

1. Window → General → Services
2. Link/create Unity project ID
3. Enable Ads service
4. In AdManager.cs, uncomment Unity Ads code
5. Replace placeholders with your Ad Unit IDs

### Option 2: Google AdMob

1. Download AdMob SDK: https://developers.google.com/admob/unity/start
2. Import the .unitypackage
3. Create AdMob account and get App ID
4. In AdManager.cs, uncomment AdMob code
5. Add your App ID and Ad Unit IDs

### In-App Purchases

1. Window → General → Services
2. Enable In-App Purchasing
3. Configure products in Unity Dashboard
4. Uncomment IAP code in AdManager.cs

### Monetization Strategy

```
FREE VERSION:
├── Interstitial ads every 3 levels
├── Banner ad on main menu
├── Rewarded video for hints/extra cannons
└── Energy system (5 lives, regenerate over time)

PREMIUM ($2.99):
├── Remove all ads
├── Unlimited lives
└── Unlock all levels
```

---

## 📱 Publishing to App Store

### Step 1: Prepare App

1. Create app icons (1024x1024 master)
2. Take screenshots (6.5", 5.5", iPad sizes)
3. Write app description
4. Set age rating (likely 4+)

### Step 2: Configure in Unity

1. Edit → Project Settings → Player
2. iOS tab settings:
   - Company Name: Your name
   - Product Name: Pixel Cannon
   - Bundle Identifier: com.yourname.pixelcannon
   - Version: 1.0.0
   - Signing Team ID: Your Apple Team ID
   - Icon: Set app icons

### Step 3: Build for iOS

1. File → Build Settings
2. Click **Build**
3. Save Xcode project

### Step 4: In Xcode

1. Open `.xcodeproj`
2. Select your Team in Signing
3. Set Deployment Target (iOS 13.0+)
4. Product → Archive
5. Distribute App → App Store Connect

### Step 5: App Store Connect

1. Go to appstoreconnect.apple.com
2. Create new App
3. Fill in all metadata
4. Upload build from Xcode
5. Submit for review

---

## 📁 Project Structure

```
PixelCannonGame/
├── Assets/
│   ├── Scripts/
│   │   ├── GameManager.cs
│   │   ├── PixelGrid.cs
│   │   ├── Cannon.cs
│   │   ├── LevelData.cs
│   │   ├── UIManager.cs
│   │   ├── MainMenu.cs
│   │   ├── AudioManager.cs
│   │   ├── AdManager.cs
│   │   └── SceneSetup.cs
│   ├── Scenes/
│   │   ├── MainMenu.unity
│   │   └── GameScene.unity
│   ├── Resources/
│   │   └── Levels/
│   │       ├── Easy/
│   │       ├── Medium/
│   │       ├── Hard/
│   │       └── SuperHard/
│   ├── Prefabs/
│   ├── Sprites/
│   └── UI/
└── ProjectSettings/
```

---

## 🐛 Troubleshooting

**"Script not found" error:**
- Make sure all .cs files are in Assets/Scripts
- Check for compilation errors in Console

**Scene is empty:**
- Use SceneSetup tool to create objects
- Or manually create required GameObjects

**Build fails on iOS:**
- Check Xcode Command Line Tools installed
- Verify Bundle Identifier is unique
- Make sure Signing Team is set

**Ads not showing:**
- Ads don't work in Editor - test on device
- Check ad unit IDs are correct
- Verify SDK is properly initialized

---

## 📞 Support

For questions or issues, check:
- Unity Documentation: https://docs.unity3d.com
- Unity Forums: https://forum.unity.com
- Stack Overflow: Tag [unity3d]

---

## 📜 License

This project template is provided for educational purposes.
Feel free to modify and use for your own games.

Good luck with your game! 🚀🎮
