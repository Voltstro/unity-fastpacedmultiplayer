using UnityEngine;

namespace Structs
{
	[System.Serializable]
	public struct CharacterState
	{
		public Vector3 position;
		public Vector3 velocity;
		public Vector3 angularVelocity;
		public int moveNum;
		public int timestamp;
    
		public override string ToString()
		{
			return 
				$"CharacterState Pos:{position}|Vel:{velocity}|AngVel:{angularVelocity}|MoveNum:{moveNum}|Timestamp:{timestamp}";
		}

		public static CharacterState Zero =>
			new CharacterState {
				position = Vector3.zero,
				moveNum = 0,
				timestamp = 0
			};

		public static CharacterState Interpolate(CharacterState from, CharacterState to, int clientTick)
		{
			float t = ((float)(clientTick - from.timestamp)) / (to.timestamp - from.timestamp);
			return new CharacterState
			{
				position = Vector3.Lerp(from.position, to.position, t),
				moveNum = 0,
				timestamp = 0
			};
		}

		public static CharacterState Extrapolate(CharacterState from, int clientTick)
		{
			int t = clientTick - from.timestamp;
			return new CharacterState
			{
				position = from.position + from.velocity * t,
				moveNum = from.moveNum,
				timestamp = from.timestamp
			};
		}

		public static CharacterState Move(CharacterState previous, CharacterInput input, float speed, int timestamp)
		{
			var state =  new CharacterState
			{
				position = speed * Time.fixedDeltaTime * new Vector3(input.Directions.x, 0, input.Directions.y) + previous.position,
				moveNum = previous.moveNum + 1,
				timestamp = timestamp
			};

			int timestepInterval = timestamp - previous.timestamp + 1;
			state.velocity = (state.position - previous.position) / timestepInterval;

			return state;
		}
	}
}