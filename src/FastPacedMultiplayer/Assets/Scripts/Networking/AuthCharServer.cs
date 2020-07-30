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

		private void Awake()
		{
			inputBuffer = new Queue<CharacterInput>();
			character = GetComponent<AuthoritativeCharacter>();
			character.state = CharacterState.Zero;
			characterController = GetComponent<CharacterController>();
		}

		private void Update()
		{
			if (movesMade > 0)
				movesMade--;
			if (movesMade != 0) return;

			CharacterState state = character.state;
			while (movesMade < character.InputBufferSize && inputBuffer.Count > 0)
			{
				state = character.Move(state, inputBuffer.Dequeue(), serverTick);
				characterController.Move(state.position - characterController.transform.position);
				movesMade++;
			}

			if (movesMade <= 0) return;

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