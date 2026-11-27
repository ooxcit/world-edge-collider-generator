using NUnit.Framework;
using Oox.WorldEdgeColliderGenerator.Editor;
using UnityEngine;

namespace Oox.WorldEdgeColliderGenerator.Tests.Editor
{
    public class WorldEdgeBoxColliderBuilderTests
    {
        private const float TOLERANCE = 1e-4f;

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
        public void Generate_SlopedEdge_WallFollowsTheEdge()
        {
            _generator.SetPoint(1, new Vector3(4f, 3f, 0f));

            WorldEdgeBoxColliderBuilder.GenerateBoxColliders(_generator);

            var box = _generatorObject.transform.GetChild(0).GetComponent<BoxCollider>();
            var bottomStart = box.transform.TransformPoint(box.center + new Vector3(0f, -box.size.y, -box.size.z) * 0.5f);
            var bottomEnd = box.transform.TransformPoint(box.center + new Vector3(0f, -box.size.y, box.size.z) * 0.5f);
            Assert.That(Vector3.Distance(Vector3.zero, bottomStart), Is.LessThan(TOLERANCE));
            Assert.That(Vector3.Distance(new Vector3(4f, 3f, 0f), bottomEnd), Is.LessThan(TOLERANCE));
        }

        [Test]
        public void Generate_ScaledBoxParent_KeepsWorldSizes()
        {
            _generatorObject.transform.localScale = new Vector3(2f, 2f, 2f);
            _generator.ClearPoints();
            _generator.AddPoint(Vector3.zero);
            _generator.AddPoint(new Vector3(2f, 0f, 0f));

            WorldEdgeBoxColliderBuilder.GenerateBoxColliders(_generator);

            var box = _generatorObject.transform.GetChild(0).GetComponent<BoxCollider>();
            var worldLength = box.size.z * box.transform.lossyScale.z;
            var worldHeight = box.size.y * box.transform.lossyScale.y;
            Assert.AreEqual(4f, worldLength, TOLERANCE);
            Assert.AreEqual(_generator.Height, worldHeight, TOLERANCE);
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
