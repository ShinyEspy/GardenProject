using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class World : MonoBehaviour
{
    public List<NodePlanning> openSet;
    public HashSet<NodePlanning> closedSet;

    public List<NodePlanning> plan;

    [Tooltip("CurrentWorldState")]
    public WorldState m_WorldState;

    public List<Action> m_ActionList;

    [Header("Movement Costs")]
    public float m_CostGoToWell = 2.0f;
    public float m_CostGoToTulips = 3.0f;
    public float m_CostGoToHibiscus = 5.0f;
    public float m_CostGoToStorage = 6.0f;

    /***************************************************************************/

    public enum WorldState
    {
        WORLD_STATE_NONE = 0,
        // World position
        WORLD_STATE_AT_WELL = 1,
        WORLD_STATE_AT_TULIPS = 2,
        WORLD_STATE_AT_HIBISCUS = 4,
        WORLD_STATE_AT_STORAGE = 8,
        // Watering can
        WORLD_STATE_HAS_WATERING_CAN = 16,
        WORLD_STATE_WATERING_CAN_FULL = 32,
        // Flower watering
        WORLD_STATE_TULIPS_WATERED = 64,
        WORLD_STATE_HIBISCUS_WATERED = 128,
        //Flower harvesting
        WORLD_STATE_TULIPS_HARVESTED = 256,
        WORLD_STATE_HIBISCUS_HARVESTED = 512,
        // Objetivo final
        WORLD_STATE_FLOWERS_STORED = 1024
    }

    /***************************************************************************/

    void Awake()
    {
        /*
        m_ActionList.Add(
            new Action(
            Action.ActionType.ACTIONTYPE,
            Preconditions
            Effects (positive)
            NegativeEffects
            cost, "name")
        );

        m_ActionList.Add(
            new Action( //text
                Action.ActionType.ACTION_TYPE_GET_LINE_OF_SIGHT_TO_ENEMY,
                WorldState.WORLD_STATE_GUN_LOADED | WorldState.WORLD_STATE_GUN_OWNED,
                WorldState.WORLD_STATE_LINE_OF_SIGHT_TO_ENEMY,
                WorldState.WORLD_STATE_NONE,
                4.0f, "text")
            );
        */

        m_ActionList = new List<Action>();

        m_ActionList.Add(
          new Action( //Go to well
            Action.ActionType.ACTION_TYPE_GO_TO_WELL,
            WorldState.WORLD_STATE_NONE,
            WorldState.WORLD_STATE_AT_WELL,
            WorldState.WORLD_STATE_AT_TULIPS | WorldState.WORLD_STATE_AT_HIBISCUS | WorldState.WORLD_STATE_AT_STORAGE,
            m_CostGoToWell, "Go to well")
        );

        m_ActionList.Add(
          new Action( //Go to tulips
            Action.ActionType.ACTION_TYPE_GO_TO_TULIPS,
            WorldState.WORLD_STATE_NONE,
            WorldState.WORLD_STATE_AT_TULIPS,
            WorldState.WORLD_STATE_AT_WELL | WorldState.WORLD_STATE_AT_HIBISCUS | WorldState.WORLD_STATE_AT_STORAGE,
            m_CostGoToTulips, "Go to tulips")
        );

        m_ActionList.Add(
          new Action( //Go to hibiscus
            Action.ActionType.ACTION_TYPE_GO_TO_HIBISCUS,
            WorldState.WORLD_STATE_NONE,
            WorldState.WORLD_STATE_AT_HIBISCUS,
            WorldState.WORLD_STATE_AT_WELL | WorldState.WORLD_STATE_AT_TULIPS | WorldState.WORLD_STATE_AT_STORAGE,
            m_CostGoToHibiscus, "Go to hibiscus")
        );

        m_ActionList.Add(
          new Action( //Go to storage
            Action.ActionType.ACTION_TYPE_GO_TO_STORAGE,
            WorldState.WORLD_STATE_NONE,
            WorldState.WORLD_STATE_AT_STORAGE,
            WorldState.WORLD_STATE_AT_WELL | WorldState.WORLD_STATE_AT_TULIPS | WorldState.WORLD_STATE_AT_HIBISCUS,
            m_CostGoToStorage, "Go to storage")
        );

        m_ActionList.Add(
          new Action( //Pick up watering can
            Action.ActionType.ACTION_TYPE_PICK_UP_WATERING_CAN,
            WorldState.WORLD_STATE_NONE,
            WorldState.WORLD_STATE_HAS_WATERING_CAN,
            WorldState.WORLD_STATE_NONE,
            1.0f, "Pick up watering can")
        );

        m_ActionList.Add(
          new Action( //Fill watering can
            Action.ActionType.ACTION_TYPE_FILL_WATERING_CAN,
            WorldState.WORLD_STATE_AT_WELL | WorldState.WORLD_STATE_HAS_WATERING_CAN,
            WorldState.WORLD_STATE_WATERING_CAN_FULL,
            WorldState.WORLD_STATE_NONE,
            0.5f, "Fill watering can")
        );

        m_ActionList.Add(
          new Action( //Water tulips
            Action.ActionType.ACTION_TYPE_WATER_TULIPS,
            WorldState.WORLD_STATE_AT_TULIPS | WorldState.WORLD_STATE_HAS_WATERING_CAN | WorldState.WORLD_STATE_WATERING_CAN_FULL,
            WorldState.WORLD_STATE_TULIPS_WATERED,
            WorldState.WORLD_STATE_WATERING_CAN_FULL, //The watering can is now empty
            1.0f, "Water tulips")
        );

        m_ActionList.Add(
          new Action( //Water hibiscus
            Action.ActionType.ACTION_TYPE_WATER_HIBISCUS,
            WorldState.WORLD_STATE_AT_HIBISCUS | WorldState.WORLD_STATE_HAS_WATERING_CAN | WorldState.WORLD_STATE_WATERING_CAN_FULL,
            WorldState.WORLD_STATE_HIBISCUS_WATERED,
            WorldState.WORLD_STATE_WATERING_CAN_FULL, //The watering can is now empty
            2.0f, "Water hibiscus")
        );

        m_ActionList.Add(
          new Action( //Harvest tulips
            Action.ActionType.ACTION_TYPE_HARVEST_TULIPS,
            WorldState.WORLD_STATE_AT_TULIPS | WorldState.WORLD_STATE_TULIPS_WATERED,
            WorldState.WORLD_STATE_TULIPS_HARVESTED,
            WorldState.WORLD_STATE_NONE,
            5.0f, "Harvest tulips")
        );

        m_ActionList.Add(
          new Action( //Harvest hibiscus
            Action.ActionType.ACTION_TYPE_HARVEST_HIBISCUS,
            WorldState.WORLD_STATE_AT_HIBISCUS | WorldState.WORLD_STATE_HIBISCUS_WATERED,
            WorldState.WORLD_STATE_HIBISCUS_HARVESTED,
            WorldState.WORLD_STATE_NONE,
            4.0f, "Harvest hibiscus")
        );

        m_ActionList.Add(
          new Action( //Store flowers
            Action.ActionType.ACTION_TYPE_STORE_FLOWERS,
            WorldState.WORLD_STATE_TULIPS_HARVESTED | WorldState.WORLD_STATE_HIBISCUS_HARVESTED | WorldState.WORLD_STATE_AT_STORAGE,
            WorldState.WORLD_STATE_FLOWERS_STORED,
            WorldState.WORLD_STATE_NONE,
            2.0f, "Store flowers")
        );
    }

    /***************************************************************************/

    public List<NodePlanning> GetNeighbours(NodePlanning node)
    {
        List<NodePlanning> neighbours = new List<NodePlanning>();

        foreach (Action action in m_ActionList)
        {
            // If preconditions are met we can apply effects and the new state is valid
            if ((node.m_WorldState & action.m_Preconditions) == action.m_Preconditions)
            {
                // Apply action and effects (positive and negative)
                NodePlanning newNodePlanning = new NodePlanning((node.m_WorldState | action.m_Effects) & ~action.m_NegativeEffects, action);
                neighbours.Add(newNodePlanning);
            }
        }

        return neighbours;
    }

    /***************************************************************************/

    public static int PopulationCount(int n)
    {
        return System.Convert.ToString(n, 2).ToCharArray().Count(c => c == '1');
    }

    /***************************************************************************/

}