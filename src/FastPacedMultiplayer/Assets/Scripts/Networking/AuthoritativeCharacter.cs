using System;
using Interfaces;
using Mirror;
using Structs;
using UnityEngine;

namespace Networking
{
	public class AuthoritativeCharacter : NetworkBehaviour
	{
		[SerializeField] private Transform groundCheck;
		[SerializeField] private float groundDistance = 0.7f;
		[SerializeField] private LayerMask groundMask;

		#region Movement Controls

		[Header("Movement Settings")]
		[SerializeField] private float moveSpeed = 11.0f;
		[SerializeField] private float jumpHeight = 3f;

		[SerializeField] private float gravityAmount = 9.81f;

		#endregion

		[Header("Network")]
		[SerializeField, Range(1, 60), Tooltip("In steps per second")]  
		public int interpolationDelay = 12;

		[SerializeField, Range(10, 50), Tooltip("In steps per second")]
		private int inputUpdateRate = 10;

		/// <summary>
		/// Controls how many inputs are needed before sending update command
		/// </summary>
		public int InputBufferSize { get; private set; }

		[SyncVar(hook = nameof(OnServerStateChange))]
		public CharacterState state = CharacterState.Zero;

		private CharacterState workingState;
    
		private IAuthCharStateHandler stateHandler;
		private AuthCharServer server;

		private CharacterController characterController;

		private void Awake()
		{
			InputBufferSize = (int)(1 / Time.fixedDeltaTime) / inputUpdateRate;
		}

		private void OnGUI()
		{
			GUI.Label(new Rect(10, 10, 1000, 20), state.velocity.ToString());
			GUI.Label(new Rect(10, 30, 1000, 20), transform.position.ToString());
			GUI.Label(new Rect(10, 50, 1000, 20), $"Is Ground: {Physics.CheckSphere(groundCheck.position, groundDistance, groundMask)}");
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

		public void OnServerStateChange(CharacterState oldState, CharacterState newState)
		{
			state = newState;
			stateHandler?.OnStateChange(state);
		}

		[Command(channel = 0)]
		public void CmdMove(CharacterInput input)
		{
			server.Move(input);
		}

		#region Movement Methods

		public CharacterState Move(CharacterState previous, CharacterInput input, int timestamp)
		{
			CharacterState characterState = new CharacterState
			{
				moveNum = previous.moveNum + 1,
				timestamp = timestamp,
				position = previous.position,
				velocity = previous.velocity
			};

			bool isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

			characterState.velocity.y = previous.velocity.y;

			//Calculate velocity
			//characterState.velocity = new Vector3(input.Directions.x, 0, input.Directions.y) * Time.deltaTime * moveSpeed;
			//characterState.velocity = new Vector3(transform.right.x * input.Directions.x, 0, transform.forward.z * input.Directions.y) * moveSpeed;
			characterState.velocity = (transform.right * input.Directions.x + transform.forward * input.Directions.y) * moveSpeed;

			//Gravity
			if(!isGrounded)
				characterState.velocity.y -= gravityAmount;
			else
				characterState.velocity.y = -2f;

			//Jumping
			//TODO: Add jumping

			//Apply velocity to position
			characterState.position += characterState.velocity * Time.deltaTime;

			return characterState;
		}

		#endregion
	}
}