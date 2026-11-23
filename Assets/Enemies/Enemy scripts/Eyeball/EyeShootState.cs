using System;
using UnityEngine;
using UnityHFSM;
using VirtualWorlds.FSM;

namespace VitrualWorlds.FSM
{
	public class EyeShootState : EyeStateBase
	{
		private Eyeball eyeball;
		public EyeShootState(bool needsExitTime,
			 Eyeball enemy,
			 Action<State<EyeState, EyeStateEvent>> onEnter,
			 float exitTime = 0.09f) : base(needsExitTime, enemy, exitTime, onEnter)
		{
			eyeball = enemy;
		}

		public override void OnEnter()
		{
			// Can handle damage here with the knowledge in THIS video
			// https://www.youtube.com/watch?v=6yvUmSxlGQo
			//agent.isStopped = true;  <-- if we want our guy to stop moving during attack
			//agent.move() is a method you can use to move your guy without it continuing
			//to move at a given destination, rather just move straight forward so it doesn't perfectly
			//track the player.  
			base.OnEnter();
			animator.Play("Shoot");
		}

		public override void OnLogic()
		{
			base.OnLogic();
			//look at player
			Quaternion lookRot = Quaternion.LookRotation(eyeball.player.transform.position - eyeball.lookPivot.transform.position, eyeball.transform.up);
			eyeball.lookPivot.transform.rotation = Quaternion.Slerp(eyeball.lookPivot.transform.rotation, lookRot, eyeball.turnSpeedAgro * Time.deltaTime);
		}
	}
}