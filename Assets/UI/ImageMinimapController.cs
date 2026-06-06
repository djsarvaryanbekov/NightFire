using UnityEngine;

public class ImageMinimapController : MonoBehaviour
{
	[Header("Player & World Settings")]
	public Transform PlayerTransform;
	[Tooltip("Размер вашей игровой зоны/террейна в метрах (например, X = 500, Y = 500).")]
	public Vector2 WorldSize = new Vector2(500f, 500f);
	[Tooltip("Центр вашей игровой зоны в мировых координатах. Если террейн начинается в (0,0), то центр будет равен половине его размера.")]
	public Vector2 WorldCenter = new Vector2(250f, 250f);

	[Header("Small Minimap Settings")]
	public GameObject SmallMapPanel;
	public RectTransform SmallMapImage;

	// Ползунок масштаба прямо в инспекторе Unity (от 0.1 до 5)
	[Range(0.1f, 5f)]
	public float SmallMapZoom = 1.0f;

	[Header("Large Map Settings")]
	public GameObject LargeMapPanel;
	public RectTransform LargeMapImage;

	// Ползунок масштаба прямо в инспекторе Unity (от 0.1 до 5)
	[Range(0.1f, 5f)]
	public float LargeMapZoom = 1.0f;

	private bool isExpanded = false;

	// Размеры картинок в пикселях (берутся автоматически)
	private Vector2 smallMapPixelSize;
	private Vector2 largeMapPixelSize;

	private void Start()
	{
		// Автоматически запоминаем исходный размер картинок в пикселях
		if (SmallMapImage != null) smallMapPixelSize = SmallMapImage.rect.size;
		if (LargeMapImage != null) largeMapPixelSize = LargeMapImage.rect.size;

		SetMapState(false);
	}

	private void Update()
	{
		// Переключение карт на М
		if (Input.GetKeyDown(KeyCode.M))
		{
			isExpanded = !isExpanded;
			SetMapState(isExpanded);
		}
	}

	private void LateUpdate()
	{
		if (PlayerTransform == null)
		{
			if (PlayerControls.Instance != null)
			{
				PlayerTransform = PlayerControls.Instance.transform;
			}
			else
			{
				return; // Ждем игрока
			}
		}

		// Координаты игрока на плоскости X/Z
		Vector2 playerWorldPos = new Vector2(PlayerTransform.position.x, PlayerTransform.position.z);

		// Смещение игрока относительно центра игрового мира в метрах
		Vector2 playerOffset = playerWorldPos - WorldCenter;

		if (!isExpanded)
		{
			if (SmallMapImage != null)
			{
				UpdateMapImage(SmallMapImage, playerOffset, smallMapPixelSize, SmallMapZoom);
			}
		}
		else
		{
			if (LargeMapImage != null)
			{
				UpdateMapImage(LargeMapImage, playerOffset, largeMapPixelSize, LargeMapZoom);
			}
		}
	}

	private void UpdateMapImage(RectTransform mapImage, Vector2 playerOffset, Vector2 mapPixelSize, float zoom)
	{
		// Масштабируем картинку карты на основе значения ползунка из инспектора
		mapImage.localScale = new Vector3(zoom, zoom, 1f);

		// Вычисляем соотношение: сколько пикселей UI приходится на 1 метр игрового мира
		float ratioX = mapPixelSize.x / WorldSize.x;
		float ratioY = mapPixelSize.y / WorldSize.y;

		// Сдвигаем карту в UI в противоположную сторону движения игрока
		float uiOffsetX = -playerOffset.x * ratioX * zoom;
		float uiOffsetY = -playerOffset.y * ratioY * zoom;

		mapImage.anchoredPosition = new Vector2(uiOffsetX, uiOffsetY);
	}

	private void SetMapState(bool expand)
	{
		if (SmallMapPanel != null) SmallMapPanel.SetActive(!expand);
		if (LargeMapPanel != null) LargeMapPanel.SetActive(expand);
	}
}