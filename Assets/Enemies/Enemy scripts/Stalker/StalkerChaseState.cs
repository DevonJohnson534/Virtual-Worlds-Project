using UnityEngine;
using VirtualWorlds.FSM;

namespace VirtualWorlds.FSM
{

    public class StalkerChaseState : StalkerStateBase
    {
        private PlayerController target;
        public StalkerChaseState(bool needsExitTime, Stalker stalker, PlayerController target) : base(needsExitTime, stalker)
        {
            this.target = target;
        }

        public override void OnEnter()
        {
            base.OnEnter();
            agent.enabled = true;
            agent.isStopped = false;
            Debug.Log("Entered Chase! Playing animation...");
            animator.SetTrigger("big_walk");
        }


        public override void OnLogic()
        {
            base.OnLogic();
            if (!requestedExit)
            {
                if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Big_walk_cycle"))
                {
                    animator.SetTrigger("big_walk");
                }
                // A more complex, predictive movement algorithm can be added here instead,
                // see AI series 44 from the llama guy for that. For now we just
                // start to move toward the target's exact position.
                agent.SetDestination(target.transform.position);
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