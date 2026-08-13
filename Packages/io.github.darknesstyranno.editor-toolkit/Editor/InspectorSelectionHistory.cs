using System.Collections.Generic;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;

namespace Tyranno.EditorToolkit
{
    [InitializeOnLoad]
    internal static class InspectorSelectionHistory
    {
        private const int MaxHistoryCount = 128;
        private const string BackShortcutId = "Tyranno Editor Toolkit/Inspector Back";
        private const string ForwardShortcutId = "Tyranno Editor Toolkit/Inspector Forward";

        private static readonly List<Object> History = new List<Object>(MaxHistoryCount);
        private static int historyIndex = -1;
        private static bool isNavigating;

        static InspectorSelectionHistory()
        {
            Selection.selectionChanged -= RecordSelection;
            Selection.selectionChanged += RecordSelection;

            if (Selection.activeObject != null)
            {
                History.Add(Selection.activeObject);
                historyIndex = 0;
            }
        }

        [MenuItem("Tools/Tyranno Editor Toolkit/Inspector Back", false, 100)]
        [Shortcut(BackShortcutId, KeyCode.Mouse3)]
        private static void GoBack()
        {
            Navigate(-1);
        }

        [MenuItem("Tools/Tyranno Editor Toolkit/Inspector Back", true)]
        private static bool CanGoBackFromMenu()
        {
            return FindNextValidIndex(historyIndex - 1, -1) >= 0;
        }

        [MenuItem("Tools/Tyranno Editor Toolkit/Inspector Forward", false, 101)]
        [Shortcut(ForwardShortcutId, KeyCode.Mouse4)]
        private static void GoForward()
        {
            Navigate(1);
        }

        [MenuItem("Tools/Tyranno Editor Toolkit/Inspector Forward", true)]
        private static bool CanGoForwardFromMenu()
        {
            return FindNextValidIndex(historyIndex + 1, 1) >= 0;
        }

        private static void RecordSelection()
        {
            if (isNavigating || Selection.activeObject == null)
            {
                return;
            }

            EnsureHistoryIndexInRange();
            Object selectedObject = Selection.activeObject;
            if (historyIndex >= 0 && History[historyIndex] == selectedObject)
            {
                return;
            }

            if (historyIndex < History.Count - 1)
            {
                History.RemoveRange(historyIndex + 1, History.Count - historyIndex - 1);
            }

            History.Add(selectedObject);
            historyIndex = History.Count - 1;
            TrimHistoryOverflow();
        }

        private static void Navigate(int direction)
        {
            int targetIndex = FindNextValidIndex(historyIndex + direction, direction);
            if (targetIndex < 0)
            {
                return;
            }

            isNavigating = true;
            try
            {
                historyIndex = targetIndex;
                Object target = History[historyIndex];
                Selection.activeObject = target;
                EditorGUIUtility.PingObject(target);
                FocusInspectorWindow();
            }
            finally
            {
                isNavigating = false;
            }
        }

        private static int FindNextValidIndex(int startIndex, int direction)
        {
            for (int index = startIndex; index >= 0 && index < History.Count; index += direction)
            {
                if (History[index] != null)
                {
                    return index;
                }
            }

            return -1;
        }

        private static void EnsureHistoryIndexInRange()
        {
            historyIndex = History.Count == 0
                ? -1
                : Mathf.Clamp(historyIndex, 0, History.Count - 1);
        }

        private static void TrimHistoryOverflow()
        {
            int overflow = History.Count - MaxHistoryCount;
            if (overflow <= 0)
            {
                return;
            }

            History.RemoveRange(0, overflow);
            historyIndex = Mathf.Max(0, historyIndex - overflow);
        }

        private static void FocusInspectorWindow()
        {
            if (!EditorApplication.ExecuteMenuItem("Window/General/Inspector"))
            {
                EditorApplication.ExecuteMenuItem("Window/Inspector");
            }
        }
    }
}
