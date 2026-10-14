using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class SpearSensor : MonoBehaviour
{

    public delegate void CollisionEvent(Collision collison);
    public event CollisionEvent onCollision;

    public void OnCollisionEnter(Collision collision)
    {
        onCollision?.Invoke(collision);
    }
}
