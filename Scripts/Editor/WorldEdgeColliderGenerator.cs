#if UNITY_EDITOR
using System.Collections;
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

        public void GenerateBoxColliders()
        {
            StartCoroutine(GenerateBoxCollidersCoroutine());
        }

        private IEnumerator GenerateBoxCollidersCoroutine()
        {
            for (var i = 0; i < _points.Count; i++)
            {
                var vertex0 = transform.TransformPoint(_points[i]);
                var vertex1 = transform.TransformPoint(_points[(i + 1) % _points.Count]);
                var vertex2 = vertex0 + Vector3.up * _height;
                var vertex3 = vertex1 + Vector3.up * _height;

                var box = new GameObject("Box " + i);
                box.AddComponent<BoxCollider>();
                var boxTransform = box.transform;
                boxTransform.parent = _boxParent;

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

                yield return null;
            }
            EditorUtility.SetDirty(this);
        }

        public void ClearBoxColliders()
        {
            StopAllCoroutines();
            while (_boxParent.childCount > 0)
            {
                foreach (Transform box in _boxParent)
                {
                    DestroyImmediate(box.gameObject);
                }
            }
        }
    }
}
#endif