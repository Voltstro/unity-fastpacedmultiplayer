using Mirror;
using UnityEngine;

public class NetManager : NetworkManager
{
	public GameObject mainCamera;

	public override void OnStartClient()
	{
		base.OnStartClient();

		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;

		mainCamera.SetActive(false);
	}

	public override void OnStopClient()
	{
		base.OnStopClient();

		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;

		mainCamera.SetActive(true);
	}
}