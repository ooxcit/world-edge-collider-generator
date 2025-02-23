#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Oox.WorldEdgeColliderGenerator.Editor
{
    [CustomEditor(typeof(WorldEdgeCollidersGenerator))]
    public class WorldEdgeCollidersGeneratorEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            GUILayout.Space(30);

            var generator = (WorldEdgeCollidersGenerator)target;
            if (GUILayout.Button("Generate Box Colliders"))
            {
                generator.GenerateBoxColliders();
            }
            if (GUILayout.Button("Clear Box Colliders"))
            {
                generator.ClearBoxColliders();
            }

            GUILayout.Space(60);

            if (GUILayout.Button("Calculate Angle"))
            {
                Debug.Log(generator.CalculateAngle());
            }
        }
    }
}
#endif