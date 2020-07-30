using System.Collections.Generic;
using Structs;
using UnityEngine;

namespace Networking
{
	public class AuthCharServer : MonoBehaviour
	{
		private Queue<CharacterInput> inputBuffer;
		private AuthoritativeCharacter character;
		private int movesMade;
		private int serverTick;

		private CharacterController characterController;

		private CharacterInput lastInput;

		private void Awake()
		{
			inputBuffer = new Queue<CharacterInput>();
			character = GetComponent<AuthoritativeCharacter>();
			character.state = CharacterState.Zero;
			characterController = GetComponent<CharacterController>();
		}

		private void Update()
		{
			CharacterState state = character.state;

			if (inputBuffer.Count != 0)
				lastInput = inputBuffer.Dequeue();

			state = character.Move(state, lastInput, serverTick);
			character.SyncState(state);

			state.position = transform.position;
			character.state = state;
			character.OnServerStateChange(state, state);
		}

		private void FixedUpdate()
		{
			serverTick++;    
		}

		public void Move(CharacterInput[] inputs)
		{
			foreach (var input in inputs)
				inputBuffer.Enqueue(input);
		}
	}
}