using UnityEngine;
using ThreeDee.Core;

namespace ThreeDee.Grid
{
    public static class PlaceholderBuildings
    {
        public static GameObject CreateBuilding(string name, Vector3 position, Vector3 scale, Color color)
        {
            var building = GameObject.CreatePrimitive(PrimitiveType.Cube);
            building.name = name;
            building.transform.position = position + Vector3.up * (scale.y / 2f);
            building.transform.localScale = scale;

            var renderer = building.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = MaterialHelper.CreateLit(color);
            }

            return building;
        }

        public static void SpawnDefaultBuildings(GridManager grid)
        {
            float cellSize = grid.CellSize;

            // Town Hall — center, large
            CreateBuilding("TownHall",
                grid.GridToWorldPosition(4, 4),
                new Vector3(cellSize * 0.8f, 3f, cellSize * 0.8f),
                new Color(0.8f, 0.6f, 0.3f));

            // Barracks
            CreateBuilding("Barracks",
                grid.GridToWorldPosition(2, 2),
                new Vector3(cellSize * 0.7f, 2f, cellSize * 0.7f),
                new Color(0.6f, 0.3f, 0.3f));

            // Resource storage
            CreateBuilding("Storage",
                grid.GridToWorldPosition(6, 3),
                new Vector3(cellSize * 0.6f, 1.5f, cellSize * 0.6f),
                new Color(0.4f, 0.5f, 0.7f));

            // Walls — a few segments
            for (int i = 0; i < 5; i++)
            {
                CreateBuilding($"Wall_{i}",
                    grid.GridToWorldPosition(i + 1, 7),
                    new Vector3(cellSize * 0.9f, 1f, cellSize * 0.2f),
                    new Color(0.5f, 0.5f, 0.5f));
            }

            // Tower
            CreateBuilding("Tower",
                grid.GridToWorldPosition(7, 7),
                new Vector3(cellSize * 0.4f, 4f, cellSize * 0.4f),
                new Color(0.7f, 0.7f, 0.3f));
        }
    }
}
