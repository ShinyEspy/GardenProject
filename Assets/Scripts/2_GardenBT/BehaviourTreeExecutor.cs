using System.Collections.Generic;
using UnityEngine;

public class BehaviourTreeExecutor : MonoBehaviour
{
    private World m_World;
    private Planning m_Planner;

    private int m_currentIndex = 0;

    private List<NodePlanning> m_currentPlan;

    private bool m_isDone = false;

    private Unit m_Unit;

    [Tooltip("Target transform for the Well location.")]
    public Transform wellTarget;
    [Tooltip("Target transform for the Tulips location.")]
    public Transform tulipsTarget;
    [Tooltip("Target transform for the Hibiscus location.")]
    public Transform hibiscusTarget;
    [Tooltip("Target transform for the Storage location.")]
    public Transform storageTarget;

    private bool m_waitingForMovement = false;
    private Action m_pendingMovementAction = null;

    private bool m_waitingForAction = false;
    private float m_actionTimer = 0f;
    private Action m_pendingAction = null;

    [Tooltip("How much time the non-movement actions take (in seconds).")]
    public float actionDuration = 0.75f;

    private void Awake()
    {
        m_Unit = GetComponent<Unit>();
        m_World = GetComponent<World>();
        m_Planner = GetComponent<Planning>();

        Debug.Log("[BT]: wellTarget = " + wellTarget);
        Debug.Log("[BT]: tulipsTarget = " + tulipsTarget);
        Debug.Log("[BT]: hibiscusTarget = " + hibiscusTarget);
        Debug.Log("[BT]: storageTarget = " + storageTarget);
    }

    void Start()
    {
        CreatePlan();
    }

    private void Update()
    {
        if (!m_isDone)
        {
            ExecuteNextAction();
        }
    }

    private bool CreatePlan()
    {
        Debug.Log("[BT]: Creating plan...");

        m_currentPlan = m_Planner.FindPlan(
            m_Planner.WorldStateStart,
            m_Planner.WorldStateFinish
            );

        m_currentIndex = 0;

        if(m_currentPlan == null || m_currentPlan.Count == 0)
        {
            Debug.Log("[BT]: Failed: No plan found!");
            return false;
        }

        Debug.Log("[BT]: Plan created with " + m_currentPlan.Count + " steps.");
        return true;
    }

    private bool ExecuteNextAction()
    {
        if (m_currentIndex >= m_currentPlan.Count)
        {
            Debug.Log("[BT]: Success: Plan completed");
            m_isDone = true;
            return true;
        }

        if (m_waitingForMovement)
        {
            if (m_Unit.HasArrived)
            {
                ApplyEffects(m_pendingMovementAction);

                Debug.Log("[BT]: Arrived and executed: " + m_pendingMovementAction.m_Name);

                m_currentIndex++;
                m_waitingForMovement = false;
                m_pendingMovementAction = null;
            }

            return true;
        }

        if (m_waitingForAction)
        {
            m_actionTimer -= Time.deltaTime;

            if (m_actionTimer <= 0f)
            {
                ApplyEffects(m_pendingAction);

                Debug.Log("[BT]: Executed: " + m_pendingAction.m_Name);

                m_currentIndex++;
                m_waitingForAction = false;
                m_pendingAction = null;
            }

            return true;
        }

        Action action = m_currentPlan[m_currentIndex].m_Action;

        if (!CheckPreconditions(action))
        {
            Debug.Log("[BT]: Preconditions failed for " + action.m_Name);
            return false;
        }

        if (IsMovementAction(action))
        {
            Transform target = GetTargetForAction(action);

            if (target == null)
            {
                Debug.LogError("[BT]: Missing target for action: " + action.m_Name);
                return false;
            }

            m_Unit.MoveTo(target.position);

            m_waitingForMovement = true;
            m_pendingMovementAction = action;

            return true;
        }

        m_waitingForAction = true;
        m_actionTimer = actionDuration;
        m_pendingAction = action;

        Debug.Log("[BT]: Starting action: " + action.m_Name);

        return true;
    }

    private bool CheckPreconditions(Action action)
    {
        return (m_World.m_WorldState & action.m_Preconditions) == action.m_Preconditions;
    }

    private void ApplyEffects(Action action)
    {
        // Add effects
        m_World.m_WorldState |= action.m_Effects;

        // Remove negative effects
        m_World.m_WorldState &= ~action.m_NegativeEffects;
    }

    private bool IsMovementAction(Action action)
    {
        return action.m_ActionType == Action.ActionType.ACTION_TYPE_GO_TO_WELL ||
               action.m_ActionType == Action.ActionType.ACTION_TYPE_GO_TO_TULIPS ||
               action.m_ActionType == Action.ActionType.ACTION_TYPE_GO_TO_HIBISCUS ||
               action.m_ActionType == Action.ActionType.ACTION_TYPE_GO_TO_STORAGE;
    }

    private Transform GetTargetForAction(Action action)
    {
        if (action.m_ActionType == Action.ActionType.ACTION_TYPE_GO_TO_WELL)
            return wellTarget;

        if (action.m_ActionType == Action.ActionType.ACTION_TYPE_GO_TO_TULIPS)
            return tulipsTarget;

        if (action.m_ActionType == Action.ActionType.ACTION_TYPE_GO_TO_HIBISCUS)
            return hibiscusTarget;

        if (action.m_ActionType == Action.ActionType.ACTION_TYPE_GO_TO_STORAGE)
            return storageTarget;

        Debug.LogError("[BT]: No target mapping for action type: " + action.m_ActionType);
        return null;
    }
}
