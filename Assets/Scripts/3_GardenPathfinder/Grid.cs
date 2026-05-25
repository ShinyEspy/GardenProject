using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Grid : MonoBehaviour
{

    public LayerMask unwalkableMask;
    public LayerMask costMultiplierMask;
    public Vector2 gridWorldSize;
    public float costMultiplierValue = 1.5f;
    public float nodeRadius;
    NodePathfinding[,] grid;

    float nodeDiameter;
    int gridSizeX, gridSizeY;

    public List<NodePathfinding> openSet;
    public HashSet<NodePathfinding> closedSet;

    public List<NodePathfinding> path;
    public List<NodePathfinding> smoothPath;

    /***************************************************************************/

    void Awake()
    {
        nodeDiameter = nodeRadius * 2;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);
        CreateGrid();
    }

    /***************************************************************************/

    void CreateGrid()
    {
        grid = new NodePathfinding[gridSizeX, gridSizeY];
        Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.y / 2;

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (y * nodeDiameter + nodeRadius);
                bool walkable = !(Physics.CheckSphere(worldPoint, nodeRadius, unwalkableMask));
                float costMultiplier =
                  (Physics.CheckSphere(worldPoint, nodeRadius, costMultiplierMask)) ?
                    costMultiplierValue : 1.0f;

                grid[x, y] = new NodePathfinding(walkable, worldPoint, x, y, costMultiplier);
            }
        }
    }

    /***************************************************************************/

    public List<NodePathfinding> GetNeighbours(NodePathfinding node, bool eightConnectivity)
    {
        List<NodePathfinding> neighbours = new List<NodePathfinding>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if ((x == 0 && y == 0))
                {
                    continue;
                }
                if (!eightConnectivity && (Mathf.Abs(x) + Mathf.Abs(y) > 1))
                {
                    continue;
                }

                int checkX = node.m_GridX + x;
                int checkY = node.m_GridY + y;

                if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
                {
                    neighbours.Add(grid[checkX, checkY]);
                }
            }
        }

        return neighbours;
    }

    /***************************************************************************/

    public NodePathfinding NodeFromWorldPoint(Vector3 worldPosition)
    {
        float percentX = (worldPosition.x + gridWorldSize.x / 2) / gridWorldSize.x;
        float percentY = (worldPosition.z + gridWorldSize.y / 2) / gridWorldSize.y;
        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
        int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);
        return grid[x, y];
    }

    /***************************************************************************/

    void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, 1, gridWorldSize.y));

        if (grid != null)
        {
            foreach (NodePathfinding n in grid)
            {
                Gizmos.color = (n.m_Walkable) ? Color.white : Color.red;

                // High cost terrain
                if (n.m_CostMultiplier > 1.0f)
                {
                    Gizmos.color += Color.blue;
                }

                // Open set
                if (openSet != null && openSet.Contains(n))
                {
                    Gizmos.color = Color.green;
                }

                // Closed set
                if (closedSet != null && closedSet.Contains(n))
                {
                    Gizmos.color = Color.yellow;
                }

                // Original A* path
                if (path != null && path.Contains(n))
                {
                    Gizmos.color = Color.black;
                }

                Gizmos.DrawCube(n.m_WorldPosition, Vector3.one * (nodeDiameter - .1f));
            }

            // Draw Bresenham critical points
            if (smoothPath != null)
            {
                foreach (NodePathfinding n in smoothPath)
                {
                    Gizmos.color = Color.magenta;
                    Gizmos.DrawSphere(n.m_WorldPosition, nodeDiameter * 0.5f);
                }
            }

            // Draw smoothed path lines
            if (smoothPath != null && smoothPath.Count > 1)
            {
                Gizmos.color = Color.magenta;

                for (int i = 0; i < smoothPath.Count - 1; i++)
                {
                    Gizmos.DrawLine(
                        smoothPath[i].m_WorldPosition,
                        smoothPath[i + 1].m_WorldPosition
                    );
                }
            }
        }
    }

    /***************************************************************************/

    public NodePathfinding GetNode(int x, int y)
    {
        if (x < 0 || x >= gridSizeX || y < 0 || y >= gridSizeY)
            return null;

        return grid[x, y];
    }

}