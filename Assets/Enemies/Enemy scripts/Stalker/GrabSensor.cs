using UnityEngine;
using VirtualWorlds.FSM;



public class GrabSensor : MonoBehaviour
{

    public delegate void AttackEvent(Collider other);
    public event AttackEvent SuccessfulAttack;

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PlayerController player))
        {
            SuccessfulAttack?.Invoke(other);
        }
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out PlayerController player))
        {
            SuccessfulAttack?.Invoke(other);
        }
    }
}
