using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Tyranno.EditorToolkit
{
    internal static class MissingReferenceFinder
    {
        [MenuItem("Tools/Tyranno Editor Toolkit/Find Missing References in Open Scenes", false, 200)]
        private static void FindInOpenScenes()
        {
            int missingScriptCount = 0;
            int missingReferenceCount = 0;

            for (int sceneIndex = 0; sceneIndex < SceneManager.sceneCount; sceneIndex++)
            {
                Scene scene = SceneManager.GetSceneAt(sceneIndex);
                if (!scene.isLoaded)
                {
                    continue;
                }

                foreach (GameObject root in scene.GetRootGameObjects())
                {
                    ScanHierarchy(
                        root,
                        scene.name,
                        ref missingScriptCount,
                        ref missingReferenceCount);
                }
            }

            LogSummary("the open scenes", missingScriptCount, missingReferenceCount);
        }

        [MenuItem("Assets/Tyranno Editor Toolkit/Find Missing References in Selection", false, 2000)]
        private static void FindInSelectedPrefabs()
        {
            HashSet<string> prefabPaths = GetSelectedPrefabPaths();
            if (prefabPaths.Count == 0)
            {
                Debug.LogWarning("Tyranno Editor Toolkit: The selection contains no prefab assets.");
                return;
            }

            int missingScriptCount = 0;
            int missingReferenceCount = 0;
            foreach (string prefabPath in prefabPaths)
            {
                GameObject prefabRoot = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                ScanHierarchy(
                    prefabRoot,
                    prefabPath,
                    ref missingScriptCount,
                    ref missingReferenceCount);
            }

            LogSummary(
                $"{prefabPaths.Count} selected prefab(s)",
                missingScriptCount,
                missingReferenceCount);
        }

        [MenuItem("Assets/Tyranno Editor Toolkit/Find Missing References in Selection", true)]
        private static bool CanFindInSelectedPrefabs()
        {
            foreach (UnityEngine.Object selectedObject in Selection.objects)
            {
                string path = AssetDatabase.GetAssetPath(selectedObject);
                if (AssetDatabase.IsValidFolder(path) || IsPrefabPath(path))
                {
                    return true;
                }
            }

            return false;
        }

        private static void ScanHierarchy(
            GameObject root,
            string sourceName,
            ref int missingScriptCount,
            ref int missingReferenceCount)
        {
            foreach (Transform transform in root.GetComponentsInChildren<Transform>(true))
            {
                GameObject gameObject = transform.gameObject;
                string objectPath = $"{sourceName}/{GetTransformPath(transform)}";

                int objectMissingScriptCount =
                    GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(gameObject);
                if (objectMissingScriptCount > 0)
                {
                    missingScriptCount += objectMissingScriptCount;
                    Debug.LogError(
                        $"[Missing Script] {objectPath} ({objectMissingScriptCount})",
                        gameObject);
                }

                foreach (Component component in gameObject.GetComponents<Component>())
                {
                    if (component == null)
                    {
                        continue;
                    }

                    missingReferenceCount += FindInComponent(component, objectPath);
                }
            }
        }

        private static int FindInComponent(Component component, string objectPath)
        {
            int missingReferenceCount = 0;
            SerializedObject serializedObject = new SerializedObject(component);
            SerializedProperty property = serializedObject.GetIterator();

            while (property.NextVisible(true))
            {
                if (property.propertyType != SerializedPropertyType.ObjectReference ||
                    property.objectReferenceValue != null ||
                    property.objectReferenceInstanceIDValue == 0)
                {
                    continue;
                }

                missingReferenceCount++;
                Debug.LogError(
                    $"[Missing Reference] {objectPath} | " +
                    $"{component.GetType().Name}.{property.propertyPath}",
                    component);
            }

            return missingReferenceCount;
        }

        private static HashSet<string> GetSelectedPrefabPaths()
        {
            var prefabPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (UnityEngine.Object selectedObject in Selection.objects)
            {
                string selectedPath = AssetDatabase.GetAssetPath(selectedObject);
                if (AssetDatabase.IsValidFolder(selectedPath))
                {
                    foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { selectedPath }))
                    {
                        string prefabPath = AssetDatabase.GUIDToAssetPath(guid);
                        if (IsPrefabPath(prefabPath))
                        {
                            prefabPaths.Add(prefabPath);
                        }
                    }
                }
                else if (IsPrefabPath(selectedPath))
                {
                    prefabPaths.Add(selectedPath);
                }
            }

            return prefabPaths;
        }

        private static bool IsPrefabPath(string path)
        {
            return path.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase);
        }

        private static void LogSummary(
            string scope,
            int missingScriptCount,
            int missingReferenceCount)
        {
            if (missingScriptCount + missingReferenceCount == 0)
            {
                Debug.Log($"Tyranno Editor Toolkit: No missing references were found in {scope}.");
                return;
            }

            Debug.LogWarning(
                $"Tyranno Editor Toolkit: Found {missingScriptCount} missing script(s) and " +
                $"{missingReferenceCount} missing reference(s) in {scope}.");
        }

        private static string GetTransformPath(Transform transform)
        {
            string path = transform.name;
            for (Transform parent = transform.parent; parent != null; parent = parent.parent)
            {
                path = $"{parent.name}/{path}";
            }

            return path;
        }
    }
}
