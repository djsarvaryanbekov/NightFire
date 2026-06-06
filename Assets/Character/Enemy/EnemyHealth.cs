using UnityEngine;
using UnityEngine.UI;

public class EnemyHealth : MonoBehaviour
{
	[Header("Health Settings")]
	public float MaxHealth = 100f;
	public float CurrentHealth;

	[Header("UI (Optional)")]
	[Tooltip("UI картинка полоски здоровь€ над головой врага (World Space Image с типом Fill Amount).")]
	public Image HpBarFill;

	[Header("Death Effects")]
	public GameObject DeathEffectPrefab; // Ёффект взрыва/смерти
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
		if (DeathEffectPrefab != null)
		{
			Instantiate(DeathEffectPrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity);
		}

		if (DeathSound != null && DeathSound.clip != null)
		{
			// ѕроигрываем звук смерти в точке гибели, так как сам объект будет удален
			AudioSource.PlayClipAtPoint(DeathSound.clip, transform.position);
		}

		Destroy(gameObject);
	}
}