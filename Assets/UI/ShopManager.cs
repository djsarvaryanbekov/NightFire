using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopManager : MonoBehaviour
{
	public static ShopManager Instance;

	[Header("Economy settings")]
	public static int TotalPoints = 0;

	[Header("UI Panels")]
	[Tooltip("The parent Panel UI object representing the Shop/Pause Menu.")]
	public GameObject ShopMenuPanel;
	[Tooltip("TextMeshPro Text element to display current points.")]
	public TextMeshProUGUI PointsText;

	[Header("Upgrade Settings - Speed")]
	public int SpeedBaseCost = 30;
	public int SpeedCostIncreasePerLevel = 20;
	public float SpeedIncreaseAmount = 1.5f;
	private int speedUpgradeLevel = 1;

	[Tooltip("Text element that displays only the cost of the speed upgrade.")]
	public TextMeshProUGUI SpeedCostText;
	[Tooltip("Text element that displays only the current speed level.")]
	public TextMeshProUGUI SpeedLevelText;

	[Header("Upgrade Settings - Dash")]
	public int DashCost = 100;
	[Tooltip("The Dash Button component. Will be made unpressable (non-interactable) once bought.")]
	public Button DashButton;

	private bool isShopOpen = false;

	private void Awake()
	{
		Instance = this;
	}

	private void Start()
	{
		PlayerControls.IsDashUnlocked = false;

		if (ShopMenuPanel != null)
		{
			ShopMenuPanel.SetActive(false);
		}
		UpdateUI();
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			if (isShopOpen)
			{
				CloseShop();
			}
			else
			{
				OpenShop();
			}
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

		isShopOpen = true;
		ShopMenuPanel.SetActive(true);

		Time.timeScale = 0f;

		Cursor.visible = true;
		Cursor.lockState = CursorLockMode.None;

		UpdateUI();
	}

	public void CloseShop()
	{
		isShopOpen = false;
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
			SpeedCostText.text = $"{nextCost} pts";
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