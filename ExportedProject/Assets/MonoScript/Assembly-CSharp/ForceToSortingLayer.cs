using UnityEngine;

public class ForceToSortingLayer : MonoBehaviour
{
	public DepthLookupManager.GungeonSortingLayer sortingLayer;

	public int targetSortingOrder = -1;

	private void OnEnable()
	{
		DepthLookupManager.AssignRendererToSortingLayer(GetComponent<Renderer>(), sortingLayer);
		if (targetSortingOrder != -1)
		{
			DepthLookupManager.UpdateRendererWithWorldYPosition(GetComponent<Renderer>(), base.transform.position.y);
		}
	}
}
