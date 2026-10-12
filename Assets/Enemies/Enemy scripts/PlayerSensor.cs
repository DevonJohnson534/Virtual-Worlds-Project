using UnityEngine;

public class PlayerSensor : MonoBehaviour
{
    public delegate void PlayerEnterEvent(Transform player);
    public delegate void PlayerExitEvent(Vector3 lastKnownPosition);
    public event PlayerEnterEvent onPlayerEnter;
    public event PlayerExitEvent onPlayerExit;

    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent(out Player player))
        {
            onPlayerEnter?.Invoke(player.transform);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.TryGetComponent(out Player player))
        {
            onPlayerExit?.Invoke(other.transform.position);
        }
    }
}
