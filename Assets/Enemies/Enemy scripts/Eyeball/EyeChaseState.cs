using Unity.Mathematics;
using UnityEngine;
using VirtualWorlds.FSM;

namespace VitrualWorlds.FSM
{
	public class EyeChaseState : EyeStateBase
	{
		private int tooCloseRadius = 3; //if the player is in this radius, try to move up to get away (or be an elevator)
		private Transform target;
		private Eyeball eyeball;

		public EyeChaseState(bool needsExitTime, Eyeball enemy, Transform target) : base(needsExitTime, enemy)
		{
			this.target = target;
			eyeball = enemy;
		}

		public override void OnEnter()
		{
			base.OnEnter();
			// Let's the guy move.
			agent.enabled = true;
			// Also makes sure the guy isn't frozen
			agent.isStopped = false;
			animator.Play("Chase");
		}

		private void moveVertical()
		{
			Vector3 distToPlayer = (eyeball.player.transform.position - eyeball.lookPivot.transform.position);
			//Debug.Log(distToPlayer);

			if (distToPlayer.magnitude <= tooCloseRadius)
			{
				//too close, move up
				eyeball.lookPivot.transform.position += new Vector3(0, eyeball.verticalMoveSpeed * Time.deltaTime, 0);
			}
			else if (math.abs(distToPlayer.y) > eyeball.verticalChaseDist)
			{
				//too far, move closer
				Vector3 currentPos = eyeball.lookPivot.transform.position;
				eyeball.lookPivot.transform.position = Vector3.Slerp(eyeball.lookPivot.transform.position,
					new Vector3(currentPos.x, eyeball.player.transform.position.y, currentPos.z), eyeball.verticalMoveSpeed * Time.deltaTime);
			}
		}

		public override void OnLogic()
		{
			base.OnLogic();
			moveVertical();

			//look at player
			Quaternion lookRot = Quaternion.LookRotation(eyeball.player.transform.position - eyeball.lookPivot.transform.position, eyeball.transform.up);
			eyeball.lookPivot.transform.rotation = Quaternion.Slerp(eyeball.lookPivot.transform.rotation, lookRot, eyeball.turnSpeedAgro * Time.deltaTime);

			if (!requestedExit)
			{
				// A more complex, predictive movement algorithm can be added here instead,
				// see AI series 44 from the llama guy for that. For now we just
				// start to move toward the target's exact position.
				agent.SetDestination(target.position);
			}
			// If we request to exit, we continue to move toward the last known position
			// prior to transitioning out to idle.
			else if (agent.remainingDistance <= agent.stoppingDistance)
			{
				// StateCanExit just signals to the FSM, hey, we can exit this state safely
				// now. 
				fsm.StateCanExit();
			}
		}
	}
}