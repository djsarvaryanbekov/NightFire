using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopManager : MonoBehaviour
{
	public static ShopManager Instance;

	// Статическая переменная, которую проверяет PlayerControls
	public static bool IsShopOpen = false;

	[Header("Economy settings")]
	public static int TotalPoints = 0;

	[Header("UI Panels")]
	[Tooltip("Панель меню магазина.")]
	public GameObject ShopMenuPanel;
	public TextMeshProUGUI PointsText;

	[Header("Upgrade Settings - Speed")]
	public int SpeedBaseCost = 30;
	public int SpeedCostIncreasePerLevel = 20;
	public float SpeedIncreaseAmount = 1.5f;
	private int speedUpgradeLevel = 0;

	public TextMeshProUGUI SpeedCostText;
	public TextMeshProUGUI SpeedLevelText;

	[Header("Upgrade Settings - Dash")]
	public int DashCost = 100;
	public Button DashButton;

	private void Awake()
	{
		Instance = this;
	}

	private void Start()
	{
		PlayerControls.IsDashUnlocked = false;
		IsShopOpen = false;

		if (ShopMenuPanel != null)
		{
			ShopMenuPanel.SetActive(false);
		}
		UpdateUI();
	}

	// Срабатывает, когда игрок заходит в зону базы (триггер)
	private void OnTriggerEnter(Collider other)
	{
		// Проверяем, что вошел именно игрок (ищем скрипт управления)
		if (other.GetComponent<PlayerControls>() != null)
		{
			OpenShop();
		}
	}

	// Срабатывает, когда игрок выходит из зоны базы
	private void OnTriggerExit(Collider other)
	{
		if (other.GetComponent<PlayerControls>() != null)
		{
			CloseShop();
		}
	}

	public static void AddPoints(int amount)
	{
		TotalPoints += amount;
		if (Instance != null)
		{
			Instance.UpdateUI();
		}
	}

	public void OpenShop()
	{
		if (!MainMenu.IsGameStarted) return;

		IsShopOpen = true;
		ShopMenuPanel.SetActive(true);

		// Время НЕ останавливаем (Scale = 1), чтобы игрок мог ходить внутри домика
		Time.timeScale = 1f;

		// Показываем курсор для покупок
		Cursor.visible = true;
		Cursor.lockState = CursorLockMode.None;

		UpdateUI();
	}

	public void CloseShop()
	{
		IsShopOpen = false;
		ShopMenuPanel.SetActive(false);

		Time.timeScale = 1f;

		// Скрываем курсор обратно при выходе из магазина (если это нужно для вашей игры)
		// Cursor.visible = false;

		UpdateUI();
	}

	public void BuySpeedUpgrade()
	{
		int currentCost = SpeedBaseCost + (speedUpgradeLevel * SpeedCostIncreasePerLevel);

		if (TotalPoints >= currentCost && PlayerControls.Instance != null)
		{
			TotalPoints -= currentCost;
			speedUpgradeLevel++;

			PlayerControls.Instance.MaxMoveSpeed += SpeedIncreaseAmount;

			UpdateUI();
		}
	}

	public void BuyDashUnlock()
	{
		if (PlayerControls.IsDashUnlocked) return;

		if (TotalPoints >= DashCost)
		{
			TotalPoints -= DashCost;
			PlayerControls.IsDashUnlocked = true;

			UpdateUI();
		}
	}

	public void UpdateUI()
	{
		if (PointsText != null)
		{
			PointsText.text = $"Points: {TotalPoints}";
		}

		if (SpeedCostText != null)
		{
			int nextCost = SpeedBaseCost + (speedUpgradeLevel * SpeedCostIncreasePerLevel);
			SpeedCostText.text = $"Cost: {nextCost} pts";
		}

		if (SpeedLevelText != null)
		{
			SpeedLevelText.text = $"Lvl {speedUpgradeLevel}";
		}

		if (DashButton != null)
		{
			DashButton.interactable = !PlayerControls.IsDashUnlocked;
		}
	}
}