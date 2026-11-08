using System.Collections;
using UnityEngine;
using VirtualWorlds.FSM;

namespace VirtualWorlds.FSM { 

public class StalkState : StalkerAttackBase
    {
        bool movingToLocation = false;
        bool movingToPlayer = false;
        bool frozen = false;
        bool isExiting = false;

        private Coroutine frozenRoutine;
        private Coroutine movingToLocationRoutine;
        private Coroutine movingToPlayerRoutine;


        private PlayerController target;
        public StalkState(bool needsExitTime, Stalker stalker, PlayerController target) : base(needsExitTime, stalker)
        {
            this.target = target;
        }

        public override void OnEnter()
        {
            movingToLocation = false;
            movingToPlayer = false;
            frozen = false;
            isExiting = false;
            base.OnEnter();
            if (BeingLookedAt())
            {
                frozen = true;
                stalker.StartCoroutine(FrozenSupervisor());
            }
            else
            {
               OnUnfreeze();
            }
            
        }


        public override void OnLogic()
        {
            base.OnLogic();
            if (frozen || movingToLocation || movingToPlayer || isExiting)
            {
                
            }
            else if (requestedExit && animator.GetCurrentAnimatorStateInfo(0).IsName("Small_walkcycle"))
            {
                fsm.StateCanExit();
            }
            else if (BeingLookedAt())
            {
                OnSpot();
            }
            else
            {
                OnUnfreeze();
            }
        }

        private bool BeingLookedAt()
        {
            // Okay, so put simply, we get the direction from the player to the stalker, then use
            // the direction they're currently facing, in addition to their FOV, to determine if they're
            // looking at the stalker's direction.
            Vector3 direction = (stalker.transform.position - target.transform.position).normalized;
            float dotProduct = Vector3.Dot(target.transform.forward, direction);
            if (dotProduct + 0.1 >= Mathf.Cos(90))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void OnUnfreeze()
        {
            movingToPlayer = true;
            agent.enabled = true;
            agent.isStopped = false;
            animator.SetTrigger("activation");
            animator.SetTrigger("small_walk");
            stalker.StartCoroutine(ApproachPlayerSupervisor());
        }

        private void OnSpot()
        {
            frozen = true;
            // You have to stop the agent BEFORE disabling it - 
            // Trying to do so after disabling will not work. Vice versa is true. 
            agent.isStopped = true;
            agent.enabled = false;
            // This is to stop physics from shoving the hat into the sky when he retreats into it.
            stalker.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
            stalker.GetComponent<CapsuleCollider>().height = 1;
            animator.SetTrigger("small_start_transition");
            animator.SetTrigger("small_to_idle");
            stalker.StartCoroutine(FrozenSupervisor());
        }

        IEnumerator FrozenSupervisor()
        {
            Debug.Log("Currently frozen.");
            yield return stalker.StartCoroutine(Waiting());
            stalker.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
            stalker.GetComponent<CapsuleCollider>().height = 5;
            frozen = false;
        }

        IEnumerator Waiting()
        {
            while (BeingLookedAt())
            {
                yield return new WaitForSeconds(3f);
            }
        }

        IEnumerator ApproachPlayerSupervisor()
        {
            Debug.Log("Attempting to reach player.");
            yield return stalker.StartCoroutine(AttemptToReachPlayer());
            movingToPlayer = false;
        }

        IEnumerator AttemptToReachPlayer()
        {
            agent.SetDestination(target.transform.position);
            // This little wait allows the agent time to update its destination.
            yield return new WaitForSeconds(0.1f);
            while (!BeingLookedAt() &&
                agent.remainingDistance > agent.stoppingDistance)
            {
                yield return agent.SetDestination(target.transform.position);
            }
            if (BeingLookedAt())
            {
                Debug.Log("I WAS LOOKED AT!!!");
            }
            else
            {
                Debug.Log("I exited because agent's remaining distance was: " + agent.remainingDistance);
            }
        }
    }   
}
