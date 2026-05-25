using UnityEngine;

public class WateringCanController : MonoBehaviour
{
    public GameObject m_WorldWateringCan;
    public GameObject m_GardenerWateringCan;

    public World m_World;

    private void Update()
    {
        if (m_World == null)
        {
            Debug.LogWarning("[WC]: World reference is missing.");
            return;
        }

        bool hasWateringCan =
            (m_World.m_WorldState & World.WorldState.WORLD_STATE_HAS_WATERING_CAN)
            == World.WorldState.WORLD_STATE_HAS_WATERING_CAN;

        m_WorldWateringCan.SetActive(!hasWateringCan);
        m_GardenerWateringCan.SetActive(hasWateringCan);
    }
}