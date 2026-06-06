using UnityEngine;

public class AmbientZone : MonoBehaviour
{
	[Header("Audio Settings")]
	[Tooltip("Аудиоклип фоновой музыки или эмбиента для этой конкретной зоны/локации.")]
	public AudioClip ZoneClip;

	private void Start()
	{
		Collider col = GetComponent<Collider>();
		if (col != null)
		{
			col.isTrigger = true; // Принудительно делаем его триггером
		}
		else
		{
			Debug.LogWarning("AmbientZone: На этом объекте нет коллайдера! Пожалуйста, добавьте Box Collider в эдиторе.", this);
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		// Оптимизация Unity 6: бесконтактный поиск через TryGetComponent
		if (other.TryGetComponent<PlayerControls>(out _))
		{
			if (AmbientAudioManager.Instance != null)
			{
				AmbientAudioManager.Instance.EnterZone(this);
			}
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.TryGetComponent<PlayerControls>(out _))
		{
			if (AmbientAudioManager.Instance != null)
			{
				AmbientAudioManager.Instance.ExitZone(this);
			}
		}
	}
}