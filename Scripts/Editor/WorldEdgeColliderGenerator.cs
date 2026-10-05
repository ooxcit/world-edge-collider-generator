#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Oox.WorldEdgeColliderGenerator.Editor
{
    public class WorldEdgeCollidersGenerator : MonoBehaviour
    {
        [SerializeField] private List<Vector3> _points = new();
        [SerializeField] private Transform _boxParent;

        [Header("Dimensions")]
        [SerializeField] private float _height = 1f;
        [SerializeField] private float _thickness = 0.1f;

        public List<Vector3> Points => _points;

        public void AddPoint(Vector3 localPoint)
        {
            _points.Add(localPoint);
        }

        public void InsertPoint(int index, Vector3 localPoint)
        {
            _points.Insert(index, localPoint);
        }

        public void SetPoint(int index, Vector3 localPoint)
        {
            _points[index] = localPoint;
        }

        public void RemovePointAt(int index)
        {
            _points.RemoveAt(index);
        }

        public void ClearPoints()
        {
            _points.Clear();
        }

        // Falls back to this transform so generating never fails on a missing reference.
        private Transform BoxParent => _boxParent != null ? _boxParent : transform;

        public void GenerateBoxColliders()
        {
            Undo.IncrementCurrentGroup();
            var undoGroup = Undo.GetCurrentGroup();
            var boxParent = BoxParent;
            for (var i = 0; i < _points.Count; i++)
            {
                var vertex0 = transform.TransformPoint(_points[i]);
                var vertex1 = transform.TransformPoint(_points[(i + 1) % _points.Count]);
                var vertex2 = vertex0 + Vector3.up * _height;
                var vertex3 = vertex1 + Vector3.up * _height;

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
                    x = _thickness,
                    y = Vector3.Distance((vertex0 + vertex1) * 0.5f, boxCenter) * 2f,
                    z = Vector3.Distance((vertex0 + vertex2) * 0.5f, boxCenter) * 2f
                };
                boxTransform.localScale = scale;

                #endregion Scale
            }
            Undo.CollapseUndoOperations(undoGroup);
        }

        public void ClearBoxColliders()
        {
            Undo.IncrementCurrentGroup();
            var undoGroup = Undo.GetCurrentGroup();
            var boxParent = BoxParent;
            for (var i = boxParent.childCount - 1; i >= 0; i--)
            {
                var box = boxParent.GetChild(i);
                if (box.GetComponent<BoxCollider>() != null)
                {
                    Undo.DestroyObjectImmediate(box.gameObject);
                }
            }
            Undo.CollapseUndoOperations(undoGroup);
        }
    }
}
#endif