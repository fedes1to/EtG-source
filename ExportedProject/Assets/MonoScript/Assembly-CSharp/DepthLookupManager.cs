using UnityEngine;

public static class DepthLookupManager
{
	public enum GungeonSortingLayer
	{
		BACKGROUND,
		PLAYFIELD,
		FOREGROUND
	}

	public static float DEPTH_RESOLUTION_PER_UNIT = 5f;

	public static void PinRendererToRenderer(Renderer attachment, Renderer target)
	{
		tk2dSprite component = attachment.GetComponent<tk2dSprite>();
		if (component != null)
		{
			component.automaticallyManagesDepth = false;
		}
		attachment.sortingLayerName = target.sortingLayerName;
		attachment.sortingOrder = target.sortingOrder;
	}

	public static void ProcessRenderer(Renderer r)
	{
		AssignRendererToSortingLayer(r, GungeonSortingLayer.PLAYFIELD);
		UpdateRenderer(r);
	}

	public static void ProcessRenderer(Renderer r, GungeonSortingLayer l)
	{
		AssignRendererToSortingLayer(r, l);
		UpdateRenderer(r);
	}

	public static void UpdateRenderer(Renderer r)
	{
	}

	public static void UpdateRendererWithWorldYPosition(Renderer r, float worldY)
	{
	}

	public static void AssignSortingOrder(Renderer r, int order)
	{
	}

	public static void AssignRendererToSortingLayer(Renderer r, GungeonSortingLayer targetLayer)
	{
		string sortingLayerName = string.Empty;
		switch (targetLayer)
		{
		case GungeonSortingLayer.BACKGROUND:
			sortingLayerName = "Background";
			break;
		case GungeonSortingLayer.PLAYFIELD:
			sortingLayerName = "Player";
			break;
		case GungeonSortingLayer.FOREGROUND:
			sortingLayerName = "Foreground";
			break;
		default:
			BraveUtility.Log("Switching on invalid sorting layer in AssignRendererToSortingLayer!", Color.red, BraveUtility.LogVerbosity.IMPORTANT);
			break;
		}
		r.sortingLayerName = sortingLayerName;
	}

	private static void AssignSortingOrderByDepth(Renderer r, float yPosition)
	{
	}
}
