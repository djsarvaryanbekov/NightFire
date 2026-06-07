using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public enum HelperState { GoingToBush, Harvesting, Returning }

[RequireComponent(typeof(NavMeshAgent))]
public class EmberHelper : MonoBehaviour
{
	private NavMeshAgent agent;
	private HelperState currentState = HelperState.GoingToBush;
	private ResourceNode targetNode;

	[Header("Harvest Settings")]
	[Tooltip("Сколько секунд уголек ковыряется внутри куста.")]
	public float HarvestDuration = 3f;
	[Tooltip("Сколько здоровья (секунд) приносит одна принесенная веточка костру (например, 0.05f — это 5-6 секунд).")]
	public float FirepitRefuelAmount = 0.05f;

	[Header("Visuals & Points")]
	[Tooltip("Основной 3D-меш (моделька) уголька, которая будет скрываться, когда он запрыгивает внутрь куста.")]
	public GameObject VisualModel;
	[Tooltip("3D-моделька веточки в зубах у уголька. Включается, когда он несет дрова.")]
	public GameObject CarriedTwigVisual;

	private void Start()
	{
		agent = GetComponent<NavMeshAgent>();

		if (CarriedTwigVisual != null)
		{
			CarriedTwigVisual.SetActive(false); // На старте веточки в зубах нет
		}

		FindNewTargetBush();
	}

	private void Update()
	{
		if (Firepit.Instance == null || !MainMenu.IsGameStarted || ShopManager.IsShopOpen)
		{
			if (agent.enabled && !agent.isStopped) agent.isStopped = true;
			return;
		}

		if (agent.enabled) agent.isStopped = false;

		// 1. Идем к кусту
		if (currentState == HelperState.GoingToBush)
		{
			if (targetNode == null)
			{
				FindNewTargetBush();
				return;
			}

			float dist = Vector3.Distance(transform.position, targetNode.transform.position);
			if (dist <= 1.5f)
			{
				StartCoroutine(HarvestCoroutine());
			}
		}
		// 2. Несем веточку к костру
		else if (currentState == HelperState.Returning)
		{
			float dist = Vector3.Distance(transform.position, Firepit.Instance.transform.position);
			if (dist <= 3.8f) // Радиус костра
			{
				DepositFuel();
			}
		}
	}

	private void FindNewTargetBush()
	{
		if (ResourceNode.ActiveNodes.Count == 0) return;

		// Ищем случайный куст из списка активных
		targetNode = ResourceNode.ActiveNodes[Random.Range(0, ResourceNode.ActiveNodes.Count)];

		if (targetNode != null)
		{
			agent.SetDestination(targetNode.transform.position);
		}
	}

	private IEnumerator HarvestCoroutine()
	{
		currentState = HelperState.Harvesting;
		agent.isStopped = true;

		// Прыгнули в куст: скрываем модельку уголька (он «внутри» меша куста)
		if (VisualModel != null) VisualModel.SetActive(false);

		yield return new WaitForSeconds(HarvestDuration);

		// Вылезли из куста: показываем модельку обратно
		if (VisualModel != null) VisualModel.SetActive(true);

		// Появляется веточка в зубах
		if (CarriedTwigVisual != null) CarriedTwigVisual.SetActive(true);

		// Переключаем цель на костер
		currentState = HelperState.Returning;
		agent.isStopped = false;
		agent.SetDestination(Firepit.Instance.transform.position);
	}

	private void DepositFuel()
	{
		// Подкидываем веточку в костер (лечим его)
		Firepit.Instance.AddHealth(FirepitRefuelAmount, 0);

		// Убираем веточку из зубов
		if (CarriedTwigVisual != null)
		{
			CarriedTwigVisual.SetActive(false);
		}

		// Возвращаемся к сбору
		currentState = HelperState.GoingToBush;
		FindNewTargetBush();
	}
}