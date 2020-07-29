using System;
using System.Linq;
using Mirror;
using UnityEngine;

namespace Utilities
{
	public class ServerStarter : MonoBehaviour
	{
		private void Start()
		{
			string[] args = Environment.GetCommandLineArgs();
			if (args.Any((arg) => arg == "servermode"))
			{
				Application.targetFrameRate = 128;
				NetworkManager.singleton.StartServer();
			}
			else if (args.Any((arg) => arg == "gamemode"))
			{
				NetworkManager.singleton.StartClient();
			}

			Time.fixedDeltaTime = 1 / 60f;
			Destroy(gameObject);
		}
	}
}