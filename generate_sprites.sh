#!/usr/bin/env bash
set -e

ROOT="test_sprites"
rm -rf "$ROOT"

sprites=(
  "characters/player|player_idle_01|32|48|#3498db|P_ID1"
  "characters/player|player_idle_02|32|48|#2980b9|P_ID2"
  "characters/player|player_run_01|36|48|#1abc9c|P_RN1"
  "characters/player|player_run_02|36|48|#16a085|P_RN2"
  "characters/player|player_jump|34|52|#9b59b6|P_JMP"
  "characters/enemies|slime_green_01|28|20|#2ecc71|SLM1"
  "characters/enemies|slime_green_02|32|18|#27ae60|SLM2"
  "characters/enemies|skeleton_walk_01|32|48|#ecf0f1|SKL1"
  "characters/enemies|skeleton_walk_02|32|48|#bdc3c7|SKL2"
  "characters/enemies|ogre_boss|64|64|#e74c3c|BOSS"
  "environment/tiles|grass_top|32|32|#2ecc71|GRS"
  "environment/tiles|dirt_center|32|32|#8e5b3e|DRT"
  "environment/tiles|stone_brick|32|32|#7f8c8d|STN"
  "environment/tiles|water_surface|32|32|#3498db|WTR"
  "environment/props|oak_tree|64|96|#1e8449|TREE"
  "environment/props|wooden_crate|32|32|#d35400|CRAT"
  "environment/props|gold_coin|16|16|#f1c40f|CN"
  "environment/props|treasure_chest|40|32|#f39c12|CHST"
  "ui/icons|heart_full|16|16|#e74c3c|HP"
  "ui/icons|mana_potion|16|24|#2980b9|MP"
  "ui/icons|sword_icon|24|24|#95a5a6|SWD"
  "ui/bars|health_bar_bg|128|20|#2c3e50|HP_BAR"
  "ui/bars|dialogue_box|160|48|#34495e|DIALOG"
)

for entry in "${sprites[@]}"; do
  IFS="|" read -r subdir name w h color label <<< "$entry"
  mkdir -p "$ROOT/$subdir"
  magick -size "${w}x${h}" "xc:${color}" \
    -gravity center -fill black -pointsize 9 -annotate 0 "$label" \
    "$ROOT/$subdir/${name}.png"
done

echo "Generated ${#sprites[@]} test sprites in '$ROOT'."