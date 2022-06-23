using System;
using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class CompanionController : BraveBehaviour
{
	public enum CompanionIdentifier
	{
		NONE,
		SUPER_SPACE_TURTLE,
		PAYDAY_SHOOT,
		PAYDAY_BLOCK,
		PAYDAY_STUN,
		BABY_GOOD_MIMIC,
		PHOENIX,
		PIG,
		SHELLETON,
		GATLING_GULL
	}

	public bool CanInterceptBullets;

	public bool IsCopDead;

	public bool IsCop;

	public StatModifier CopDeathStatModifier;

	public int CurseOnCopDeath = 2;

	public bool CanCrossPits;

	public bool BlanksOnActiveItemUsed;

	public float InternalBlankCooldown = 10f;

	public bool HasStealthMode;

	public bool PredictsChests;

	[LongNumericEnum]
	public CustomSynergyType PredictsChestSynergy;

	public bool CanBePet;

	public CompanionIdentifier companionID;

	public HeatRingModule TeaSynergyHeatRing;

	protected PlayerController m_owner;

	protected Chest m_lastPredictedChest;

	protected HologramDoer m_hologram;

	protected float m_internalBlankCooldown;

	protected CellType m_lastCellType = CellType.FLOOR;

	protected Vector2 m_cachedRollDirection;

	protected bool m_isStealthed;

	protected float m_timeInDeadlyRoom;

	private bool m_hasDoneJunkanCheck;

	private bool m_hasAttemptedSynergy;

	private bool m_hasLuteBuff;

	private GameObject m_luteOverheadVfx;

	private bool m_isMimicTransforming;

	public PlayerController m_pettingDoer;

	public Vector2 m_petOffset;

	public bool IsBeingPet
	{
		get
		{
			return m_pettingDoer != null;
		}
	}

	private IEnumerator HandleDelayedInitialization()
	{
		yield return null;
		if (CanCrossPits)
		{
			base.aiActor.PathableTiles = base.aiActor.PathableTiles | CellTypes.PIT;
			base.aiActor.SetIsFlying(true, "innate", false);
		}
	}

	public void Initialize(PlayerController owner)
	{
		m_owner = owner;
		base.gameActor.ImmuneToAllEffects = true;
		base.aiActor.SetResistance(EffectResistanceType.Poison, 1f);
		base.aiActor.SetResistance(EffectResistanceType.Fire, 1f);
		base.aiActor.SetResistance(EffectResistanceType.Freeze, 1f);
		base.aiActor.SetResistance(EffectResistanceType.Charm, 1f);
		base.aiActor.specRigidbody.AddCollisionLayerIgnoreOverride(CollisionMask.LayerToMask(CollisionLayer.PlayerHitBox, CollisionLayer.PlayerCollider));
		if (companionID == CompanionIdentifier.GATLING_GULL)
		{
			base.aiActor.specRigidbody.AddCollisionLayerOverride(CollisionMask.LayerToMask(CollisionLayer.EnemyCollider));
		}
		base.aiActor.IsNormalEnemy = false;
		base.aiActor.CompanionOwner = m_owner;
		base.aiActor.CanTargetPlayers = false;
		base.aiActor.CanTargetEnemies = true;
		base.aiActor.CustomPitDeathHandling += CustomPitDeathHandling;
		base.aiActor.State = AIActor.ActorState.Normal;
		SpeculativeRigidbody speculativeRigidbody = base.specRigidbody;
		speculativeRigidbody.OnPreRigidbodyCollision = (SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate)Delegate.Combine(speculativeRigidbody.OnPreRigidbodyCollision, new SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate(HandlePreCollision));
		if ((bool)base.bulletBank)
		{
			AIBulletBank aIBulletBank = base.bulletBank;
			aIBulletBank.OnProjectileCreated = (Action<Projectile>)Delegate.Combine(aIBulletBank.OnProjectileCreated, new Action<Projectile>(MarkNondamaging));
		}
		if (!CanInterceptBullets)
		{
			base.specRigidbody.HitboxPixelCollider.IsTrigger = true;
			base.specRigidbody.HitboxPixelCollider.CollisionLayerIgnoreOverride |= CollisionMask.LayerToMask(CollisionLayer.Projectile);
		}
		if (IsCop)
		{
			base.healthHaver.ManualDeathHandling = true;
			base.healthHaver.OnPreDeath += HandleCopDeath;
			HealthHaver obj = base.healthHaver;
			obj.ModifyDamage = (Action<HealthHaver, HealthHaver.ModifyDamageEventArgs>)Delegate.Combine(obj.ModifyDamage, new Action<HealthHaver, HealthHaver.ModifyDamageEventArgs>(HandleCopModifyDamage));
		}
		if (PredictsChests)
		{
			m_hologram = GetComponentInChildren<HologramDoer>();
		}
		if ((bool)base.bulletBank)
		{
			AIBulletBank aIBulletBank2 = base.bulletBank;
			aIBulletBank2.OnProjectileCreated = (Action<Projectile>)Delegate.Combine(aIBulletBank2.OnProjectileCreated, new Action<Projectile>(HandleCompanionPostProcessProjectile));
		}
		if ((bool)base.aiShooter)
		{
			AIShooter aIShooter = base.aiShooter;
			aIShooter.PostProcessProjectile = (Action<Projectile>)Delegate.Combine(aIShooter.PostProcessProjectile, new Action<Projectile>(HandleCompanionPostProcessProjectile));
		}
		if (BlanksOnActiveItemUsed)
		{
			owner.OnUsedPlayerItem += HandleItemUsed;
		}
		owner.OnPitfall += HandlePitfall;
		owner.OnRoomClearEvent += HandleRoomCleared;
		owner.companions.Add(base.aiActor);
		StartCoroutine(HandleDelayedInitialization());
	}

	private void HandleCopModifyDamage(HealthHaver source, HealthHaver.ModifyDamageEventArgs args)
	{
		if (args != EventArgs.Empty && (bool)m_owner && m_owner.HasActiveBonusSynergy(CustomSynergyType.COP_VEST))
		{
			args.ModifiedDamage /= 2f;
		}
	}

	protected virtual void HandleRoomCleared(PlayerController callingPlayer)
	{
	}

	protected void MarkNondamaging(Projectile obj)
	{
		if ((bool)obj)
		{
			obj.collidesWithPlayer = false;
		}
	}

	protected void HandlePitfall()
	{
	}

	protected void HandleCompanionPostProcessProjectile(Projectile obj)
	{
		if ((bool)obj)
		{
			obj.collidesWithPlayer = false;
			obj.TreatedAsNonProjectileForChallenge = true;
		}
		if ((bool)m_owner)
		{
			if (PassiveItem.IsFlagSetForCharacter(m_owner, typeof(BattleStandardItem)))
			{
				obj.baseData.damage *= BattleStandardItem.BattleStandardCompanionDamageMultiplier;
			}
			if ((bool)m_owner.CurrentGun && m_owner.CurrentGun.LuteCompanionBuffActive)
			{
				obj.baseData.damage *= 2f;
				obj.RuntimeUpdateScale(1f / obj.AdditionalScaleMultiplier);
				obj.RuntimeUpdateScale(1.75f);
			}
			m_owner.DoPostProcessProjectile(obj);
		}
	}

	protected void HandleItemUsed(PlayerController arg1, PlayerItem arg2)
	{
		if (arg1.HasActiveBonusSynergy(CustomSynergyType.ELDER_AND_YOUNGER))
		{
			GameManager.Instance.Dungeon.StartCoroutine(HandleDelayedBlank(arg1));
		}
		else if (m_internalBlankCooldown <= 0f)
		{
			Vector2? overrideCenter = base.sprite.WorldCenter;
			arg1.ForceBlank(25f, 0.5f, false, true, overrideCenter);
			m_internalBlankCooldown = InternalBlankCooldown;
		}
	}

	private IEnumerator HandleDelayedBlank(PlayerController arg1)
	{
		yield return new WaitForSeconds(1f);
		if (m_internalBlankCooldown <= 0f)
		{
			Vector2? overrideCenter = base.sprite.WorldCenter;
			arg1.ForceBlank(25f, 0.5f, false, true, overrideCenter);
			m_internalBlankCooldown = InternalBlankCooldown;
		}
	}

	protected void HandleCopDeath(Vector2 obj)
	{
		StartCoroutine(HandleCopDeath_CR());
	}

	public virtual void Update()
	{
		if (!GameManager.Instance || GameManager.Instance.IsLoadingLevel)
		{
			return;
		}
		if (IsBeingPet && (!m_pettingDoer || m_pettingDoer.m_pettingTarget != this || !base.aiAnimator.IsPlaying("pet") || Vector2.Distance(base.specRigidbody.UnitCenter, m_pettingDoer.specRigidbody.UnitCenter) > 3f))
		{
			StopPet();
		}
		if (!m_hasDoneJunkanCheck)
		{
			if (m_owner.companions.Count >= 2)
			{
				int num = 0;
				for (int i = 0; i < m_owner.companions.Count; i++)
				{
					if ((bool)m_owner.companions[i])
					{
						num++;
					}
				}
				if (num >= 2)
				{
					GameStatsManager.Instance.SetFlag(GungeonFlags.ITEMSPECIFIC_SER_JUNKAN_MAXLVL, true);
				}
			}
			m_hasDoneJunkanCheck = true;
		}
		if (m_internalBlankCooldown > 0f)
		{
			m_internalBlankCooldown -= BraveTime.DeltaTime;
		}
		if (BlanksOnActiveItemUsed && (bool)m_owner && m_owner.HasActiveBonusSynergy(CustomSynergyType.MY_LITTLE_FRIEND))
		{
			if (!m_hasAttemptedSynergy && (bool)m_owner.CurrentGun && m_owner.CurrentGun.ClipShotsRemaining == 0)
			{
				m_hasAttemptedSynergy = true;
				if (UnityEngine.Random.value < 0.25f)
				{
					HandleItemUsed(m_owner, null);
				}
			}
			else if (m_hasAttemptedSynergy && (bool)m_owner.CurrentGun && m_owner.CurrentGun.ClipShotsRemaining != 0)
			{
				m_hasAttemptedSynergy = false;
			}
		}
		if (companionID == CompanionIdentifier.SUPER_SPACE_TURTLE && (bool)m_owner && m_owner.HasActiveBonusSynergy(CustomSynergyType.OUTER_TURTLE) && !base.aiActor.IsBlackPhantom)
		{
			base.aiActor.BecomeBlackPhantom();
		}
		if (HasStealthMode && (bool)m_owner)
		{
			if (m_owner.IsStealthed && !m_isStealthed)
			{
				m_isStealthed = true;
				base.aiAnimator.IdleAnimation.AnimNames[0] = "sst_dis_idle_right";
				base.aiAnimator.IdleAnimation.AnimNames[1] = "sst_dis_idle_left";
				base.aiAnimator.MoveAnimation.AnimNames[0] = "sst_dis_move_right";
				base.aiAnimator.MoveAnimation.AnimNames[1] = "sst_dis_move_left";
			}
			else if (!m_owner.IsStealthed && m_isStealthed)
			{
				m_isStealthed = false;
				base.aiAnimator.IdleAnimation.AnimNames[0] = "sst_idle_right";
				base.aiAnimator.IdleAnimation.AnimNames[1] = "sst_idle_left";
				base.aiAnimator.MoveAnimation.AnimNames[0] = "sst_move_right";
				base.aiAnimator.MoveAnimation.AnimNames[1] = "sst_move_left";
			}
		}
		if ((bool)m_owner && (bool)base.sprite && (bool)m_owner.CurrentGun && m_owner.CurrentGun.IsReloading && m_owner.HasActiveBonusSynergy(CustomSynergyType.TEA_FOR_TWO))
		{
			AuraOnReloadModifier component = m_owner.CurrentGun.GetComponent<AuraOnReloadModifier>();
			if (TeaSynergyHeatRing == null)
			{
				TeaSynergyHeatRing = new HeatRingModule();
			}
			if (TeaSynergyHeatRing != null && !TeaSynergyHeatRing.IsActive && (bool)component && component.IgnitesEnemies && (bool)m_owner && (bool)m_owner.CurrentGun && (bool)base.sprite)
			{
				TeaSynergyHeatRing.Trigger(component.AuraRadius, m_owner.CurrentGun.reloadTime, component.IgniteEffect, base.sprite);
			}
		}
		if ((bool)m_owner && companionID == CompanionIdentifier.SHELLETON)
		{
			bool flag = m_owner.HasActiveBonusSynergy(CustomSynergyType.BIRTHRIGHT);
			ShootBehavior shootBehavior = (ShootBehavior)base.behaviorSpeculator.AttackBehaviors[1];
			shootBehavior.IsBlackPhantom = !flag;
			flag = m_owner.HasActiveBonusSynergy(CustomSynergyType.SHELL_A_TON);
			base.behaviorSpeculator.LocalTimeScale = ((!flag) ? 1f : 2f);
		}
		if (IsCop && GameManager.Instance.Dungeon.tileIndices.tilesetId == GlobalDungeonData.ValidTilesets.FORGEGEON)
		{
			CellData cell = base.transform.position.GetCell();
			if (base.transform.position.GetAbsoluteRoom().area.PrototypeRoomCategory == PrototypeDungeonRoom.RoomCategory.BOSS)
			{
				base.healthHaver.ApplyDamage(1000000f, Vector2.zero, "Inevitability", CoreDamageTypes.None, DamageCategory.Unstoppable);
			}
			else if (base.transform.position.GetAbsoluteRoom().HasActiveEnemies(RoomHandler.ActiveEnemyType.RoomClear) && m_owner.CurrentRoom.distanceFromEntrance > 1 && (cell == null || !cell.isExitCell))
			{
				if (m_timeInDeadlyRoom > 5f && Vector2.Distance(m_owner.CenterPosition, base.transform.position.XY()) < 12f)
				{
					base.healthHaver.ApplyDamage(1000000f, Vector2.zero, "Inevitability", CoreDamageTypes.None, DamageCategory.Unstoppable);
				}
				else
				{
					m_timeInDeadlyRoom += BraveTime.DeltaTime;
				}
			}
			else
			{
				m_timeInDeadlyRoom = 0f;
			}
		}
		IntVector2 intVector = base.specRigidbody.UnitCenter.ToIntVector2(VectorConversions.Floor);
		if (GameManager.Instance.Dungeon.data.CheckInBounds(intVector))
		{
			CellData cellData = GameManager.Instance.Dungeon.data[intVector];
			if (cellData != null)
			{
				m_lastCellType = cellData.type;
			}
		}
		if (PredictsChests && (bool)m_owner && m_owner.HasActiveBonusSynergy(PredictsChestSynergy))
		{
			Chest chest = null;
			float num2 = float.MaxValue;
			for (int j = 0; j < StaticReferenceManager.AllChests.Count; j++)
			{
				Chest chest2 = StaticReferenceManager.AllChests[j];
				if ((bool)chest2 && (bool)chest2.sprite && !chest2.IsOpen && !chest2.IsBroken && !chest2.IsSealed)
				{
					float num3 = Vector2.Distance(m_owner.CenterPosition, chest2.sprite.WorldCenter);
					if (num3 < num2)
					{
						chest = chest2;
						num2 = num3;
					}
				}
			}
			if (num2 > 5f)
			{
				chest = null;
			}
			if (m_lastPredictedChest != chest)
			{
				if ((bool)m_lastPredictedChest)
				{
					m_hologram.HideSprite(base.gameObject);
				}
				if ((bool)chest)
				{
					List<PickupObject> list = chest.PredictContents(m_owner);
					if (list.Count > 0 && (bool)list[0].encounterTrackable)
					{
						tk2dSpriteCollectionData encounterIconCollection = AmmonomiconController.ForceInstance.EncounterIconCollection;
						m_hologram.ChangeToSprite(base.gameObject, encounterIconCollection, encounterIconCollection.GetSpriteIdByName(list[0].encounterTrackable.journalData.AmmonomiconSprite));
					}
				}
				m_lastPredictedChest = chest;
			}
		}
		else if ((bool)m_hologram && m_hologram.ArcRenderer.enabled)
		{
			m_hologram.HideSprite(base.gameObject, true);
		}
		if (companionID == CompanionIdentifier.BABY_GOOD_MIMIC && !m_isMimicTransforming)
		{
			HandleBabyGoodMimic();
		}
		if (!m_owner || !m_owner.CurrentGun || !base.aiActor)
		{
			return;
		}
		if (m_hasLuteBuff && !m_owner.CurrentGun.LuteCompanionBuffActive)
		{
			if ((bool)m_luteOverheadVfx)
			{
				UnityEngine.Object.Destroy(m_luteOverheadVfx);
				m_luteOverheadVfx = null;
			}
			m_hasLuteBuff = false;
		}
		else if (!m_hasLuteBuff && m_owner.CurrentGun.LuteCompanionBuffActive)
		{
			GameObject effect = (GameObject)ResourceCache.Acquire("Global VFX/VFX_Buff_Status");
			m_luteOverheadVfx = base.aiActor.PlayEffectOnActor(effect, new Vector3(0f, 1.25f, 0f), true, true);
			m_hasLuteBuff = true;
		}
	}

	protected void HandleBabyGoodMimic()
	{
		if (!m_owner)
		{
			return;
		}
		CompanionItem companionItem = null;
		string text = string.Empty;
		for (int i = 0; i < m_owner.passiveItems.Count; i++)
		{
			if (m_owner.passiveItems[i] is CompanionItem)
			{
				companionItem = m_owner.passiveItems[i] as CompanionItem;
				if (!(companionItem.ExtantCompanion != base.gameObject))
				{
					break;
				}
				companionItem = null;
			}
		}
		for (int j = 0; j < m_owner.companions.Count; j++)
		{
			CompanionController component = m_owner.companions[j].GetComponent<CompanionController>();
			if ((!component || component.companionID != CompanionIdentifier.GATLING_GULL) && (!component || component.companionID != CompanionIdentifier.BABY_GOOD_MIMIC))
			{
				text = m_owner.companions[j].EnemyGuid;
				break;
			}
		}
		PlayerOrbitalItem playerOrbitalItem = null;
		if (string.IsNullOrEmpty(text))
		{
			for (int k = 0; k < m_owner.passiveItems.Count; k++)
			{
				if (m_owner.passiveItems[k] is PlayerOrbitalItem)
				{
					PlayerOrbitalItem playerOrbitalItem2 = m_owner.passiveItems[k] as PlayerOrbitalItem;
					if (playerOrbitalItem2.CanBeMimicked)
					{
						playerOrbitalItem = playerOrbitalItem2;
						break;
					}
				}
			}
		}
		if ((bool)companionItem && (!string.IsNullOrEmpty(text) || playerOrbitalItem != null))
		{
			StartCoroutine(HandleBabyMimicTransform(companionItem, text, playerOrbitalItem));
		}
	}

	private IEnumerator HandleBabyMimicTransform(CompanionItem sourceItem, string targetGuid, PlayerOrbitalItem orbitalItemTarget = null)
	{
		m_isMimicTransforming = true;
		base.behaviorSpeculator.enabled = false;
		base.aiAnimator.PlayUntilFinished("transform");
		yield return new WaitForSeconds(1.4f);
		Vector2 sourcePosition = base.transform.position;
		sourceItem.ForceDisconnectCompanion();
		if (string.IsNullOrEmpty(targetGuid) && (bool)orbitalItemTarget)
		{
			sourceItem.BabyGoodMimicOrbitalOverridden = true;
			sourceItem.OverridePlayerOrbitalItem = orbitalItemTarget;
		}
		else
		{
			sourceItem.CompanionGuid = targetGuid;
			sourceItem.CompanionPastGuid = string.Empty;
		}
		sourceItem.UsesAlternatePastPrefab = false;
		sourceItem.ForceCompanionRegeneration(m_owner, sourcePosition);
		yield return new WaitForSeconds(0.5f);
		UnityEngine.Object.Destroy(base.gameObject);
	}

	protected bool PlayerRoomHasActiveEnemies()
	{
		bool flag = base.transform.position.GetAbsoluteRoom().HasActiveEnemies(RoomHandler.ActiveEnemyType.All);
		if (!flag)
		{
			flag = GameManager.Instance.PrimaryPlayer.CurrentRoom.HasActiveEnemies(RoomHandler.ActiveEnemyType.All);
		}
		return flag;
	}

	private IEnumerator HandleCopDeath_CR()
	{
		IsCopDead = true;
		for (int i = 0; i < m_owner.passiveItems.Count; i++)
		{
			if (m_owner.passiveItems[i] is CompanionItem)
			{
				CompanionItem companionItem = m_owner.passiveItems[i] as CompanionItem;
				if ((bool)companionItem && companionItem.CompanionGuid == base.aiActor.EnemyGuid)
				{
					companionItem.PreventRespawnOnFloorLoad = true;
				}
			}
		}
		m_owner.companions.Remove(base.aiActor);
		base.sprite.HeightOffGround = -1f;
		base.sprite.UpdateZDepth();
		base.aiAnimator.PlayUntilCancelled("die");
		if ((bool)base.knockbackDoer)
		{
			base.knockbackDoer.SetImmobile(true, "dying");
		}
		bool playerRoomHasActiveEnemies = true;
		while (playerRoomHasActiveEnemies)
		{
			yield return null;
			playerRoomHasActiveEnemies = PlayerRoomHasActiveEnemies();
			if (!playerRoomHasActiveEnemies)
			{
				while (GameManager.Instance.MainCameraController.ManualControl)
				{
					yield return null;
				}
				yield return new WaitForSeconds(0.5f);
				playerRoomHasActiveEnemies = PlayerRoomHasActiveEnemies();
			}
			if (GameManager.Instance.Dungeon.tileIndices.tilesetId == GlobalDungeonData.ValidTilesets.FORGEGEON && GameManager.Instance.PrimaryPlayer.CurrentRoom.area.PrototypeRoomCategory == PrototypeDungeonRoom.RoomCategory.BOSS)
			{
				break;
			}
		}
		yield return null;
		if (Vector2.Distance(m_owner.CenterPosition, base.sprite.WorldCenter) < 15f)
		{
			GetDungeonFSM().SendEvent("copdeath");
			yield return null;
		}
		GameObject instanceIcon = Minimap.Instance.RegisterRoomIcon(base.talkDoer.transform.position.GetAbsoluteRoom(), ResourceCache.Acquire("Global Prefabs/Minimap_NPC_Icon") as GameObject);
		bool didBreak = false;
		while (!base.talkDoer.IsTalking)
		{
			yield return null;
		}
		Minimap.Instance.DeregisterRoomIcon(base.talkDoer.transform.position.GetAbsoluteRoom(), instanceIcon);
		if (!didBreak)
		{
			while (base.talkDoer.IsTalking)
			{
				yield return null;
			}
		}
		if (didBreak)
		{
			while ((bool)m_owner && m_owner.IsInCombat)
			{
				yield return null;
			}
		}
		m_owner.ownerlessStatModifiers.Add(CopDeathStatModifier);
		StatModifier curseMod = new StatModifier
		{
			statToBoost = PlayerStats.StatType.Curse,
			modifyType = StatModifier.ModifyMethod.ADDITIVE,
			amount = CurseOnCopDeath
		};
		m_owner.ownerlessStatModifiers.Add(curseMod);
		m_owner.stats.RecalculateStats(m_owner);
		yield return new WaitForSeconds(0.25f);
		string header = StringTableManager.GetString("#COP_REVENGE_HEADER");
		string body = StringTableManager.GetString("#COP_REVENGE_BODY");
		GameUIRoot.Instance.notificationController.DoCustomNotification(header, body, base.sprite.Collection, base.sprite.spriteId, UINotificationController.NotificationColor.GOLD);
		base.healthHaver.DeathAnimationComplete(null, null);
		for (int j = 0; j < m_owner.passiveItems.Count; j++)
		{
			if (m_owner.passiveItems[j] is CompanionItem)
			{
				CompanionItem companionItem2 = m_owner.passiveItems[j] as CompanionItem;
				if ((bool)companionItem2 && companionItem2.CompanionGuid == base.aiActor.EnemyGuid)
				{
					m_owner.RemovePassiveItem(companionItem2.PickupObjectId);
					break;
				}
			}
		}
	}

	protected void HandlePreCollision(SpeculativeRigidbody myRigidbody, PixelCollider myPixelCollider, SpeculativeRigidbody otherRigidbody, PixelCollider otherPixelCollider)
	{
		if (otherRigidbody.transform.parent != null && ((bool)otherRigidbody.transform.parent.GetComponent<DungeonDoorController>() || (bool)otherRigidbody.transform.parent.GetComponent<DungeonDoorSubsidiaryBlocker>()))
		{
			PhysicsEngine.SkipCollision = true;
		}
		if (IsCop && IsCopDead)
		{
			PhysicsEngine.SkipCollision = true;
		}
		if (GameManager.Instance.IsFoyer && (bool)otherRigidbody.GetComponent<TalkDoerLite>())
		{
			PhysicsEngine.SkipCollision = true;
		}
		if (companionID == CompanionIdentifier.GATLING_GULL && (bool)otherRigidbody.aiActor && (bool)otherRigidbody.healthHaver && !otherRigidbody.healthHaver.IsBoss)
		{
			PhysicsEngine.SkipCollision = true;
		}
	}

	private void CustomPitDeathHandling(AIActor actor, ref bool suppressDamage)
	{
		suppressDamage = true;
		if ((bool)m_owner && m_owner.IsInMinecart)
		{
			StartCoroutine(DelayedPitReturn());
			return;
		}
		base.transform.position = m_owner.transform.position;
		base.specRigidbody.Reinitialize();
		base.aiActor.RecoverFromFall();
	}

	private IEnumerator DelayedPitReturn()
	{
		while (m_owner.IsInMinecart)
		{
			yield return null;
		}
		base.transform.position = m_owner.transform.position;
		base.specRigidbody.Reinitialize();
		base.aiActor.RecoverFromFall();
	}

	private IEnumerator ScoopPlayerToSafety()
	{
		RoomHandler currentRoom = m_owner.CurrentRoom;
		if (currentRoom.area.PrototypeRoomNormalSubcategory != PrototypeDungeonRoom.RoomNormalSubCategory.TRAP)
		{
			yield break;
		}
		bool hasFoundExit = false;
		float maxDistance = float.MinValue;
		IntVector2 mostDistantExit = IntVector2.NegOne;
		for (int i = 0; i < currentRoom.connectedRooms.Count; i++)
		{
			PrototypeRoomExit exitConnectedToRoom = currentRoom.GetExitConnectedToRoom(currentRoom.connectedRooms[i]);
			if (exitConnectedToRoom != null)
			{
				IntVector2 intVector = exitConnectedToRoom.GetExitAttachPoint() - IntVector2.One;
				IntVector2 intVector2 = intVector + currentRoom.area.basePosition + DungeonData.GetIntVector2FromDirection(exitConnectedToRoom.exitDirection);
				hasFoundExit = true;
				float num = Vector2.Distance(m_owner.CenterPosition, intVector2.ToCenterVector2());
				if (num > maxDistance)
				{
					maxDistance = num;
					mostDistantExit = intVector2;
				}
			}
		}
		if (!hasFoundExit)
		{
			yield break;
		}
		CompanionFollowPlayerBehavior followBehavior = base.behaviorSpeculator.MovementBehaviors[0] as CompanionFollowPlayerBehavior;
		followBehavior.TemporarilyDisabled = true;
		base.aiActor.ClearPath();
		base.sprite.SpriteChanged += UpdatePlayerPosition;
		base.aiAnimator.PlayUntilFinished("grab");
		yield return null;
		Transform attachPoint = base.transform.Find("carry");
		while (base.aiAnimator.IsPlaying("grab"))
		{
			Vector2 preferredPrimaryPosition = attachPoint.position.XY() + (m_owner.transform.position.XY() - m_owner.sprite.WorldBottomCenter) + new Vector2(0f, -0.125f);
			m_owner.transform.position = preferredPrimaryPosition;
			m_owner.specRigidbody.Reinitialize();
			yield return null;
		}
		float cachedSpeed = base.aiActor.MovementSpeed;
		base.aiActor.MovementSpeed = 12f;
		m_owner.SetInputOverride("raccoon");
		m_owner.SetIsFlying(true, "raccoon");
		m_owner.IsEthereal = true;
		m_owner.healthHaver.IsVulnerable = false;
		base.aiActor.PathableTiles = CellTypes.FLOOR | CellTypes.PIT;
		base.aiActor.PathfindToPosition(mostDistantExit.ToVector2());
		base.aiAnimator.PlayUntilCancelled("carry", true);
		while (!base.aiActor.PathComplete)
		{
			if ((bool)m_owner)
			{
				Vector2 vector = attachPoint.position.XY() + (m_owner.transform.position.XY() - m_owner.sprite.WorldBottomCenter) + new Vector2(0f, -0.125f);
				m_owner.transform.position = vector;
				m_owner.specRigidbody.Reinitialize();
			}
			yield return null;
		}
		base.sprite.SpriteChanged -= UpdatePlayerPosition;
		base.aiActor.MovementSpeed = cachedSpeed;
		m_owner.healthHaver.IsVulnerable = true;
		m_owner.SetIsFlying(false, "raccoon");
		m_owner.ClearInputOverride("raccoon");
		m_owner.IsEthereal = false;
		base.aiActor.PathableTiles = CellTypes.FLOOR;
		followBehavior.TemporarilyDisabled = false;
	}

	private void UpdatePlayerPosition(tk2dBaseSprite obj)
	{
		if ((bool)m_owner && (bool)obj)
		{
			Transform transform = obj.transform.Find("carry");
			if ((bool)transform)
			{
				Vector2 vector = transform.position.XY() + (m_owner.transform.position.XY() - m_owner.sprite.WorldBottomCenter) + new Vector2(0f, -0.125f);
				m_owner.transform.position = vector;
				m_owner.specRigidbody.Reinitialize();
			}
		}
	}

	public void DoPet(PlayerController player)
	{
		base.aiAnimator.LockFacingDirection = true;
		if (base.specRigidbody.UnitCenter.x > player.specRigidbody.UnitCenter.x)
		{
			base.aiAnimator.FacingDirection = 180f;
			m_petOffset = new Vector2(0.3125f, -0.625f);
		}
		else
		{
			base.aiAnimator.FacingDirection = 0f;
			m_petOffset = new Vector2(-0.8125f, -0.625f);
		}
		base.aiAnimator.PlayUntilCancelled("pet");
		m_pettingDoer = player;
	}

	public void StopPet()
	{
		if (m_pettingDoer != null)
		{
			base.aiAnimator.EndAnimationIf("pet");
			base.aiAnimator.LockFacingDirection = false;
			m_pettingDoer = null;
		}
	}

	protected override void OnDestroy()
	{
		if ((bool)m_owner)
		{
			m_owner.OnUsedPlayerItem -= HandleItemUsed;
			m_owner.companions.Remove(base.aiActor);
			m_owner.OnPitfall -= HandlePitfall;
			m_owner.OnRoomClearEvent -= HandleRoomCleared;
		}
		if ((bool)base.aiShooter)
		{
			AIShooter aIShooter = base.aiShooter;
			aIShooter.PostProcessProjectile = (Action<Projectile>)Delegate.Remove(aIShooter.PostProcessProjectile, new Action<Projectile>(HandleCompanionPostProcessProjectile));
		}
		base.OnDestroy();
	}
}
