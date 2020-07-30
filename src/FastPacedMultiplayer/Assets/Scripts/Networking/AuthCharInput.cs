using System.Collections.Generic;
using Structs;
using UnityEngine;

namespace Networking
{
	public class AuthCharInput : MonoBehaviour
	{
		private List<CharacterInput> inputBuffer;
		private AuthoritativeCharacter character;
		private AuthCharPredictor predictor;
		private int currentInput;

		private CharacterInput lastInputSent;

		private void Awake()
		{
			inputBuffer = new List<CharacterInput>();
			character = GetComponent<AuthoritativeCharacter>();
			predictor = GetComponent<AuthCharPredictor>();
		}

		private void Update()
		{
			Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
			if (inputBuffer.Count == 0 && input == Vector2.zero && lastInputSent.Directions == Vector2.zero)
				return;

			CharacterInput charInput = new CharacterInput(input, currentInput++);
			predictor.AddInput(charInput);
			inputBuffer.Add(charInput);

			lastInputSent = charInput;
		}

		private void FixedUpdate()
		{
			if (inputBuffer.Count < character.InputBufferSize)
				return;

			character.CmdMove(inputBuffer.ToArray());
			inputBuffer.Clear();
		}
	}
}