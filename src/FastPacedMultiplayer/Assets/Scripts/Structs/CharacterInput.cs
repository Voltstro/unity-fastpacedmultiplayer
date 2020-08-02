using UnityEngine;

namespace Structs
{
	public struct CharacterInput
	{
		public CharacterInput(Vector2 dirs, Vector2 mouseDirs, int inputNum)
		{
			Directions = dirs;
			MouseDirections = mouseDirs;
			InputNum = inputNum;
		}

		public Vector2 Directions;
		public Vector2 MouseDirections;

		public int InputNum;
	}
}