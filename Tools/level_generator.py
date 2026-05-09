#!/usr/bin/env python3
"""
Level Generator for Pixel Cannon Game
Converts pixel art images to game level JSON files.

Requirements:
    pip install Pillow

Usage:
    python level_generator.py <image_path> <level_id> <difficulty> [output_path]
    
    difficulty: 0=Easy, 1=Medium, 2=Hard, 3=SuperHard
    
Examples:
    python level_generator.py heart.png 0 0
    python level_generator.py moon.png 5 2 Assets/Resources/Levels/Hard/5.json
"""

import json
import sys
import os
import math
from pathlib import Path

try:
    from PIL import Image
except ImportError:
    print("Error: Pillow library required. Install with: pip install Pillow")
    sys.exit(1)


# Color definitions matching the game
GAME_COLORS = {
    'Red':    (255, 0, 0),
    'Blue':   (51, 153, 255),
    'Green':  (0, 255, 0),
    'Yellow': (255, 255, 0),
    'Purple': (153, 51, 204),
    'Orange': (255, 128, 0),
    'Pink':   (255, 102, 178),
    'Cyan':   (0, 255, 255),
    'White':  (255, 255, 255),
    'Black':  (25, 25, 25),
}

COLOR_CODES = {
    'Empty':  0,
    'Red':    1,
    'Blue':   2,
    'Green':  3,
    'Yellow': 4,
    'Purple': 5,
    'Orange': 6,
    'Pink':   7,
    'Cyan':   8,
    'White':  9,
    'Black':  10,
}

DIFFICULTY_NAMES = ['Easy', 'Medium', 'Hard', 'SuperHard']


def color_distance(c1, c2):
    """Calculate Euclidean distance between two RGB colors."""
    return math.sqrt(sum((a - b) ** 2 for a, b in zip(c1, c2)))


def find_closest_color(rgb, threshold=100):
    """Find the closest game color to the given RGB value."""
    min_dist = float('inf')
    closest_name = None
    
    for name, ref_color in GAME_COLORS.items():
        dist = color_distance(rgb, ref_color)
        if dist < min_dist:
            min_dist = dist
            closest_name = name
    
    # Return None if color is too far from any game color
    if min_dist > threshold:
        return None
    
    return closest_name


def image_to_level(image_path, level_id, difficulty, output_path=None):
    """
    Convert a pixel art image to a game level JSON file.
    
    Args:
        image_path: Path to the input image
        level_id: Integer ID for the level
        difficulty: 0-3 (Easy to SuperHard)
        output_path: Optional output path for JSON file
    
    Returns:
        dict: The level data
    """
    # Load image
    img = Image.open(image_path).convert('RGBA')
    width, height = img.size
    
    print(f"Processing image: {width}x{height} pixels")
    
    # Resize if too large (max 32x32 for mobile performance)
    max_size = 32
    if width > max_size or height > max_size:
        ratio = min(max_size / width, max_size / height)
        new_width = int(width * ratio)
        new_height = int(height * ratio)
        img = img.resize((new_width, new_height), Image.Resampling.NEAREST)
        width, height = img.size
        print(f"Resized to: {width}x{height} pixels")
    
    pixels = []
    used_colors = set()
    
    # Process each pixel
    for y in range(height):
        for x in range(width):
            r, g, b, a = img.getpixel((x, y))
            
            # Skip transparent pixels
            if a < 128:
                continue
            
            # Find closest game color
            color_name = find_closest_color((r, g, b))
            
            if color_name:
                color_code = COLOR_CODES[color_name]
                # Flip Y coordinate (image Y=0 is top, game Y=0 is bottom)
                game_y = height - 1 - y
                pixels.append({
                    "x": x,
                    "y": game_y,
                    "color": color_code
                })
                used_colors.add(color_code)
    
    # Calculate optimal moves based on pixel count and colors
    avg_cannon_power = 25  # Average cannon fills 25 pixels
    base_moves = max(1, len(pixels) // avg_cannon_power)
    optimal_moves = base_moves + len(used_colors)
    
    # Build level data
    level_data = {
        "levelId": level_id,
        "levelName": f"Level {level_id + 1}",
        "difficulty": difficulty,
        "width": width,
        "height": height,
        "optimalMoves": optimal_moves,
        "availableColors": sorted(list(used_colors)),
        "pixelData": pixels
    }
    
    # Determine output path
    if output_path is None:
        difficulty_name = DIFFICULTY_NAMES[difficulty]
        output_dir = Path(f"Assets/Resources/Levels/{difficulty_name}")
        output_dir.mkdir(parents=True, exist_ok=True)
        output_path = output_dir / f"{level_id}.json"
    
    # Save JSON
    with open(output_path, 'w') as f:
        json.dump(level_data, f, indent=2)
    
    print(f"\n✅ Level created successfully!")
    print(f"   Pixels: {len(pixels)}")
    print(f"   Colors: {len(used_colors)} ({', '.join(get_color_names(used_colors))})")
    print(f"   Optimal moves: {optimal_moves}")
    print(f"   Output: {output_path}")
    
    return level_data


def get_color_names(color_codes):
    """Get color names from color codes."""
    code_to_name = {v: k for k, v in COLOR_CODES.items()}
    return [code_to_name.get(c, 'Unknown') for c in sorted(color_codes)]


def batch_convert(input_dir, difficulty, start_id=0):
    """Convert all images in a directory to levels."""
    input_path = Path(input_dir)
    image_extensions = {'.png', '.jpg', '.jpeg', '.gif', '.bmp'}
    
    images = sorted([
        f for f in input_path.iterdir() 
        if f.suffix.lower() in image_extensions
    ])
    
    print(f"Found {len(images)} images to convert")
    
    for i, image_path in enumerate(images):
        level_id = start_id + i
        print(f"\n--- Processing {image_path.name} (Level {level_id}) ---")
        try:
            image_to_level(str(image_path), level_id, difficulty)
        except Exception as e:
            print(f"Error processing {image_path}: {e}")


def create_sample_images():
    """Create sample pixel art images for testing."""
    samples_dir = Path("sample_images")
    samples_dir.mkdir(exist_ok=True)
    
    # Heart
    heart_data = [
        "..##..##..",
        ".########.",
        "##########",
        "##########",
        ".########.",
        "..######..",
        "...####...",
        "....##....",
    ]
    create_image_from_pattern(heart_data, samples_dir / "heart.png", (255, 0, 0))
    
    # Star
    star_data = [
        "....#.....",
        "...###....",
        "...###....",
        "##########",
        ".########.",
        "..######..",
        ".##..##...",
        "##....##..",
    ]
    create_image_from_pattern(star_data, samples_dir / "star.png", (255, 255, 0))
    
    print(f"Sample images created in {samples_dir}/")


def create_image_from_pattern(pattern, output_path, color):
    """Create a pixel art image from a text pattern."""
    height = len(pattern)
    width = len(pattern[0]) if pattern else 0
    
    img = Image.new('RGBA', (width, height), (0, 0, 0, 0))
    
    for y, row in enumerate(pattern):
        for x, char in enumerate(row):
            if char == '#':
                img.putpixel((x, y), (*color, 255))
    
    img.save(output_path)


def main():
    if len(sys.argv) < 2:
        print(__doc__)
        return
    
    if sys.argv[1] == '--samples':
        create_sample_images()
        return
    
    if sys.argv[1] == '--batch':
        if len(sys.argv) < 4:
            print("Usage: python level_generator.py --batch <input_dir> <difficulty>")
            return
        batch_convert(sys.argv[2], int(sys.argv[3]))
        return
    
    if len(sys.argv) < 4:
        print("Usage: python level_generator.py <image_path> <level_id> <difficulty> [output_path]")
        print("       python level_generator.py --samples  (create sample images)")
        print("       python level_generator.py --batch <input_dir> <difficulty>")
        return
    
    image_path = sys.argv[1]
    level_id = int(sys.argv[2])
    difficulty = int(sys.argv[3])
    output_path = sys.argv[4] if len(sys.argv) > 4 else None
    
    image_to_level(image_path, level_id, difficulty, output_path)


if __name__ == "__main__":
    main()
