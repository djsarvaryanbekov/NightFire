using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
	[Header("Detection Settings")]
	[Tooltip("Если игрок находится ближе этого расстояния, спавнер временно отключается.")]
	public float PlayerProximityRadius = 15f;

	private void OnEnable()
	{
		// Автоматически регистрируемся в менеджере волн
		WaveManager.RegisterSpawner(this);
	}

	private void OnDisable()
	{
		// Удаляем себя из списка при отключении объекта
		WaveManager.UnregisterSpawner(this);
	}

	public bool IsPlayerTooClose(Vector3 playerPosition)
	{
		return Vector3.Distance(transform.position, playerPosition) < PlayerProximityRadius;
	}

	private void OnDrawGizmos()
	{
		// Рисуем красный радиус в Scene View для удобной настройки границ в редакторе
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(transform.position, PlayerProximityRadius);
	}
}