using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class GameUIBlankController : MonoBehaviour, ILevelLoadedListener
{
	public dfSprite blankSpritePrefab;

	public List<dfSprite> extantBlanks;

	public bool IsRightAligned;

	private dfPanel m_panel;

	public dfPanel Panel
	{
		get
		{
			return m_panel;
		}
	}

	private void Awake()
	{
		m_panel = GetComponent<dfPanel>();
		extantBlanks = new List<dfSprite>();
	}

	private void Start()
	{
		m_panel.IsInteractive = false;
		Collider[] components = GetComponents<Collider>();
		for (int i = 0; i < components.Length; i++)
		{
			Object.Destroy(components[i]);
		}
	}

	public void BraveOnLevelWasLoaded()
	{
		if (extantBlanks != null)
		{
			extantBlanks.Clear();
		}
	}

	public void UpdateScale()
	{
		for (int i = 0; i < extantBlanks.Count; i++)
		{
			dfSprite dfSprite2 = extantBlanks[i];
			if ((bool)dfSprite2)
			{
				Vector2 sizeInPixels = dfSprite2.SpriteInfo.sizeInPixels;
				dfSprite2.Size = sizeInPixels * Pixelator.Instance.CurrentTileScale;
			}
		}
	}

	public dfSprite AddBlank()
	{
		Vector3 position = base.transform.position;
		GameObject gameObject = Object.Instantiate(blankSpritePrefab.gameObject, position, Quaternion.identity);
		gameObject.transform.parent = base.transform.parent;
		gameObject.layer = base.gameObject.layer;
		dfSprite component = gameObject.GetComponent<dfSprite>();
		if (IsRightAligned)
		{
			component.Anchor = dfAnchorStyle.Top | dfAnchorStyle.Right;
		}
		Vector2 sizeInPixels = component.SpriteInfo.sizeInPixels;
		component.Size = sizeInPixels * Pixelator.Instance.CurrentTileScale;
		if (!IsRightAligned)
		{
			float x = (component.Width + Pixelator.Instance.CurrentTileScale) * (float)extantBlanks.Count;
			component.RelativePosition = m_panel.RelativePosition + new Vector3(x, 0f, 0f);
		}
		else
		{
			component.RelativePosition = m_panel.RelativePosition - new Vector3(component.Width, 0f, 0f);
			for (int i = 0; i < extantBlanks.Count; i++)
			{
				dfSprite dfSprite2 = extantBlanks[i];
				if ((bool)dfSprite2)
				{
					dfSprite2.RelativePosition += new Vector3(-1f * (component.Width + Pixelator.Instance.CurrentTileScale), 0f, 0f);
					GameUIRoot.Instance.UpdateControlMotionGroup(dfSprite2);
				}
			}
		}
		component.IsInteractive = false;
		extantBlanks.Add(component);
		GameUIRoot.Instance.AddControlToMotionGroups(component, (!IsRightAligned) ? DungeonData.Direction.WEST : DungeonData.Direction.EAST);
		return component;
	}

	public void RemoveBlank()
	{
		if (extantBlanks.Count <= 0)
		{
			return;
		}
		float width = extantBlanks[0].Width;
		dfSprite dfSprite2 = extantBlanks[extantBlanks.Count - 1];
		if ((bool)dfSprite2)
		{
			GameUIRoot.Instance.RemoveControlFromMotionGroups(dfSprite2);
			Object.Destroy(dfSprite2);
		}
		extantBlanks.RemoveAt(extantBlanks.Count - 1);
		if (!IsRightAligned)
		{
			return;
		}
		for (int i = 0; i < extantBlanks.Count; i++)
		{
			dfSprite dfSprite3 = extantBlanks[i];
			if ((bool)dfSprite3)
			{
				dfSprite3.RelativePosition += new Vector3(width + Pixelator.Instance.CurrentTileScale, 0f, 0f);
				GameUIRoot.Instance.UpdateControlMotionGroup(dfSprite3);
			}
		}
	}

	public void UpdateBlanks(int numBlanks)
	{
		if (GameManager.Instance.IsLoadingLevel || Time.timeSinceLevelLoad < 0.01f)
		{
			return;
		}
		if (extantBlanks.Count < numBlanks)
		{
			for (int i = extantBlanks.Count; i < numBlanks; i++)
			{
				AddBlank();
			}
		}
		else if (extantBlanks.Count > numBlanks)
		{
			while (extantBlanks.Count > numBlanks)
			{
				RemoveBlank();
			}
		}
	}
}
