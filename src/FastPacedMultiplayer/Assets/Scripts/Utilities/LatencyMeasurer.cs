using Mirror;
using UnityEngine;
using UnityEngine.UI;

namespace Utilities
{
	public class LatencyMeasurer : MonoBehaviour
	{
		public Gradient valueGradient;

		[Range(200, 500)] public float badLatency = 300f;
    
		private Text textValue;

		private void Start()
		{
			textValue = GetComponent<Text>();
		}

		private void Update()
		{
			float ping = (float)NetworkTime.rtt;
			if (ping >= 0)
			{
				textValue.text = ping.ToString();
				textValue.color = valueGradient.Evaluate(ping / badLatency);
			}
			else
			{
				textValue.text = "Off";
				textValue.color = valueGradient.Evaluate(1);
			}
		}
	}
}