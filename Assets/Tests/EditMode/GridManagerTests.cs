using NUnit.Framework;
using UnityEngine;
using ThreeDee.Grid;

namespace ThreeDee.Tests.EditMode
{
    public class GridManagerTests
    {
        private GridManager _grid;
        private GameObject _gridGo;

        [SetUp]
        public void SetUp()
        {
            _gridGo = new GameObject("TestGrid");
            _grid = _gridGo.AddComponent<GridManager>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_gridGo);
        }

        [Test]
        public void Create_ReturnsGridWithCorrectDimensions()
        {
            var grid = GridManager.Create(8, 6, 3f);

            Assert.AreEqual(8, grid.GridWidth);
            Assert.AreEqual(6, grid.GridHeight);
            Assert.AreEqual(3f, grid.CellSize);

            Object.DestroyImmediate(grid.gameObject);
        }

        [Test]
        public void IsValidPosition_ReturnsTrueForValidCoords()
        {
            var grid = GridManager.Create(10, 10, 2f);

            Assert.IsTrue(grid.IsValidPosition(0, 0));
            Assert.IsTrue(grid.IsValidPosition(9, 9));
            Assert.IsTrue(grid.IsValidPosition(5, 5));

            Object.DestroyImmediate(grid.gameObject);
        }

        [Test]
        public void IsValidPosition_ReturnsFalseForOutOfBounds()
        {
            var grid = GridManager.Create(10, 10, 2f);

            Assert.IsFalse(grid.IsValidPosition(-1, 0));
            Assert.IsFalse(grid.IsValidPosition(0, -1));
            Assert.IsFalse(grid.IsValidPosition(10, 0));
            Assert.IsFalse(grid.IsValidPosition(0, 10));

            Object.DestroyImmediate(grid.gameObject);
        }

        [Test]
        public void GridToWorldPosition_CentersGridAtOrigin()
        {
            var grid = GridManager.Create(10, 10, 2f);

            // Center cell (4,4) with 10 cells of size 2 = 20 total
            // Offset = -10 + 1 = -9, so pos = 4*2 + (-9) = -1
            Vector3 center = grid.GridToWorldPosition(4, 4);
            Assert.AreEqual(-1f, center.x, 0.01f);
            Assert.AreEqual(0f, center.y, 0.01f);
            Assert.AreEqual(-1f, center.z, 0.01f);

            Object.DestroyImmediate(grid.gameObject);
        }

        [Test]
        public void WorldToGridPosition_RoundTrips()
        {
            var grid = GridManager.Create(10, 10, 2f);

            Vector3 worldPos = grid.GridToWorldPosition(3, 7);
            var (x, z) = grid.WorldToGridPosition(worldPos);

            Assert.AreEqual(3, x);
            Assert.AreEqual(7, z);

            Object.DestroyImmediate(grid.gameObject);
        }

        [Test]
        public void WorldToGridPosition_ClampsOutOfBounds()
        {
            var grid = GridManager.Create(10, 10, 2f);

            var (x, z) = grid.WorldToGridPosition(new Vector3(100f, 0f, 100f));

            Assert.AreEqual(9, x);
            Assert.AreEqual(9, z);

            Object.DestroyImmediate(grid.gameObject);
        }

        [Test]
        public void GenerateGrid_CreatesCorrectNumberOfCells()
        {
            var grid = GridManager.Create(5, 5, 2f);
            grid.GenerateGrid();

            Assert.AreEqual(25, grid.transform.childCount);

            Object.DestroyImmediate(grid.gameObject);
        }
    }
}
