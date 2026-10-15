using UnityEngine;
using UnityEngine.AI;
using UnityHFSM;
using VitrualWorlds.FSM;

namespace VirtualWorlds.FSM
{
    [RequireComponent(typeof(Animator), typeof(NavMeshAgent))]
    public class Enemy : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private PlayerController player;

        [Header("Sensors")]
        [SerializeField]
        private PlayerSensor chasePlayerSensor;
        [SerializeField]
        private PlayerSensor swingPlayerSensor;
        [SerializeField]
        private SpearSensor spearImpactSensor;

        [Header("Attack Config")]
        [SerializeField]
        [Range(0.2f, 5f)]
        private float AttackCooldown = 1;
        [SerializeField]
        private DamageConfigScriptableObject damageConfig;


        [Space]
        [Header("Debug Info")]
        [SerializeField]
        private bool isInSwingRange;
        [SerializeField]
        private bool isInChasingRange;
        [SerializeField]
        private float lastAttackTime;

        private StateMachine<EnemyState, StateEvent> enemyFSM;
        private Animator animator;
        private NavMeshAgent agent;


        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            animator = GetComponent<Animator>();
            enemyFSM = new StateMachine<EnemyState, StateEvent>();

            // This adds our states, the first one is the one it defaults to unless given
            // an explcit state to start on.
            enemyFSM.AddState(EnemyState.Idle, new IdleState(false, this));
            enemyFSM.AddState(EnemyState.Swing, new SwingState(true, this, OnAttack));
            enemyFSM.AddState(EnemyState.Chase , new ChaseState(true, this, player.transform));

            enemyFSM.SetStartState(EnemyState.Idle);

            // These are transitions, and the order they're added in represent their 
            // respective priorities. Added first = takes priority.

            // Trigger transitions occur when a state event is raised, and only then.
            // This is an event based approach! 
            enemyFSM.AddTriggerTransition(StateEvent.DetectPlayer, new Transition<EnemyState>(EnemyState.Idle, EnemyState.Chase));
            enemyFSM.AddTriggerTransition(StateEvent.LostPlayer, new Transition<EnemyState>(EnemyState.Chase, EnemyState.Idle));
            
            // These are called polling transitions, where they're checked every call to enemyFSM's onLogic. This one is a function that basically says:
            // We'll transition from idle to chase in the event that lambda expression evaluates to true.

            // For checking idle, I don't think we need this, but it's good for illistrative purposes.
            enemyFSM.AddTransition(new Transition<EnemyState>(EnemyState.Idle, EnemyState.Chase, (transition) => isInChasingRange
                && Vector3.Distance(player.transform.position, transform.position) > agent.stoppingDistance)
                );
            enemyFSM.AddTransition(new Transition<EnemyState>(EnemyState.Chase, EnemyState.Idle, (transition) => !isInChasingRange
                || Vector3.Distance(player.transform.position, transform.position) <= agent.stoppingDistance)
                );

            // The true at the end of each of these says it's permitted to just DO it no matter what.
            // The colon is a lot like how = works with  values in python, just lets you declare
            // specifically it. 
            enemyFSM.AddTransition(new Transition<EnemyState>(EnemyState.Chase, EnemyState.Swing, shouldSwing, forceInstantly: true));
            enemyFSM.AddTransition(new Transition<EnemyState>(EnemyState.Idle, EnemyState.Swing, shouldSwing, forceInstantly: true));

            enemyFSM.AddTransition(new Transition<EnemyState>(EnemyState.Swing, EnemyState.Chase, isNotWithinIdleRange));
            enemyFSM.AddTransition(new Transition<EnemyState>(EnemyState.Swing, EnemyState.Idle, isWithinIdleRange));

            enemyFSM.Init();
        }


        private void Start()
        {
            chasePlayerSensor.onPlayerEnter += chasePlayerSensor_OnPlayerEnter;
            chasePlayerSensor.onPlayerExit += chasePlayerSensor_OnPlayerExit;

            swingPlayerSensor.onPlayerEnter += swingPlayerSensor_OnPlayerEnter;
            swingPlayerSensor.onPlayerExit += swingPlayerSensor_OnPlayerExit;

            spearImpactSensor.onCollision += spearImpactSensor_OnCollision;
        }


        private void spearImpactSensor_OnCollision(Collision collision)
        {
            enemyFSM.Trigger(StateEvent.SlapImpact);
            lastAttackTime = Time.time;
            // We don't want the hitbox to just go off over and over.
            // so we tell it to go to sleep. Then, wake it up
            // next time we attack
            if (collision.collider.TryGetComponent(out Damageable damageable))
            {
                damageable.TakeDamage(damageConfig.GetDamage(5));
            }
            spearImpactSensor.gameObject.SetActive(false);
        }


        private void chasePlayerSensor_OnPlayerEnter(Transform player)
        {
            enemyFSM.Trigger(StateEvent.DetectPlayer);
            isInChasingRange = true; 
        }

        private void chasePlayerSensor_OnPlayerExit(Vector3 lastKnownPosition)
        {
            enemyFSM.Trigger(StateEvent.LostPlayer);
            isInChasingRange = false;
        }


        private void swingPlayerSensor_OnPlayerEnter(Transform player)
        {
            isInSwingRange = true;
        }

        private void swingPlayerSensor_OnPlayerExit(Vector3 lastKnownPosition)
        {
            isInSwingRange = false;
        }


        private void OnAttack(State<EnemyState, StateEvent> state)
        {
            spearImpactSensor.gameObject.SetActive(true);
            transform.LookAt(player.transform.position);
            lastAttackTime = Time.time;
        }


        private bool shouldSwing(Transition<EnemyState> transition) =>
            lastAttackTime + AttackCooldown <= Time.time && isInSwingRange;

        private bool isWithinIdleRange(Transition<EnemyState> transition) =>
            agent.remainingDistance <= agent.stoppingDistance;

        private bool isNotWithinIdleRange(Transition<EnemyState> transition) =>
            !isWithinIdleRange(transition);


        // Update is called once per frame
        void Update()
        {
            enemyFSM.OnLogic();
        }
    }
}
