using UnityEngine;
using VirtualWorlds.FSM;

namespace VitrualWorlds.FSM
{
    public class IdleState : EnemyStateBase
    {
        public IdleState(bool needsExitTime, Enemy enemy) : base(needsExitTime, enemy) { }

        public override void OnEnter()
        {
            base.OnEnter();
            // Just stops the movement from navmesh. 
            agent.isStopped = true;
            animator.Play("Idle");
        }
    }
}
