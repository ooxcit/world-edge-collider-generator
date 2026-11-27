using NUnit.Framework;
using Oox.WorldEdgeColliderGenerator.Editor;
using UnityEngine;

namespace Oox.WorldEdgeColliderGenerator.Tests.Editor
{
    public class WorldEdgeBoxColliderBuilderTests
    {
        private GameObject _generatorObject;
        private WorldEdgeCollidersGenerator _generator;

        [SetUp]
        public void SetUp()
        {
            _generatorObject = new GameObject("generator");
            _generator = _generatorObject.AddComponent<WorldEdgeCollidersGenerator>();
            _generator.AddPoint(Vector3.zero);
            _generator.AddPoint(new Vector3(4f, 0f, 0f));
            _generator.AddPoint(new Vector3(4f, 0f, 4f));
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_generatorObject);
        }

        [Test]
        public void Generate_OneBoxPerEdge()
        {
            WorldEdgeBoxColliderBuilder.GenerateBoxColliders(_generator);

            Assert.AreEqual(3, _generatorObject.GetComponentsInChildren<WorldEdgeGeneratedCollider>().Length);
        }

        [Test]
        public void GenerateAndClear_KeepYourOwnCollidersUnderTheParent()
        {
            var ownCollider = new GameObject("Wall");
            ownCollider.AddComponent<BoxCollider>();
            ownCollider.transform.parent = _generatorObject.transform;

            WorldEdgeBoxColliderBuilder.GenerateBoxColliders(_generator);
            WorldEdgeBoxColliderBuilder.GenerateBoxColliders(_generator);
            Assert.AreEqual(4, _generatorObject.transform.childCount);

            WorldEdgeBoxColliderBuilder.ClearBoxColliders(_generator);
            Assert.AreEqual(1, _generatorObject.transform.childCount);
            Assert.AreSame(ownCollider.transform, _generatorObject.transform.GetChild(0));
        }

        [Test]
        public void Clear_RemovesBoxesFromOlderVersions()
        {
            var legacyBox = new GameObject("Box 0");
            legacyBox.AddComponent<BoxCollider>();
            legacyBox.transform.parent = _generatorObject.transform;

            WorldEdgeBoxColliderBuilder.ClearBoxColliders(_generator);

            Assert.AreEqual(0, _generatorObject.transform.childCount);
        }
    }
}
