using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using VirtualWorlds.FSM;

namespace VirtualWorlds.FSM { 

public class StalkState : StalkerAttackBase
    {
        private LayerMask hideableLayers;
        private bool previouslyHadSpot;
        bool moving = false;
        bool frozen = false;
        bool isExiting = false;
        private string lastState = "None";
        private Collider[] spots; // "More potential colliders gives more hiding options - but is less performant"
        float hideSensitivity;


        private PlayerController target;
        public StalkState(bool needsExitTime, Stalker stalker, PlayerController target, float hideSensitivity, int numOfSpots, LayerMask hideableLayers) : base(needsExitTime, stalker)
        {
            this.target = target;
            this.hideSensitivity = hideSensitivity;
            spots = new Collider[numOfSpots];
            this.hideableLayers = hideableLayers;
        }

        public override void OnEnter()
        {
            moving = false;
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
            // We skip if we're in a Coroutine. 
            if (frozen || moving || isExiting)
            {

            }
            // We say we can exit if we're in the small walkcycle, because otherwise it just don't work out.
            else if (requestedExit && animator.GetCurrentAnimatorStateInfo(0).IsName("Small_walkcycle"))
            {
                isExiting = true;
                fsm.StateCanExit();
            }
            // These ones are a binary - say we were to be looked at in such a manner that !beinglookedat proccs
            // twice in a row, or beinglookedat proccs twice in a row, despite leaving their respective coroutines the frame before.
            // Then we have animator issues. So, we know implicitly that the prior state is supposed to be whatever the opposite of what it was
            // last time. 
            else if (BeingLookedAt() || lastState == "approach")
            {
                OnSpot();
            }
            else if (!BeingLookedAt() || lastState == "frozen")
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
            lastState = "approach";
            moving = true;
            agent.enabled = true;
            agent.isStopped = false;
            animator.SetTrigger("activation");
            animator.SetTrigger("small_walk");
            FindDestination();
        }

        private void FindDestination()
        {
            for(int i = 0; i < spots.Length; ++i)
            {
                spots[i] = null;
            }

            int hits = Physics.OverlapSphereNonAlloc(stalker.transform.position, stalker.stalkSensor.GetComponent<SphereCollider>().radius, spots, hideableLayers);
            System.Array.Sort(spots, SpotsSort);
            for (int i = 0; i < spots.Length; ++i)
            {
                Debug.Log(spots[i]);
            }
            if (spots[0] == target.GetComponent<CharacterController>())
            {
                stalker.StartCoroutine(ApproachPlayerSupervisor());
            }
            else if (previouslyHadSpot && target.GetComponent<CharacterController>() == spots[1])
            {
                previouslyHadSpot = false;
                stalker.StartCoroutine (ApproachPlayerSupervisor());    
            }
            else
            {
                stalker.StartCoroutine(GoToHidingSpotSupervisor(hits));
            }
        }

        IEnumerator GoToHidingSpotSupervisor(int hits)
        {
            Debug.Log("Finding a hiding spot!");
            int i = 1;
            if (!previouslyHadSpot)
            {
                i = 0;
                previouslyHadSpot = true;
            }
            while (spots[i] != target.GetComponent<CharacterController>()
                && i < hits)
            {
                Debug.Log($"Currently looking for positions around {spots[i].name}");

                //This checks if there's a valid navmesh position near it.
                // That 10 there is the maxDistance - it's recommended you set it to double the height
                // of your agent.
                if (NavMesh.SamplePosition(spots[i].transform.position, out NavMeshHit hit, 10f, stalker.GetComponent<NavMeshAgent>().areaMask))
                {
                    // We do this so we can use our hits normal, because we wanna know if it's
                    // facing the player or facing away. We're doubling this as an error check
                    // because if it returns false we have problems.
                    if (!NavMesh.FindClosestEdge(hit.position, out hit, stalker.GetComponent<NavMeshAgent>().areaMask))
                    {
                        Debug.LogError($"Unable to find edge close to {hit.position}. Indicates invalid scene for the stalker, or an object it views as a hiding spot isn't fit to be one (it has no nearby navmesh)");
                    }
                    if (Vector3.Dot(hit.normal, (target.transform.position - hit.position).normalized) < hideSensitivity)
                    {
                        Debug.Log($"Going to position {hit.position}. Spot rating of {Vector3.Dot(hit.normal, (target.transform.position - hit.position).normalized)}");
                        yield return stalker.StartCoroutine(MoveToHidingSpot(hit));
                        break;
                    }
                    else
                    {
                        
                        if (NavMesh.SamplePosition(spots[i].transform.position - (target.transform.position - hit.position).normalized * 5, out NavMeshHit hit2, 10f, stalker.GetComponent<NavMeshAgent>().areaMask))
                        {
                            // This redo is pretty much just to say "Hey, did we grab a position that's FACING the player? Let's try again,
                            // maybe it won't be if we go in the opposite direction of the original hit. 
                            if (!NavMesh.FindClosestEdge(hit2.position, out hit2, stalker.GetComponent<NavMeshAgent>().areaMask))
                            {
                                Debug.LogError($"Unable to find edge close to {hit2.position}. Indicates invalid scene for the stalker, or an object it views as a hiding spot isn't fit to be one (it has no nearby navmesh)");
                            }
                            if (Vector3.Dot(hit2.normal, (target.transform.position - hit2.position).normalized) < hideSensitivity)
                            {
                                Debug.Log($"Going to position alternative position {hit2.position}. Spot raiting of {Vector3.Dot(hit.normal, (target.transform.position - hit.position).normalized)}");
                                yield return stalker.StartCoroutine(MoveToHidingSpot(hit2));
                                break;
                            }
                        }
                    }
                }
                else
                {
                    Debug.LogError($"Unable to find NavMesh near object {spots[i].name} at {spots[i].transform.position} Your scene setup may be invalid.");
                }
                ++i;
            }
            moving = false;
        }

        IEnumerator MoveToHidingSpot(NavMeshHit hit)
        {
            agent.SetDestination(hit.position);
            float originalSpeed = agent.speed;
            agent.speed = agent.speed * 3;
            int zero_velocity_occurences = 0;
            while (true)
            {
                if (agent.pathPending)
                {
                    yield return null;
                }
                if (agent.remainingDistance <= agent.stoppingDistance)
                {
                    if (agent.velocity.sqrMagnitude == 0f)
                    {
                        yield return new WaitForSeconds(0.3f);
                        zero_velocity_occurences++;
                    }
                    // 3. Ensure it has no active path or has completely stopped moving
                    if (!agent.hasPath || zero_velocity_occurences > 4)
                    {
                        agent.velocity = Vector3.zero;
                        break;
                    }
                }
                yield return null;
            }
            agent.speed = originalSpeed;
        }

        // Moves all the nulls and hiding spots that would get the stalker farther from the player
        // to the back, then sorts the spots by which is closest. 
        
        private int SpotsSort(Collider A, Collider B)
        {
            if (A == null && B != null)
            {
                return 1;
            }
            else if (A != null && B == null)
            {
                return -1;
            }
            else if (A == null && B == null)
            {
                return 0;
            }
            else
            {
                // If this dot product is negative, they're in opposite directions relative to the stalker
                // If the dot product is positive, they're both in the same direction.
                // We care cuz we want the stalker toward the player at all times. 
                Vector3 playerDirection = (target.transform.position - stalker.transform.position).normalized;
                Vector3 aDirection = (A.transform.position - stalker.transform.position).normalized;
                Vector3 bDirection = (B.transform.position - stalker.transform.position).normalized;
                float aDotProduct = Vector3.Dot(playerDirection, aDirection);
                float bDotProduct = Vector3.Dot(playerDirection, bDirection);


                if (aDotProduct <= 0 && bDotProduct > 0)
                {
                    return 1;
                }
                else if (aDotProduct > 0 && bDotProduct <= 0)
                {
                    return -1;
                }
                else if (aDotProduct <= 0 && bDotProduct <= 0)
                {
                    return 0;
                }
                else
                {
                    return Vector3.Distance(stalker.transform.position, A.transform.position).CompareTo(Vector3.Distance(stalker.transform.position, B.transform.position));
                }
            }

        }

        IEnumerator ApproachPlayerSupervisor()
        {
            Debug.Log("Attempting to reach player.");
            yield return stalker.StartCoroutine(AttemptToReachPlayer());
            moving = false;
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

        private void OnSpot()
        {
            frozen = true;
            // You have to stop the agent BEFORE disabling it - 
            // Trying to do so after disabling will not work. Vice versa is true. 
            agent.isStopped = true;
            // This is to stop physics from shoving the hat into the sky when he retreats into it.
            stalker.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
            stalker.GetComponent<CapsuleCollider>().height = 1;
            animator.SetTrigger("small_start_transition");
            animator.SetTrigger("small_to_idle");
            stalker.StartCoroutine(FrozenSupervisor());
        }

        IEnumerator FrozenSupervisor()
        {
            lastState = "frozen";
            Debug.Log("Currently frozen.");
            yield return stalker.StartCoroutine(Waiting());
            stalker.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
            stalker.GetComponent<CapsuleCollider>().height = 5;
            frozen = false;
        }

        IEnumerator Waiting()
        {
            yield return new WaitForSeconds(3f);
            while (BeingLookedAt())
            {
                yield return new WaitForSeconds(3f);
            }
        }
    }   
}
