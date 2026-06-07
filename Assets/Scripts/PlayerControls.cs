using UnityEngine;
using UnityEngine.UI;

public class PlayerControls : MonoBehaviour
{
	public static PlayerControls Instance;
	public static bool IsDashUnlocked = false;

	public float MaxMoveSpeed = 8;

	public AudioSource DashSound;
	public AudioSource StepSound;

	[Header("Super Attack Charge")]
	[Tooltip("Сколько секунд нужно удерживать Пробел для полной зарядки супер-атаки.")]
	public float ChargeDuration = 1.0f;
	[Tooltip("UI картинка (Image с типом Fill Amount) для шкалы зарядки супер-атаки (необязательно).")]
	public Image ChargeProgressBar;

	private CharacterController controllerComponent;
	private Animator animatorComponent;
	private PlayerGrabTrigger grabTrigger;

	private Vector3 moveSpeed;
	private float grabCooldown;
	private float dashingTimeLeft;
	private float spaceHoldTimer = 0f; // Таймер удержания кнопки

	private static readonly int GrabParam = Animator.StringToHash("grab"); 
	private static readonly int WalkSpeedParam = Animator.StringToHash("walk speed");

	private void Start()
	{
		Instance = this;
		animatorComponent = GetComponent<Animator>();
		controllerComponent = GetComponent<CharacterController>();

		grabTrigger = GetComponentInChildren<PlayerGrabTrigger>();

		// Скрываем шкалу зарядки на старте
		if (ChargeProgressBar != null)
		{
			ChargeProgressBar.gameObject.SetActive(false);
		}
	}

	private void Update()
	{
		if (MainMenu.IsGameStarted && Time.timeScale > 0)
		{
			UpdateWalk();

			if (!ShopManager.IsShopOpen)
			{
				HandleSpaceInput();

				// Обычный быстрый укус на другие клавиши (Z и RightControl) без зарядки
				if (Input.GetKeyDown(KeyCode.RightControl) || Input.GetKeyDown(KeyCode.Z))
				{
					Grab();
				}

				if (IsDashUnlocked)
				{
					if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift) || Input.GetKeyDown(KeyCode.X)) Dash(false);
					else if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) || Input.GetKey(KeyCode.X)) Dash(true);
				}
			}
			grabCooldown -= Time.deltaTime;
		}

		float currentSpeed = new Vector3(moveSpeed.x, 0, moveSpeed.z).magnitude;
		float normalizedSpeed = MaxMoveSpeed > 0 ? (currentSpeed / MaxMoveSpeed) : 0f;

		if (dashingTimeLeft > 0)
		{
			normalizedSpeed = 1.5f;
		}
		else
		{
			normalizedSpeed = Mathf.Clamp(normalizedSpeed, 0f, 1f);
		}

		animatorComponent.SetFloat(WalkSpeedParam, normalizedSpeed);
	}

	private void HandleSpaceInput()
	{
		// Пока игрок удерживает Пробел — копим заряд
		if (Input.GetKey(KeyCode.Space))
		{
			spaceHoldTimer += Time.deltaTime;

			// Отображаем шкалу зарядки на экране
			if (ChargeProgressBar != null)
			{
				ChargeProgressBar.gameObject.SetActive(true);
				ChargeProgressBar.fillAmount = Mathf.Min(spaceHoldTimer / ChargeDuration, 1f);
			}
		}

		// Когда игрок отпускает Пробел
		if (Input.GetKeyUp(KeyCode.Space))
		{
			if (spaceHoldTimer >= ChargeDuration)
			{
				// Заряд полный — делаем СУПЕР-АТАКУ по кругу!
				if (PlayerGrabTrigger.Instance != null)
				{
					PlayerGrabTrigger.Instance.PerformSuperAttack();
				}
			}
			else
			{
				// Быстрое нажатие — обычный укус/подбор дров
				Grab();
			}

			// Сбрасываем таймер и скрываем шкалу зарядки
			spaceHoldTimer = 0f;
			if (ChargeProgressBar != null)
			{
				ChargeProgressBar.gameObject.SetActive(false);
			}
		}
	}

	private void Dash(bool holding)
	{
		if (dashingTimeLeft < (holding ? -.4f : -.2f))
		{
			dashingTimeLeft = .3f;
			DashSound.Play();
		}
	}

	public void StepAnimationCallback()
	{
		if (dashingTimeLeft > 0) return;

		if (StepSound.pitch < 1) StepSound.pitch = Random.Range(1.05f, 1.15f);
		else StepSound.pitch = Random.Range(0.9f, 0.95f);
		StepSound.Play();
	}

	private void UpdateWalk()
	{
		float ySpeed = moveSpeed.y;
		moveSpeed.y = 0;
		if (dashingTimeLeft <= 0)
		{
			Vector3 target = MaxMoveSpeed *
							 new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")).normalized;
			moveSpeed = Vector3.MoveTowards(moveSpeed, target, Time.deltaTime * 300);

			if (moveSpeed.magnitude > 0.1f)
			{
				transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(moveSpeed),
					Time.deltaTime * 720);
			}
		}
		else
		{
			moveSpeed = MaxMoveSpeed * 5 * moveSpeed.normalized;
		}

		dashingTimeLeft -= Time.deltaTime;

		moveSpeed.y = ySpeed + Physics.gravity.y * Time.deltaTime;
		controllerComponent.Move(moveSpeed * Time.deltaTime);
	}

	private void Grab()
	{
		if (grabCooldown > 0) return;

		if (grabTrigger.GrabbedObject != null)
		{
			grabTrigger.Release();
			return;
		}

		animatorComponent.SetTrigger(GrabParam);

		grabCooldown = .5f;
	}

	public void GrabAnimationCallback()
	{
		grabTrigger.Grab();
	}
}