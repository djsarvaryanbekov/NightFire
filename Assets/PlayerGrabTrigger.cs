using System.Collections.Generic;
using UnityEngine;

public class PlayerGrabTrigger : MonoBehaviour
{
	public static PlayerGrabTrigger Instance; // Переменная объявлена на уровне класса

	public Rigidbody GrabbedObject;
	public GameObject LanternPickedPrefab;

	[Header("Player Attack Settings")]
	public float PlayerDamageAmount = 34f;
	public GameObject EnemyHitEffectPrefab;
	public AudioSource PlayerAttackSound;

	[Header("Original Audio")]
	public AudioSource ChopTreeSound;
	public AudioSource GrabSound;
	public AudioSource GrabShroomSound;
	public AudioSource LanternPickSound;

	private int grabLayerMask;
	private List<Collider> collidersInTrigger = new List<Collider>();
	private List<EnemyHealth> enemiesInTrigger = new List<EnemyHealth>();

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
		if (((1 << other.gameObject.layer) & grabLayerMask) != 0)
		{
			if (!collidersInTrigger.Contains(other))
			{
				collidersInTrigger.Add(other);
			}
		}

		// Оптимизация Unity 6: используем TryGetComponent вместо GetComponent
		if (other.TryGetComponent<EnemyHealth>(out var enemy))
		{
			if (!enemiesInTrigger.Contains(enemy))
			{
				enemiesInTrigger.Add(enemy);
			}
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (collidersInTrigger.Contains(other))
		{
			collidersInTrigger.Remove(other);
		}

		if (other.TryGetComponent<EnemyHealth>(out var enemy))
		{
			if (enemiesInTrigger.Contains(enemy))
			{
				enemiesInTrigger.Remove(enemy);
			}
		}
	}

	public void Grab()
	{
		DamageEnemiesInTrigger();

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
		enemiesInTrigger.RemoveAll(e => e == null);

		if (enemiesInTrigger.Count > 0)
		{
			if (PlayerAttackSound != null) PlayerAttackSound.Play();

			for (int i = 0; i < enemiesInTrigger.Count; i++)
			{
				EnemyHealth enemy = enemiesInTrigger[i];
				if (enemy != null)
				{
					enemy.TakeDamage(PlayerDamageAmount);

					if (EnemyHitEffectPrefab != null)
					{
						Instantiate(EnemyHitEffectPrefab, enemy.transform.position + Vector3.up * 0.5f, Quaternion.identity);
					}
				}
			}
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
}