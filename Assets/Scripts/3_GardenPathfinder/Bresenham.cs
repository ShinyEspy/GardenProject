
using System.Collections.Generic;
using UnityEngine;


public class Bresenham : MonoBehaviour
{
    Grid grid;

    void Awake()
    {
        grid = GetComponent<Grid>();
    }

    public void SmoothPath(List<NodePathfinding> path)
    {
        if (path == null || path.Count < 2)
            return;

        List<NodePathfinding> smooth = new List<NodePathfinding>();

        int currentIndex = 0;
        smooth.Add(path[currentIndex]);

        while (currentIndex < path.Count - 1)
        {
            int nextIndex = path.Count - 1;

            // Try to connect directly to the farthest node
            for (int i = path.Count - 1; i > currentIndex; i--)
            {
                if (BresenhamWalkable(
                    path[currentIndex].m_GridX,
                    path[currentIndex].m_GridY,
                    path[i].m_GridX,
                    path[i].m_GridY))
                {
                    nextIndex = i;
                    break;
                }
            }

            smooth.Add(path[nextIndex]);
            currentIndex = nextIndex;
        }

        grid.smoothPath = smooth;
    }

    /***************************************************************************/

    bool BresenhamWalkable(int x, int y, int x2, int y2)
    {
        int w = x2 - x;
        int h = y2 - y;

        int dx1 = 0, dy1 = 0, dx2 = 0, dy2 = 0;

        if (w < 0) dx1 = -1;
        else if (w > 0) dx1 = 1;

        if (h < 0) dy1 = -1;
        else if (h > 0) dy1 = 1;

        if (w < 0) dx2 = -1;
        else if (w > 0) dx2 = 1;

        int longest = Mathf.Abs(w);
        int shortest = Mathf.Abs(h);

        if (!(longest > shortest))
        {
            longest = Mathf.Abs(h);
            shortest = Mathf.Abs(w);

            if (h < 0)
                dy2 = -1;
            else if (h > 0)
                dy2 = 1;

            dx2 = 0;
        }

        int numerator = longest >> 1;

        for (int i = 0; i <= longest; i++)
        {
            NodePathfinding node = grid.GetNode(x, y);

            // obstacle check
            if (node == null || !node.m_Walkable)
                return false;

            // terrain cost check
            if (node.m_CostMultiplier > 1.0f)
                return false;

            numerator += shortest;

            if (!(numerator < longest))
            {
                numerator -= longest;
                x += dx1;
                y += dy1;
            }
            else
            {
                x += dx2;
                y += dy2;
            }
        }

        return true;
    }

}