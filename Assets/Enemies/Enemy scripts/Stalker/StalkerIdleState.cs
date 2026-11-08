using UnityEngine;
using VirtualWorlds.FSM;

namespace VitrualWorlds.FSM
{

    public class StalkerIdleState : StalkerStateBase
    {
        public StalkerIdleState(bool needsExitTime, Stalker stalker, float exitTime) : base(needsExitTime, stalker, exitTime) { }

        public override void OnEnter()
        {
            base.OnEnter();
            // Makes it can effectively "reset" its knowledge of where things are. This one has exit time
            // To prevent players from just going in and out of the collider and breaking shit.
            stalker.stalkSensor.GetComponent<SphereCollider>().radius = 0.1f;
            agent.isStopped = true;
        }

        public override void OnLogic()
        {
            if (timer.Elapsed >= exitTime)
            {
                stalker.stalkSensor.GetComponent<SphereCollider>().radius = 50f;
            }
            base.OnLogic();
        }
    }
}
