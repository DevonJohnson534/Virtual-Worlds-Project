using UnityHFSM;
using System;
using UnityEngine;
using UnityEngine.AI;

namespace VirtualWorlds.FSM
{

    public abstract class StalkerAttackBase : State<StalkerState, StalkerEvent>
    {
        protected readonly Stalker stalker;
        protected readonly NavMeshAgent agent;
        protected readonly Animator animator;
        // requestedExit is called whenever the FSM is trying to exit a given
        // state
        protected bool requestedExit;
        protected float exitTime;
        protected readonly Action<State<StalkerState, StalkerEvent>> onEnter;
        protected readonly Action<State<StalkerState, StalkerEvent>> onLogic;
        protected readonly Action<State<StalkerState, StalkerEvent>> onExit;
        protected readonly Func<State<StalkerState, StalkerEvent>, bool> canExit;

        public StalkerAttackBase(bool needsExitTime,
            Stalker enemy,
            float exitTime = 0.1f,
            Action<State<StalkerState, StalkerEvent>> onEnter = null,
            Action<State<StalkerState, StalkerEvent>> onLogic = null,
            Action<State<StalkerState, StalkerEvent>> onExit = null,
            Func<State<StalkerState, StalkerEvent>, bool> canExit = null)
        {
            this.stalker = enemy;
            this.onEnter = onEnter;
            this.onLogic = onLogic;
            this.onExit = onExit;
            this.canExit = canExit;
            this.exitTime = exitTime;
            this.needsExitTime = needsExitTime;
            agent = enemy.GetComponent<NavMeshAgent>();
            animator = enemy.GetComponent<Animator>();
        }


        //This is called when the state is entered/transitioned to. 
        public override void OnEnter()
        {
            // Calls the original OnEnter function of the parent class.
            base.OnEnter();
            requestedExit = false;
            onEnter?.Invoke(this);
        }

        // Called every tick/update time window.
        public override void OnLogic()
        {
            base.OnLogic();
        }

        // This is called if the state machine wants to exit, and allows it to do so
        // Once the conditions are met, or, if the conditions aren't met, it tells 
        // on Logic that there's a requested exit which onLogic then takes care of
        // in teh event it needs to and exit conditions are fit. 
        public override void OnExitRequest()
        {
            if ((!needsExitTime) || (canExit != null && canExit(this)))
            {
                fsm.StateCanExit();
            }
            else
            {
                requestedExit = true;
            }
        }
    }

}

