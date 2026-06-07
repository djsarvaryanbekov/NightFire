using System.Collections.Generic;
using UnityEngine;

public class GameplayUIActivator : MonoBehaviour
{
	[Header("UI Settings")]
	[Tooltip("Список игровых объектов UI (HUD), которые должны быть СКРЫТЫ в главном меню и автоматически ВКЛЮЧЕНЫ при старте игры.")]
	public List<GameObject> GameplayUIElements = new List<GameObject>();

	private bool hasStarted = false;

	private void Start()
	{
		// В самом начале (в главном меню) принудительно скрываем все игровые элементы интерфейса
		foreach (GameObject uiElement in GameplayUIElements)
		{
			if (uiElement != null)
			{
				uiElement.SetActive(false);
			}
		}
	}

	private void Update()
	{
		// Ждем, пока игрок нажмет кнопку "Play" и MainMenu.IsGameStarted станет true
		if (!hasStarted && MainMenu.IsGameStarted)
		{
			hasStarted = true;
			ActivateGameplayUI();
		}
	}

	private void ActivateGameplayUI()
	{
		// Включаем все элементы игрового интерфейса (очки, мини-карту, ХП костра и т.д.)
		foreach (GameObject uiElement in GameplayUIElements)
		{
			if (uiElement != null)
			{
				uiElement.SetActive(true);
			}
		}

		// Отключаем этот скрипт, так как он выполнил свою задачу при старте
		enabled = false;
	}
}