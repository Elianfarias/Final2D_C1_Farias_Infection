using UnityEngine;

namespace RPGCorruption.Map
{
    /// <summary>
    /// Sistema de grilla para el mapa.
    /// Convierte entre posiciones del mundo y coordenadas de grilla.
    /// </summary>
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
                {
                    instance = FindObjectOfType<TileGrid>();
                }
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

        /// <summary>
        /// Convierte una posición del mundo a coordenadas de grilla
        /// </summary>
        public Vector2Int WorldToGrid(Vector3 worldPosition)
        {
            int x = Mathf.FloorToInt(worldPosition.x / cellSize);
            int y = Mathf.FloorToInt(worldPosition.y / cellSize);
            return new Vector2Int(x, y);
        }

        /// <summary>
        /// Convierte coordenadas de grilla a posición del mundo (centro de la celda)
        /// </summary>
        public Vector3 GridToWorld(Vector2Int gridPosition)
        {
            float x = gridPosition.x * cellSize + cellSize / 2f;
            float y = gridPosition.y * cellSize + cellSize / 2f;
            return new Vector3(x, y, 0f);
        }

        /// <summary>
        /// Snap una posición al centro de la celda más cercana
        /// </summary>
        public Vector3 SnapToGrid(Vector3 worldPosition)
        {
            Vector2Int gridPos = WorldToGrid(worldPosition);
            return GridToWorld(gridPos);
        }

        /// <summary>
        /// Verifica si una posición de grilla está dentro de los límites
        /// </summary>
        public bool IsValidGridPosition(Vector2Int gridPosition)
        {
            return gridPosition.x >= 0 && gridPosition.x < gridSize.x &&
                   gridPosition.y >= 0 && gridPosition.y < gridSize.y;
        }

        /// <summary>
        /// Calcula la distancia en tiles entre dos posiciones
        /// </summary>
        public int GetDistance(Vector2Int from, Vector2Int to)
        {
            return Mathf.Abs(from.x - to.x) + Mathf.Abs(from.y - to.y);
        }

        /// <summary>
        /// Dibuja la grilla en el editor
        /// </summary>
        private void OnDrawGizmos()
        {
            if (!showGrid) return;

            Gizmos.color = gridColor;

            // Dibujar líneas verticales
            for (int x = 0; x <= gridSize.x; x++)
            {
                Vector3 start = new Vector3(x * cellSize, 0, 0);
                Vector3 end = new Vector3(x * cellSize, gridSize.y * cellSize, 0);
                Gizmos.DrawLine(start, end);
            }

            // Dibujar líneas horizontales
            for (int y = 0; y <= gridSize.y; y++)
            {
                Vector3 start = new Vector3(0, y * cellSize, 0);
                Vector3 end = new Vector3(gridSize.x * cellSize, y * cellSize, 0);
                Gizmos.DrawLine(start, end);
            }
        }
    }
}