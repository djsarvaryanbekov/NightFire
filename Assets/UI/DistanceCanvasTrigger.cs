using UnityEngine;

namespace NightFire
{
	public class DistanceCanvasTrigger : MonoBehaviour
	{
		[Header("Target Settings")]
		[Tooltip("The player transform. If left empty, will try to find the player by tag.")]
		public Transform Player;

		[Header("Trigger Configuration")]
		[Tooltip("Distance at which the canvas is completely deactivated (hidden).")]
		public float HideDistance = 5f;

		[Tooltip("The Canvas or visual GameObject to enable/disable.")]
		public GameObject CanvasObject;

		[Header("Editor Debug Info (Read Only)")]
		[SerializeField] private float currentDistance;
		[SerializeField] private bool isCanvasActive = true;

		private void Start()
		{
			// Fallback to find player by tag if not manually assigned
			if (Player == null)
			{
				GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
				if (playerObj != null)
				{
					Player = playerObj.transform;
				}
			}
		}

		private void Update()
		{
			if (Player == null || CanvasObject == null) return;

			// Calculate flat 2D distance ignoring the Y height difference
			Vector3 playerPos2D = new Vector3(Player.position.x, 0, Player.position.z);
			Vector3 triggerPos2D = new Vector3(transform.position.x, 0, transform.position.z);
			currentDistance = Vector3.Distance(playerPos2D, triggerPos2D);

			// Canvas is active only when the player is outside the HideDistance
			isCanvasActive = currentDistance > HideDistance;

			// Apply state to CanvasObject only when it changes to prevent unnecessary SetActive overhead
			if (CanvasObject.activeSelf != isCanvasActive)
			{
				CanvasObject.SetActive(isCanvasActive);
			}
		}

		private void OnDrawGizmos()
		{
			// Draw the boundary circle/sphere of the trigger in the Scene View
			Gizmos.color = isCanvasActive ? Color.green : Color.red;
			Gizmos.DrawWireSphere(transform.position, HideDistance);

			// Draw a connection line to the player to easily measure distance visually
			if (Player != null)
			{
				Gizmos.color = Color.yellow;
				Gizmos.DrawLine(transform.position, Player.position);
			}
		}
	}
}