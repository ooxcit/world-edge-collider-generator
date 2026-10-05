using UnityEngine;
using UnityEngine.Rendering;

namespace Oox.WorldEdgeColliderGenerator.Samples
{
    /// <summary>
    /// Gives the sample terrain the active render pipeline's default terrain material,
    /// so the sample renders correctly in the Built-in Render Pipeline, URP and HDRP.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(Terrain))]
    public class SampleTerrainMaterial : MonoBehaviour
    {
        private static Material _builtInTerrainMaterial;

        private void OnEnable()
        {
            var pipeline = GraphicsSettings.currentRenderPipeline;
            GetComponent<Terrain>().materialTemplate = pipeline != null ? pipeline.defaultTerrainMaterial : BuiltInTerrainMaterial();
        }

        private static Material BuiltInTerrainMaterial()
        {
            if (_builtInTerrainMaterial == null)
            {
                _builtInTerrainMaterial = new Material(Shader.Find("Nature/Terrain/Standard")) { hideFlags = HideFlags.DontSave };
            }
            return _builtInTerrainMaterial;
        }
    }
}
