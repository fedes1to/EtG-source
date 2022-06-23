using System;
using System.Collections.Generic;
using UnityEngine;

public class BattleStandardItem : PassiveItem
{
	public static float BattleStandardCharmDurationMultiplier = 2f;

	public static float BattleStandardCompanionDamageMultiplier = 2f;

	public float CharmDurationMultiplier = 2f;

	public float CompanionDamageMultiplier = 2f;

	public GameObject OverheadVFXSprite;

	private GameObject m_instanceOverhead;

	private tk2dSprite m_instanceOverheadSprite;

	private bool m_hiddenForFall;

	private bool m_isBackfacing;

	protected override void Update()
	{
		if (!(m_owner != null) || !m_pickedUp)
		{
			return;
		}
		if ((bool)m_instanceOverheadSprite)
		{
			if (Time.frameCount % 10 == 0 && (bool)m_instanceOverhead && m_instanceOverhead.transform.parent == null)
			{
				DisengageEffect(m_owner);
				EngageEffect(m_owner);
			}
			if (m_owner.IsFalling)
			{
				m_hiddenForFall = true;
				m_instanceOverheadSprite.renderer.enabled = false;
				return;
			}
			if (m_hiddenForFall)
			{
				m_hiddenForFall = false;
				m_instanceOverheadSprite.renderer.enabled = true;
			}
			if (m_isBackfacing != m_owner.IsBackfacing())
			{
				m_isBackfacing = !m_isBackfacing;
				if (m_isBackfacing)
				{
					m_instanceOverheadSprite.transform.localPosition = m_instanceOverheadSprite.transform.localPosition.WithY(m_instanceOverheadSprite.transform.localPosition.y - 0.25f);
					m_instanceOverheadSprite.SetSprite("battle_standard_back_001");
				}
				else
				{
					m_instanceOverheadSprite.transform.localPosition = m_instanceOverheadSprite.transform.localPosition.WithY(m_instanceOverheadSprite.transform.localPosition.y + 0.25f);
					m_instanceOverheadSprite.SetSprite("battle_standard_001");
				}
			}
			if (m_instanceOverheadSprite.FlipX != m_owner.sprite.FlipX)
			{
				m_instanceOverheadSprite.FlipX = m_owner.sprite.FlipX;
				m_instanceOverheadSprite.transform.localPosition = m_instanceOverheadSprite.transform.localPosition.WithX(m_instanceOverheadSprite.transform.localPosition.x * -1f);
			}
			if (GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.END_TIMES)
			{
				DisengageEffect(m_owner);
			}
		}
		else if (GameManager.Instance.CurrentLevelOverrideState != GameManager.LevelOverrideState.END_TIMES)
		{
			EngageEffect(m_owner);
		}
	}

	public override void Pickup(PlayerController player)
	{
		if (!m_pickedUp)
		{
			BattleStandardCharmDurationMultiplier = CharmDurationMultiplier;
			BattleStandardCompanionDamageMultiplier = CompanionDamageMultiplier;
			if (!PassiveItem.ActiveFlagItems.ContainsKey(player))
			{
				PassiveItem.ActiveFlagItems.Add(player, new Dictionary<Type, int>());
			}
			if (!PassiveItem.ActiveFlagItems[player].ContainsKey(GetType()))
			{
				PassiveItem.ActiveFlagItems[player].Add(GetType(), 1);
			}
			else
			{
				PassiveItem.ActiveFlagItems[player][GetType()] = PassiveItem.ActiveFlagItems[player][GetType()] + 1;
			}
			EngageEffect(player);
			base.Pickup(player);
		}
	}

	protected void EngageEffect(PlayerController user)
	{
		if (!m_instanceOverhead)
		{
			m_instanceOverhead = user.RegisterAttachedObject(OverheadVFXSprite, "jetpack", 0.1f);
		}
		m_instanceOverheadSprite = m_instanceOverhead.GetComponentInChildren<tk2dSprite>();
	}

	protected void DisengageEffect(PlayerController user)
	{
		if ((bool)m_instanceOverhead)
		{
			user.DeregisterAttachedObject(m_instanceOverhead);
			m_instanceOverhead = null;
			m_instanceOverheadSprite = null;
		}
	}

	public override DebrisObject Drop(PlayerController player)
	{
		DisengageEffect(player);
		DebrisObject debrisObject = base.Drop(player);
		if (PassiveItem.ActiveFlagItems.ContainsKey(player) && PassiveItem.ActiveFlagItems[player].ContainsKey(GetType()))
		{
			PassiveItem.ActiveFlagItems[player][GetType()] = Mathf.Max(0, PassiveItem.ActiveFlagItems[player][GetType()] - 1);
			if (PassiveItem.ActiveFlagItems[player][GetType()] == 0)
			{
				PassiveItem.ActiveFlagItems[player].Remove(GetType());
			}
		}
		debrisObject.GetComponent<BattleStandardItem>().m_pickedUpThisRun = true;
		return debrisObject;
	}

	protected override void OnDestroy()
	{
		if ((bool)m_owner)
		{
			DisengageEffect(m_owner);
		}
		BraveTime.ClearMultiplier(base.gameObject);
		if (m_pickedUp && PassiveItem.ActiveFlagItems.ContainsKey(m_owner) && PassiveItem.ActiveFlagItems[m_owner].ContainsKey(GetType()))
		{
			PassiveItem.ActiveFlagItems[m_owner][GetType()] = Mathf.Max(0, PassiveItem.ActiveFlagItems[m_owner][GetType()] - 1);
			if (PassiveItem.ActiveFlagItems[m_owner][GetType()] == 0)
			{
				PassiveItem.ActiveFlagItems[m_owner].Remove(GetType());
			}
		}
		base.OnDestroy();
	}
}
