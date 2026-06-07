using System.Collections.Generic;
using UnityEngine;

public class ResourceNode : MonoBehaviour
{
	// √лобальный список всех доступных кустов на карте
	public static List<ResourceNode> ActiveNodes = new List<ResourceNode>();

	private void OnEnable()
	{
		ActiveNodes.Add(this);
	}

	private void OnDisable()
	{
		ActiveNodes.Remove(this);
	}
}