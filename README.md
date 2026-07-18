# LiveSplit.BelowZero
This LiveSplit Auto Splitter provides automatic start, split, and reset support for Subnautica: Below Zero by tracking in-game memory values.

## Features
- Automatic run start for any game mode
- Customizable Auto Splits
- Conditional Auto Splits
- Automatic reset on returning to the main menu
- Ordered split handling for LiveSplit order or auto-split order
- Prefabricated split support for common Below Zero route events

## Supported Game Versions
- August 2021
- October 2025

Other versions may partially work as well. When an exact version is not matched, the autosplitter falls back to the closest supported Below Zero data set using executable and build metadata. Unknown versions default to the newer 2025 behavior set.

## How to use
1. Open LiveSplit
2. Right-click and choose `Edit Splits`
3. Set `Game Name` to `Subnautica: Below Zero`
4. Activate the auto splitter
5. Open `Settings` and configure your split list

# Settings

## Start / Reset
- `Start` starts the timer on the first real movement or interaction after you gain control
- `Reset` resets the timer when the main menu is shown

## Others
- `Warn On Reset If Gold Split` asks to save golds before an automatic reset
- `Ordered Splits (LiveSplit)` matches auto-splits to the current LiveSplit split order
- `Ordered Splits (Auto-Splits)` makes configured auto-splits trigger in their own sequence

## Auto Splits
Auto splits can have additional conditions, such as being in a specific biome while crafting a specific item
Some auto splits are further configurable, such as Inventory auto splits

## Generate Splits
This button generates LiveSplit splits based on the Auto Splits you have configured

## Known Issues
- Game updates may temporarily break memory signatures or offsets
- Only August 2021 and October 2025 are directly targeted and verified
- Other versions may fall back to 2025 data and can require later adjustments
- Restarting the game may rarely break the Auto Splitter, If this happens, restart LiveSplit or reload the Auto Splitter

# Contributing
Bug reports and code improvements are welcome
