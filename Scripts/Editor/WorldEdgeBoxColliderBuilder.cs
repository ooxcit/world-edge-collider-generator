using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Oox.WorldEdgeColliderGenerator.Editor
{
    /// <summary>
    /// Creates and removes the box colliders for a <see cref="WorldEdgeCollidersGenerator"/>, with undo support.
    /// </summary>
    public static class WorldEdgeBoxColliderBuilder
    {
        private const string BOX_NAME_PREFIX = "Box ";
        // A box from before the marker component: a Transform and a BoxCollider, named like "Box 3".
        private const int LEGACY_BOX_COMPONENT_COUNT = 2;
        private const float MIN_EDGE_LENGTH = 0.0001f;
        private static readonly Regex LEGACY_BOX_NAME = new("^" + BOX_NAME_PREFIX + "[0-9]+$");

        /// <summary>
        /// Replaces any previously generated box colliders with a new set along the generator's points.
        /// </summary>
        public static void GenerateBoxColliders(WorldEdgeCollidersGenerator generator)
        {
            Undo.IncrementCurrentGroup();
            var undoGroup = Undo.GetCurrentGroup();
            RemoveBoxColliders(generator);

            var points = generator.Points;
            var generatorTransform = generator.transform;
            var boxParent = generator.BoxParent;
            for (var i = 0; i < points.Count; i++)
            {
                var vertex0 = generatorTransform.TransformPoint(points[i]);
                var vertex1 = generatorTransform.TransformPoint(points[(i + 1) % points.Count]);
                var edge = vertex1 - vertex0;
                var edgeLength = edge.magnitude;
                // Two points in the same place make no wall.
                if (edgeLength < MIN_EDGE_LENGTH)
                {
                    continue;
                }

                var box = new GameObject(BOX_NAME_PREFIX + i);
                var boxCollider = box.AddComponent<BoxCollider>();
                box.AddComponent<WorldEdgeGeneratedCollider>();
                var boxTransform = box.transform;
                boxTransform.parent = boxParent;
                Undo.RegisterCreatedObjectUndo(box, "Generate Box Colliders");

                #region Rotation

                // Along the edge, slope included; the wall's height stands perpendicular to it.
                var rotation = Quaternion.LookRotation(edge, Vector3.up);
                boxTransform.rotation = rotation;

                #endregion Rotation

                #region Position

                boxTransform.position = (vertex0 + vertex1) * 0.5f + rotation * Vector3.up * (generator.Height * 0.5f);

                #endregion Position

                #region Size

                // World sizes go on the collider, not the scale, so a scaled box parent doesn't stretch them.
                boxTransform.localScale = Vector3.one;
                var lossyScale = boxTransform.lossyScale;
                boxCollider.size = new Vector3(
                    generator.Thickness / lossyScale.x,
                    generator.Height / lossyScale.y,
                    edgeLength / lossyScale.z);

                #endregion Size
            }

            Undo.CollapseUndoOperations(undoGroup);
        }

        public static void ClearBoxColliders(WorldEdgeCollidersGenerator generator)
        {
            Undo.IncrementCurrentGroup();
            var undoGroup = Undo.GetCurrentGroup();
            RemoveBoxColliders(generator);
            Undo.CollapseUndoOperations(undoGroup);
        }

        /// <summary>
        /// Whether this child of the box parent is a collider the generator made (not one of your own).
        /// </summary>
        public static bool IsGenerated(Transform child)
        {
            if (child.TryGetComponent<WorldEdgeGeneratedCollider>(out _))
            {
                return true;
            }
            return child.childCount == 0
                   && LEGACY_BOX_NAME.IsMatch(child.name)
                   && child.TryGetComponent<BoxCollider>(out _)
                   && child.GetComponents<Component>().Length == LEGACY_BOX_COMPONENT_COUNT;
        }

        private static void RemoveBoxColliders(WorldEdgeCollidersGenerator generator)
        {
            var boxParent = generator.BoxParent;
            for (var i = boxParent.childCount - 1; i >= 0; i--)
            {
                var box = boxParent.GetChild(i);
                if (IsGenerated(box))
                {
                    Undo.DestroyObjectImmediate(box.gameObject);
                }
            }
        }
    }
}
