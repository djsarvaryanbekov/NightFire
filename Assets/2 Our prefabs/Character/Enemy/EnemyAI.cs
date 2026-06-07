using UnityEngine;
using UnityEngine.AI;

public enum EnemyType { Biter, Jumper, Sentry }

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
	public EnemyType Type = EnemyType.Biter;

	[Header("Targeting Settings")]
	public float PlayerDetectionRange = 8f;
	public float PlayerLoseRange = 12f;

	[Header("Aggro Behavior Settings")]
	[Tooltip("If enabled, the enemy will only target the player after taking damage.")]
	public bool AggroOnlyOnDamage = false;
	private bool hasBeenDamaged = false;

	[Header("Attack Ranges")]
	public float FirepitAttackRange = 3.8f;
	public float PlayerAttackRange = 2.6f;

	[Header("Attack Settings")]
	public float FirepitDamage = 0.05f;
	public float PlayerDamage = 15f;
	public float AttackCooldown = 1.5f;
	public float SuicideJumpForce = 6f;

	[Header("Effects")]
	public GameObject DamageVisualEffect;
	public AudioSource AttackSound;

	private NavMeshAgent agent;
	private float lastAttackTime;
	private bool hasSuicided = false;
	private bool isTargetingPlayer = false;
	private Vector3 homePosition; // Saved spawn position for Sentry type

	private void Start()
	{
		agent = GetComponent<NavMeshAgent>();

		if (Type == EnemyType.Sentry)
		{
			// Save the initial position to return to later
			homePosition = transform.position;
		}
		else if (Firepit.Instance != null)
		{
			agent.SetDestination(Firepit.Instance.transform.position);
		}
	}

	// Triggered from EnemyHealth script when taking damage
	public void OnTakeDamage()
	{
		hasBeenDamaged = true;
	}

	private void Update()
	{
		if (!MainMenu.IsGameStarted || Firepit.Instance == null || ShopManager.IsShopOpen)
		{
			if (agent.enabled && !agent.isStopped) agent.isStopped = true;
			return;
		}

		Vector3 playerPos = PlayerControls.Instance != null ? PlayerControls.Instance.transform.position : transform.position;
		Vector3 enemyPos = transform.position;

		playerPos.y = 0;
		enemyPos.y = 0;

		float distanceToPlayer = PlayerControls.Instance != null ? Vector3.Distance(enemyPos, playerPos) : float.MaxValue;

		// Sentry behavior runs on its own logic branch
		if (Type == EnemyType.Sentry)
		{
			UpdateSentryBehavior(distanceToPlayer);
			return;
		}

		// Standard Biter and Jumper logic
		Vector3 firePos = Firepit.Instance.transform.position;
		firePos.y = 0;
		float distanceToFire = Vector3.Distance(enemyPos, firePos);

		HandleAggro(distanceToPlayer);

		if (isTargetingPlayer)
		{
			if (distanceToPlayer <= PlayerAttackRange)
			{
				if (agent.enabled && !agent.isStopped)
				{
					agent.isStopped = true;
				}
				AttackPlayer();
			}
			else
			{
				if (agent.enabled && agent.isStopped)
				{
					agent.isStopped = false;
				}
				agent.SetDestination(PlayerControls.Instance.transform.position);
			}
		}
		else
		{
			if (distanceToFire <= FirepitAttackRange)
			{
				if (agent.enabled && !agent.isStopped)
				{
					agent.isStopped = true;
				}

				if (Type == EnemyType.Biter)
				{
					AttackFirepit();
				}
				else if (Type == EnemyType.Jumper && !hasSuicided)
				{
					SuicideJump();
				}
			}
			else
			{
				if (agent.enabled && agent.isStopped)
				{
					agent.isStopped = false;
				}
				agent.SetDestination(Firepit.Instance.transform.position);
			}
		}
	}

	private void UpdateSentryBehavior(float distanceToPlayer)
	{
		HandleAggro(distanceToPlayer);

		if (isTargetingPlayer)
		{
			if (distanceToPlayer <= PlayerAttackRange)
			{
				if (agent.enabled && !agent.isStopped)
				{
					agent.isStopped = true;
				}
				AttackPlayer();
			}
			else
			{
				if (agent.enabled && agent.isStopped)
				{
					agent.isStopped = false;
				}
				agent.SetDestination(PlayerControls.Instance.transform.position);
			}
		}
		else
		{
			// If target is lost, return home
			float distanceToHome = Vector3.Distance(transform.position, homePosition);
			if (distanceToHome > 0.5f)
			{
				if (agent.enabled && agent.isStopped)
				{
					agent.isStopped = false;
				}
				agent.SetDestination(homePosition);
			}
			else
			{
				// Stop moving once reached home
				if (agent.enabled && !agent.isStopped)
				{
					agent.isStopped = true;
				}
			}
		}
	}

	private void HandleAggro(float distanceToPlayer)
	{
		if (PlayerControls.Instance == null) return;

		if (Type == EnemyType.Jumper)
		{
			isTargetingPlayer = false;
			return;
		}

		// Prevent tracking unless the enemy was hit
		if (AggroOnlyOnDamage && !hasBeenDamaged)
		{
			isTargetingPlayer = false;
			return;
		}

		if (!isTargetingPlayer && distanceToPlayer <= PlayerDetectionRange)
		{
			isTargetingPlayer = true;
		}
		else if (isTargetingPlayer && distanceToPlayer > PlayerLoseRange)
		{
			isTargetingPlayer = false;

			// Reset aggro trigger if the player escapes
			if (Type == EnemyType.Sentry)
			{
				hasBeenDamaged = false;
			}
		}
	}

	private void AttackFirepit()
	{
		if (Time.time >= lastAttackTime + AttackCooldown)
		{
			lastAttackTime = Time.time;
			Firepit.Instance.AddHealth(-FirepitDamage, 0);

			if (AttackSound != null) AttackSound.Play();
			if (DamageVisualEffect != null)
			{
				Instantiate(DamageVisualEffect, Firepit.Instance.transform.position + Vector3.up * 0.5f, Quaternion.identity);
			}
		}
	}

	private void AttackPlayer()
	{
		if (Time.time >= lastAttackTime + AttackCooldown)
		{
			lastAttackTime = Time.time;

			if (PlayerHealth.Instance != null)
			{
				PlayerHealth.Instance.TakeDamage(PlayerDamage);
			}

			if (AttackSound != null) AttackSound.Play();
			if (DamageVisualEffect != null)
			{
				Instantiate(DamageVisualEffect, PlayerControls.Instance.transform.position + Vector3.up * 0.5f, Quaternion.identity);
			}
		}
	}

	private void SuicideJump()
	{
		hasSuicided = true;
		agent.enabled = false;

		Rigidbody rb = GetComponent<Rigidbody>();
		if (rb == null)
		{
			rb = gameObject.AddComponent<Rigidbody>();
		}

		Vector3 jumpDir = (Firepit.Instance.transform.position - transform.position).normalized;
		jumpDir.y = 1.2f;

		rb.isKinematic = false;
		rb.AddForce(jumpDir * SuicideJumpForce, ForceMode.Impulse);

		Invoke(nameof(ExplodeInFire), 0.5f);
	}

	private void ExplodeInFire()
	{
		Firepit.Instance.AddHealth(-FirepitDamage * 2.5f, 0);

		if (AttackSound != null) AttackSound.Play();
		if (DamageVisualEffect != null)
		{
			Instantiate(DamageVisualEffect, transform.position, Quaternion.identity);
		}

		Destroy(gameObject);
	}
}