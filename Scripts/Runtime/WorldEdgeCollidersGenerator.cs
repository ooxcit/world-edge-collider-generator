using System.Collections.Generic;
using UnityEngine;

namespace Oox.WorldEdgeColliderGenerator
{
    /// <summary>
    /// Holds a closed path of points along which box colliders are generated in the editor.
    /// The points are local to this transform.
    /// </summary>
    [AddComponentMenu("Oox/World Edge Collider Generator")]
    public class WorldEdgeCollidersGenerator : MonoBehaviour
    {
        [SerializeField] private List<Vector3> _points = new();
        [SerializeField] private Transform _boxParent;

        [Header("Dimensions")]
        [SerializeField] private float _height = 1f;
        [SerializeField] private float _thickness = 0.1f;

        public List<Vector3> Points => _points;

        // Falls back to this transform so generating never fails on a missing reference.
        public Transform BoxParent => _boxParent != null ? _boxParent : transform;

        public float Height => _height;

        public float Thickness => _thickness;

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
    }
}
