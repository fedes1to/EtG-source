using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class GameUIHeartController : MonoBehaviour, ILevelLoadedListener
{
	public dfSprite heartSpritePrefab;

	public dfSprite damagedHeartAnimationPrefab;

	public Vector3 damagedPrefabOffset;

	public dfSprite armorSpritePrefab;

	public dfSprite damagedArmorAnimationPrefab;

	public Vector3 damagedArmorPrefabOffset;

	public dfSprite crestSpritePrefab;

	public dfSprite damagedCrestAnimationPrefab;

	public List<dfSprite> extantHearts;

	public List<dfSprite> extantArmors;

	public string fullHeartSpriteName;

	public string halfHeartSpriteName;

	public string emptyHeartSpriteName;

	public bool IsRightAligned;

	private dfPanel m_panel;

	private string m_currentFullHeartName;

	private string m_currentHalfHeartName;

	private string m_currentEmptyHeartName;

	private string m_currentArmorName;

	public dfPanel Panel
	{
		get
		{
			return m_panel;
		}
	}

	private void Awake()
	{
		m_currentFullHeartName = fullHeartSpriteName;
		m_currentHalfHeartName = halfHeartSpriteName;
		m_currentEmptyHeartName = emptyHeartSpriteName;
		m_currentArmorName = armorSpritePrefab.SpriteName;
		m_panel = GetComponent<dfPanel>();
		extantHearts = new List<dfSprite>();
		extantArmors = new List<dfSprite>();
	}

	private void Start()
	{
		Collider[] components = GetComponents<Collider>();
		for (int i = 0; i < components.Length; i++)
		{
			Object.Destroy(components[i]);
		}
	}

	public void BraveOnLevelWasLoaded()
	{
		if (extantHearts != null)
		{
			for (int i = 0; i < extantHearts.Count; i++)
			{
				if (!extantHearts[i])
				{
					extantHearts.RemoveAt(i);
					i--;
				}
			}
		}
		if (extantArmors == null)
		{
			return;
		}
		for (int j = 0; j < extantArmors.Count; j++)
		{
			if (!extantArmors[j])
			{
				extantArmors.RemoveAt(j);
				j--;
			}
		}
	}

	public void UpdateScale()
	{
		for (int i = 0; i < extantHearts.Count; i++)
		{
			dfSprite dfSprite2 = extantHearts[i];
			if ((bool)dfSprite2)
			{
				Vector2 sizeInPixels = dfSprite2.SpriteInfo.sizeInPixels;
				dfSprite2.Size = sizeInPixels * Pixelator.Instance.CurrentTileScale;
			}
		}
		for (int j = 0; j < extantArmors.Count; j++)
		{
			dfSprite dfSprite3 = extantArmors[j];
			if ((bool)dfSprite3)
			{
				Vector2 sizeInPixels2 = dfSprite3.SpriteInfo.sizeInPixels;
				dfSprite3.Size = sizeInPixels2 * Pixelator.Instance.CurrentTileScale;
			}
		}
	}

	public void AddArmor()
	{
		Vector3 position = base.transform.position;
		GameObject gameObject = Object.Instantiate(armorSpritePrefab.gameObject, position, Quaternion.identity);
		gameObject.transform.parent = base.transform.parent;
		gameObject.layer = base.gameObject.layer;
		dfSprite component = gameObject.GetComponent<dfSprite>();
		if (IsRightAligned)
		{
			component.Anchor = dfAnchorStyle.Top | dfAnchorStyle.Right;
		}
		Vector2 sizeInPixels = component.SpriteInfo.sizeInPixels;
		component.Size = sizeInPixels * Pixelator.Instance.CurrentTileScale;
		component.IsInteractive = false;
		if (!IsRightAligned)
		{
			float num = ((extantHearts.Count <= 0) ? 0f : ((extantHearts[0].Width + Pixelator.Instance.CurrentTileScale) * (float)extantHearts.Count));
			float num2 = (component.Width + Pixelator.Instance.CurrentTileScale) * (float)extantArmors.Count;
			component.RelativePosition = m_panel.RelativePosition + new Vector3(num + num2, 0f, 0f);
		}
		else
		{
			component.RelativePosition = m_panel.RelativePosition - new Vector3(component.Width, 0f, 0f);
			for (int i = 0; i < extantArmors.Count; i++)
			{
				dfSprite dfSprite2 = extantArmors[i];
				if ((bool)dfSprite2)
				{
					GameUIRoot.Instance.TransitionTargetMotionGroup(dfSprite2, true, false, true);
					dfSprite2.RelativePosition += new Vector3(-1f * (component.Width + Pixelator.Instance.CurrentTileScale), 0f, 0f);
					GameUIRoot.Instance.UpdateControlMotionGroup(dfSprite2);
					GameUIRoot.Instance.TransitionTargetMotionGroup(dfSprite2, GameUIRoot.Instance.IsCoreUIVisible(), false, true);
				}
			}
			for (int j = 0; j < extantHearts.Count; j++)
			{
				dfSprite dfSprite3 = extantHearts[j];
				if ((bool)dfSprite3)
				{
					GameUIRoot.Instance.TransitionTargetMotionGroup(dfSprite3, true, false, true);
					dfSprite3.RelativePosition += new Vector3(-1f * (component.Width + Pixelator.Instance.CurrentTileScale), 0f, 0f);
					GameUIRoot.Instance.UpdateControlMotionGroup(dfSprite3);
					GameUIRoot.Instance.TransitionTargetMotionGroup(dfSprite3, GameUIRoot.Instance.IsCoreUIVisible(), false, true);
				}
			}
		}
		extantArmors.Add(component);
		GameUIRoot.Instance.AddControlToMotionGroups(component, (!IsRightAligned) ? DungeonData.Direction.WEST : DungeonData.Direction.EAST);
	}

	public void RemoveArmor()
	{
		if (extantArmors.Count <= 0)
		{
			return;
		}
		dfSprite dfSprite2 = damagedArmorAnimationPrefab;
		dfSprite dfSprite3 = extantArmors[extantArmors.Count - 1];
		if ((bool)dfSprite3)
		{
			if (dfSprite3.SpriteName == crestSpritePrefab.SpriteName)
			{
				dfSprite2 = damagedCrestAnimationPrefab;
			}
			if (dfSprite2 != null)
			{
				GameObject gameObject = Object.Instantiate(dfSprite2.gameObject);
				gameObject.transform.parent = base.transform.parent;
				gameObject.layer = base.gameObject.layer;
				dfSprite component = gameObject.GetComponent<dfSprite>();
				component.BringToFront();
				dfSprite3.Parent.AddControl(component);
				dfSprite3.Parent.BringToFront();
				component.ZOrder = dfSprite3.ZOrder - 1;
				component.RelativePosition = dfSprite3.RelativePosition + damagedArmorPrefabOffset;
				m_panel.AddControl(component);
			}
		}
		float width = extantArmors[0].Width;
		if ((bool)dfSprite3)
		{
			GameUIRoot.Instance.RemoveControlFromMotionGroups(dfSprite3);
			Object.Destroy(extantArmors[extantArmors.Count - 1]);
		}
		extantArmors.RemoveAt(extantArmors.Count - 1);
		if (!IsRightAligned)
		{
			return;
		}
		for (int i = 0; i < extantArmors.Count; i++)
		{
			dfSprite dfSprite4 = extantArmors[i];
			if ((bool)dfSprite4)
			{
				GameUIRoot.Instance.TransitionTargetMotionGroup(dfSprite4, true, false, true);
				dfSprite4.RelativePosition += new Vector3(width + Pixelator.Instance.CurrentTileScale, 0f, 0f);
				GameUIRoot.Instance.UpdateControlMotionGroup(dfSprite4);
				GameUIRoot.Instance.TransitionTargetMotionGroup(dfSprite4, GameUIRoot.Instance.IsCoreUIVisible(), false, true);
			}
		}
		for (int j = 0; j < extantHearts.Count; j++)
		{
			dfSprite dfSprite5 = extantHearts[j];
			if ((bool)dfSprite5)
			{
				GameUIRoot.Instance.TransitionTargetMotionGroup(dfSprite5, true, false, true);
				dfSprite5.RelativePosition += new Vector3(width + Pixelator.Instance.CurrentTileScale, 0f, 0f);
				GameUIRoot.Instance.UpdateControlMotionGroup(dfSprite5);
				GameUIRoot.Instance.TransitionTargetMotionGroup(dfSprite5, GameUIRoot.Instance.IsCoreUIVisible(), false, true);
			}
		}
	}

	private void ClearAllArmor()
	{
		if (extantArmors.Count > 0)
		{
			while (extantArmors.Count > 0)
			{
				RemoveArmor();
			}
		}
	}

	public dfSprite AddHeart()
	{
		int count = extantArmors.Count;
		ClearAllArmor();
		Vector3 position = base.transform.position;
		GameObject gameObject = Object.Instantiate(heartSpritePrefab.gameObject, position, Quaternion.identity);
		gameObject.transform.parent = base.transform.parent;
		gameObject.layer = base.gameObject.layer;
		dfSprite component = gameObject.GetComponent<dfSprite>();
		if (IsRightAligned)
		{
			component.Anchor = dfAnchorStyle.Top | dfAnchorStyle.Right;
		}
		Vector2 sizeInPixels = component.SpriteInfo.sizeInPixels;
		component.Size = sizeInPixels * Pixelator.Instance.CurrentTileScale;
		component.IsInteractive = false;
		if (!IsRightAligned)
		{
			float x = (component.Width + Pixelator.Instance.CurrentTileScale) * (float)extantHearts.Count;
			component.RelativePosition = m_panel.RelativePosition + new Vector3(x, 0f, 0f);
		}
		else
		{
			component.RelativePosition = m_panel.RelativePosition - new Vector3(component.Width, 0f, 0f);
			for (int i = 0; i < extantHearts.Count; i++)
			{
				dfSprite dfSprite2 = extantHearts[i];
				if ((bool)dfSprite2)
				{
					GameUIRoot.Instance.TransitionTargetMotionGroup(dfSprite2, true, false, true);
					dfSprite2.RelativePosition += new Vector3(-1f * (component.Width + Pixelator.Instance.CurrentTileScale), 0f, 0f);
					GameUIRoot.Instance.UpdateControlMotionGroup(dfSprite2);
					GameUIRoot.Instance.TransitionTargetMotionGroup(dfSprite2, GameUIRoot.Instance.IsCoreUIVisible(), false, true);
				}
			}
			for (int j = 0; j < extantArmors.Count; j++)
			{
				dfSprite dfSprite3 = extantArmors[j];
				if ((bool)dfSprite3)
				{
					GameUIRoot.Instance.TransitionTargetMotionGroup(dfSprite3, true, false, true);
					dfSprite3.RelativePosition += new Vector3(-1f * (component.Width + Pixelator.Instance.CurrentTileScale), 0f, 0f);
					GameUIRoot.Instance.UpdateControlMotionGroup(dfSprite3);
					GameUIRoot.Instance.TransitionTargetMotionGroup(dfSprite3, GameUIRoot.Instance.IsCoreUIVisible(), false, true);
				}
			}
		}
		extantHearts.Add(component);
		GameUIRoot.Instance.AddControlToMotionGroups(component, (!IsRightAligned) ? DungeonData.Direction.WEST : DungeonData.Direction.EAST);
		for (int k = 0; k < count; k++)
		{
			AddArmor();
		}
		return component;
	}

	public void RemoveHeart()
	{
		if (extantHearts.Count <= 0)
		{
			return;
		}
		float width = extantHearts[0].Width;
		dfSprite dfSprite2 = extantHearts[extantHearts.Count - 1];
		if ((bool)dfSprite2)
		{
			GameUIRoot.Instance.RemoveControlFromMotionGroups(dfSprite2);
			Object.Destroy(dfSprite2);
		}
		extantHearts.RemoveAt(extantHearts.Count - 1);
		if (IsRightAligned)
		{
			for (int i = 0; i < extantHearts.Count; i++)
			{
				dfSprite dfSprite3 = extantHearts[i];
				if ((bool)dfSprite3)
				{
					GameUIRoot.Instance.TransitionTargetMotionGroup(dfSprite3, true, false, true);
					dfSprite3.RelativePosition += new Vector3(width + Pixelator.Instance.CurrentTileScale, 0f, 0f);
					GameUIRoot.Instance.UpdateControlMotionGroup(dfSprite3);
					GameUIRoot.Instance.TransitionTargetMotionGroup(dfSprite3, GameUIRoot.Instance.IsCoreUIVisible(), false, true);
				}
			}
		}
		else if (extantArmors != null && extantArmors.Count > 0 && extantHearts.Count > 0)
		{
			for (int j = 0; j < extantArmors.Count; j++)
			{
				float x = extantHearts[0].Size.x + Pixelator.Instance.CurrentTileScale;
				dfSprite dfSprite4 = extantArmors[j];
				if ((bool)dfSprite4)
				{
					GameUIRoot.Instance.TransitionTargetMotionGroup(dfSprite4, true, false, true);
					dfSprite4.RelativePosition -= new Vector3(x, 0f, 0f);
					GameUIRoot.Instance.UpdateControlMotionGroup(dfSprite4);
					GameUIRoot.Instance.TransitionTargetMotionGroup(dfSprite4, GameUIRoot.Instance.IsCoreUIVisible(), false, true);
				}
			}
		}
		ClearAllArmor();
	}

	public void UpdateHealth(HealthHaver hh)
	{
		float num = hh.GetCurrentHealth();
		float maxHealth = hh.GetMaxHealth();
		float num2 = hh.Armor;
		if (hh.NextShotKills)
		{
			num = 0.5f;
			num2 = 0f;
		}
		int num3 = Mathf.CeilToInt(maxHealth);
		int num4 = Mathf.CeilToInt(num2);
		if (extantHearts.Count < num3)
		{
			for (int i = extantHearts.Count; i < num3; i++)
			{
				dfSprite dfSprite2 = AddHeart();
				if ((float)(i + 1) > num)
				{
					if ((float)Mathf.FloorToInt(num) != num && num + 1f > (float)(i + 1))
					{
						dfSprite2.SpriteName = m_currentHalfHeartName;
					}
					else
					{
						dfSprite2.SpriteName = m_currentEmptyHeartName;
					}
				}
			}
		}
		else if (extantHearts.Count > num3)
		{
			while (extantHearts.Count > num3)
			{
				RemoveHeart();
			}
		}
		if (extantArmors.Count < num4)
		{
			for (int j = extantArmors.Count; j < num4; j++)
			{
				AddArmor();
			}
		}
		else if (extantArmors.Count > num4)
		{
			while (extantArmors.Count > num4)
			{
				RemoveArmor();
			}
		}
		int num5 = Mathf.FloorToInt(num);
		for (int k = 0; k < extantHearts.Count; k++)
		{
			dfSprite dfSprite3 = extantHearts[k];
			if (!dfSprite3)
			{
				continue;
			}
			if (k < num5)
			{
				dfSprite3.SpriteName = m_currentFullHeartName;
			}
			else if (k != num5 || !(num - (float)num5 > 0f))
			{
				if (dfSprite3.SpriteName == m_currentFullHeartName || dfSprite3.SpriteName == m_currentHalfHeartName)
				{
					GameObject gameObject = Object.Instantiate(damagedHeartAnimationPrefab.gameObject);
					gameObject.transform.parent = base.transform.parent;
					gameObject.layer = base.gameObject.layer;
					dfSprite component = gameObject.GetComponent<dfSprite>();
					component.BringToFront();
					dfSprite3.Parent.AddControl(component);
					dfSprite3.Parent.BringToFront();
					component.ZOrder = dfSprite3.ZOrder - 1;
					component.RelativePosition = dfSprite3.RelativePosition + damagedPrefabOffset;
				}
				dfSprite3.SpriteName = m_currentEmptyHeartName;
			}
		}
		if (num - (float)num5 > 0f && extantHearts != null && extantHearts.Count > 0)
		{
			dfSprite dfSprite4 = extantHearts[num5];
			if ((bool)dfSprite4)
			{
				if (dfSprite4.SpriteName == m_currentFullHeartName)
				{
					GameObject gameObject2 = Object.Instantiate(damagedHeartAnimationPrefab.gameObject);
					gameObject2.transform.parent = base.transform.parent;
					gameObject2.layer = base.gameObject.layer;
					dfSprite component2 = gameObject2.GetComponent<dfSprite>();
					component2.BringToFront();
					dfSprite4.Parent.AddControl(component2);
					dfSprite4.Parent.BringToFront();
					component2.ZOrder = dfSprite4.ZOrder - 1;
					component2.RelativePosition = dfSprite4.RelativePosition + damagedPrefabOffset;
				}
				dfSprite4.SpriteName = m_currentHalfHeartName;
			}
		}
		PlayerController associatedPlayer = hh.gameActor as PlayerController;
		ProcessHeartSpriteModifications(associatedPlayer);
		if (hh.HasCrest && num2 > 0f)
		{
			for (int l = 0; l < extantArmors.Count; l++)
			{
				dfSprite dfSprite5 = extantArmors[l];
				if (!dfSprite5)
				{
					continue;
				}
				if (l < extantArmors.Count - 1)
				{
					if (dfSprite5.SpriteName != m_currentArmorName)
					{
						dfSprite5.SpriteName = m_currentArmorName;
						dfSprite5.Color = armorSpritePrefab.Color;
						dfPanel motionGroupParent = GameUIRoot.Instance.GetMotionGroupParent(dfSprite5);
						motionGroupParent.Width -= Pixelator.Instance.CurrentTileScale;
						motionGroupParent.Height -= Pixelator.Instance.CurrentTileScale;
						dfSprite5.RelativePosition = dfSprite5.RelativePosition.WithY(0f);
					}
				}
				else if (dfSprite5.SpriteName != crestSpritePrefab.SpriteName)
				{
					dfSprite5.SpriteName = crestSpritePrefab.SpriteName;
					dfSprite5.Color = crestSpritePrefab.Color;
					dfPanel motionGroupParent2 = GameUIRoot.Instance.GetMotionGroupParent(dfSprite5);
					motionGroupParent2.Width += Pixelator.Instance.CurrentTileScale;
					motionGroupParent2.Height += Pixelator.Instance.CurrentTileScale;
					dfSprite5.RelativePosition = dfSprite5.RelativePosition.WithY(Pixelator.Instance.CurrentTileScale);
				}
			}
		}
		else
		{
			for (int m = 0; m < extantArmors.Count; m++)
			{
				dfSprite dfSprite6 = extantArmors[m];
				if ((bool)dfSprite6)
				{
					if (dfSprite6.SpriteName != m_currentArmorName)
					{
						dfSprite6.SpriteName = m_currentArmorName;
						dfPanel motionGroupParent3 = GameUIRoot.Instance.GetMotionGroupParent(dfSprite6);
						motionGroupParent3.Width -= Pixelator.Instance.CurrentTileScale;
						motionGroupParent3.Height -= Pixelator.Instance.CurrentTileScale;
						dfSprite6.RelativePosition = dfSprite6.RelativePosition.WithY(0f);
						GameUIRoot.Instance.TransitionTargetMotionGroup(dfSprite6, true, false, true);
						GameUIRoot.Instance.UpdateControlMotionGroup(dfSprite6);
						GameUIRoot.Instance.TransitionTargetMotionGroup(dfSprite6, GameUIRoot.Instance.IsCoreUIVisible(), false, true);
					}
					dfSprite6.Color = armorSpritePrefab.Color;
					dfSprite6.RelativePosition = dfSprite6.RelativePosition.WithY(0f);
				}
			}
		}
		for (int n = 0; n < extantHearts.Count; n++)
		{
			dfSprite dfSprite7 = extantHearts[n];
			if ((bool)dfSprite7)
			{
				dfSprite7.Size = dfSprite7.SpriteInfo.sizeInPixels * Pixelator.Instance.CurrentTileScale;
			}
		}
		for (int num6 = 0; num6 < extantArmors.Count; num6++)
		{
			dfSprite dfSprite8 = extantArmors[num6];
			if ((bool)dfSprite8)
			{
				dfSprite8.Size = dfSprite8.SpriteInfo.sizeInPixels * Pixelator.Instance.CurrentTileScale;
			}
		}
	}

	private void ProcessHeartSpriteModifications(PlayerController associatedPlayer)
	{
		bool flag = false;
		if ((bool)associatedPlayer)
		{
			if (associatedPlayer.HealthAndArmorSwapped)
			{
				m_currentFullHeartName = "heart_shield_full_001";
				m_currentHalfHeartName = "heart_shield_half_001";
				m_currentEmptyHeartName = "heart_shield_empty_001";
				m_currentArmorName = "armor_shield_heart_idle_001";
				flag = true;
			}
			else if ((bool)associatedPlayer.CurrentGun && associatedPlayer.CurrentGun.IsUndertaleGun)
			{
				m_currentFullHeartName = "heart_full_yellow_001";
				m_currentHalfHeartName = "heart_half_yellow_001";
				flag = true;
			}
		}
		if (!flag)
		{
			m_currentFullHeartName = fullHeartSpriteName;
			m_currentHalfHeartName = halfHeartSpriteName;
			m_currentEmptyHeartName = emptyHeartSpriteName;
			m_currentArmorName = armorSpritePrefab.SpriteName;
		}
	}
}
