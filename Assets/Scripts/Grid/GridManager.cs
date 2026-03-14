using UnityEngine;

namespace ThreeDee.Grid
{
    public class GridManager : MonoBehaviour
    {
        [SerializeField] private int _gridWidth = 10;
        [SerializeField] private int _gridHeight = 10;
        [SerializeField] private float _cellSize = 2f;

        private GameObject[,] _cells;

        public int GridWidth => _gridWidth;
        public int GridHeight => _gridHeight;
        public float CellSize => _cellSize;

        public void GenerateGrid()
        {
            _cells = new GameObject[_gridWidth, _gridHeight];

            for (int x = 0; x < _gridWidth; x++)
            {
                for (int z = 0; z < _gridHeight; z++)
                {
                    _cells[x, z] = CreateCell(x, z);
                }
            }
        }

        private GameObject CreateCell(int x, int z)
        {
            var cell = GameObject.CreatePrimitive(PrimitiveType.Plane);
            cell.name = $"Cell_{x}_{z}";
            cell.transform.SetParent(transform);

            // Plane default is 10x10 units, scale to cell size
            float scale = _cellSize / 10f;
            cell.transform.localScale = new Vector3(scale, 1f, scale);
            cell.transform.localPosition = GridToWorldPosition(x, z);

            // Alternate colors for checkerboard
            var renderer = cell.GetComponent<Renderer>();
            if (renderer != null)
            {
                bool isLight = (x + z) % 2 == 0;
                var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                material.color = isLight
                    ? new Color(0.45f, 0.65f, 0.35f)
                    : new Color(0.35f, 0.55f, 0.28f);
                renderer.material = material;
            }

            return cell;
        }

        public Vector3 GridToWorldPosition(int x, int z)
        {
            float offsetX = -(_gridWidth * _cellSize) / 2f + _cellSize / 2f;
            float offsetZ = -(_gridHeight * _cellSize) / 2f + _cellSize / 2f;
            return new Vector3(x * _cellSize + offsetX, 0f, z * _cellSize + offsetZ);
        }

        public (int x, int z) WorldToGridPosition(Vector3 worldPos)
        {
            float offsetX = -(_gridWidth * _cellSize) / 2f + _cellSize / 2f;
            float offsetZ = -(_gridHeight * _cellSize) / 2f + _cellSize / 2f;
            int x = Mathf.RoundToInt((worldPos.x - offsetX) / _cellSize);
            int z = Mathf.RoundToInt((worldPos.z - offsetZ) / _cellSize);
            return (Mathf.Clamp(x, 0, _gridWidth - 1), Mathf.Clamp(z, 0, _gridHeight - 1));
        }

        public bool IsValidPosition(int x, int z)
        {
            return x >= 0 && x < _gridWidth && z >= 0 && z < _gridHeight;
        }

        public static GridManager Create(int width = 10, int height = 10, float cellSize = 2f)
        {
            var go = new GameObject("Grid");
            var grid = go.AddComponent<GridManager>();
            grid._gridWidth = width;
            grid._gridHeight = height;
            grid._cellSize = cellSize;
            return grid;
        }
    }
}
