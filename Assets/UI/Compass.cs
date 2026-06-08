using UnityEngine;
using UnityEngine.UI;

public class Compass : MonoBehaviour
{
	public Transform Player;

	[Tooltip("The destination the compass will point to. If left empty, defaults to the world origin (0,0,0).")]
	public Transform Target;

	public float StartAppearingRange = 10;
	public float EndAppearingRange = 100;

	private Image imageComponent;

	private void Start()
	{
		imageComponent = GetComponent<Image>();
	}

	private void Update()
	{
		if (Player == null) return;

		// Get the target's position or default to the world origin (0,0,0)
		Vector3 targetPosition = Target != null ? Target.position : Vector3.zero;

		// Calculate the direction from the target to the player on the flat ground plane
		Vector3 targetToPlayer = Player.position - targetPosition;
		targetToPlayer.y = 0; // Ignore height difference for isometric coordinates

		// Calculate distance from the player to the target
		float distance = targetToPlayer.magnitude;

		// Smoothly fade the compass UI alpha based on the distance thresholds
		float a = Mathf.Clamp01((distance - StartAppearingRange) / (EndAppearingRange - StartAppearingRange));
		if (imageComponent != null)
		{
			imageComponent.color = new Color(0.96f, 0.77f, 0.41f, a);
		}

		// Calculate the rotation angle and apply it around the Z-axis of the UI element
		float angle = Vector3.SignedAngle(Vector3.back, targetToPlayer, Vector3.down);
		transform.localRotation = Quaternion.Euler(0, 0, angle);
	}
}