using UnityEngine;
using UnityEngine.AI;
using UnityHFSM;
using VitrualWorlds.FSM;

namespace VirtualWorlds.FSM
{
	[RequireComponent(typeof(Animator), typeof(NavMeshAgent),typeof(AudioSource))]
	public class Eyeball : MonoBehaviour
	{
		[SerializeField]
		private Bullet laser;
		public Transform launchSpot; //to change where the lasers come from
		public LayerMask losMask;
		[SerializeField]
		private Vector3 aimOffset; //to change where the lasers aim at (also impacts where they are launched from)
		[SerializeField]
		private ParticleSystem shootParticleSystem;

		public float turnSpeedAgro = 5;
		public float turnSpeedIdle = 5;

		public float verticalMoveSpeed = 5;
		public float verticalChaseDist = 5;

		public AudioSource AudioSource { get; private set; }
		public AudioClip[] shootSounds;
		public AudioClip idleSound;
		public AudioClip[] hurtSounds; //TODO play
		public AudioClip deathSound; //TODO play

		[Header("References")]
		[SerializeField]
		public GameObject lookPivot; //this is to override the navmesh not wanting to look down
		[SerializeField]
		public PlayerController player;

		[Header("Sensors")]
		[SerializeField]
		private PlayerSensor chasePlayerSensor;
		[SerializeField]
		private PlayerSensor swingPlayerSensor;

		[Header("Attack Config")]
		[SerializeField]
		[Range(0.1f, 5f)]
		private float AttackCooldown = 1;


		[Space]
		[Header("Debug Info")]
		[SerializeField]
		private bool isInSwingRange;
		public bool awareOfPlayer;
		[SerializeField]
		private float lastAttackTime;

		private StateMachine<EyeState, EyeStateEvent> enemyFSM;
		private Animator animator;
		private NavMeshAgent agent;


		// Start is called once before the first execution of Update after the MonoBehaviour is created
		void Awake()
		{
			agent = GetComponent<NavMeshAgent>();
			AudioSource = GetComponent<AudioSource>();
			animator = GetComponent<Animator>();
			enemyFSM = new StateMachine<EyeState, EyeStateEvent>();

			// This adds our states, the first one is the one it defaults to unless given
			// an explcit state to start on.
			enemyFSM.AddState(EyeState.Idle, new EyeIdleState(false, this));
			enemyFSM.AddState(EyeState.Shoot, new EyeShootState(true, this, OnAttack));
			enemyFSM.AddState(EyeState.Chase, new EyeChaseState(true, this, player.transform));

			enemyFSM.SetStartState(EyeState.Idle);

			// These are transitions, and the order they're added in represent their 
			// respective priorities. Added first = takes priority.

			// Trigger transitions occur when a state event is raised, and only then.
			// This is an event based approach! 
			enemyFSM.AddTriggerTransition(EyeStateEvent.DetectPlayer, new Transition<EyeState>(EyeState.Idle, EyeState.Chase));
			enemyFSM.AddTriggerTransition(EyeStateEvent.LostPlayer, new Transition<EyeState>(EyeState.Chase, EyeState.Idle));

			// The true at the end of each of these says it's permitted to just DO it no matter what.
			// The colon is a lot like how = works with  values in python, just lets you declare
			// specifically it. 
			enemyFSM.AddTransition(new Transition<EyeState>(EyeState.Chase, EyeState.Shoot, shouldSwing, forceInstantly: true));
			enemyFSM.AddTransition(new Transition<EyeState>(EyeState.Idle, EyeState.Shoot, shouldSwing, forceInstantly: true));

			enemyFSM.AddTransition(new Transition<EyeState>(EyeState.Shoot, EyeState.Chase, isNotWithinIdleRange));
			enemyFSM.AddTransition(new Transition<EyeState>(EyeState.Shoot, EyeState.Idle, isWithinIdleRange));

			enemyFSM.Init();
		}


		private void Start()
		{
			//The plus equals sign pretty much indicatest that the sensor is listening for the event triggers onPlayerEnter and onPlayerExit.
			chasePlayerSensor.onPlayerEnter += chasePlayerSensor_OnPlayerEnter;
			chasePlayerSensor.onPlayerExit += chasePlayerSensor_OnPlayerExit;

			swingPlayerSensor.onPlayerEnter += swingPlayerSensor_OnPlayerEnter;
			swingPlayerSensor.onPlayerExit += swingPlayerSensor_OnPlayerExit;

		}

		private void chasePlayerSensor_OnPlayerEnter(Transform player)
		{
			enemyFSM.Trigger(EyeStateEvent.DetectPlayer);
			awareOfPlayer = true;
		}

		private void chasePlayerSensor_OnPlayerExit(Vector3 lastKnownPosition)
		{
			enemyFSM.Trigger(EyeStateEvent.LostPlayer);
			awareOfPlayer = false;
		}


		private void swingPlayerSensor_OnPlayerEnter(Transform player)
		{
			isInSwingRange = true;
		}

		private void swingPlayerSensor_OnPlayerExit(Vector3 lastKnownPosition)
		{
			isInSwingRange = false;
		}


		private void OnAttack(State<EyeState, EyeStateEvent> state)
		{
			lookPivot.transform.LookAt(player.transform.position, transform.up); //look at player instantly, regardless of turnSpeed

			Bullet b = Instantiate(laser);
			b.Launch(launchSpot.position + aimOffset, launchSpot.forward);

			shootParticleSystem.Play();
			//if (!AudioSource.isPlaying)
			//{
			AudioSource.PlayOneShot(shootSounds[Random.Range(0, shootSounds.Length)]);
			//}

			lastAttackTime = Time.time;
		}

		private bool shouldSwing(Transition<EyeState> transition) =>
			lastAttackTime + AttackCooldown <= Time.time && isInSwingRange;
		

		private bool isWithinIdleRange(Transition<EyeState> transition) =>
			 agent.remainingDistance <= agent.stoppingDistance;

		private bool isNotWithinIdleRange(Transition<EyeState> transition) =>
			 !isWithinIdleRange(transition);


		// Update is called once per frame
		void Update()
		{
			enemyFSM.OnLogic();
		}
	}
}
