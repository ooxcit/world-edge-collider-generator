#if UNITY_EDITOR
using System.Collections;
using UnityEditor;
using UnityEngine;

namespace Oox.WorldEdgeColliderGenerator.Editor
{
    public class WorldEdgeCollidersGenerator : MonoBehaviour
    {
        [SerializeField] private Mesh _mesh;
        [SerializeField] private Transform _boxParent;
        [SerializeField] private bool _firstIsDifferent;
        [SerializeField] private bool _reverseXY;
        [SerializeField, Range(0, 3)] private int _bVertex;

        [Space]

        [SerializeField] private float _thickness = 0.1f;

        [SerializeField] private Transform _a;
        [SerializeField] private Transform _b;
        [SerializeField] private Transform _c;

        public float CalculateAngle()
        {
            return CalculateAngle(_a.position, _b.position, _c.position);
        }

        private static float CalculateAngle(Vector3 a, Vector3 b, Vector3 c)
        {
            var ab = b - a;
            var ac = c - a;
            var dot = Vector3.Dot(ab, ac);
            var abMag = ab.magnitude;
            var acMag = ac.magnitude;
            var cos = dot / (abMag * acMag);
            var theta = Mathf.Acos(cos);
            var angle = Mathf.Rad2Deg * theta;
            var cross = Vector3.Cross(ab, ac);
            return cross.y < 0 ? angle : -angle;
        }

        private void OnValidate()
        {
            if (!_mesh)
            {
                _mesh = GetComponentInChildren<MeshFilter>().sharedMesh;
            }
            if (!_boxParent)
            {
                _boxParent = GetComponentInChildren<MeshFilter>().transform;
            }
        }

        public void GenerateBoxColliders()
        {
            StartCoroutine(GenerateBoxCollidersCoroutine());
        }

        private IEnumerator GenerateBoxCollidersCoroutine()
        {
            var vertices = _mesh.vertices;
            for (var i = 0; i < vertices.Length; i += 4)
            {
                Vector3 vertex0;
                Vector3 vertex1;
                Vector3 vertex2;
                Vector3 vertex3;
                if (_firstIsDifferent && i == 0)
                {
                    vertex0 = vertices[i + 1];
                    vertex1 = vertices[i + 2];
                    vertex2 = vertices[i + 3];
                    vertex3 = vertices[i];
                }
                else
                {
                    vertex0 = vertices[i];
                    vertex1 = vertices[i + 1];
                    vertex2 = vertices[i + 2];
                    vertex3 = vertices[i + 3];
                }
                var fourVertices = new [] { vertex0, vertex1, vertex2, vertex3 };

                var box = new GameObject("Box " + i / 4);
                box.AddComponent<BoxCollider>();

                var boxTransform = box.transform;
                boxTransform.parent = _boxParent;
                var boxCenter = (vertex0 + vertex1 + vertex2 + vertex3) / 4;
                boxTransform.position = boxCenter;

                var scale = new Vector3
                {
                    x = Vector3.Distance((vertex1 + vertex2) * 0.5f, boxCenter) * 2,
                    y = Vector3.Distance((vertex0 + vertex1) * 0.5f, boxCenter) * 2,
                    z = _thickness
                };
                if (_reverseXY)
                {
                    (scale.x, scale.y) = (scale.y, scale.x);
                }
                boxTransform.localScale = scale;

                #region rotation

                var a = boxCenter + new Vector3(0, scale.y / 2, 0);
                var b = fourVertices[_bVertex];
                var c = boxCenter + new Vector3(-scale.x / 2, scale.y / 2, 0);

                /*var center = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
                center.name = "center";
                var aa = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
                aa.name = "aa";
                var bb = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
                bb.name = "bb";
                var cc = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
                cc.name = "cc";
                center.position = vertex0;
                aa.position = vertex1;
                bb.position = vertex2;
                cc.position = vertex3;
                aa.localScale = Vector3.one * 0.7f;
                bb.localScale = Vector3.one * 0.5f;
                cc.localScale = Vector3.one * 0.3f;*/

                var angle = CalculateAngle(a, b, c);
                boxTransform.rotation = Quaternion.Euler(0, angle, 0);

                if (i == 100)
                {
                    i -= 2;
                }

                /*if (i == 2 * 4)
                {
                    var center = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
                    var aa = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
                    var bb = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
                    var cc = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
                    center.position = boxCenter;
                    aa.position = a;
                    bb.position = b;
                    cc.position = c;
                    aa.localScale = Vector3.one * 0.5f;
                    bb.localScale = Vector3.one * 0.3f;
                    cc.localScale = Vector3.one * 0.1f;
                    yield break;
                }*/

                /*if (i == 2 * 4)
                {
                    var aa = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
                    var bb = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
                    var cc = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
                    var dd = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
                    var ee = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
                    aa.position = vertex0;
                    bb.position = vertex1;
                    cc.position = vertex2;
                    dd.position = vertex3;
                    ee.position = boxCenter;
                    yield break;
                }*/

                #endregion

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