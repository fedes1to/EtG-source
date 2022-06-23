using System;
using System.Collections;
using Dungeonator;
using UnityEngine;

public class SprenOrbitalItem : PlayerOrbitalItem
{
	public enum SprenTrigger
	{
		UNASSIGNED,
		USED_LAST_BLANK,
		LOST_LAST_ARMOR,
		REDUCED_TO_ONE_HEALTH,
		GUN_OUT_OF_AMMO,
		SET_ON_FIRE,
		ELECTROCUTED_OR_POISONED,
		FELL_IN_PIT,
		TOOK_ANY_HEART_DAMAGE,
		FLIPPED_TABLE,
		ACTIVE_ITEM_USED
	}

	private enum SprenTransformationState
	{
		NORMAL,
		PRE_TRANSFORM,
		TRANSFORMED
	}

	[PickupIdentifier]
	public int LimitGunId = -1;

	public float LimitDuration = 15f;

	public string IdleAnimation;

	public string GunChangeAnimation;

	public string GunChangeMoreAnimation;

	public string BackchangeAnimation;

	private SprenTrigger m_trigger;

	private SprenTrigger m_secondaryTrigger;

	private PlayerController m_player;

	private Gun m_extantGun;

	private SprenTransformationState m_transformation;

	private int m_lastEquippedGunID = -1;

	private int m_lastEquippedGunAmmo = -1;

	private void Start()
	{
		AssignTrigger();
	}

	private void AssignTrigger()
	{
		if (m_trigger == SprenTrigger.UNASSIGNED)
		{
			m_trigger = (SprenTrigger)UnityEngine.Random.Range(1, 11);
		}
		if (m_secondaryTrigger == SprenTrigger.UNASSIGNED)
		{
			while (m_secondaryTrigger == SprenTrigger.UNASSIGNED || m_secondaryTrigger == m_trigger)
			{
				m_secondaryTrigger = (SprenTrigger)UnityEngine.Random.Range(1, 11);
			}
		}
	}

	private bool CheckTrigger(SprenTrigger target, bool force = false)
	{
		if (force || ((bool)m_owner && m_owner.HasActiveBonusSynergy(CustomSynergyType.SHARDBLADE) && m_secondaryTrigger == target))
		{
			return true;
		}
		return m_trigger == target;
	}

	public override void Pickup(PlayerController player)
	{
		base.Pickup(player);
		m_player = player;
		AssignTrigger();
		if (CheckTrigger(SprenTrigger.USED_LAST_BLANK, true))
		{
			player.OnUsedBlank += HandleBlank;
		}
		if (CheckTrigger(SprenTrigger.LOST_LAST_ARMOR, true))
		{
			player.LostArmor = (Action)Delegate.Combine(player.LostArmor, new Action(HandleLostArmor));
		}
		if (CheckTrigger(SprenTrigger.ELECTROCUTED_OR_POISONED, true) || CheckTrigger(SprenTrigger.TOOK_ANY_HEART_DAMAGE, true) || CheckTrigger(SprenTrigger.REDUCED_TO_ONE_HEALTH, true))
		{
			player.healthHaver.OnDamaged += HandleDamaged;
		}
		if (CheckTrigger(SprenTrigger.ACTIVE_ITEM_USED, true))
		{
			player.OnUsedPlayerItem += HandleActiveItemUsed;
		}
		if (CheckTrigger(SprenTrigger.FLIPPED_TABLE, true))
		{
			player.OnTableFlipped = (Action<FlippableCover>)Delegate.Combine(player.OnTableFlipped, new Action<FlippableCover>(HandleTableFlipped));
		}
		if (CheckTrigger(SprenTrigger.FELL_IN_PIT, true))
		{
			player.OnPitfall += HandlePitfall;
		}
		if (CheckTrigger(SprenTrigger.SET_ON_FIRE, true))
		{
			player.OnIgnited = (Action<PlayerController>)Delegate.Combine(player.OnIgnited, new Action<PlayerController>(HandleIgnited));
		}
	}

	protected override void Update()
	{
		if (m_transformation == SprenTransformationState.TRANSFORMED && (GameManager.Instance.IsLoadingLevel || Dungeon.IsGenerating || ((bool)m_player && m_player.CurrentRoom != null && m_player.CurrentRoom.IsWinchesterArcadeRoom)))
		{
			DetransformSpren();
		}
		if (m_transformation == SprenTransformationState.NORMAL && CheckTrigger(SprenTrigger.GUN_OUT_OF_AMMO) && (bool)m_player && (bool)m_player.CurrentGun)
		{
			if (!m_player.CurrentGun.InfiniteAmmo && m_player.CurrentGun.ammo <= 0 && m_player.CurrentGun.PickupObjectId == m_lastEquippedGunID && m_lastEquippedGunAmmo > 0)
			{
				TransformSpren();
			}
			m_lastEquippedGunID = m_player.CurrentGun.PickupObjectId;
			m_lastEquippedGunAmmo = m_player.CurrentGun.ammo;
		}
		base.Update();
	}

	private void HandleIgnited(PlayerController obj)
	{
		if (m_transformation == SprenTransformationState.NORMAL && CheckTrigger(SprenTrigger.SET_ON_FIRE))
		{
			TransformSpren();
		}
	}

	private void HandlePitfall()
	{
		if (m_transformation == SprenTransformationState.NORMAL && CheckTrigger(SprenTrigger.FELL_IN_PIT))
		{
			TransformSpren();
		}
	}

	private void HandleTableFlipped(FlippableCover obj)
	{
		if (m_transformation == SprenTransformationState.NORMAL && CheckTrigger(SprenTrigger.FLIPPED_TABLE))
		{
			TransformSpren();
		}
	}

	private void HandleActiveItemUsed(PlayerController arg1, PlayerItem arg2)
	{
		if (m_transformation == SprenTransformationState.NORMAL && CheckTrigger(SprenTrigger.ACTIVE_ITEM_USED))
		{
			TransformSpren();
		}
	}

	private void HandleLostArmor()
	{
		if (m_transformation == SprenTransformationState.NORMAL && CheckTrigger(SprenTrigger.LOST_LAST_ARMOR) && ((!m_player.ForceZeroHealthState && m_player.healthHaver.Armor == 0f) || (m_player.ForceZeroHealthState && m_player.healthHaver.Armor == 1f)))
		{
			TransformSpren();
		}
	}

	private void HandleDamaged(float resultValue, float maxValue, CoreDamageTypes damageTypes, DamageCategory damageCategory, Vector2 damageDirection)
	{
		if (m_transformation == SprenTransformationState.NORMAL)
		{
			if (CheckTrigger(SprenTrigger.ELECTROCUTED_OR_POISONED) && ((damageTypes | CoreDamageTypes.Electric) == damageTypes || (damageTypes | CoreDamageTypes.Poison) == damageTypes))
			{
				TransformSpren();
			}
			else if (CheckTrigger(SprenTrigger.TOOK_ANY_HEART_DAMAGE) && m_player.healthHaver.Armor == 0f)
			{
				TransformSpren();
			}
			else if (CheckTrigger(SprenTrigger.REDUCED_TO_ONE_HEALTH) && m_player.healthHaver.GetCurrentHealth() <= 0.5f)
			{
				TransformSpren();
			}
		}
	}

	private void HandleBlank(PlayerController arg1, int BlanksRemaining)
	{
		if (m_transformation == SprenTransformationState.NORMAL && CheckTrigger(SprenTrigger.USED_LAST_BLANK) && BlanksRemaining == 0)
		{
			TransformSpren();
		}
	}

	private void Disconnect(PlayerController player)
	{
		player.OnUsedBlank -= HandleBlank;
		player.LostArmor = (Action)Delegate.Remove(player.LostArmor, new Action(HandleLostArmor));
		player.healthHaver.OnDamaged -= HandleDamaged;
		player.OnUsedPlayerItem -= HandleActiveItemUsed;
		player.OnTableFlipped = (Action<FlippableCover>)Delegate.Remove(player.OnTableFlipped, new Action<FlippableCover>(HandleTableFlipped));
		player.OnPitfall -= HandlePitfall;
		player.OnIgnited = (Action<PlayerController>)Delegate.Remove(player.OnIgnited, new Action<PlayerController>(HandleIgnited));
	}

	public override DebrisObject Drop(PlayerController player)
	{
		Disconnect(player);
		return base.Drop(player);
	}

	protected void TransformSpren()
	{
		if (m_transformation == SprenTransformationState.NORMAL && (!m_player || m_player.CurrentRoom == null || !m_player.CurrentRoom.IsWinchesterArcadeRoom))
		{
			m_transformation = SprenTransformationState.PRE_TRANSFORM;
			if ((bool)m_player && !m_player.IsGhost)
			{
				m_player.StartCoroutine(HandleTransformationDuration());
			}
		}
	}

	private IEnumerator HandleTransformationDuration()
	{
		tk2dSpriteAnimator extantAnimator = m_extantOrbital.GetComponentInChildren<tk2dSpriteAnimator>();
		extantAnimator.Play(GunChangeAnimation);
		PlayerOrbitalFollower follower = m_extantOrbital.GetComponent<PlayerOrbitalFollower>();
		if ((bool)follower)
		{
			follower.OverridePosition = true;
		}
		float elapsed2 = 0f;
		extantAnimator.sprite.HeightOffGround = 5f;
		while (elapsed2 < 1f)
		{
			elapsed2 += BraveTime.DeltaTime;
			if ((bool)follower && (bool)m_player)
			{
				follower.OverrideTargetPosition = m_player.CenterPosition;
			}
			yield return null;
		}
		extantAnimator.Play(GunChangeMoreAnimation);
		while (extantAnimator.IsPlaying(GunChangeMoreAnimation))
		{
			if ((bool)follower && (bool)m_player)
			{
				follower.OverrideTargetPosition = m_player.CenterPosition;
			}
			yield return null;
		}
		if ((bool)follower)
		{
			follower.ToggleRenderer(false);
		}
		m_player.inventory.GunChangeForgiveness = true;
		m_transformation = SprenTransformationState.TRANSFORMED;
		Gun limitGun = PickupObjectDatabase.GetById(LimitGunId) as Gun;
		m_extantGun = m_player.inventory.AddGunToInventory(limitGun, true);
		m_extantGun.CanBeDropped = false;
		m_extantGun.CanBeSold = false;
		m_player.inventory.GunLocked.SetOverride("spren gun", true);
		elapsed2 = 0f;
		while (elapsed2 < LimitDuration)
		{
			if ((bool)follower && (bool)m_player)
			{
				follower.OverrideTargetPosition = m_player.CenterPosition;
			}
			elapsed2 += BraveTime.DeltaTime;
			yield return null;
		}
		if ((bool)follower)
		{
			follower.ToggleRenderer(true);
		}
		if ((bool)extantAnimator)
		{
			extantAnimator.PlayForDuration(BackchangeAnimation, -1f, IdleAnimation);
		}
		while (extantAnimator.IsPlaying(BackchangeAnimation))
		{
			if ((bool)follower && (bool)m_player)
			{
				follower.OverrideTargetPosition = m_player.CenterPosition;
			}
			yield return null;
		}
		follower.OverridePosition = false;
		DetransformSpren();
	}

	protected void DetransformSpren()
	{
		if (m_transformation != SprenTransformationState.TRANSFORMED || !this || !m_player || !m_extantGun)
		{
			return;
		}
		m_transformation = SprenTransformationState.NORMAL;
		if ((bool)m_player)
		{
			if (!GameManager.Instance.IsLoadingLevel && !Dungeon.IsGenerating)
			{
				Minimap.Instance.ToggleMinimap(false);
			}
			m_player.inventory.GunLocked.RemoveOverride("spren gun");
			m_player.inventory.DestroyGun(m_extantGun);
			m_extantGun = null;
		}
		m_player.inventory.GunChangeForgiveness = false;
	}

	protected override void OnDestroy()
	{
		if ((bool)m_player)
		{
			Disconnect(m_player);
		}
		m_player = null;
		base.OnDestroy();
	}
}
