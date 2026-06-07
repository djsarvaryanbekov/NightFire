using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class LumberMill : MonoBehaviour
{
	[Header("Prefabs")]
	public Rigidbody FirewoodPrefab;

	[Header("Setup Points (Точки позиционирования)")]
	[Tooltip("Точка входа бревна. Создайте пустой объект прямо в устье входа лесопилки и разверните его синей стрелочкой (Z) внутрь лесопилки.")]
	public Transform IntakePoint;
	[Tooltip("Точка выхода дров. Создайте пустой объект на выходе лесопилки и разверните его синей стрелочкой (Z) наружу (в направлении вылета дров).")]
	public Transform OutputPoint;

	[Header("Settings")]
	[Tooltip("Время затягивания и перемалывания бревна в секундах.")]
	public float ConsumeDuration = 1.5f;
	[Tooltip("Интервал времени между вылетом первого и второго полена дров.")]
	public float SpawnInterval = 0.8f;
	[Tooltip("Сила импульса, с которой дрова выталкиваются (выстреливают) из лесопилки.")]
	public float EjectForce = 5f;

	[Header("Audio")]
	public AudioSource WorkSound;

	private void Start()
	{
		if (IntakePoint == null || OutputPoint == null)
		{
			Debug.LogError("LumberMill: Пожалуйста, назначьте IntakePoint и OutputPoint в инспекторе!", this);
		}
	}

	// Срабатывает автоматически, когда срубленное дерево задевает триггер-зону
	private void OnTriggerEnter(Collider other)
	{
		if (!MainMenu.IsGameStarted) return;

		// ИЗМЕНЕНО: Используем attachedRigidbody, чтобы найти Rigidbody на корневом объекте дерева,
		// даже если триггер сначала задел дочерние листья
		Rigidbody treeBody = other.attachedRigidbody;

		if (treeBody != null)
		{
			// Проверяем слой именно у корневого объекта, на котором висит Rigidbody и настроен слой Tree
			if (treeBody.gameObject.layer == LayerMask.NameToLayer("Tree"))
			{
				// Проверяем, что дерево не кинематическое (его уже срубили и оно упало)
				if (!treeBody.isKinematic)
				{
					StartCoroutine(ConsumeTree(treeBody));
				}
			}
		}
	}

	private IEnumerator ConsumeTree(Rigidbody treeBody)
	{
		GameObject tree = treeBody.gameObject;

		// Отключаем физику, чтобы дерево застыло в воздухе и плавно затягивалось
		Destroy(treeBody);

		if (WorkSound != null) WorkSound.Play();

		Vector3 startPos = tree.transform.position;
		Quaternion startRot = tree.transform.rotation;

		// Финальная точка затягивания (чуть глубже входа)
		Vector3 targetPosInside = IntakePoint.position + IntakePoint.forward * 3f;

		float elapsed = 0f;
		while (elapsed < ConsumeDuration)
		{
			if (tree == null) yield break;

			elapsed += Time.deltaTime;
			float percent = elapsed / ConsumeDuration;

			// Плавное сглаживание движения
			float t = Mathf.SmoothStep(0f, 1f, percent);

			// 1. Плавно двигаем бревно внутрь лесопилки
			tree.transform.position = Vector3.Lerp(startPos, targetPosInside, t);

			// 2. Плавно разворачиваем бревно по направлению входа лесопилки (IntakePoint)
			tree.transform.rotation = Quaternion.Slerp(startRot, IntakePoint.rotation, t);

			// 3. Плавно уменьшаем масштаб до 0, имитируя перемолку бревна внутри меша
			tree.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, t);

			yield return null;
		}

		// Уничтожаем остатки бревна
		Destroy(tree);

		// Запускаем последовательный спавн 2 полений дров на выходе
		yield return StartCoroutine(SpawnFirewoodSequential());
	}

	private IEnumerator SpawnFirewoodSequential()
	{
		for (int i = 0; i < 2; i++)
		{
			if (OutputPoint == null || FirewoodPrefab == null) yield break;

			// Создаем дрова на выходе с небольшим случайным отклонением по горизонтали для реалистичности
			Quaternion rotation = OutputPoint.rotation * Quaternion.Euler(0, Random.Range(-15f, 15f), 0);
			Rigidbody firewood = Instantiate(FirewoodPrefab, OutputPoint.position, rotation);

			// Физически выталкиваем дрова вперед в направлении стрелочки Z (forward) точки OutputPoint
			firewood.isKinematic = false;
			firewood.AddForce(OutputPoint.forward * EjectForce, ForceMode.Impulse);

			// Ждем заданный интервал времени перед выталкиванием следующего бревна
			yield return new WaitForSeconds(SpawnInterval);
		}
	}
}