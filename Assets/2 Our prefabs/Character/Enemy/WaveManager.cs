using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WaveManager : MonoBehaviour
{
	public static WaveManager Instance;

	// Holds active spawners for the current match
	private static List<EnemySpawner> activeSpawners = new List<EnemySpawner>();

	[Header("Wave Configuration")]
	[Tooltip("Drag and drop your WaveData ScriptableObjects here in sequence.")]
	public List<WaveData> Waves;

	[Header("UI Reference")]
	public TextMeshProUGUI WaveText;

	private int currentWaveIndex = 0;
	private float waveTimer = 0f;
	private bool isSpawning = false;

	private void Awake()
	{
		Instance = this;
	}

	private void Start()
	{
		// Initialize the timer for the first wave
		if (Waves.Count > 0 && Waves[currentWaveIndex] != null)
		{
			waveTimer = Waves[currentWaveIndex].TimeBeforeTrigger;
		}
		UpdateUI();
	}

	private void Update()
	{
		if (!MainMenu.IsGameStarted) return;

		// Endless mode: if all waves are completed
		if (currentWaveIndex >= Waves.Count)
		{
			waveTimer -= Time.deltaTime;
			if (waveTimer <= 0)
			{
				waveTimer = 40f;
				// Repeat the last wave in the list
				if (Waves.Count > 0 && Waves[Waves.Count - 1] != null)
				{
					StartCoroutine(SpawnWave(Waves[Waves.Count - 1]));
				}
			}
			return;
		}

		// Count down to trigger the next wave
		waveTimer -= Time.deltaTime;
		if (waveTimer <= 0f && !isSpawning)
		{
			if (Waves[currentWaveIndex] != null)
			{
				TriggerWave(Waves[currentWaveIndex]);
			}
			else
			{
				// Fallback safety if a wave reference is missing
				currentWaveIndex++;
				UpdateUI();
			}
		}
	}

	private void TriggerWave(WaveData wave)
	{
		StartCoroutine(SpawnWave(wave));
		currentWaveIndex++;

		// Configure timer for the following wave if available
		if (currentWaveIndex < Waves.Count && Waves[currentWaveIndex] != null)
		{
			waveTimer = Waves[currentWaveIndex].TimeBeforeTrigger;
		}
		UpdateUI();
	}

	private System.Collections.IEnumerator SpawnWave(WaveData wave)
	{
		isSpawning = true;

		for (int i = 0; i < wave.EnemyCount; i++)
		{
			// Fetch a suitable random spawner away from player
			EnemySpawner selectedSpawner = GetRandomValidSpawner();

			if (selectedSpawner != null && wave.EnemyPrefabs != null && wave.EnemyPrefabs.Length > 0)
			{
				// Pick a random prefab from the list
				GameObject randomEnemyPrefab = wave.EnemyPrefabs[Random.Range(0, wave.EnemyPrefabs.Length)];

				if (randomEnemyPrefab != null)
				{
					// Instantiate enemy
					Instantiate(randomEnemyPrefab, selectedSpawner.transform.position, Quaternion.identity);
				}
			}

			// Delay to prevent enemy clustering
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

		// Filter active spawners outside player proximity
		List<EnemySpawner> validSpawners = new List<EnemySpawner>();
		foreach (var spawner in activeSpawners)
		{
			if (spawner == null) continue;

			if (!isPlayerActive || !spawner.IsPlayerTooClose(playerPos))
			{
				validSpawners.Add(spawner);
			}
		}

		// Fallback option if all active spawners are blocked
		if (validSpawners.Count == 0)
		{
			return activeSpawners[Random.Range(0, activeSpawners.Count)];
		}

		return validSpawners[Random.Range(0, validSpawners.Count)];
	}

	private void UpdateUI()
	{
		if (WaveText != null)
		{
			if (currentWaveIndex < Waves.Count && Waves[currentWaveIndex] != null)
			{
				WaveText.text = $"Wave {currentWaveIndex + 1}: {Waves[currentWaveIndex].WaveName}";
			}
			else
			{
				WaveText.text = "Survival Mode!";
			}
		}
	}

	// Methods to handle dynamically registered spawners
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