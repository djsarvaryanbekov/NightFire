using UnityEngine;

public class Billboard : MonoBehaviour
{
	private Camera mainCamera;

	private void Start()
	{
		// Находим главную камеру на сцене
		mainCamera = Camera.main;
	}

	private void LateUpdate()
	{
		if (mainCamera != null)
		{
			// Выравниваем вращение объекта точно по вращению камеры.
			// Это самый стабильный способ для World Space UI, предотвращающий искажения.
			transform.rotation = mainCamera.transform.rotation;
		}
	}
}