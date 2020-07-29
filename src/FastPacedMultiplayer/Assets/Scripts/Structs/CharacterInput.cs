using UnityEngine;

namespace Structs
{
	public struct CharacterInput
	{
		public CharacterInput(Vector2 dirs, int inputNum)
		{
			Directions = dirs;
			InputNum = inputNum;
		}

		public Vector2 Directions;
		public int InputNum;
	}
}