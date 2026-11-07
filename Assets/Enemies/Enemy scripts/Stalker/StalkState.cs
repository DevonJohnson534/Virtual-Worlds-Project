using UnityEngine;
using VirtualWorlds.FSM;

namespace VirtualWorlds.FSM { 

public class StalkState : StalkerStateBase
{
    private PlayerController target;
    public StalkState(bool needsExitTime, Stalker stalker, PlayerController target) : base(needsExitTime, stalker)
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
        animator.SetTrigger("small_walk");
    }


    public override void OnLogic()
    {
        base.OnLogic();
        if (!requestedExit)
        {
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
