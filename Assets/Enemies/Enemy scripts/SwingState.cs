using System;
using UnityEngine;
using UnityHFSM;
using VirtualWorlds.FSM;

namespace VitrualWorlds.FSM
{
    public class SwingState : EnemyStateBase
    {
        public SwingState(bool needsExitTime, 
            Enemy enemy, 
            Action<State<EnemyState, StateEvent>> onEnter,
            float exitTime = 0.23f) : base(needsExitTime, enemy, exitTime, onEnter) { }

        public override void OnEnter()
        {
            // Can handle damage here with the knowledge in THIS video
            // https://www.youtube.com/watch?v=6yvUmSxlGQo
            //agent.isStopped = true;  <-- if we want our guy to stop moving during attack
            //agent.move() is a method you can use to move your guy without it continuing
            //to move at a given destination, rather just move straight forward so it doesn't perfectly
            //track the player.  
            base.OnEnter();
            animator.Play("Swing");
        }
    }
}
