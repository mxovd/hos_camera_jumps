# Camera Jumps Mod

Save up to ten custom camera bookmarks and warp back to them instantly while playing Hex of Steel.

## Controls

- **Save bookmark** – Hold `Ctrl` and press a number key (`0-9`). Bookmarks store position and zoom.
- **Jump to bookmark** – Hold `Alt` and press the matching number key.
- **Cycle bookmarks** – Hold `Alt` and press `[` for the previous bookmark or `]` for the next one.

Shortcuts work with both the number row and the numpad. Hotkeys are ignored while UI text inputs are focused to avoid accidentally overwriting bookmarks.

## Building

1. Install the .NET SDK 8.0 (or newer) and make sure `dotnet` is on your PATH.
2. Run `python hos_mod_utils.py --deploy` to build and package the mod (add `-r` if you need to refresh the `Libraries` folder from a local Hex of Steel install).
3. Use the switch --install or -i along the deploy argument to directly install the folder in your Hex of Steel mods folder. Otherwise, manually copy the package.

## Notes

- Bookmarks are stored per session. Saving a new slot overwrites the previous value immediately.
- Camera coordinates are clamped to the current map limits when saving or recalling to prevent jumping outside the playable area.
- Vanilla "Next/Previous unit" hotkeys (], [, 1-3) are ignored whenever Ctrl or Alt is held so they don't fire while using the bookmark modifiers.
- Slots automatically clear every time a new scenario/map loads so positions from one game never leak into the next.
