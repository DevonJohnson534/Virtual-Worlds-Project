using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(Rigidbody))]
public class Bullet : MonoBehaviour
{
	public float autoDestroyTime = 5f;
	public float movespeed = 2f;
	public int damage = 5;

	private Rigidbody rigidbody;

	void Awake()
	{
		rigidbody = GetComponent<Rigidbody>();
		Invoke(nameof(DestroyBullet), autoDestroyTime);
	}

	//position and fire the bullet.
	public void Launch(Vector3 position, Vector3 direction)
	{
		transform.SetPositionAndRotation(position, Quaternion.LookRotation(direction));
		rigidbody.linearVelocity = direction.normalized * movespeed;
	}

	private void OnTriggerEnter(Collider other)
	{
		//print("collision with" + other.gameObject.name);
		if (other.TryGetComponent(out Damageable damageable))
		{
			damageable.TakeDamage(damage);
		}
		DestroyBullet();
	}

	private void DestroyBullet()
	{
		CancelInvoke(nameof(DestroyBullet));
		Destroy(this.gameObject);
	}
}
