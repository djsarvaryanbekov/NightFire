using UnityEngine;

[CreateAssetMenu(fileName = "NewWave", menuName = "NightFire/Wave Data")]
public class WaveData : ScriptableObject
{
	public string WaveName = "Wave 1";

	[Tooltip("Time delay before this wave is automatically triggered.")]
	public float TimeBeforeTrigger = 30f;

	[Tooltip("Array of enemy prefabs that can spawn in this wave.")]
	public GameObject[] EnemyPrefabs;

	[Tooltip("Total count of enemies in this wave.")]
	public int EnemyCount = 5;
}