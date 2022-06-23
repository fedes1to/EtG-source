using System;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class WingsItem : PassiveItem
{
	public GameObject prefabToAttachToPlayer;

	public string animPrefix = "white_wing";

	public bool usesCardinalAnimations;

	public bool GoopsOnRoll;

	public GoopDefinition RollGoop;

	public float RollGoopRadius = 1f;

	public bool DoesRadialBurstOnDodgeRoll;

	public RadialBurstInterface RadialBurstOnDodgeRoll;

	public bool IsCatThrone;

	[EnemyIdentifier]
	public List<string> CatThroneCharmGuids;

	public float RadialBurstCooldown = 2f;

	private float m_radialBurstCooldown;

	public GameActorCharmEffect CatCharmEffect;

	private GameObject instanceWings;

	private tk2dSprite instanceWingsSprite;

	private bool m_isCurrentlyActive;

	private bool m_hiddenForFall;

	private bool wasRolling;

	private Vector2 GetLocalOffsetForCharacter(PlayableCharacters character)
	{
		switch (character)
		{
		case PlayableCharacters.Bullet:
			return new Vector2(-0.5625f, -15f / 32f);
		case PlayableCharacters.Convict:
			return new Vector2(-0.625f, -0.5f);
		case PlayableCharacters.Guide:
			return new Vector2(-0.5625f, -0.5f);
		case PlayableCharacters.Pilot:
			return new Vector2(-0.5625f, -0.5f);
		case PlayableCharacters.Robot:
			return new Vector2(-0.5625f, -0.5f);
		case PlayableCharacters.Soldier:
			return new Vector2(-0.5f, -0.5f);
		default:
			return new Vector2(-0.5625f, -0.5f);
		}
	}

	protected override void Update()
	{
		base.Update();
		if (!(m_owner != null) || !m_pickedUp)
		{
			return;
		}
		m_radialBurstCooldown -= BraveTime.DeltaTime;
		if (IsCatThrone && wasRolling && !m_owner.IsDodgeRolling)
		{
			m_owner.IsVisible = true;
			wasRolling = false;
		}
		if (m_isCurrentlyActive)
		{
			if (m_owner.IsFalling)
			{
				m_hiddenForFall = true;
				instanceWingsSprite.renderer.enabled = false;
			}
			else
			{
				if (m_hiddenForFall)
				{
					m_hiddenForFall = false;
					instanceWingsSprite.renderer.enabled = true;
				}
				string text = animPrefix + m_owner.GetBaseAnimationSuffix(usesCardinalAnimations);
				if (!instanceWingsSprite.spriteAnimator.IsPlaying(text) && (!IsCatThrone || !m_owner.IsDodgeRolling))
				{
					instanceWingsSprite.spriteAnimator.Play(text);
				}
				if (GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.END_TIMES)
				{
					DisengageEffect(m_owner);
				}
			}
		}
		else if (GameManager.Instance.CurrentLevelOverrideState != GameManager.LevelOverrideState.END_TIMES)
		{
			EngageEffect(m_owner);
		}
		if (IsCatThrone && (bool)m_owner && m_owner.HasActiveBonusSynergy(CustomSynergyType.TRUE_CAT_KING) && m_owner.CurrentRoom != null)
		{
			List<AIActor> activeEnemies = m_owner.CurrentRoom.GetActiveEnemies(RoomHandler.ActiveEnemyType.All);
			ProcessEnemies(activeEnemies);
		}
	}

	private void DoGoop()
	{
		DeadlyDeadlyGoopManager.GetGoopManagerForGoopType(RollGoop).AddGoopCircle(m_owner.specRigidbody.UnitCenter, RollGoopRadius);
	}

	private void OnRollFrame(PlayerController obj)
	{
		if (GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.END_TIMES)
		{
			return;
		}
		if (GoopsOnRoll)
		{
			DoGoop();
		}
		if (IsCatThrone)
		{
			wasRolling = true;
			obj.IsVisible = false;
			instanceWingsSprite.renderer.enabled = true;
			if (!instanceWingsSprite.spriteAnimator.IsPlaying("cat_throne_spin"))
			{
				instanceWingsSprite.spriteAnimator.Play("cat_throne_spin");
			}
		}
	}

	public override void Pickup(PlayerController player)
	{
		if (!m_pickedUp)
		{
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
			if (GoopsOnRoll || IsCatThrone)
			{
				player.OnIsRolling += OnRollFrame;
			}
			if (DoesRadialBurstOnDodgeRoll)
			{
				player.OnRollStarted += HandleRollStarted;
			}
			EngageEffect(player);
			base.Pickup(player);
		}
	}

	private void HandleRollStarted(PlayerController p, Vector2 rollDirection)
	{
		if (DoesRadialBurstOnDodgeRoll && m_radialBurstCooldown <= 0f)
		{
			m_radialBurstCooldown = RadialBurstCooldown;
			RadialBurstOnDodgeRoll.DoBurst(p, null, Vector2.up * 0.625f);
		}
	}

	public override DebrisObject Drop(PlayerController player)
	{
		DebrisObject debrisObject = base.Drop(player);
		if (PassiveItem.ActiveFlagItems[player].ContainsKey(GetType()))
		{
			PassiveItem.ActiveFlagItems[player][GetType()] = Mathf.Max(0, PassiveItem.ActiveFlagItems[player][GetType()] - 1);
			if (PassiveItem.ActiveFlagItems[player][GetType()] == 0)
			{
				PassiveItem.ActiveFlagItems[player].Remove(GetType());
			}
		}
		player.OnIsRolling -= OnRollFrame;
		player.OnRollStarted -= HandleRollStarted;
		DisengageEffect(player);
		debrisObject.GetComponent<WingsItem>().m_pickedUpThisRun = true;
		return debrisObject;
	}

	protected override void OnDestroy()
	{
		if (m_pickedUp)
		{
			if (PassiveItem.ActiveFlagItems.ContainsKey(m_owner) && PassiveItem.ActiveFlagItems[m_owner].ContainsKey(GetType()))
			{
				PassiveItem.ActiveFlagItems[m_owner][GetType()] = Mathf.Max(0, PassiveItem.ActiveFlagItems[m_owner][GetType()] - 1);
				if (PassiveItem.ActiveFlagItems[m_owner][GetType()] == 0)
				{
					PassiveItem.ActiveFlagItems[m_owner].Remove(GetType());
				}
			}
			m_owner.OnIsRolling -= OnRollFrame;
			m_owner.OnRollStarted -= HandleRollStarted;
			DisengageEffect(m_owner);
		}
		base.OnDestroy();
	}

	protected void EngageEffect(PlayerController user)
	{
		if (!Dungeon.IsGenerating && (bool)user && (bool)user.sprite && (bool)user.sprite.GetComponent<tk2dSpriteAttachPoint>())
		{
			m_isCurrentlyActive = true;
			user.SetIsFlying(true, "wings");
			instanceWings = user.RegisterAttachedObject(prefabToAttachToPlayer, "jetpack", 0.1f);
			instanceWingsSprite = instanceWings.GetComponent<tk2dSprite>();
			if (!instanceWingsSprite)
			{
				instanceWingsSprite = instanceWings.GetComponentInChildren<tk2dSprite>();
			}
			if (usesCardinalAnimations)
			{
				instanceWingsSprite.transform.localPosition = GetLocalOffsetForCharacter(user.characterIdentity).ToVector3ZUp();
			}
		}
	}

	private void ProcessEnemies(List<AIActor> enemies)
	{
		if (enemies == null)
		{
			return;
		}
		for (int i = 0; i < enemies.Count; i++)
		{
			if ((bool)enemies[i] && enemies[i].GetEffect(CatCharmEffect.effectIdentifier) == null && CatThroneCharmGuids.Contains(enemies[i].EnemyGuid))
			{
				enemies[i].ApplyEffect(CatCharmEffect);
			}
		}
	}

	protected void DisengageEffect(PlayerController user)
	{
		m_isCurrentlyActive = false;
		user.SetIsFlying(false, "wings");
		user.DeregisterAttachedObject(instanceWings);
		instanceWingsSprite = null;
		if (IsCatThrone && wasRolling)
		{
			user.IsVisible = true;
			wasRolling = false;
		}
	}
}
