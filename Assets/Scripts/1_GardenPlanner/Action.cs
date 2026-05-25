
public class Action
{
    public ActionType m_ActionType;
    public World.WorldState m_Preconditions;
    public World.WorldState m_Effects;
    public World.WorldState m_NegativeEffects;
    public float m_Cost;
    public string m_Name;

    /***************************************************************************/

    public enum ActionType //La lista de acciones del mundo
    {
        ACTION_TYPE_NONE = -1,
        //Movement
        ACTION_TYPE_GO_TO_WELL,
        ACTION_TYPE_GO_TO_TULIPS,
        ACTION_TYPE_GO_TO_HIBISCUS,
        ACTION_TYPE_GO_TO_STORAGE,
        //Watering can
        ACTION_TYPE_PICK_UP_WATERING_CAN,
        ACTION_TYPE_FILL_WATERING_CAN,
        //Water flowers
        ACTION_TYPE_WATER_TULIPS,
        ACTION_TYPE_WATER_HIBISCUS,
        //Harvest flowers
        ACTION_TYPE_HARVEST_TULIPS,
        ACTION_TYPE_HARVEST_HIBISCUS,
        //Store picked flowers == END of PLAN
        ACTION_TYPE_STORE_FLOWERS,

        ACTION_TYPES
    }

    /***************************************************************************/

    public Action(ActionType actionType, World.WorldState preconditions, World.WorldState effects, World.WorldState negativeEffects, float cost, string name)
    {
        m_ActionType = actionType;
        m_Preconditions = preconditions;
        m_Effects = effects;
        m_NegativeEffects = negativeEffects;
        m_Cost = cost;
        m_Name = name;
    }

    /***************************************************************************/

}
