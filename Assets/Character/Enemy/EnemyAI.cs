using UnityEngine;
using UnityEngine.AI;

public enum EnemyType { Biter, Jumper }

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
	public EnemyType Type = EnemyType.Biter;

	[Header("Attack Settings")]
	[Tooltip("Сколько здоровья костра отнимает одна атака (от 0 до 1).")]
	public float DamageAmount = 0.05f;
	public float AttackCooldown = 1.5f;
	public float SuicideJumpForce = 6f; // Сила прыжка для камикадзе

	[Header("Effects")]
	public GameObject DamageVisualEffect; // Префаб эффекта удара/взрыва
	public AudioSource AttackSound;

	private NavMeshAgent agent;
	private float lastAttackTime;
	private bool hasSuicided = false;

	private void Start()
	{
		agent = GetComponent<NavMeshAgent>();

		// Направляем врага к костру
		if (Firepit.Instance != null)
		{
			agent.SetDestination(Firepit.Instance.transform.position);
		}
	}
	private void Update()
	{
		// ИЗМЕНЕНО: Если игра не началась, костер потух ИЛИ открыт магазин — останавливаем врагов на месте
		if (!MainMenu.IsGameStarted || Firepit.Instance == null || ShopManager.IsShopOpen)
		{
			if (agent.enabled) agent.isStopped = true;
			return;
		}


		if (agent.enabled) agent.isStopped = false;

		// Игнорируем разницу по высоте (ось Y) для расчета расстояния до костра
		Vector3 firePos = Firepit.Instance.transform.position;
		Vector3 enemyPos = transform.position;
		firePos.y = 0;
		enemyPos.y = 0;

		float distanceToFire = Vector3.Distance(enemyPos, firePos);

		// Увеличиваем порог атаки до 5f, чтобы точно компенсировать физический Sphere Collider костра
		if (distanceToFire <= 5f)
		{
			if (Type == EnemyType.Biter)
			{
				BiteAttack();
			}
			else if (Type == EnemyType.Jumper && !hasSuicided)
			{
				SuicideJump();
			}
		}
	}
	private void BiteAttack()
	{
		// Останавливаемся перед костром для атаки
		if (agent.enabled) agent.isStopped = true;

		if (Time.time >= lastAttackTime + AttackCooldown)
		{
			lastAttackTime = Time.time;

			// Наносим отрицательное здоровье костру
			Firepit.Instance.AddHealth(-DamageAmount, 0);

			if (AttackSound != null) AttackSound.Play();

			if (DamageVisualEffect != null)
			{
				Instantiate(DamageVisualEffect, Firepit.Instance.transform.position + Vector3.up * 0.5f, Quaternion.identity);
			}
		}
	}

	private void SuicideJump()
	{
		hasSuicided = true;
		agent.enabled = false; // Отключаем навигацию, чтобы применить физику прыжка

		Rigidbody rb = GetComponent<Rigidbody>();
		if (rb == null)
		{
			rb = gameObject.AddComponent<Rigidbody>();
		}

		// Вычисляем направление прыжка к центру костра
		Vector3 jumpDir = (Firepit.Instance.transform.position - transform.position).normalized;
		jumpDir.y = 1.2f; // Направляем вектор прыжка немного вверх для дуги

		rb.isKinematic = false;
		rb.AddForce(jumpDir * SuicideJumpForce, ForceMode.Impulse);

		// Взрываемся через полсекунды (когда подлетим к пламени)
		Invoke(nameof(ExplodeInFire), 0.5f);
	}

	private void ExplodeInFire()
	{
		// Наносим повышенный мгновенный урон костру
		Firepit.Instance.AddHealth(-DamageAmount * 2.5f, 0);

		if (AttackSound != null) AttackSound.Play();

		if (DamageVisualEffect != null)
		{
			Instantiate(DamageVisualEffect, transform.position, Quaternion.identity);
		}

		Destroy(gameObject); // Уничтожаем камикадзе
	}
}