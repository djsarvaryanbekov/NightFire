using System.Collections.Generic;
using UnityEngine;

public class PlayerGrabTrigger : MonoBehaviour
{
	public static PlayerGrabTrigger Instance;

	public Rigidbody GrabbedObject;
	public GameObject LanternPickedPrefab;

	[Header("Player Attack Settings")]
	public float PlayerDamageAmount = 34f;
	public GameObject EnemyHitEffectPrefab;
	public AudioSource PlayerAttackSound;

	[Header("Super Attack Settings")]
	[Tooltip("Радиус кругового удара супер-атаки.")]
	public float SuperAttackRadius = 8.5f;
	[Tooltip("Урон, наносимый супер-атакой (по умолчанию 75).")]
	public float SuperAttackDamage = 75f;
	[Tooltip("Эффект круговой ударной волны/взрыва.")]
	public GameObject SuperAttackEffectPrefab;
	[Tooltip("Звук супер-атаки.")]
	public AudioSource SuperAttackSound;

	[Header("Original Audio")]
	public AudioSource ChopTreeSound;
	public AudioSource GrabSound;
	public AudioSource GrabShroomSound;
	public AudioSource LanternPickSound;

	private int grabLayerMask;
	private List<Collider> collidersInTrigger = new List<Collider>();

	private void Start()
	{
		Instance = this;
		grabLayerMask = LayerMask.GetMask("Firewood", "Tree", "Lantern", "Mushroom");

		Collider triggerCollider = GetComponent<Collider>();
		if (triggerCollider != null)
		{
			triggerCollider.isTrigger = true;
		}
		else
		{
			Debug.LogWarning("PlayerGrabTrigger: На объекте нет коллайдера триггера!", this);
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		// Оставляем триггер только для пассивного сбора дров/грибов
		if (((1 << other.gameObject.layer) & grabLayerMask) != 0)
		{
			if (!collidersInTrigger.Contains(other))
			{
				collidersInTrigger.Add(other);
			}
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (collidersInTrigger.Contains(other))
		{
			collidersInTrigger.Remove(other);
		}
	}

	public void Grab()
	{
		// 1. АТАКА ВРАГОВ (Теперь работает через точный OverlapBox в момент укуса!)
		DamageEnemiesInTrigger();

		// 2. СБОР ПРЕДМЕТОВ (Остался без изменений)
		collidersInTrigger.RemoveAll(c => c == null);

		if (collidersInTrigger.Count > 0)
		{
			Collider bestTarget = GetClosestCollider();
			if (bestTarget == null) return;

			Rigidbody target = bestTarget.attachedRigidbody;
			if (target == null) return;

			if (target.gameObject.layer == LayerMask.NameToLayer("Tree") && target.isKinematic)
			{
				target.isKinematic = false;
				target.transform.SetParent(null, true);
				target.AddForceAtPosition(
					(transform.position - target.position).normalized * 15,
					target.centerOfMass + Vector3.up * 3);

				var obstacle = target.GetComponent<UnityEngine.AI.NavMeshObstacle>();
				if (obstacle != null) obstacle.enabled = false;

				ChopTreeSound.Play();
				collidersInTrigger.Remove(bestTarget);
			}
			else if (target.gameObject.layer == LayerMask.NameToLayer("Lantern"))
			{
				Firepit.Instance.AddHealth(0.16f, 0);

				Destroy(target.gameObject);
				Instantiate(LanternPickedPrefab, target.position, Quaternion.identity);

				LanternPickSound.Play();
				collidersInTrigger.Remove(bestTarget);
			}
			else if (!target.isKinematic || target.gameObject.layer == LayerMask.NameToLayer("Mushroom"))
			{
				GrabbedObject = target;
				GrabbedObject.isKinematic = true;
				GrabbedObject.transform.SetParent(transform, true);
				GrabbedObject.transform.Translate(-transform.forward * .5f, Space.World);

				if (target.gameObject.layer == LayerMask.NameToLayer("Mushroom")) GrabShroomSound.Play();
				else GrabSound.Play();

				collidersInTrigger.Remove(bestTarget);
			}
		}
	}

	private void DamageEnemiesInTrigger()
	{
		BoxCollider box = GetComponent<BoxCollider>();
		if (box == null) return;

		// Вычисляем мировые координаты, вращение и масштаб вашего BoxCollider на сцене
		Vector3 center = transform.TransformPoint(box.center);
		Vector3 halfExtents = Vector3.Scale(box.size, transform.lossyScale) * 0.5f;
		Quaternion rotation = transform.rotation;

		// Мгновенно сканируем пространство внутри коробки в момент укуса
		Collider[] hitColliders = Physics.OverlapBox(center, halfExtents, rotation);
		bool hitAnyEnemy = false;

		foreach (Collider col in hitColliders)
		{
			// Если внутри коробки оказался враг со здоровьем — наносим урон
			if (col.TryGetComponent<EnemyHealth>(out var enemy))
			{
				enemy.TakeDamage(PlayerDamageAmount);
				hitAnyEnemy = true;

				if (EnemyHitEffectPrefab != null)
				{
					Instantiate(EnemyHitEffectPrefab, enemy.transform.position + Vector3.up * 0.5f, Quaternion.identity);
				}
			}
		}

		if (hitAnyEnemy && PlayerAttackSound != null)
		{
			PlayerAttackSound.Play();
		}
	}

	private Collider GetClosestCollider()
	{
		Collider closest = null;
		float minDistance = float.MaxValue;
		Vector3 currentPos = transform.position;

		for (int i = 0; i < collidersInTrigger.Count; i++)
		{
			Collider c = collidersInTrigger[i];
			if (c == null) continue;

			float dist = Vector3.SqrMagnitude(c.transform.position - currentPos);
			if (dist < minDistance)
			{
				minDistance = dist;
				closest = c;
			}
		}
		return closest;
	}

	public void Release()
	{
		if (GrabbedObject != null)
		{
			GrabbedObject.transform.SetParent(null, true);
			GrabbedObject.isKinematic = false;

			Collider col = GrabbedObject.GetComponent<Collider>();
			Collider myCollider = GetComponent<Collider>();
			if (col != null && myCollider != null)
			{
				if (myCollider.bounds.Intersects(col.bounds))
				{
					if (!collidersInTrigger.Contains(col))
					{
						collidersInTrigger.Add(col);
					}
				}
			}

			GrabbedObject = null;
		}
	}

	public void PerformSuperAttack()
	{
		if (SuperAttackSound != null) SuperAttackSound.Play();

		if (SuperAttackEffectPrefab != null)
		{
			Instantiate(SuperAttackEffectPrefab, transform.position, Quaternion.identity);
		}

		Collider[] hitColliders = Physics.OverlapSphere(transform.position, SuperAttackRadius);

		foreach (Collider col in hitColliders)
		{
			if (col.TryGetComponent<EnemyHealth>(out var enemy))
			{
				enemy.TakeDamage(SuperAttackDamage);
			}
		}
	}
}