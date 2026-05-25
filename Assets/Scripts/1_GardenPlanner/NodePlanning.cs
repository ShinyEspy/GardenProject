
public class NodePlanning
{
    public World.WorldState m_WorldState;

    public Action m_Action;

    public float gCost;
    public float hCost;

    public NodePlanning m_Parent;

    /***************************************************************************/

    public NodePlanning(World.WorldState worldState, Action action)
    {
        m_WorldState = worldState;
        m_Action = action;

        gCost = 0.0f;
        hCost = 0.0f;
        m_Parent = null;
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

    public bool Equals(NodePlanning other)
    {
        return m_WorldState == other.m_WorldState;
    }

    /***************************************************************************/

}
