using UnityEngine;
using System.Collections;

public class NodePathfinding
{

    public bool m_Walkable;
    public Vector3 m_WorldPosition;
    public int m_GridX;
    public int m_GridY;
    public float m_CostMultiplier;

    public float gCost;
    public float hCost;
    public NodePathfinding m_Parent;

    /***************************************************************************/

    public NodePathfinding(bool walkable, Vector3 worldPosition, int gridX, int gridY, float costMultiplier)
    {
        m_Walkable = walkable;
        m_WorldPosition = worldPosition;
        m_GridX = gridX;
        m_GridY = gridY;
        m_CostMultiplier = costMultiplier;
    }

    /***************************************************************************/

    public float fCost
    {
        get
        {
            return gCost + hCost;
        }
    }

    /***************************************************************************/

}
