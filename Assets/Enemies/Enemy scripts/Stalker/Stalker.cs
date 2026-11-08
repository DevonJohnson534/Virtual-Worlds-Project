using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using UnityHFSM;
using VitrualWorlds.FSM;

namespace VirtualWorlds.FSM
{
    [RequireComponent(typeof(Animator), typeof(NavMeshAgent))]
    public class Stalker : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private PlayerController player;
        [SerializeField]
        private InputHandler playerInput;
        [SerializeField]
        private Camera playerCamera;

        [Header("Stalker Models")]
        [SerializeField] private Transform ArmBone;
        [SerializeField] private Renderer HeadTransparency;
        [SerializeField] private Renderer GlassesTransparency;
        [SerializeField] private Renderer GlassTransparency;


        [Header("Damage Config")]
        [SerializeField]
        private DamageConfigScriptableObject damageConfig;

        [Header("Sensors")]
        [SerializeField]
        public PlayerSensor stalkSensor;
        [SerializeField]
        private PlayerSensor chaseSensor;
        [SerializeField]
        public PlayerSensor attackSensor;
        [SerializeField]
        public GrabSensor grabSensor;

        [Header("Attack Config")]
        [SerializeField]
        [Range(0.2f, 5f)]
        private float AttackCooldown = 1;


        [Space]
        [Header("Debug Info")]
        [SerializeField]
        private bool isInAttackRange;
        [SerializeField]
        private bool isInChaseRange;
        [SerializeField]
        private bool isInStalkRange;
        [SerializeField]
        private float lastAttackTime;
        [SerializeField]
        private bool isTransitioning = false;



        private StateMachine<StalkerState, StalkerEvent> stalkerFSM;
        private Animator animator;
        private NavMeshAgent agent;
        private StalkerAttackState attackState;


        void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            animator = GetComponent<Animator>();
            stalkerFSM = new StateMachine<StalkerState, StalkerEvent>();
            attackState = new StalkerAttackState(true, this, playerInput, ArmBone: ArmBone, damageConfig: damageConfig, playerCamera: playerCamera, 
                HeadTransparency: HeadTransparency, GlassesTransparency: GlassesTransparency, GlassTransparency: GlassTransparency);

            // In the states, the first argument is whether or not it can be interuptted, it has exit time.
            // If it does, it cannot be interupttd, and needs to call StateCanExit before being permitted to exit.
            // if it doesn't (if it's false) it can be interrupted no problem.
            stalkerFSM.AddState(StalkerState.Idle, new StalkerIdleState(true, this, 2f));
            stalkerFSM.AddState(StalkerState.Stalk, new StalkState(true, this, player));
            stalkerFSM.AddState(StalkerState.Chase, new StalkerChaseState(true, this, player));
            stalkerFSM.AddState(StalkerState.Attack, attackState);

            // These are transitions, and the order they're added in represent their 
            // respective priorities. Added first = takes priority.

            // Trigger transitions occur when a state event is raised, and only then.
            // This is an event based approach! 
            //Procced by entering the stalk bubble
            stalkerFSM.AddTriggerTransition(StalkerEvent.StalkerDetectPlayer, new Transition<StalkerState>(StalkerState.Idle, StalkerState.Stalk, condition: canTransition => !isTransitioning));
            //Procced by the player leaving the stalk bubble
            stalkerFSM.AddTriggerTransition(StalkerEvent.LostPlayerWhileStalking, new Transition<StalkerState>(StalkerState.Stalk, StalkerState.Idle, onTransition: StalkToIdle, condition: canTransition => !isTransitioning));
            //Procced by the Player entering chases initial box
            stalkerFSM.AddTriggerTransition(StalkerEvent.ReachedPlayer, new Transition<StalkerState>(StalkerState.Stalk, StalkerState.Chase, onTransition: StalkToChase, condition: canTransition => !isTransitioning));
            //Procced by Successful attack
            stalkerFSM.AddTriggerTransition(StalkerEvent.AttackSuccess, new Transition<StalkerState>(StalkerState.Attack, StalkerState.Idle, onTransition: RevertToIdle, condition: canTransition => !isTransitioning));
            //Procced by entering Attack bubble
            stalkerFSM.AddTriggerTransition(StalkerEvent.InAttackRange, new Transition<StalkerState>(StalkerState.Chase, StalkerState.Attack));
            //Procced by chasing bubble being left
            stalkerFSM.AddTriggerTransition(StalkerEvent.LostPlayerWhileChasing, new Transition<StalkerState>(StalkerState.Chase, StalkerState.Idle, onTransition: RevertToIdle, condition: canTransition => !isTransitioning));
            //Procced by Leaving attack bubble.
            stalkerFSM.AddTriggerTransition(StalkerEvent.AttackRangeLeft, new Transition<StalkerState>(StalkerState.Attack, StalkerState.Chase, onTransition: AttackWhiff));

            stalkerFSM.Init();
        }

        private void StalkToIdle(Transition<StalkerState> transition)
        {
            StartCoroutine(TriggerAnimation("small_start_transition", "Small_transition_to_idle"));
            StartCoroutine(TriggerAnimation("small_to_idle", "Small_Idle"));
        }

        private void StalkToChase(Transition<StalkerState> transition)
        {
            SphereCollider chaseCollider = chaseSensor.GetComponent<SphereCollider>();
            // Make the sphere collider BIGGER
            chaseCollider.radius = 35f;
            SphereCollider stalkCollider = stalkSensor.GetComponent<SphereCollider>();
            // Disable the stalk box. 
            stalkCollider.enabled = false;
            StartCoroutine(StalkToChaseRoutine("small_to_big", "Small_To_Big"));
        }

        IEnumerator StalkToChaseRoutine(string animationTrigger, string animationName)
        {
            yield return StartCoroutine(TriggerAnimation("small_to_big", "Small_To_Big"));
            // This is here so as to ensure it can properly finish the animation before the attack box is active.
            SphereCollider attackCollider = attackSensor.GetComponent<SphereCollider>();
            attackCollider.enabled = true;

        }

        private void AttackWhiff(Transition<StalkerState> transition)
        {
            StartCoroutine(TriggerAnimation("whiff", "Big_attack_whiff"));
        }

        private void RevertToIdle(Transition<StalkerState> transition)
        {
            StartCoroutine(RevertToIdleRoutine());
        }

        IEnumerator RevertToIdleRoutine()
        {
            yield return StartCoroutine(TriggerAnimation("big_to_small", "Big_to_small"));
            yield return StartCoroutine(TriggerAnimation("small_start_transition", "Small_transition_to_idle"));
            yield return StartCoroutine(TriggerAnimation("small_to_idle", "Small_Idle"));
            SphereCollider chaseCollider = chaseSensor.GetComponent<SphereCollider>();
            chaseCollider.radius = 2f;
            SphereCollider attackCollider = attackSensor.GetComponent<SphereCollider>();
            attackCollider.enabled = false;
            SphereCollider stalkCollider = stalkSensor.GetComponent<SphereCollider>();
            stalkCollider.enabled = true;
            if (isInStalkRange)
            {
                // Checks if a player is in the bubble after the stalker has reverted to idle. For reaggroing. 
                stalkerFSM.Trigger(StalkerEvent.StalkerDetectPlayer);
            }
        }

        

        private void StalkSensor_OnPlayerEnter(Transform player)
        {
            stalkerFSM.Trigger(StalkerEvent.StalkerDetectPlayer);
            isInStalkRange = true;
        }

        private void StalkSensor_OnPlayerExit(Vector3 lastKnownPosition)
        {
            stalkerFSM.Trigger(StalkerEvent.LostPlayerWhileStalking);
            isInStalkRange = false;
        }

        private void ChaseSensor_OnPlayerEnter(Transform player)
        {
            stalkerFSM.Trigger(StalkerEvent.ReachedPlayer);
            isInChaseRange = true;
        }

        private void ChaseSensor_OnPlayerExit(Vector3 lastKnownPosition)
        {
            stalkerFSM.Trigger(StalkerEvent.LostPlayerWhileChasing);
            isInChaseRange = false;
        }

        private void AttackSensor_OnPlayerEnter(Transform player)
        {
            stalkerFSM.Trigger(StalkerEvent.InAttackRange);
            isInAttackRange = true;
        }
        private void AttackSensor_OnPlayerExit(Vector3 lastKnownPosition)
        {
            stalkerFSM.Trigger(StalkerEvent.AttackRangeLeft);
            isInAttackRange = false;
        }

        private void OnSuccessfulAttack()
        {
            stalkerFSM.Trigger(StalkerEvent.AttackSuccess);

        }
        //Special transtions for Idle->Stalk|, Stalk->Idle|, Stalk->Chase|, Chase->Attack, Chase->Idle|, Attack->Idle(Succeeded with attack)|, Attack->Chase(Missed and they left attack range)
                
        
        
        private void Start()
        {
            //Maybe 2 sensors for following the player - one for chasing, the other for stalking, one turns off, the other turns on.

            stalkSensor.onPlayerEnter += StalkSensor_OnPlayerEnter;
            stalkSensor.onPlayerExit += StalkSensor_OnPlayerExit;

            chaseSensor.onPlayerEnter += ChaseSensor_OnPlayerEnter;
            chaseSensor.onPlayerExit += ChaseSensor_OnPlayerExit;

            attackSensor.onPlayerEnter += AttackSensor_OnPlayerEnter;
            attackSensor.onPlayerExit += AttackSensor_OnPlayerExit;

            attackState.SuccessfulAttack += OnSuccessfulAttack; 
        }

        public IEnumerator TriggerAnimation(string animationTrigger, string animationName)
        {
            Debug.Log("Attempting to trigger" + animationTrigger);
            isTransitioning = true;
            yield return PlayAnimation(animationTrigger, animationName);
            isTransitioning = false;
        }

        IEnumerator PlayAnimation(string animationTrigger, string animationName)
        {
            Debug.Log("Triggering " + animationTrigger + " !");
            animator.SetTrigger(animationTrigger);
            Debug.Log("Waiting for current animation to finish...");
            while (!animator.GetCurrentAnimatorStateInfo(0).IsName(animationName))
            {
                yield return null;
            }
            Debug.Log("Waiting for ours to finish...");
            while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f && animator.GetCurrentAnimatorStateInfo(0).IsName(animationName))
            {
                yield return null;
            }
            Debug.Log("Done!");
        }


        // Update is called once per frame
        void Update()
        {
            if (isTransitioning)
            {

            }
            else
            {
                stalkerFSM.OnLogic();
            }
            
        }
    }
}