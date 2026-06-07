using UnityEngine;
using UnityEngine.UI;

public class EnemyHealth : MonoBehaviour
{
	[Header("Health Settings")]
	public float MaxHealth = 100f;
	public float CurrentHealth;

	[Header("UI (Optional)")]
	public Image HpBarFill;

	[Header("Economy Settings")]
	public int PointsOnDeath = 25;

	[Header("Death Effects")]
	public GameObject DeathEffectPrefab;
	public AudioSource DeathSound;

	private void Start()
	{
		CurrentHealth = MaxHealth;
		UpdateUI();
	}

	public void TakeDamage(float amount)
	{
		CurrentHealth -= amount;
		CurrentHealth = Mathf.Max(0f, CurrentHealth);
		UpdateUI();

		// Оптимизация Unity 6: используем TryGetComponent
		if (TryGetComponent<EnemyAI>(out var ai))
		{
			ai.OnTakeDamage();
		}

		if (CurrentHealth <= 0f)
		{
			Die();
		}
	}

	private void UpdateUI()
	{
		if (HpBarFill != null)
		{
			HpBarFill.fillAmount = CurrentHealth / MaxHealth;
		}
	}

	private void Die()
	{
		ShopManager.AddPoints(PointsOnDeath);

		if (DeathEffectPrefab != null)
		{
			Instantiate(DeathEffectPrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity);
		}

		if (DeathSound != null && DeathSound.clip != null)
		{
			AudioSource.PlayClipAtPoint(DeathSound.clip, transform.position);
		}

		Destroy(gameObject);
	}
}