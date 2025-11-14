using UnityEngine;

namespace RPGCorruption.Map
{
    public class TileGrid : MonoBehaviour
    {
        [Header("Grid Configuration")]
        [SerializeField] private float cellSize = 1f;
        [SerializeField] private Vector2Int gridSize = new Vector2Int(20, 20);

        [Header("Debug")]
        [SerializeField] private bool showGrid = true;
        [SerializeField] private Color gridColor = Color.green;

        // Singleton
        private static TileGrid instance;
        public static TileGrid Instance
        {
            get
            {
                if (instance == null)
                    instance = Object.FindFirstObjectByType<TileGrid>();

                return instance;
            }
        }

        public float CellSize => cellSize;
        public Vector2Int GridSize => gridSize;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
        }

        public Vector2Int WorldToGrid(Vector3 worldPosition)
        {
            int x = Mathf.FloorToInt(worldPosition.x / cellSize);
            int y = Mathf.FloorToInt(worldPosition.y / cellSize);
            return new Vector2Int(x, y);
        }

        public Vector3 GridToWorld(Vector2Int gridPosition)
        {
            float x = gridPosition.x * cellSize + cellSize / 2f;
            float y = gridPosition.y * cellSize + cellSize / 2f;
            return new Vector3(x, y, 0f);
        }

        public Vector3 SnapToGrid(Vector3 worldPosition)
        {
            Vector2Int gridPos = WorldToGrid(worldPosition);
            return GridToWorld(gridPos);
        }

        public bool IsValidGridPosition(Vector2Int gridPosition)
        {
            return gridPosition.x >= 0 && gridPosition.x < gridSize.x &&
                   gridPosition.y >= 0 && gridPosition.y < gridSize.y;
        }

        public int GetDistance(Vector2Int from, Vector2Int to)
        {
            return Mathf.Abs(from.x - to.x) + Mathf.Abs(from.y - to.y);
        }

        private void OnDrawGizmos()
        {
            if (!showGrid) return;

            Gizmos.color = gridColor;

            for (int x = 0; x <= gridSize.x; x++)
            {
                Vector3 start = new(x * cellSize, 0, 0);
                Vector3 end = new(x * cellSize, gridSize.y * cellSize, 0);
                Gizmos.DrawLine(start, end);
            }

            for (int y = 0; y <= gridSize.y; y++)
            {
                Vector3 start = new(0, y * cellSize, 0);
                Vector3 end = new(gridSize.x * cellSize, y * cellSize, 0);
                Gizmos.DrawLine(start, end);
            }
        }
    }
}