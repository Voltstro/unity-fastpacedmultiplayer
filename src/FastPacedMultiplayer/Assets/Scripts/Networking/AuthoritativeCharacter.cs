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
		[SerializeField] private float jumpSpeed = 8.0f;
		[SerializeField] private float sideStrafeSpeed = 1.0f;

		[SerializeField] private float airAcceleration = 2.0f;
		[SerializeField] private float airDeacceleration = 2.0f;
		[SerializeField] private float airControl = 5.0f;

		[SerializeField] private float runAcceleration = 14.0f;
		[SerializeField] private float runDeacceleration = 10.0f;
		
		[SerializeField] private float sideStrafeAcceleration = 50.0f;

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

			//Calculate velocity
			//TODO: For some reason velocity is being added a fuck ton on the server
			if (characterController.isGrounded)
			{
				GroundMove(input, ref characterState.velocity);
			}
			else if (!characterController.isGrounded)
				AirMove(input, ref characterState.velocity);

			characterState.position += characterState.velocity * Time.deltaTime;

			return characterState;
		}

		private void GroundMove(CharacterInput input, ref Vector3 playerVelocity)
		{
			if (!input.Jump)
				ApplyFriction(1.0f, ref playerVelocity);
			else
				ApplyFriction(0, ref playerVelocity);

			Vector3 wishDirection = new Vector3(input.Directions.x, 0, input.Directions.y);
			wishDirection = transform.TransformDirection(wishDirection);
			wishDirection.Normalize();

			float wishSpeed = wishDirection.magnitude;
			wishSpeed *= moveSpeed;

			Accelerate(wishDirection, wishSpeed, runAcceleration, ref playerVelocity);

			//Reset the gravity velocity
			playerVelocity.y = -gravityAmount * Time.fixedDeltaTime;

			if (!input.Jump) return;

			playerVelocity.y = jumpSpeed;
			input.Jump = false;
		}

		private void AirMove(CharacterInput input, ref Vector3 playerVelocity)
		{
			Vector3 wishDirection = new Vector3(input.Directions.x, 0, input.Directions.y);
			wishDirection = transform.TransformDirection(wishDirection);

			float wishSpeed = wishDirection.magnitude;
			wishSpeed *= moveSpeed;

			wishDirection.Normalize();

			float acceleration = Vector3.Dot(playerVelocity, wishDirection) < 0 ? airDeacceleration : airAcceleration;

			//If the player is ONLY strafing left or right

			// ReSharper disable CompareOfFloatsByEqualityOperator
			if (input.Directions.x == 0 && input.Directions.y != 0)
			{
				if (wishSpeed > sideStrafeSpeed)
					wishSpeed = sideStrafeSpeed;
				acceleration = sideStrafeAcceleration;
			}
			// ReSharper restore CompareOfFloatsByEqualityOperator

			Accelerate(wishDirection, wishSpeed, acceleration, ref playerVelocity);
			if (airControl > 0)
				AirControl(wishDirection, wishSpeed, input, ref playerVelocity);
			
			//Apply gravity
			playerVelocity.y -= gravityAmount * Time.deltaTime;
		}

		private void AirControl(Vector3 wishDirection, float wishSpeed, CharacterInput input, ref Vector3 playerVelocity)
		{
			//Can't control movement if not moving forward or backward
			if (Mathf.Abs(input.Directions.y) < 0.001 || Mathf.Abs(wishSpeed) < 0.001)
				return;

			float zSpeed = playerVelocity.y;
			playerVelocity.y = 0;

			//Next two lines are equivalent to idTech's VectorNormalize()
			float speed = playerVelocity.magnitude;
			playerVelocity.Normalize();

			float dot = Vector3.Dot(playerVelocity, wishDirection);
			float k = 32;
			k *= airControl * dot * dot * Time.deltaTime;

			//Change direction while slowing down
			if (dot > 0)
			{
				playerVelocity.x = playerVelocity.x * speed + wishDirection.x * k;
				playerVelocity.y = playerVelocity.y * speed + wishDirection.y * k;
				playerVelocity.z = playerVelocity.z * speed + wishDirection.z * k;

				playerVelocity.Normalize();
			}

			playerVelocity.x *= speed;
			playerVelocity.y = zSpeed;
			playerVelocity.z *= speed;
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