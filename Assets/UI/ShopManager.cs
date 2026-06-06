using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopManager : MonoBehaviour
{
	public static ShopManager Instance;
	public static bool IsShopOpen = false;

	[Header("Economy settings")]
	public static int TotalPoints = 0;

	[Header("UI Panels")]
	public GameObject ShopMenuPanel;
	public TextMeshProUGUI PointsText;

	[Header("Upgrade Settings - Speed")]
	public int SpeedBaseCost = 30;
	public int SpeedCostIncreasePerLevel = 20;
	public float SpeedIncreaseAmount = 1.5f;
	private int speedUpgradeLevel = 0;

	public TextMeshProUGUI SpeedCostText;
	public TextMeshProUGUI SpeedLevelText;

	[Header("Upgrade Settings - Bite (Damage)")]
	public int BiteBaseCost = 40;
	public int BiteCostIncreasePerLevel = 25;
	public float BiteDamageIncreaseAmount = 15f;
	private int biteUpgradeLevel = 0;

	public TextMeshProUGUI BiteCostText;
	public TextMeshProUGUI BiteLevelText;

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

	private void OnTriggerEnter(Collider other)
	{
		if (other.GetComponent<PlayerControls>() != null)
		{
			OpenShop();
		}
	}

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
		Time.timeScale = 1f;

		Cursor.visible = true;
		Cursor.lockState = CursorLockMode.None;

		UpdateUI();
	}

	public void CloseShop()
	{
		IsShopOpen = false;
		ShopMenuPanel.SetActive(false);
		Time.timeScale = 1f;

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

	public void BuyBiteUpgrade()
	{
		int currentCost = BiteBaseCost + (biteUpgradeLevel * BiteCostIncreasePerLevel);

		if (TotalPoints >= currentCost && PlayerGrabTrigger.Instance != null)
		{
			TotalPoints -= currentCost;
			biteUpgradeLevel++;

			// ”величиваем урон игрока на заданную величину
			PlayerGrabTrigger.Instance.PlayerDamageAmount += BiteDamageIncreaseAmount;

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

		// Speed UI
		if (SpeedCostText != null)
		{
			int nextCost = SpeedBaseCost + (speedUpgradeLevel * SpeedCostIncreasePerLevel);
			SpeedCostText.text = $"Cost: {nextCost} pts";
		}

		if (SpeedLevelText != null)
		{
			SpeedLevelText.text = $"Lvl {speedUpgradeLevel}";
		}

		// Bite UI
		if (BiteCostText != null)
		{
			int nextBiteCost = BiteBaseCost + (biteUpgradeLevel * BiteCostIncreasePerLevel);
			BiteCostText.text = $"Cost: {nextBiteCost} pts";
		}

		if (BiteLevelText != null)
		{
			BiteLevelText.text = $"Lvl {biteUpgradeLevel}";
		}

		if (DashButton != null)
		{
			DashButton.interactable = !PlayerControls.IsDashUnlocked;
		}
	}
}