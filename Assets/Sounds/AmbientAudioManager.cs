using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmbientAudioManager : MonoBehaviour
{
	public static AmbientAudioManager Instance;

	[Header("General Audio Settings")]
	[Tooltip("Стандартная фоновая музыка/тишина, когда игрок находится вне каких-либо зон.")]
	public AudioClip DefaultAmbientClip;
	[Tooltip("Время плавного перехода (затухания и нарастания звука) между треками в секундах.")]
	public float FadeDuration = 2.0f;

	[Header("Audio Sources")]
	[Tooltip("Два источника звука для плавного кроссфейда. Если оставить пустыми, добавятся автоматически.")]
	public AudioSource AudioSourceA;
	public AudioSource AudioSourceB;

	private AudioSource activeSource;
	private AudioSource inactiveSource;
	private Coroutine fadeCoroutine;

	// Список зон, внутри которых сейчас находится игрок
	private List<AmbientZone> insideZones = new List<AmbientZone>();

	private void Awake()
	{
		Instance = this;
	}

	private void Start()
	{
		// Автоматически добавляем источники, если они не привязаны
		if (AudioSourceA == null || AudioSourceB == null)
		{
			AudioSourceA = gameObject.AddComponent<AudioSource>();
			AudioSourceB = gameObject.AddComponent<AudioSource>();
		}

		ConfigureSource(AudioSourceA);
		ConfigureSource(AudioSourceB);

		activeSource = AudioSourceA;
		inactiveSource = AudioSourceB;

		// Запускаем дефолтный эмбиент на старте
		if (DefaultAmbientClip != null)
		{
			activeSource.clip = DefaultAmbientClip;
			activeSource.volume = 1f;
			activeSource.Play();
		}
	}

	private void ConfigureSource(AudioSource source)
	{
		source.loop = true;
		source.playOnAwake = false;
		source.volume = 0f;
	}

	public void EnterZone(AmbientZone zone)
	{
		if (!insideZones.Contains(zone))
		{
			insideZones.Add(zone);
		}

		// Запускаем трек последней зоны, в которую вошел игрок
		PlayTrack(zone.ZoneClip);
	}

	public void ExitZone(AmbientZone zone)
	{
		if (insideZones.Contains(zone))
		{
			insideZones.Remove(zone);
		}

		if (insideZones.Count > 0)
		{
			// Если вышли из одной зоны, но всё еще находимся в другой — возвращаем трек предыдущей зоны
			PlayTrack(insideZones[insideZones.Count - 1].ZoneClip);
		}
		else
		{
			// Если вышли из всех зон — возвращаем дефолтный трек (или затухаем в тишину)
			PlayTrack(DefaultAmbientClip);
		}
	}

	private void PlayTrack(AudioClip newClip)
	{
		// Если этот клип уже плавно проигрывается на активном источнике — ничего не делаем
		if (activeSource.clip == newClip && activeSource.isPlaying) return;

		if (fadeCoroutine != null)
		{
			StopCoroutine(fadeCoroutine);
		}

		fadeCoroutine = StartCoroutine(CrossfadeCoroutine(newClip));
	}

	private IEnumerator CrossfadeCoroutine(AudioClip newClip)
	{
		// Настраиваем второй (неактивный) источник на новый трек
		inactiveSource.clip = newClip;

		if (newClip != null)
		{
			inactiveSource.Play();
		}

		float elapsed = 0f;
		float startActiveVolume = activeSource.volume;
		float targetInactiveVolume = newClip != null ? 1f : 0f;

		while (elapsed < FadeDuration)
		{
			elapsed += Time.deltaTime;
			float percent = elapsed / FadeDuration;

			// Плавно глушим активный источник и выводим громкость неактивного
			activeSource.volume = Mathf.Lerp(startActiveVolume, 0f, percent);
			inactiveSource.volume = Mathf.Lerp(0f, targetInactiveVolume, percent);

			yield return null;
		}

		activeSource.volume = 0f;
		activeSource.Stop();

		inactiveSource.volume = targetInactiveVolume;

		// Меняем источники местами для следующего перехода
		AudioSource temp = activeSource;
		activeSource = inactiveSource;
		inactiveSource = temp;

		fadeCoroutine = null;
	}
}