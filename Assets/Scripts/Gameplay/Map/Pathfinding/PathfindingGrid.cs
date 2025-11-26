using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

namespace RPGCorruption.Map
{
    /// <summary>
    /// Grid system for A* pathfinding.
    /// Generates nodes from Unity Tilemaps for navigation.
    /// </summary>
    public class PathfindingGrid : MonoBehaviour
    {
        [Header("References")]
        public Tilemap groundTilemap;
        public Tilemap obstaclesTilemap;

        private Node[,] grid;
        private int gridSizeX, gridSizeY;
        private Vector3Int minPosition;

        public static PathfindingGrid Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            CreateGrid();
        }

        void CreateGrid()
        {
            BoundsInt bounds = groundTilemap.cellBounds;
            minPosition = bounds.min;

            gridSizeX = bounds.size.x;
            gridSizeY = bounds.size.y;

            Debug.Log($"Creating pathfinding grid of size: {gridSizeX} x {gridSizeY}");

            grid = new Node[gridSizeX, gridSizeY];

            for (int x = 0; x < gridSizeX; x++)
            {
                for (int y = 0; y < gridSizeY; y++)
                {
                    Vector3Int tilePosition = new(
                        minPosition.x + x,
                        minPosition.y + y,
                        0
                    );

                    bool isWalkable = IsTileWalkable(tilePosition);

                    Vector3 worldPosition = groundTilemap.GetCellCenterWorld(tilePosition);

                    grid[x, y] = new Node(isWalkable, worldPosition, x, y);
                }
            }

            Debug.Log("Pathfinding grid created successfully!");
        }

        bool IsTileWalkable(Vector3Int tilePosition)
        {
            TileBase groundTile = groundTilemap.GetTile(tilePosition);

            if (groundTile == null)
                return false;

            TileBase obstacleTile = obstaclesTilemap.GetTile(tilePosition);
            if (obstacleTile != null)
                return false;

            Vector3 worldPosition = groundTilemap.GetCellCenterWorld(tilePosition);
            Collider2D collider = Physics2D.OverlapPoint(worldPosition);

            if (collider != null && !collider.isTrigger)
                return false;

            return true;
        }

        /// <summary>
        /// Gets node from world position
        /// </summary>
        public Node NodeFromWorldPoint(Vector3 worldPos)
        {
            Vector3Int cellPos = groundTilemap.WorldToCell(worldPos);

            int x = cellPos.x - minPosition.x;
            int y = cellPos.y - minPosition.y;

            if (x >= 0 && x < gridSizeX && y >= 0 && y < gridSizeY)
            {
                return grid[x, y];
            }

            return null;
        }

        /// <summary>
        /// Gets neighbors of a node (4 or 8 directional)
        /// </summary>
        public List<Node> GetNeighbors(Node node)
        {
            List<Node> neighbors = new();

            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x == 0 && y == 0)
                        continue;

                    int checkX = node.gridX + x;
                    int checkY = node.gridY + y;

                    if (checkX >= 0 && checkX < gridSizeX &&
                        checkY >= 0 && checkY < gridSizeY)
                        neighbors.Add(grid[checkX, checkY]);
                }
            }

            return neighbors;
        }

        public int GridSizeX => gridSizeX;
        public int GridSizeY => gridSizeY;
    }
}
