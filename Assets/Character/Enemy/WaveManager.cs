using System.Collections.Generic;
using UnityEngine;
using TMPro; // Для отображения информации о волне

[System.Serializable]
public class Wave
{
	public string WaveName = "Wave 1";
	[Tooltip("Через сколько секунд после начала предыдущей волны начнется эта волна.")]
	public float TimeBeforeTrigger = 30f;
	[Tooltip("Список префабов врагов, которые могут появиться в этой волне (Biter, Jumper и т.д.).")]
	public GameObject[] EnemyPrefabs;
	[Tooltip("Количество врагов в этой волне.")]
	public int EnemyCount = 5;
}

public class WaveManager : MonoBehaviour
{
	public static WaveManager Instance;

	// Глобальный статический список всех активных точек спавна
	private static List<EnemySpawner> activeSpawners = new List<EnemySpawner>();

	[Header("Wave Configuration")]
	public List<Wave> Waves;

	[Header("UI Reference")]
	public TextMeshProUGUI WaveText; // Текстовое поле на экране (необязательно)

	private int currentWaveIndex = 0;
	private float waveTimer = 0f;
	private bool isSpawning = false;

	private void Awake()
	{
		Instance = this;
	}

	private void Start()
	{
		// Инициализируем таймер для первой волны
		if (Waves.Count > 0)
		{
			waveTimer = Waves[currentWaveIndex].TimeBeforeTrigger;
		}
		UpdateUI();
	}

	private void Update()
	{
		if (!MainMenu.IsGameStarted) return;

		// Если все заготовленные волны пройдены:
		if (currentWaveIndex >= Waves.Count)
		{
			// Бесконечно запускаем последнюю волну каждые 40 секунд (можно настроить)
			waveTimer -= Time.deltaTime;
			if (waveTimer <= 0)
			{
				waveTimer = 40f;
				StartCoroutine(SpawnWave(Waves[Waves.Count - 1]));
			}
			return;
		}

		// Таймер обратного отсчета до следующей волны
		waveTimer -= Time.deltaTime;
		if (waveTimer <= 0f && !isSpawning)
		{
			TriggerWave(Waves[currentWaveIndex]);
		}
	}

	private void TriggerWave(Wave wave)
	{
		StartCoroutine(SpawnWave(wave));
		currentWaveIndex++;

		// Запускаем таймер для следующей волны
		if (currentWaveIndex < Waves.Count)
		{
			waveTimer = Waves[currentWaveIndex].TimeBeforeTrigger;
		}
		UpdateUI();
	}

	private System.Collections.IEnumerator SpawnWave(Wave wave)
	{
		isSpawning = true;

		for (int i = 0; i < wave.EnemyCount; i++)
		{
			// Выбираем случайный безопасный спавнер
			EnemySpawner selectedSpawner = GetRandomValidSpawner();

			if (selectedSpawner != null && wave.EnemyPrefabs.Length > 0)
			{
				// Выбираем случайный тип врага из списка текущей волны
				GameObject randomEnemyPrefab = wave.EnemyPrefabs[Random.Range(0, wave.EnemyPrefabs.Length)];

				// Спавним врага
				Instantiate(randomEnemyPrefab, selectedSpawner.transform.position, Quaternion.identity);
			}

			// Небольшая задержка (0.3 сек) между появлением врагов, чтобы они не появлялись в одной точке одновременно
			yield return new WaitForSeconds(0.3f);
		}

		isSpawning = false;
	}

	private EnemySpawner GetRandomValidSpawner()
	{
		if (activeSpawners.Count == 0) return null;

		Vector3 playerPos = Vector3.zero;
		bool isPlayerActive = PlayerControls.Instance != null;
		if (isPlayerActive)
		{
			playerPos = PlayerControls.Instance.transform.position;
		}

		// Фильтруем спавнеры: берем только те, где игрока нет в радиусе блокировки
		List<EnemySpawner> validSpawners = new List<EnemySpawner>();
		foreach (var spawner in activeSpawners)
		{
			if (spawner == null) continue;

			if (!isPlayerActive || !spawner.IsPlayerTooClose(playerPos))
			{
				validSpawners.Add(spawner);
			}
		}

		// Если игрок умудрился заблокировать ВСЕ спавнеры на карте:
		// Используем любой спавнер в качестве запасного, чтобы игра не сломалась
		if (validSpawners.Count == 0)
		{
			return activeSpawners[Random.Range(0, activeSpawners.Count)];
		}

		// Возвращаем случайную точку из разрешенных
		return validSpawners[Random.Range(0, validSpawners.Count)];
	}

	private void UpdateUI()
	{
		if (WaveText != null)
		{
			if (currentWaveIndex < Waves.Count)
			{
				WaveText.text = $"Wave {currentWaveIndex + 1}: {Waves[currentWaveIndex].WaveName}";
			}
			else
			{
				WaveText.text = "Survival Mode!";
			}
		}
	}

	// Методы для динамической саморегистрации спавнеров
	public static void RegisterSpawner(EnemySpawner spawner)
	{
		if (!activeSpawners.Contains(spawner))
		{
			activeSpawners.Add(spawner);
		}
	}

	public static void UnregisterSpawner(EnemySpawner spawner)
	{
		if (activeSpawners.Contains(spawner))
		{
			activeSpawners.Remove(spawner);
		}
	}
}