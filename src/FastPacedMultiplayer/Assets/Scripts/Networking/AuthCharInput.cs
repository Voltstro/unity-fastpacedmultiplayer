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

		private void FixedUpdate()
		{
			Vector2 keyboardInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
			Vector2 mouseInput = new Vector2(Input.GetAxisRaw("Mouse X") * mouseSensitivity, Input.GetAxisRaw("Mouse Y") * mouseSensitivity);

			//if (inputBuffer.Count == 0 && input == Vector2.zero && !jump && lastInputSent.Directions == Vector2.zero)
			//	return;

			CharacterInput charInput = new CharacterInput(keyboardInput, mouseInput, currentInput++);
			predictor.AddInput(charInput);

			character.CmdMove(charInput);
			inputBuffer.Add(charInput);
		}
	}
}