using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Unit : MonoBehaviour
{
    [Tooltip("How fast the Seeker NPC moves around.")]
    public float m_Speed = 6.0f;
    [Tooltip("Drag here the A* GameObject with the Pathfinding, Grid and Bresenham scripts.")]
    public GameObject AStar;

    public bool IsMoving { get; private set; }
    public bool HasArrived { get; private set; }

    private List<NodePathfinding> mPath;
    private Grid grid;
    private Pathfinding pathInstance;

    private int targetIndex;

    /***************************************************************************/

    void Start()
    {
        mPath = new List<NodePathfinding>();
        grid = AStar.GetComponent<Grid>();
        pathInstance = AStar.GetComponent<Pathfinding>();
    }

    /***************************************************************************/

    public void MoveTo(Vector3 targetPosition)
    {
        pathInstance.FindPath(transform.position, targetPosition, -1);

        mPath.Clear();

        List<NodePathfinding> pathToUse =
            grid.smoothPath != null && grid.smoothPath.Count > 0
            ? grid.smoothPath
            : grid.path;

        if (pathToUse == null || pathToUse.Count == 0)
        {
            Debug.LogWarning("[PF-U]: No path found.");
            IsMoving = false;
            HasArrived = false;
            return;
        }

        foreach (NodePathfinding node in pathToUse)
        {
            mPath.Add(node);
        }

        targetIndex = 0;
        IsMoving = true;
        HasArrived = false;

        StopCoroutine("FollowPath");
        StartCoroutine("FollowPath");
    }

    /***************************************************************************/

    IEnumerator FollowPath()
    {
        Vector3 currentWaypoint = mPath[0].m_WorldPosition;
        currentWaypoint.y = transform.position.y;

        while (true)
        {
            if (Vector3.Distance(transform.position, currentWaypoint) < 0.1f)
            {
                targetIndex++;

                if (targetIndex >= mPath.Count)
                {
                    IsMoving = false;
                    HasArrived = true;
                    yield break;
                }
                currentWaypoint = mPath[targetIndex].m_WorldPosition;
                currentWaypoint.y = transform.position.y;
            }

            Vector3 lookTarget = new Vector3(currentWaypoint.x, transform.position.y, currentWaypoint.z);
            transform.LookAt(lookTarget);

            transform.position = Vector3.MoveTowards(transform.position, currentWaypoint, m_Speed * Time.deltaTime);
            yield return null;

        }
    }

    /***************************************************************************/

    public void OnDrawGizmos()
    {
        if (mPath != null)
        {
            for (int i = targetIndex; i < mPath.Count; i++)
            {
                Gizmos.color = Color.black;
                Gizmos.DrawCube(mPath[i].m_WorldPosition, Vector3.one);

                if (i == targetIndex)
                {
                    Gizmos.DrawLine(transform.position, mPath[i].m_WorldPosition);
                }
                else
                {
                    Gizmos.DrawLine(mPath[i - 1].m_WorldPosition, mPath[i].m_WorldPosition);
                }
            }
        }
    }

    /***************************************************************************/

}
