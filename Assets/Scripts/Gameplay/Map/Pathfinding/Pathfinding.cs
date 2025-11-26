using System.Collections.Generic;
using UnityEngine;

namespace RPGCorruption.Map
{
    /// <summary>
    /// A* Pathfinding algorithm implementation.
    /// Finds the shortest path between two points on the grid.
    /// </summary>
    public class Pathfinding : MonoBehaviour
    {
        private PathfindingGrid grid;

        public static Pathfinding Instance { get; private set; }

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
            grid = PathfindingGrid.Instance;

            if (grid == null)
                Debug.LogError("PathfindingGrid not found! Make sure it exists in the scene.");
        }

        /// <summary>
        /// Finds a path from startPos to targetPos using A* algorithm
        /// </summary>
        public List<Vector3> FindPath(Vector3 startPos, Vector3 targetPos)
        {
            Node startNode = grid.NodeFromWorldPoint(startPos);
            Node targetNode = grid.NodeFromWorldPoint(targetPos);

            if (startNode == null || targetNode == null)
                return null;

            if (!targetNode.walkable)
                return null;

            List<Node> openSet = new();
            HashSet<Node> closedSet = new();

            openSet.Add(startNode);

            while (openSet.Count > 0)
            {
                Node currentNode = openSet[0];
                for (int i = 1; i < openSet.Count; i++)
                {
                    if (openSet[i].fCost < currentNode.fCost ||
                        (openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost))
                        currentNode = openSet[i];
                }

                openSet.Remove(currentNode);
                closedSet.Add(currentNode);

                if (currentNode == targetNode)
                    return RetracePath(startNode, targetNode);

                foreach (Node neighbor in grid.GetNeighbors(currentNode))
                {
                    if (!neighbor.walkable || closedSet.Contains(neighbor))
                        continue;

                    int newMovementCostToNeighbor = currentNode.gCost + GetDistance(currentNode, neighbor);

                    if (newMovementCostToNeighbor < neighbor.gCost || !openSet.Contains(neighbor))
                    {
                        neighbor.gCost = newMovementCostToNeighbor;
                        neighbor.hCost = GetDistance(neighbor, targetNode);
                        neighbor.parent = currentNode;

                        if (!openSet.Contains(neighbor))
                        {
                            openSet.Add(neighbor);
                        }
                    }
                }
            }

            Debug.LogWarning("No path found to target!");
            return null;
        }

        /// <summary>
        /// Retraces the path from end node to start node
        /// </summary>
        private List<Vector3> RetracePath(Node startNode, Node endNode)
        {
            List<Node> path = new();
            Node currentNode = endNode;

            while (currentNode != startNode)
            {
                path.Add(currentNode);
                currentNode = currentNode.parent;
            }

            path.Reverse();

            List<Vector3> waypoints = new();
            foreach (Node node in path)
            {
                waypoints.Add(node.worldPosition);
            }

            return waypoints;
        }

        /// <summary>
        /// Calculates distance between two nodes (Manhattan distance)
        /// </summary>
        private int GetDistance(Node nodeA, Node nodeB)
        {
            int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
            int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

            if (dstX > dstY)
                return 14 * dstY + 10 * (dstX - dstY);
            return 14 * dstX + 10 * (dstY - dstX);
        }

        /// <summary>
        /// Debug: Draws the path in Scene View
        /// </summary>
        public void DrawPath(List<Vector3> path)
        {
            if (path == null || path.Count == 0) return;

            for (int i = 0; i < path.Count - 1; i++)
            {
                Debug.DrawLine(path[i], path[i + 1], Color.cyan, 2f);
            }
        }
    }
}