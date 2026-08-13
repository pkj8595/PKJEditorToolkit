using System.IO;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;

namespace Tyranno.EditorToolkit
{
    internal static class FolderShortcuts
    {
        internal const int SlotCount = 8;

        private const string ShortcutPrefix = "Tyranno Editor Toolkit/Folder Shortcuts";

        [Shortcut(ShortcutPrefix + "/Open Slot 1", KeyCode.Keypad1)]
        private static void OpenSlot1() => OpenSlot(0);

        [Shortcut(ShortcutPrefix + "/Open Slot 2", KeyCode.Keypad2)]
        private static void OpenSlot2() => OpenSlot(1);

        [Shortcut(ShortcutPrefix + "/Open Slot 3", KeyCode.Keypad3)]
        private static void OpenSlot3() => OpenSlot(2);

        [Shortcut(ShortcutPrefix + "/Open Slot 4", KeyCode.Keypad4)]
        private static void OpenSlot4() => OpenSlot(3);

        [Shortcut(ShortcutPrefix + "/Open Slot 5", KeyCode.Keypad5)]
        private static void OpenSlot5() => OpenSlot(4);

        [Shortcut(ShortcutPrefix + "/Open Slot 6", KeyCode.Keypad6)]
        private static void OpenSlot6() => OpenSlot(5);

        [Shortcut(ShortcutPrefix + "/Open Slot 7", KeyCode.Keypad7)]
        private static void OpenSlot7() => OpenSlot(6);

        [Shortcut(ShortcutPrefix + "/Open Slot 8", KeyCode.Keypad8)]
        private static void OpenSlot8() => OpenSlot(7);

        [Shortcut(ShortcutPrefix + "/Save Slot 1", KeyCode.Keypad1, ShortcutModifiers.Action)]
        private static void SaveSlot1() => SaveSlot(0);

        [Shortcut(ShortcutPrefix + "/Save Slot 2", KeyCode.Keypad2, ShortcutModifiers.Action)]
        private static void SaveSlot2() => SaveSlot(1);

        [Shortcut(ShortcutPrefix + "/Save Slot 3", KeyCode.Keypad3, ShortcutModifiers.Action)]
        private static void SaveSlot3() => SaveSlot(2);

        [Shortcut(ShortcutPrefix + "/Save Slot 4", KeyCode.Keypad4, ShortcutModifiers.Action)]
        private static void SaveSlot4() => SaveSlot(3);

        [Shortcut(ShortcutPrefix + "/Save Slot 5", KeyCode.Keypad5, ShortcutModifiers.Action)]
        private static void SaveSlot5() => SaveSlot(4);

        [Shortcut(ShortcutPrefix + "/Save Slot 6", KeyCode.Keypad6, ShortcutModifiers.Action)]
        private static void SaveSlot6() => SaveSlot(5);

        [Shortcut(ShortcutPrefix + "/Save Slot 7", KeyCode.Keypad7, ShortcutModifiers.Action)]
        private static void SaveSlot7() => SaveSlot(6);

        [Shortcut(ShortcutPrefix + "/Save Slot 8", KeyCode.Keypad8, ShortcutModifiers.Action)]
        private static void SaveSlot8() => SaveSlot(7);

        private static void OpenSlot(int index)
        {
            string folderGuid = FolderShortcutSettings.instance.GetFolderGuid(index);
            string folderPath = AssetDatabase.GUIDToAssetPath(folderGuid);
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                Debug.LogWarning($"Tyranno Editor Toolkit: Folder shortcut {index + 1} is not assigned.");
                return;
            }

            Object folder = AssetDatabase.LoadAssetAtPath<DefaultAsset>(folderPath);
            EditorApplication.ExecuteMenuItem("Window/General/Project");
            Selection.activeObject = folder;
            EditorGUIUtility.PingObject(folder);
        }

        private static void SaveSlot(int index)
        {
            string folderPath = GetSelectedFolderPath();
            if (string.IsNullOrEmpty(folderPath))
            {
                Debug.LogWarning("Tyranno Editor Toolkit: Select a folder or an asset in the Project window before saving a folder shortcut.");
                return;
            }

            string folderGuid = AssetDatabase.AssetPathToGUID(
                folderPath,
                AssetPathToGUIDOptions.OnlyExistingAssets);
            FolderShortcutSettings.instance.SetFolderGuid(index, folderGuid);
            Debug.Log($"Tyranno Editor Toolkit: Saved folder shortcut {index + 1} as '{folderPath}'.");
        }

        private static string GetSelectedFolderPath()
        {
            string selectedPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (AssetDatabase.IsValidFolder(selectedPath))
            {
                return selectedPath;
            }

            string parentPath = Path.GetDirectoryName(selectedPath)?.Replace('\\', '/');
            return AssetDatabase.IsValidFolder(parentPath) ? parentPath : string.Empty;
        }
    }

    [FilePath(
        "UserSettings/TyrannoEditorToolkitFolderShortcuts.asset",
        FilePathAttribute.Location.ProjectFolder)]
    internal sealed class FolderShortcutSettings : ScriptableSingleton<FolderShortcutSettings>
    {
        [SerializeField]
        private string[] folderGuids = new string[FolderShortcuts.SlotCount];

        internal string GetFolderGuid(int index)
        {
            return folderGuids[index];
        }

        internal void SetFolderGuid(int index, string folderGuid)
        {
            folderGuids[index] = folderGuid;
            Save(true);
        }
    }
}
