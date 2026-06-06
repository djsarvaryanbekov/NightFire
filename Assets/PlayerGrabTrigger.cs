using System.Collections.Generic;
using UnityEngine;

public class PlayerGrabTrigger : MonoBehaviour
{
	// Поле GrabRadius больше не нужно, так как форма задается коллайдером в эдиторе
	public Rigidbody GrabbedObject;

	public GameObject LanternPickedPrefab;

	public AudioSource ChopTreeSound;
	public AudioSource GrabSound;
	public AudioSource GrabShroomSound;
	public AudioSource LanternPickSound;

	private int grabLayerMask;

	// Список объектов, которые сейчас находятся внутри вашего триггера
	private List<Collider> collidersInTrigger = new List<Collider>();

	private void Start()
	{
		grabLayerMask = LayerMask.GetMask("Firewood", "Tree", "Lantern", "Mushroom");

		// Проверяем, есть ли физический коллайдер на объекте Grab Trigger
		Collider triggerCollider = GetComponent<Collider>();
		if (triggerCollider != null)
		{
			triggerCollider.isTrigger = true; // Принудительно делаем его триггером
		}
		else
		{
			Debug.LogWarning("PlayerGrabTrigger: На этом объекте нет коллайдера! Пожалуйста, добавьте Box Collider, Sphere Collider или другой коллайдер в эдиторе.", this);
		}
	}

	// Срабатывает, когда объект входит в зону физического триггера
	private void OnTriggerEnter(Collider other)
	{
		// Проверяем, относится ли объект к нужным слоям
		if (((1 << other.gameObject.layer) & grabLayerMask) != 0)
		{
			if (!collidersInTrigger.Contains(other))
			{
				collidersInTrigger.Add(other);
			}
		}
	}

	// Срабатывает, когда объект выходит из зоны физического триггера
	private void OnTriggerExit(Collider other)
	{
		if (collidersInTrigger.Contains(other))
		{
			collidersInTrigger.Remove(other);
		}
	}

	public void Grab()
	{
		// Очищаем список от объектов, которые могли быть уничтожены (например, сгорели в костре)
		collidersInTrigger.RemoveAll(c => c == null);

		if (collidersInTrigger.Count > 0)
		{
			// Находим самый близкий к игроку объект среди тех, что находятся в триггере
			Collider bestTarget = GetClosestCollider();
			if (bestTarget == null) return;

			Rigidbody target = bestTarget.attachedRigidbody;
			if (target == null) return;

			if (target.gameObject.layer == LayerMask.NameToLayer("Tree") && target.isKinematic)
			{
				// Статичное дерево, рубим его 
				target.isKinematic = false;
				target.transform.SetParent(null, true);
				target.AddForceAtPosition(
					(transform.position - target.position).normalized * 15,
					target.centerOfMass + Vector3.up * 3);

				ChopTreeSound.Play();

				// Убираем из списка доступных для подбора, так как дерево упало/изменило состояние
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

				// Убираем из списка, пока держим в руках
				collidersInTrigger.Remove(bestTarget);
			}
		}
	}

	// Метод для поиска ближайшего объекта
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

			// Если мы отпустили объект и он всё еще внутри нашего триггера, возвращаем его в список
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