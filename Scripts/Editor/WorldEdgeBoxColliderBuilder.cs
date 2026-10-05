using UnityEditor;
using UnityEngine;

namespace Oox.WorldEdgeColliderGenerator.Editor
{
    /// <summary>
    /// Creates and removes the box colliders for a <see cref="WorldEdgeCollidersGenerator"/>, with undo support.
    /// </summary>
    public static class WorldEdgeBoxColliderBuilder
    {
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
                var vertex2 = vertex0 + Vector3.up * generator.Height;
                var vertex3 = vertex1 + Vector3.up * generator.Height;

                var box = new GameObject("Box " + i);
                box.AddComponent<BoxCollider>();
                var boxTransform = box.transform;
                boxTransform.parent = boxParent;
                Undo.RegisterCreatedObjectUndo(box, "Generate Box Colliders");

                #region Position

                var boxCenter = (vertex0 + vertex1 + vertex2 + vertex3) / 4f;
                boxTransform.position = boxCenter;

                #endregion Position

                #region Rotation

                var dir = vertex1 - vertex0;
                var angle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
                boxTransform.rotation = Quaternion.Euler(0f, angle, 0f);

                #endregion Rotation

                #region Scale

                var scale = new Vector3
                {
                    x = generator.Thickness,
                    y = Vector3.Distance((vertex0 + vertex1) * 0.5f, boxCenter) * 2f,
                    z = Vector3.Distance((vertex0 + vertex2) * 0.5f, boxCenter) * 2f
                };
                boxTransform.localScale = scale;

                #endregion Scale
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

        private static void RemoveBoxColliders(WorldEdgeCollidersGenerator generator)
        {
            var boxParent = generator.BoxParent;
            for (var i = boxParent.childCount - 1; i >= 0; i--)
            {
                var box = boxParent.GetChild(i);
                if (box.GetComponent<BoxCollider>() != null)
                {
                    Undo.DestroyObjectImmediate(box.gameObject);
                }
            }
        }
    }
}
