using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Pathfinding : MonoBehaviour
{
    public HeuristicType heuristicMode;

    [Tooltip("Drag here the seeker GameObject, for the GardenPlanner if should be whatever represents the Gardener NPC.")]
    public Transform m_Seeker;
    [Tooltip("This is only used if Basic Pathfinding is true, else you can leave it empty.")]
    public Transform m_Target;

    NodePathfinding CurrentStartNode;
    NodePathfinding CurrentTargetNode;

    Grid Grid;
    Bresenham bresenham;

    int Iterations = 0;
    float LastStepTime = 0.0f;
    float TimeBetweenSteps = 0.01f;

    public bool BasicPathfinding = false;

    bool EightConnectivity = true;

    /***************************************************************************/

    void Awake()
    {
        Grid = GetComponent<Grid>();
        bresenham = GetComponent<Bresenham>();

        Iterations = 0;
        LastStepTime = 0.0f;
    }

    /***************************************************************************/

    void Update()
    {
        if (BasicPathfinding)
        {
            SeekerTargetPath();
        }
    }

    /***************************************************************************/

    void SeekerTargetPath()
    {
        // Positions changed?
        if (PathInvalid())
        {
            // Remove old path
            if (Grid.path != null)
            {
                Grid.path.Clear();
            }
            // Start calculating path again
            Iterations = 0;
            if (TimeBetweenSteps == 0.0f)
            {
                Iterations = -1;
            }
            FindPath(m_Seeker.position, m_Target.position, Iterations);
        }
        else
        {
            // Path found?
            if (Iterations >= 0)
            {
                // One or more iterations?
                if (TimeBetweenSteps == 0.0f)
                {
                    // One iteration, look until path is found
                    Iterations = -1;
                    FindPath(m_Seeker.position, m_Target.position, Iterations);
                }
                else if (Time.time > LastStepTime + TimeBetweenSteps)
                {
                    // Iterate increasing depth every time step
                    LastStepTime = Time.time;
                    Iterations++;
                    FindPath(m_Seeker.position, m_Target.position, Iterations);
                }
            }
        }
    }

    bool PathInvalid()
    {
        return CurrentStartNode != Grid.NodeFromWorldPoint(m_Seeker.position) || CurrentTargetNode != Grid.NodeFromWorldPoint(m_Target.position);
    }

    /***************************************************************************/

    public void FindPath(Vector3 startPos, Vector3 targetPos, int iterations)
    {
        CurrentStartNode = Grid.NodeFromWorldPoint(startPos);
        CurrentTargetNode = Grid.NodeFromWorldPoint(targetPos);

        // Initialize start node costs
        CurrentStartNode.gCost = 0;
        CurrentStartNode.hCost = Heuristic(CurrentStartNode, CurrentTargetNode);
        CurrentStartNode.m_Parent = null;

        List<NodePathfinding> openSet = new List<NodePathfinding>();
        HashSet<NodePathfinding> closedSet = new HashSet<NodePathfinding>();

        openSet.Add(CurrentStartNode);
        Grid.openSet = openSet;

        int currentIteration = 0;
        NodePathfinding node = CurrentStartNode;
        while (openSet.Count > 0 && node != CurrentTargetNode && (iterations == -1 || currentIteration < iterations))
        {
            // Select best node from open list
            node = openSet[0];

            // Find node with lowest fCost
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].fCost < node.fCost ||
                    (openSet[i].fCost == node.fCost && openSet[i].hCost < node.hCost))
                {
                    node = openSet[i];
                }
            }

            // Manage open/closed list
            openSet.Remove(node);
            closedSet.Add(node);
            Grid.openSet = openSet;
            Grid.closedSet = closedSet;



            // Check destination
            if (node != CurrentTargetNode)
            {

                // Open neighbours
                foreach (NodePathfinding neighbour in Grid.GetNeighbours(node, EightConnectivity))
                {
                    // Ignore non-walkable nodes or already evaluated nodes
                    if (!neighbour.m_Walkable || closedSet.Contains(neighbour))
                    {
                        continue;
                    }

                    float newCostToNeighbour = node.gCost + GetDistance(node, neighbour) * neighbour.m_CostMultiplier;

                    if (newCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                    {
                        neighbour.gCost = newCostToNeighbour;
                        neighbour.hCost = Heuristic(neighbour, CurrentTargetNode);
                        neighbour.m_Parent = node;

                        if (!openSet.Contains(neighbour))
                        {
                            openSet.Add(neighbour);
                        }
                    }
                }

                currentIteration++;
            }
            else
            {
                // Path found!
                RetracePath(CurrentStartNode, CurrentTargetNode);

                // Path found
                Iterations = -1;

                Debug.Log("[PF]: Pathfinding statistics:");
                Debug.LogFormat("[PF]: Total nodes:  {0}", openSet.Count + closedSet.Count);
                Debug.LogFormat("[PF]: Open nodes:   {0}", openSet.Count);
                Debug.LogFormat("[PF]: Closed nodes: {0}", closedSet.Count);
            }
        }
    }

    /***************************************************************************/

    void RetracePath(NodePathfinding startNode, NodePathfinding endNode)
    {
        List<NodePathfinding> path = new List<NodePathfinding>();

        NodePathfinding currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.m_Parent;
        }

        path.Reverse();

        Grid.path = path; 

        //We smooth the path using Bresenham
        bresenham.SmoothPath(Grid.path);
    }

    /***************************************************************************/

    float GetDistance(NodePathfinding nodeA, NodePathfinding nodeB)
    {
        // Distance function
        //nodeA.mGridX   nodeB.mGridX
        //nodeA.mGridY   nodeB.mGridY
        if (EightConnectivity)
        {
            int dstX = Mathf.Abs(nodeA.m_GridX - nodeB.m_GridX);
            int dstY = Mathf.Abs(nodeA.m_GridY - nodeB.m_GridY);

            if (dstX > dstY)
                return 14 * dstY + 10 * (dstX - dstY);
            return 14 * dstX + 10 * (dstY - dstX);
        }
        else
        {
            int dstX = Mathf.Abs(nodeA.m_GridX - nodeB.m_GridX);
            int dstY = Mathf.Abs(nodeA.m_GridY - nodeB.m_GridY);

            return 10 * (dstX + dstY);
        }
    }

    /***************************************************************************/

    float Heuristic(NodePathfinding nodeA, NodePathfinding nodeB)
    {
        // Heuristic function

        int dstX = Mathf.Abs(nodeA.m_GridX - nodeB.m_GridX);
        int dstY = Mathf.Abs(nodeA.m_GridY - nodeB.m_GridY);

        switch (heuristicMode)
        {
            case HeuristicType.AStar_Normal:
            default:
                return 10 * (dstX + dstY);
            
            case HeuristicType.Dijkstra_NoHeuristic:
                return 0;
            
            case HeuristicType.NonAdmissible_Aggressive:
                return 20 * (dstX + dstY);
        }
    }

    /***************************************************************************/

}


public enum HeuristicType
{
    AStar_Normal,
    Dijkstra_NoHeuristic,
    NonAdmissible_Aggressive
}