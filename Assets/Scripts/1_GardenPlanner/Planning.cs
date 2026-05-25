using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Planning : MonoBehaviour
{
    NodePlanning CurrentStartNode;
    NodePlanning CurrentTargetNode;

    World m_World;

    [Header("World State Parameters")]
    [Tooltip("How the world starts.")]
    public World.WorldState WorldStateStart = World.WorldState.WORLD_STATE_NONE;
    [Tooltip("The goal of the NPC")]
    public World.WorldState WorldStateFinish = World.WorldState.WORLD_STATE_FLOWERS_STORED;

    [Header("Behaviour Tree Properties")]
    public bool isBehaviourTreeOn = false;

    /***************************************************************************/

    void Awake()
    {
        m_World = GetComponent<World>();

        if (!isBehaviourTreeOn)
        {
            Debug.Log("[PL]: Planning...");
            FindPlan(WorldStateStart, WorldStateFinish);
        }
        
    }

    /***************************************************************************/

    void Update()
    {
    }

    /***************************************************************************/

    public List<NodePlanning> FindPlan(World.WorldState startWorldState, World.WorldState targetWorldState)
    {
        CurrentStartNode = new NodePlanning(startWorldState, null);
        CurrentTargetNode = new NodePlanning(targetWorldState, null);

        List<NodePlanning> openSet = new List<NodePlanning>();
        HashSet<NodePlanning> closedSet = new HashSet<NodePlanning>();
        openSet.Add(CurrentStartNode);
        m_World.openSet = openSet;

        NodePlanning node = CurrentStartNode;
        while (openSet.Count > 0 && ((node.m_WorldState & CurrentTargetNode.m_WorldState) != CurrentTargetNode.m_WorldState))
        {
            // Select best node from open list
            node = openSet[0];

            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].fCost < node.fCost || (openSet[i].fCost == node.fCost && openSet[i].hCost < node.hCost))
                {
                    node = openSet[i];
                }
            }

            // Manage open/closed list
            openSet.Remove(node);
            closedSet.Add(node);
            m_World.openSet = openSet;
            m_World.closedSet = closedSet;



            // Check destination
            if (((node.m_WorldState & CurrentTargetNode.m_WorldState) != CurrentTargetNode.m_WorldState))
            {

                // Open neighbours
                foreach (NodePlanning neighbour in m_World.GetNeighbours(node))
                {
                    if ( /*!neighbour.mWalkable ||*/ closedSet.Any(n => n.m_WorldState == neighbour.m_WorldState))
                    {
                        continue;
                    }

                    float newCostToNeighbour = node.gCost + GetDistance(node, neighbour);
                    if (newCostToNeighbour < neighbour.gCost || !openSet.Any(n => n.m_WorldState == neighbour.m_WorldState))
                    {
                        neighbour.gCost = newCostToNeighbour;
                        neighbour.hCost = Heuristic(neighbour, CurrentTargetNode);
                        neighbour.m_Parent = node;

                        if (!openSet.Any(n => n.m_WorldState == neighbour.m_WorldState))
                        {
                            openSet.Add(neighbour);
                            m_World.openSet = openSet;
                        }
                        else
                        {
                            // Find neighbour and replace
                            openSet[openSet.FindIndex(x => x.m_WorldState == neighbour.m_WorldState)] = neighbour;
                        }
                    }
                }
            }
            else
            {
                // Path found!

                // End node must be copied
                CurrentTargetNode.m_Parent = node.m_Parent;
                CurrentTargetNode.m_Action = node.m_Action;
                CurrentTargetNode.gCost = node.gCost;
                CurrentTargetNode.hCost = node.hCost;

                RetracePlan(CurrentStartNode, CurrentTargetNode);

                Debug.Log("[PL]: Planner statistics:");
                Debug.LogFormat("[PL]: Total nodes:  {0}", openSet.Count + closedSet.Count);
                Debug.LogFormat("[PL]: Open nodes:   {0}", openSet.Count);
                Debug.LogFormat("[PL]: Closed nodes: {0}", closedSet.Count);
            }
        }

        // Log plan
        if (m_World.plan == null)
        {
            Debug.Log("[PL]: NO PLAN FOUND");
            return null;
        }

        Debug.Log("[PL]: PLAN FOUND!");
        for (int i = 0; i < m_World.plan.Count; ++i)
        {
            Debug.LogFormat("[PL]: {0} Accumulated cost: {1}", m_World.plan[i].m_Action.m_Name, m_World.plan[i].gCost);
        }

        return m_World.plan;
    }

    /***************************************************************************/

    void RetracePlan(NodePlanning startNode, NodePlanning endNode)
    {
        List<NodePlanning> plan = new List<NodePlanning>();

        NodePlanning currentNode = endNode;

        while (currentNode != startNode)
        {
            plan.Add(currentNode);
            currentNode = currentNode.m_Parent;
        }
        plan.Reverse();

        m_World.plan = plan;
    }

    /***************************************************************************/

    float GetDistance(NodePlanning nodeA, NodePlanning nodeB)
    {
        // Distance function
        return nodeB.m_Action.m_Cost;
    }

    /***************************************************************************/

    float Heuristic(NodePlanning nodeA, NodePlanning nodeB)
    {
        World.WorldState state = nodeA.m_WorldState;
        float estimatedCost = 0.0f;

        // If final goal already satisfied, no remaining cost
        if ((state & nodeB.m_WorldState) == nodeB.m_WorldState)
        {
            return 0.0f;
        }

        // Tulip chain
        if ((state & World.WorldState.WORLD_STATE_TULIPS_HARVESTED) == 0)
        {
            if ((state & World.WorldState.WORLD_STATE_TULIPS_WATERED) == 0)
            {
                estimatedCost += 1.0f; // Water tulips
            }

            estimatedCost += 5.0f; // Harvest tulips
        }

        // Hibiscus chain
        if ((state & World.WorldState.WORLD_STATE_HIBISCUS_HARVESTED) == 0)
        {
            if ((state & World.WorldState.WORLD_STATE_HIBISCUS_WATERED) == 0)
            {
                estimatedCost += 2.0f; // Water hibiscus
            }

            estimatedCost += 4.0f; // Harvest hibiscus
        }

        // To finish, flowers must be stored
        if ((state & World.WorldState.WORLD_STATE_FLOWERS_STORED) == 0)
        {
            estimatedCost += 2.0f; // Store flowers
        }

        return estimatedCost;
    }

    /***************************************************************************/

}
