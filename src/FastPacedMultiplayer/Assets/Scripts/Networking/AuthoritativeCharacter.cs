using Interfaces;
using Mirror;
using Structs;
using UnityEngine;

namespace Networking
{
	public class AuthoritativeCharacter : NetworkBehaviour
	{
		#region Movement Controls

		[SerializeField] private float moveSpeed = 11.0f;

		[SerializeField] private float runAcceleration = 14.0f;
		[SerializeField] private float runDeacceleration = 10.0f;

		[SerializeField] private float frictionAmount = 6f;
		[SerializeField] private float gravityAmount = 20.0f;

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
		public void CmdMove(CharacterInput[] inputs)
		{
			server.Move(inputs);
		}

		#region Movement Methods

		public CharacterState Move(CharacterState previous, CharacterInput input, int timestamp)
		{
			CharacterState characterState = new CharacterState
			{
				moveNum = previous.moveNum + 1,
				timestamp = timestamp,

			};

			//Calculate velocity
			//if (characterController.isGrounded)
				GroundMove(input, ref characterState.velocity);

			characterState.position = previous.position + characterState.velocity * Time.deltaTime;

			return characterState;
		}

		private void GroundMove(CharacterInput input, ref Vector3 playerVelocity)
		{
			//TODO: Add jumping
			//if (!wishToJump)
				ApplyFriction(1.0f, ref playerVelocity);
			//else
			//	ApplyFriction(0);

			Vector3 wishDirection = new Vector3(input.Directions.x, 0, input.Directions.y);
			wishDirection = transform.TransformDirection(wishDirection);
			wishDirection.Normalize();

			float wishSpeed = wishDirection.magnitude;
			wishSpeed *= moveSpeed;

			Accelerate(wishDirection, wishSpeed, runAcceleration, ref playerVelocity);

			//Reset the gravity velocity
			playerVelocity.y = -gravityAmount * Time.deltaTime;
		}

		private void ApplyFriction(float t, ref Vector3 playerVelocity)
		{
			Vector3 vec = playerVelocity;
			vec.y = 0f;

			float speed = vec.magnitude;
			float drop = 0.0f;

			//Only if the player is on the ground then apply frictionAmount
			if (characterController.isGrounded)
			{
				float control = speed < runDeacceleration ? runDeacceleration : speed;
				drop = control * frictionAmount * Time.deltaTime * t;
			}

			float newSpeed = speed - drop;
			if (newSpeed < 0)
				newSpeed = 0;
			if (speed > 0)
				newSpeed /= speed;

			playerVelocity.x *= newSpeed;
			playerVelocity.z *= newSpeed;
		}

		private void Accelerate(Vector3 wishDirection, float wishSpeed, float acceleration, ref Vector3 playerVelocity)
		{
			float currentSpeed = Vector3.Dot(playerVelocity, wishDirection);
			float addSpeed = wishSpeed - currentSpeed;
			if (addSpeed <= 0)
				return;
			float accelerationSpeed = acceleration * Time.deltaTime * wishSpeed;
			if (accelerationSpeed > addSpeed)
				accelerationSpeed = addSpeed;

			playerVelocity.x += accelerationSpeed * wishDirection.x;
			playerVelocity.z += accelerationSpeed * wishDirection.z;
		}

		#endregion
	}
}