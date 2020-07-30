using UnityEngine;

namespace Structs
{
	public struct CharacterInput
	{
		public CharacterInput(Vector2 dirs, bool jump, int inputNum)
		{
			Directions = dirs;
			Jump = jump;
			InputNum = inputNum;
		}

		public Vector2 Directions;
		public bool Jump;

		public int InputNum;
	}
}