#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Oox.WorldEdgeColliderGenerator.Editor
{
    [CustomEditor(typeof(WorldEdgeCollidersGenerator))]
    public class WorldEdgeCollidersGeneratorEditor : UnityEditor.Editor
    {
        private bool _isPlacingPoints;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var generator = (WorldEdgeCollidersGenerator)target;

            GUILayout.Space(10);

            EditorGUI.BeginChangeCheck();
            var label = _isPlacingPoints ? "Stop Placing Points" : "Place Points";
            _isPlacingPoints = GUILayout.Toggle(_isPlacingPoints, label, "Button");
            if (EditorGUI.EndChangeCheck())
            {
                SceneView.RepaintAll();
            }

            if (GUILayout.Button("Clear Points"))
            {
                Undo.RecordObject(generator, "Clear Points");
                generator.ClearPoints();
                EditorUtility.SetDirty(generator);
            }

            GUILayout.Space(20);

            if (GUILayout.Button("Generate Box Colliders"))
            {
                generator.GenerateBoxColliders();
            }
            if (GUILayout.Button("Clear Box Colliders"))
            {
                generator.ClearBoxColliders();
            }
        }

        private void OnSceneGUI()
        {
            var generator = (WorldEdgeCollidersGenerator)target;
            var points = generator.Points;
            var generatorTransform = generator.transform;

            for (var i = 0; i < points.Count; i++)
            {
                var worldPoint = generatorTransform.TransformPoint(points[i]);

                EditorGUI.BeginChangeCheck();
                var newWorldPoint = Handles.PositionHandle(worldPoint, Quaternion.identity);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(generator, "Move Point");
                    generator.SetPoint(i, generatorTransform.InverseTransformPoint(newWorldPoint));
                    EditorUtility.SetDirty(generator);
                }

                Handles.Label(worldPoint + Vector3.up * 0.2f, i.ToString());

                if (points.Count > 1)
                {
                    var nextWorldPoint = generatorTransform.TransformPoint(points[(i + 1) % points.Count]);
                    Handles.DrawAAPolyLine(3f, worldPoint, nextWorldPoint);
                }
            }

            if (!_isPlacingPoints)
            {
                return;
            }

            var controlId = GUIUtility.GetControlID(FocusType.Passive);
            HandleUtility.AddDefaultControl(controlId);

            var currentEvent = Event.current;
            if (currentEvent.type != EventType.MouseDown || currentEvent.button != 0 || currentEvent.alt)
            {
                return;
            }

            var ray = HandleUtility.GUIPointToWorldRay(currentEvent.mousePosition);
            var plane = new Plane(Vector3.up, generatorTransform.position);
            if (plane.Raycast(ray, out var distance))
            {
                Undo.RecordObject(generator, "Add Point");
                generator.AddPoint(generatorTransform.InverseTransformPoint(ray.GetPoint(distance)));
                EditorUtility.SetDirty(generator);
            }
            currentEvent.Use();
        }
    }
}
#endif