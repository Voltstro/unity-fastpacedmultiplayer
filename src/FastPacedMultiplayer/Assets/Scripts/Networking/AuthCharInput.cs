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

		private float mouseSensitivity = 100f;

		private void Awake()
		{
			inputBuffer = new List<CharacterInput>();
			character = GetComponent<AuthoritativeCharacter>();
			predictor = GetComponent<AuthCharPredictor>();
		}

		private void Update()
		{
			Vector2 keyboardInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
			Vector2 mouseInput = new Vector2(Input.GetAxisRaw("Mouse X") * mouseSensitivity, Input.GetAxisRaw("Mouse Y") * mouseSensitivity);
			bool jump = Input.GetButton("Jump");

			//if (inputBuffer.Count == 0 && input == Vector2.zero && !jump && lastInputSent.Directions == Vector2.zero)
			//	return;

			CharacterInput charInput = new CharacterInput(keyboardInput, mouseInput, jump, currentInput++);
			predictor.AddInput(charInput);

			inputBuffer.Add(charInput);
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