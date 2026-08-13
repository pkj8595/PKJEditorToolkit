using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Tyranno.EditorToolkit
{
    [InitializeOnLoad]
    internal static class ComponentMover
    {
        static ComponentMover()
        {
            EditorApplication.hierarchyWindowItemOnGUI -= OnHierarchyItemGUI;
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyItemGUI;
        }

        private static void OnHierarchyItemGUI(int instanceId, Rect selectionRect)
        {
            Event currentEvent = Event.current;
            if (currentEvent.type != EventType.DragUpdated && currentEvent.type != EventType.DragPerform)
            {
                return;
            }

            if (!selectionRect.Contains(currentEvent.mousePosition) || !TryGetDraggedComponent(out Component source))
            {
                return;
            }

            GameObject target = EditorUtility.EntityIdToObject(instanceId) as GameObject;
            if (target == null)
            {
                return;
            }

            bool canMove = CanMove(source, target);
            if (currentEvent.type == EventType.DragUpdated)
            {
                DragAndDrop.visualMode = canMove ? DragAndDropVisualMode.Move : DragAndDropVisualMode.Rejected;
                currentEvent.Use();
                return;
            }

            if (canMove)
            {
                DragAndDrop.AcceptDrag();
                Move(source, target);
            }

            currentEvent.Use();
        }

        private static bool TryGetDraggedComponent(out Component component)
        {
            Object[] draggedObjects = DragAndDrop.objectReferences;
            component = draggedObjects.Length == 1 ? draggedObjects[0] as Component : null;
            return component != null && !(component is Transform);
        }

        private static bool CanMove(Component source, GameObject target)
        {
            return source.gameObject != target
                   && !EditorUtility.IsPersistent(source)
                   && !EditorUtility.IsPersistent(target);
        }

        private static void Move(Component source, GameObject target)
        {
            System.Type componentType = source.GetType();
            string componentName = componentType.Name;
            string sourceName = source.gameObject.name;

            if (!ComponentUtility.CopyComponent(source))
            {
                Debug.LogWarning($"Could not copy {componentName} from {sourceName}.", source);
                return;
            }

            Undo.IncrementCurrentGroup();
            int undoGroup = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName($"Move {componentName}");

            if (!ComponentPasteUtility.TryPasteAsNew(
                    target,
                    componentType,
                    $"Move {componentName}",
                    out Component movedComponent))
            {
                Undo.CollapseUndoOperations(undoGroup);
                Debug.LogWarning($"Could not add {componentName} to {target.name}.", target);
                return;
            }

            Undo.DestroyObjectImmediate(source);
            Undo.CollapseUndoOperations(undoGroup);

            Debug.Log($"Moved {componentName} from {sourceName} to {target.name}.", target);
        }
    }
}
