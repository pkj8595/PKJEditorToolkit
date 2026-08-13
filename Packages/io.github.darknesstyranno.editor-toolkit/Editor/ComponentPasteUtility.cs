using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Tyranno.EditorToolkit
{
    internal static class ComponentPasteUtility
    {
        public static bool TryPasteAsNew(
            GameObject target,
            Type componentType,
            string undoName,
            out Component pastedComponent)
        {
            Component[] previousComponents = target.GetComponents<Component>();
            var previousInstanceIds = new HashSet<int>();
            foreach (Component component in previousComponents)
            {
                if (component != null)
                {
                    previousInstanceIds.Add(component.GetInstanceID());
                }
            }

            pastedComponent = null;
            if (!ComponentUtility.PasteComponentAsNew(target))
            {
                return false;
            }

            var createdComponents = new List<Component>();
            foreach (Component component in target.GetComponents<Component>())
            {
                if (component == null || previousInstanceIds.Contains(component.GetInstanceID()))
                {
                    continue;
                }

                createdComponents.Add(component);
                if (component.GetType() == componentType)
                {
                    pastedComponent = component;
                }
            }

            if (pastedComponent == null)
            {
                throw new InvalidOperationException($"Could not identify the pasted {componentType.Name} component.");
            }

            foreach (Component component in createdComponents)
            {
                Undo.RegisterCreatedObjectUndo(component, undoName);
            }

            return true;
        }
    }
}
