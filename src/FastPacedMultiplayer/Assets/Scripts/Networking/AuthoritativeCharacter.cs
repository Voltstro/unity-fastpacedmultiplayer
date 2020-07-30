using Interfaces;
using Mirror;
using Structs;
using UnityEngine;

namespace Networking
{
	public class AuthoritativeCharacter : NetworkBehaviour
	{
		public float Speed => speed;

		/// <summary>
		/// Controls how many inputs are needed before sending update command
		/// </summary>
		public int InputBufferSize { get; private set; }

		/// <summary>
		/// Controls how many input updates are sent per second
		/// </summary>
		[SerializeField, Range(10, 50), Tooltip("In steps per second")]
		private int inputUpdateRate = 10;

		[HideInInspector, SerializeField, Range(5f, 15f)]
		private float speed = 6.25f;

		[SerializeField, Range(1, 60), Tooltip("In steps per second")]  
		public int interpolationDelay = 12;

		[SyncVar(hook = nameof(OnServerStateChange))]
		public CharacterState state = CharacterState.Zero;
    
		private IAuthCharStateHandler stateHandler;
		private AuthCharServer server;

		private CharacterController characterController;

		private void Awake()
		{
			InputBufferSize = (int)(1 / Time.fixedDeltaTime) / inputUpdateRate;
		}

		public override void OnStartServer()
		{
			base.OnStartServer();
			server = gameObject.AddComponent<AuthCharServer>();
		}

		private void Start()
		{
			characterController = GetComponent<CharacterController>();
			if (!isLocalPlayer)
			{
				stateHandler = gameObject.AddComponent<AuthCharObserver>();
				return;
			}

			//Setup for local player
			GetComponentInChildren<Renderer>().material.color = Color.green;
			stateHandler = gameObject.AddComponent<AuthCharPredictor>();
			gameObject.AddComponent<AuthCharInput>();
		}

		public void SyncState(CharacterState overrideState)
		{
			characterController.Move(overrideState.position - transform.position);
		}

		public CharacterState Move(CharacterState previous, CharacterInput input, int timestamp)
		{
			CharacterState characterState = new CharacterState
			{
				moveNum = previous.moveNum + 1,
				timestamp = timestamp,
				position = speed * Time.fixedDeltaTime * new Vector3(input.Directions.x, 0, input.Directions.y) +
				           previous.position,
			};

			return characterState;
		}

		public void OnServerStateChange(CharacterState oldState, CharacterState newState)
		{
			state = newState;
			stateHandler?.OnStateChange(state);
		}

		[Command(channel = 0)]
		public void CmdMove(CharacterInput[] inputs)
		{
			server.Move(inputs);
		}
	}
}