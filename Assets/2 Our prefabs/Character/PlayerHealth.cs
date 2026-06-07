using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
	public static PlayerHealth Instance;

	[Header("Health Settings")]
	public float MaxHealth = 100f;
	public float CurrentHealth;

	[Header("Regen Near Campfire")]
	public float RegenAmountPerSecond = 8f;
	public float RegenDistance = 5f;

	[Header("UI")]
	public Image PlayerHpBar;

	private void Awake()
	{
		Instance = this;
	}

	private void Start()
	{
		CurrentHealth = MaxHealth;
		UpdateUI();
	}

	private void Update()
	{
		if (!MainMenu.IsGameStarted) return;

		if (Firepit.Instance != null && CurrentHealth < MaxHealth)
		{
			float distanceToFire = Vector3.Distance(transform.position, Firepit.Instance.transform.position);
			if (distanceToFire <= RegenDistance)
			{
				Heal(RegenAmountPerSecond * Time.deltaTime);
			}
		}
	}

	public void TakeDamage(float amount)
	{
		CurrentHealth -= amount;
		CurrentHealth = Mathf.Max(0f, CurrentHealth);
		UpdateUI();

		if (CurrentHealth <= 0f)
		{
			Die();
		}
	}

	public void Heal(float amount)
	{
		CurrentHealth += amount;
		CurrentHealth = Mathf.Min(CurrentHealth, MaxHealth);
		UpdateUI();
	}

	private void UpdateUI()
	{
		if (PlayerHpBar != null)
		{
			PlayerHpBar.fillAmount = CurrentHealth / MaxHealth;
		}
	}

	private void Die()
	{
		Debug.Log("Игрок погиб!");
		if (Firepit.Instance != null && Firepit.Instance.GameOverScreen != null)
		{
			Firepit.Instance.GameOverScreen.gameObject.SetActive(true);
		}
	}
}