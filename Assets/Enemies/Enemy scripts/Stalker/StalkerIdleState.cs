using UnityEngine;
using VirtualWorlds.FSM;

namespace VitrualWorlds.FSM
{

    public class StalkerIdleState : StalkerStateBase
    {
        public StalkerIdleState(bool needsExitTime, Stalker stalker) : base(needsExitTime, stalker) { }

        public override void OnEnter()
        {
            base.OnEnter();
            // Just stops the movement from navmesh. 
            agent.isStopped = true;
        }
    }
}
