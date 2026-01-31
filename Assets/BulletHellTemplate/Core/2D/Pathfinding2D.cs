using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace BulletHellTemplate
{
    /// <summary>
    /// Provides A* pathfinding on a 2D grid, using a Tilemap to mark blocked cells.
    /// </summary>
    public class Pathfinding2D : MonoBehaviour
    {
        [Tooltip("Tilemap whose tiles represent obstacles (blocked cells)")]
        public Tilemap obstacleTilemap;

        private Node[,] grid;
        private int gridWidth;
        private int gridHeight;
        private Vector3Int origin;

        private void Awake()
        {
            InitializeGrid();
        }

        /// <summary>
        /// Builds the grid of nodes based on the obstacleTilemap bounds.
        /// </summary>
        private void InitializeGrid()
        {
            BoundsInt bounds = obstacleTilemap.cellBounds;
            origin = bounds.min;
            gridWidth = bounds.size.x;
            gridHeight = bounds.size.y;
            grid = new Node[gridWidth, gridHeight];

            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    Vector3Int cell = new Vector3Int(origin.x + x, origin.y + y, origin.z);
                    bool walkable = !obstacleTilemap.HasTile(cell);
                    Vector3 worldPos = obstacleTilemap.CellToWorld(cell) + obstacleTilemap.cellSize * 0.5f;
                    grid[x, y] = new Node(walkable, worldPos, x, y);
                }
            }
        }

        /// <summary>
        /// Finds a path from start to end world positions. Returns a list of world-space waypoints.
        /// </summary>
        public List<Vector3> FindPath(Vector3 startWorld, Vector3 endWorld)
        {
            Node startNode = NodeFromWorldPoint(startWorld);
            Node targetNode = NodeFromWorldPoint(endWorld);

            var openSet = new List<Node>();
            var closedSet = new HashSet<Node>();
            openSet.Add(startNode);

            while (openSet.Count > 0)
            {
                Node current = openSet[0];
                for (int i = 1; i < openSet.Count; i++)
                {
                    if (openSet[i].FCost < current.FCost || (openSet[i].FCost == current.FCost && openSet[i].HCost < current.HCost))
                        current = openSet[i];
                }

                openSet.Remove(current);
                closedSet.Add(current);

                if (current == targetNode)
                    return RetracePath(startNode, targetNode);

                foreach (Node neighbor in GetNeighbors(current))
                {
                    if (!neighbor.walkable || closedSet.Contains(neighbor))
                        continue;

                    int newCost = current.GCost + GetDistance(current, neighbor);
                    if (newCost < neighbor.GCost || !openSet.Contains(neighbor))
                    {
                        neighbor.GCost = newCost;
                        neighbor.HCost = GetDistance(neighbor, targetNode);
                        neighbor.parent = current;
                        if (!openSet.Contains(neighbor))
                            openSet.Add(neighbor);
                    }
                }
            }

            return null; // no path found
        }

        private List<Vector3> RetracePath(Node start, Node end)
        {
            var path = new List<Vector3>();
            Node current = end;
            while (current != start)
            {
                path.Add(current.worldPosition);
                current = current.parent;
            }
            path.Reverse();
            return path;
        }

        private Node NodeFromWorldPoint(Vector3 worldPos)
        {
            Vector3Int cell = obstacleTilemap.WorldToCell(worldPos);
            int x = cell.x - origin.x;
            int y = cell.y - origin.y;
            x = Mathf.Clamp(x, 0, gridWidth - 1);
            y = Mathf.Clamp(y, 0, gridHeight - 1);
            return grid[x, y];
        }

        private List<Node> GetNeighbors(Node node)
        {
            var neighbors = new List<Node>();
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0) continue;
                    // allow 4-way only
                    if (dx != 0 && dy != 0) continue;

                    int checkX = node.gridX + dx;
                    int checkY = node.gridY + dy;
                    if (checkX >= 0 && checkX < gridWidth && checkY >= 0 && checkY < gridHeight)
                        neighbors.Add(grid[checkX, checkY]);
                }
            }
            return neighbors;
        }

        private int GetDistance(Node a, Node b)
        {
            int dstX = Mathf.Abs(a.gridX - b.gridX);
            int dstY = Mathf.Abs(a.gridY - b.gridY);
            return dstX + dstY;
        }

        private class Node
        {
            public bool walkable;
            public Vector3 worldPosition;
            public int GCost, HCost;
            public Node parent;
            public int gridX, gridY;
            public int FCost => GCost + HCost;

            public Node(bool walkable, Vector3 worldPos, int x, int y)
            {
                this.walkable = walkable;
                worldPosition = worldPos;
                gridX = x;
                gridY = y;
            }
        }
    }
}
