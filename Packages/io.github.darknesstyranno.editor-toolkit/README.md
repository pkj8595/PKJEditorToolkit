# Tyranno Editor Toolkit

Small, focused workflow tools for the Unity Editor.

## Component Mover

Drag a component header from the Inspector onto another scene GameObject in the Hierarchy. The component is recreated on the target GameObject with the same serialized values, then removed from the source GameObject.

The complete move is grouped into one Undo operation.

### Limitations

- `Transform` components cannot be moved.
- Components can only be moved between scene GameObjects.
- The moved component is appended to the bottom of the target Inspector.
- References stored by the moved component are copied.
- References from other objects to the original component cannot be redirected automatically and will be lost. Use Undo if the move breaks a required reference.

## Inspector Selection History

- Mouse Back: return to the previous Inspector selection.
- Mouse Forward: advance to the next Inspector selection.
- The most recent 128 selections are kept for the current Editor session.
- The commands are also available from `Tools > Tyranno Editor Toolkit` and Unity's Shortcut Manager.

## Folder Shortcuts

- `Numpad 1` through `Numpad 8`: open the folder saved in that slot.
- `Ctrl/Cmd + Numpad 1` through `Ctrl/Cmd + Numpad 8`: save the selected Project folder to that slot.
- Saving while an asset is selected stores its parent folder.
- Slots are stored per project in `UserSettings` and follow folders when they are moved or renamed.
- Bindings can be changed from Unity's Shortcut Manager.

## Missing Reference Finder

Use either command to choose the scan scope:

- `Tools > Tyranno Editor Toolkit > Find Missing References in Open Scenes`: scan every loaded scene.
- Select prefab assets or folders, then run `Assets > Tyranno Editor Toolkit > Find Missing References in Selection`: scan those prefabs recursively.

Both commands check for:

- Missing scripts.
- Broken serialized object references.

Results are written to the Console. Select an error to locate its GameObject, component, or prefab asset.

## Installation

Add the package from this Git URL:

```text
https://github.com/DarknessTyranno/TyrannoEditorToolkit.git?path=/Packages/io.github.darknesstyranno.editor-toolkit
```

## Requirements

- Unity 6 or newer

## License

MIT
