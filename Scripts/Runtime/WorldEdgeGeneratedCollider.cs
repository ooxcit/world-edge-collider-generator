using UnityEngine;

namespace Oox.WorldEdgeColliderGenerator
{
    /// <summary>
    /// Marks a box collider made by <see cref="WorldEdgeCollidersGenerator"/>, so regenerating or clearing removes
    /// only those and never your own colliders under the same parent.
    /// </summary>
    [AddComponentMenu("")]
    [DisallowMultipleComponent]
    public class WorldEdgeGeneratedCollider : MonoBehaviour
    {
    }
}
