using UnityEngine;
using VirtualWorlds.FSM;

[RequireComponent(typeof(SphereCollider))]
public class PlayerSensorWedge : PlayerSensor
{
	public float wedgeHalfAngle = 60; //half of the wedge's angle in degrees. this is 60 for a single human eyeball
	public float alwaysDetectRadius = 8; //always detects within this radius, regardless of angle
	public float tooCloseRadius = 0; //never detects within this radius, regardless of angle

	public float losRadius = 0.1f; //adjusts line of sight sensitivity around corners

	private LayerMask losMask; //includes layers that block line of sight, plus the player's layer
	private float losMaxDistance;
	private Transform launchSpot;

	private bool awareOfPlayer = false;

	public void Awake()
	{
		losMaxDistance = GetComponent<SphereCollider>().radius;
		GetComponentInParent<Rigidbody>().sleepThreshold = 0; //keep looking for the player, even if not moving
		launchSpot = GetComponentInParent<Eyeball>().launchSpot; //if using with something other than eyeball, change this
		losMask = GetComponentInParent<Eyeball>().losMask; //if using with something other than eyeball, change this
	}

	private bool hasLineOfSight(Transform target)
	{
		//so it can't see through walls
		RaycastHit hit;
		Vector3 direction = (target.position - launchSpot.position).normalized;
		bool hit_something = Physics.SphereCast(launchSpot.position, losRadius, direction, out hit, losMaxDistance, losMask);
		//Debug.Log(hit.collider.name);
		if (hit_something) return hit.collider.TryGetComponent(out PlayerController player);
		else return false;
	}

	private bool playerInSensor(Collider other)
	{
		//skip if or the collider isn't the player
		if (!other.TryGetComponent(out PlayerController player)) return false;

		Vector3 toTarget = other.bounds.center - transform.position;
		float distBetween = toTarget.magnitude;

		if (distBetween < tooCloseRadius) return false; //too close, skip

		//Vector3 forward = transform.forward; forward.y = 0; toTarget.y = 0; //this would ignore the y axis when calculating angle
		float angleBetween = Vector3.Angle(transform.forward, toTarget);

		return (angleBetween <= wedgeHalfAngle || distBetween <= alwaysDetectRadius);
	}
	protected override void OnTriggerEnter(Collider other)
	{
		if (playerInSensor(other) && hasLineOfSight(other.transform))
		{
			awareOfPlayer = true;
			base.OnTriggerEnter(other);
		}
	}
	protected void OnTriggerStay(Collider other)
	{
		//this is for if the player enters the collider in a blind spot, then moves to a place where they can be seen
		if (playerInSensor(other) && hasLineOfSight(other.transform))
		{
			awareOfPlayer = true;
			base.OnTriggerEnter(other);
		}
		else //if los is lost, even though the player is still in the collider
		{
			OnTriggerExit(other);
		}
	}

	protected override void OnTriggerExit(Collider other)
	{
		//this is overridden bc the player could be exiting the collider without ever having been detected
		if (awareOfPlayer && other.TryGetComponent(out PlayerController player))
		{
			awareOfPlayer = false;
			base.OnTriggerExit(other);
		}
	}
}
