# MonoGame Texture Atlas Packer

A lightweight C# CLI tool that packs individual sprite images into a single power-of-2 texture atlas (`atlas.png`) and generates a corresponding XML region map (`atlas.xml`) formatted for MonoGame projects.

## Features

- **Recursive Discovery:** Scans an input directory and its subdirectories for PNG sprites while ignoring existing `atlas` output folders.
- **Shelf-Packing Layout:** Sorts sprites by height and arranges them into rows with configurable padding.
- **Power-of-2 Sizing:** Automatically scales canvas dimensions to the nearest power of 2 (capped at `4096x4096`).
- **MonoGame XML Output:** Generates a `<TextureAtlas>` XML definition mapping sprite names to their `x`, `y`, `width`, and `height` coordinates.

## Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download) (8.0 or later)
- [ImageMagick](https://imagemagick.org/) *(optional — only required to run the test sprite generator script)*

## Usage

Run the `pack` command via `dotnet run` (or the compiled executable) and specify the target sprite directory:

```bash
dotnet run -- pack --folder <path/to/sprites> [--padding <size>]
```

### Options

- `--folder <path/to/sprites>` **(Required):** Path to the root directory containing your raw `.png` sprites.
- `--padding <size>` *(Optional)*: Pixel spacing added between sprites and around the atlas borders.

### Output

The tool creates an `atlas/` subfolder inside your target sprite directory containing two files:

- `atlas.png` — The composited power-of-2 sprite sheet.
- `atlas.xml` — The coordinate lookup map for MonoGame:

```xml
<?xml version="1.0" encoding="utf-8"?>
<TextureAtlas>
  <Texture>images/atlas</Texture>
  <Regions>
    <Region name="player_idle_01" x="4" y="4" width="32" height="48" />
    <!-- ... -->
  </Regions>
</TextureAtlas>
```

## Testing with Sample Sprites

If you don't have a sprite folder handy, you can use the included `generate_sprites.sh` script (Linux/macOS) to generate a nested `test_sprites/` directory with 23 labeled placeholder sprites of varying dimensions.

1. Make the script executable:
   ```bash
   chmod +x generate_sprites.sh
   ```

2. Generate the test sprites:
   ```bash
   ./generate_sprites.sh
   ```

3. Run the packer against the generated folder:
   ```bash
   dotnet run -- pack --folder test_sprites --padding 4
   ```

4. Inspect the generated output in `test_sprites/atlas/atlas.png` and `test_sprites/atlas/atlas.xml`.