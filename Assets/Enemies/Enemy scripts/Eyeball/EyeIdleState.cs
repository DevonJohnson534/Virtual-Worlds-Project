using UnityEngine;
using VirtualWorlds.FSM;

namespace VitrualWorlds.FSM
{
	public class EyeIdleState : EyeStateBase
	{
		private Eyeball eyeball;
		private Quaternion lookAroundDir;
		private float nextLookTime;
		public EyeIdleState(bool needsExitTime, Eyeball enemy) : base(needsExitTime, enemy)
		{
			eyeball = enemy;
		}

		public override void OnEnter()
		{
			base.OnEnter();
			// Just stops the movement from navmesh. 
			agent.isStopped = true;
			if (eyeball.awareOfPlayer) animator.Play("Agro");
			else lookAround(true);
		}

		private void lookAround(bool limitDir)
		{
			if (Time.time >= nextLookTime)
			{
				if (!eyeball.AudioSource.isPlaying)
				{
					eyeball.AudioSource.PlayOneShot(eyeball.idleSound,0.3f);
				}
				animator.Play("Idle", 0, 0f);
				if (limitDir)
					lookAroundDir = Quaternion.Euler(new Vector3(Random.Range(-5, 5), Random.Range(-10, 10), 0f));
				else
					lookAroundDir = Quaternion.Euler(new Vector3(Random.Range(-15, 15), Random.Range(-120, 120), 0f));
				nextLookTime = Time.time + Random.Range(4.5f, 10f);
			}
			else
			{
				eyeball.lookPivot.transform.rotation = Quaternion.Slerp(eyeball.lookPivot.transform.rotation, lookAroundDir, eyeball.turnSpeedIdle * Time.deltaTime);
			}
		}
		public override void OnLogic()
		{
			base.OnLogic();

			if (eyeball.awareOfPlayer) {
				//look at player
				Quaternion lookRot = Quaternion.LookRotation(eyeball.player.transform.position - eyeball.lookPivot.transform.position, eyeball.transform.up);
				eyeball.lookPivot.transform.rotation = Quaternion.Slerp(eyeball.lookPivot.transform.rotation, lookRot, eyeball.turnSpeedAgro * Time.deltaTime);
			}
			else
			{
				lookAround(false);
			}
		}
		public override void OnExitRequest()
		{
			eyeball.AudioSource.Stop();
			nextLookTime = Time.time; //so coming back to the state starts with a new time
			base.OnExitRequest();
		}
	}
}
