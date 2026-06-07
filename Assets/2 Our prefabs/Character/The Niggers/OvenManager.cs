using UnityEngine;

public class OvenManager : MonoBehaviour
{
	public static OvenManager Instance;

	[Header("Oven UI Settings")]
	[Tooltip("Панель меню покупки угольков.")]
	public GameObject OvenUIPanel;

	[Header("Spawn Settings")]
	[Tooltip("Префаб нашего маленького уголька-помощника.")]
	public GameObject EmberHelperPrefab;
	[Tooltip("Точка около печи, где будет появляться купленный уголек.")]
	public Transform SpawnPoint;

	[Header("Economy Settings")]
	public int EmberCost = 150; // Стоимость одного уголька в очках

	private void Awake()
	{
		Instance = this;
	}

	private void Start()
	{
		if (OvenUIPanel != null)
		{
			OvenUIPanel.SetActive(false);
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		// Если игрок вошел в зону печи — открываем меню печи
		if (other.TryGetComponent<PlayerControls>(out _))
		{
			if (OvenUIPanel != null)
			{
				OvenUIPanel.SetActive(true);
				Cursor.visible = true;
				Cursor.lockState = CursorLockMode.None;

				// Чтобы игрок не мог бить/дашить, пока открыто меню печи:
				ShopManager.IsShopOpen = true;
			}
		}
	}

	private void OnTriggerExit(Collider other)
	{
		// При выходе игрока — закрываем меню печи
		if (other.TryGetComponent<PlayerControls>(out _))
		{
			CloseOvenUI();
		}
	}

	public void BuyEmberHelper()
	{
		// Проверяем, хватает ли очков в нашей общей экономике ShopManager
		if (ShopManager.TotalPoints >= EmberCost && EmberHelperPrefab != null && SpawnPoint != null)
		{
			// Списываем очки
			ShopManager.TotalPoints -= EmberCost;

			// Обновляем текст очков в магазине базы
			if (ShopManager.Instance != null)
			{
				ShopManager.Instance.UpdateUI();
			}

			// Спавним уголька у печи
			Instantiate(EmberHelperPrefab, SpawnPoint.position, Quaternion.identity);
		}
	}

	public void CloseOvenUI()
	{
		if (OvenUIPanel != null)
		{
			OvenUIPanel.SetActive(false);
			ShopManager.IsShopOpen = false; // Разблокируем управление игроку
		}
	}
}