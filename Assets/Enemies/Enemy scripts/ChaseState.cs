using UnityEngine;
using VirtualWorlds.FSM;

namespace VitrualWorlds.FSM
{
    public class ChaseState : EnemyStateBase
    {
        private Transform target; 
        public ChaseState(bool needsExitTime, Enemy enemy, Transform target) : base(needsExitTime, enemy) 
        { 
            this.target = target;
        }

        public override void OnEnter()
        {
            base.OnEnter();
            // Let's the guy move.
            agent.enabled = true;
            // Also makes sure the guy isn't frozen
            agent.isStopped = false;
            animator.Play("Idle");
        }


        public override void OnLogic()
        {
            base.OnLogic();
            if (!requestedExit)
            {
                // A more complex, predictive movement algorithm can be added here instead,
                // see AI series 44 from the llama guy for that. For now we just
                // start to move toward the target's exact position.
                agent.SetDestination(target.position);
            }
            // If we request to exit, we continue to move toward the last known position
            // prior to transitioning out to idle.
            else if(agent.remainingDistance <= agent.stoppingDistance)
            {
                // StateCanExit just signals to the FSM, hey, we can exit this state safely
                // now. 
                fsm.StateCanExit();
            }
        }
    }
}