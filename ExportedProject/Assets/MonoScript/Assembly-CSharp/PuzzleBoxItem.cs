using System;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

public class PuzzleBoxItem : PlayerItem
{
	public int NumberOfUsesToOpen = 3;

	public int NumUsesIncreasePerUsage = 1;

	public GameObject UseVFX;

	public GameObject OpenVFX;

	public bool ShouldUseRitualEveryUse;

	public float DemonicRitualChance = 0.05f;

	public float HurtPlayerChance = 0.2f;

	public float AmountToDamagePlayer = 0.5f;

	public float RitualChanceIncreasePerUsage = 0.05f;

	public float MaxEnemiesToSpawn = 5f;

	private float NumEnemiesToSpawn = 3f;

	public float ChanceToIncreaseCursePerAttempt = 0.5f;

	public int CurseIncreasePerAttempt;

	public float ChanceToDamagePlayerOnSuccess = 0.2f;

	public int CurseIncreasePerItem = 1;

	public float ChanceToEyeball = 0.001f;

	[EnemyIdentifier]
	public string DevilEnemyGuid;

	[EnemyIdentifier]
	public string[] AdditionalEnemyGuids;

	private int m_numberOfUses;

	public override bool CanBeUsed(PlayerController user)
	{
		if ((bool)user && user.InExitCell)
		{
			return false;
		}
		if ((bool)user && user.CurrentRoom != null && user.CurrentRoom.IsShop)
		{
			return false;
		}
		return base.CanBeUsed(user);
	}

	protected override void OnPreDrop(PlayerController user)
	{
		base.OnPreDrop(user);
	}

	public override void Pickup(PlayerController player)
	{
		base.Pickup(player);
	}

	public override void MidGameSerialize(List<object> data)
	{
		base.MidGameSerialize(data);
		data.Add(m_numberOfUses);
	}

	public override void MidGameDeserialize(List<object> data)
	{
		base.MidGameDeserialize(data);
		if (data.Count == 1)
		{
			m_numberOfUses = (int)data[0];
		}
	}

	private void PlayTeleporterEffect(PlayerController p)
	{
		for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
		{
			if (!GameManager.Instance.AllPlayers[i].IsGhost)
			{
				GameManager.Instance.AllPlayers[i].healthHaver.TriggerInvulnerabilityPeriod(1f);
				GameManager.Instance.AllPlayers[i].knockbackDoer.TriggerTemporaryKnockbackInvulnerability(1f);
			}
		}
		GameObject gameObject = (GameObject)ResourceCache.Acquire("Global VFX/VFX_Tentacleport");
		if (gameObject != null)
		{
			GameObject gameObject2 = UnityEngine.Object.Instantiate(gameObject);
			gameObject2.GetComponent<tk2dBaseSprite>().PlaceAtLocalPositionByAnchor(p.specRigidbody.UnitBottomCenter + new Vector2(0f, -1f), tk2dBaseSprite.Anchor.LowerCenter);
			gameObject2.transform.position = gameObject2.transform.position.Quantize(0.0625f);
			gameObject2.GetComponent<tk2dBaseSprite>().UpdateZDepth();
		}
	}

	protected override void DoEffect(PlayerController user)
	{
		m_numberOfUses++;
		CheckRitual(user, m_numberOfUses >= NumberOfUsesToOpen);
		GameStatsManager.Instance.RegisterStatChange(TrackedStats.LAMENT_CONFIGURUM_USES, 1f);
		if (m_numberOfUses >= NumberOfUsesToOpen)
		{
			user.PlayEffectOnActor(OpenVFX, new Vector3(1f / 32f, 1.5f, 0f));
			PickupObject pickupObject = Open(user);
			NumberOfUsesToOpen += NumUsesIncreasePerUsage;
			m_numberOfUses = 0;
			if (CurseIncreasePerItem > 0)
			{
				StatModifier statModifier = new StatModifier();
				statModifier.statToBoost = PlayerStats.StatType.Curse;
				statModifier.amount = CurseIncreasePerItem;
				statModifier.modifyType = StatModifier.ModifyMethod.ADDITIVE;
				if ((bool)pickupObject)
				{
					if (pickupObject is Gun)
					{
						Gun gun = pickupObject as Gun;
						Array.Resize(ref gun.passiveStatModifiers, gun.passiveStatModifiers.Length + 1);
						gun.passiveStatModifiers[gun.passiveStatModifiers.Length - 1] = statModifier;
					}
					else if (pickupObject is PassiveItem)
					{
						PassiveItem passiveItem = pickupObject as PassiveItem;
						Array.Resize(ref passiveItem.passiveStatModifiers, passiveItem.passiveStatModifiers.Length + 1);
						passiveItem.passiveStatModifiers[passiveItem.passiveStatModifiers.Length - 1] = statModifier;
					}
					else if (pickupObject is PlayerItem)
					{
						PlayerItem playerItem = pickupObject as PlayerItem;
						Array.Resize(ref playerItem.passiveStatModifiers, playerItem.passiveStatModifiers.Length + 1);
						playerItem.passiveStatModifiers[playerItem.passiveStatModifiers.Length - 1] = statModifier;
					}
				}
				else
				{
					user.ownerlessStatModifiers.Add(statModifier);
					user.stats.RecalculateStats(user);
				}
			}
			DemonicRitualChance += RitualChanceIncreasePerUsage;
		}
		else
		{
			if (CurseIncreasePerAttempt > 0 && UnityEngine.Random.value < ChanceToIncreaseCursePerAttempt)
			{
				StatModifier statModifier2 = new StatModifier();
				statModifier2.statToBoost = PlayerStats.StatType.Curse;
				statModifier2.amount = CurseIncreasePerAttempt;
				statModifier2.modifyType = StatModifier.ModifyMethod.ADDITIVE;
				user.ownerlessStatModifiers.Add(statModifier2);
				user.stats.RecalculateStats(user);
			}
			user.PlayEffectOnActor(UseVFX, new Vector3(1f / 32f, 1.53125f, 0f));
		}
	}

	private IEnumerator TimedKill(AIActor targetActor)
	{
		yield return new WaitForSeconds(60f);
		if ((bool)targetActor && (bool)targetActor.healthHaver && targetActor.healthHaver.IsAlive)
		{
			targetActor.EraseFromExistence();
		}
	}

	private void DoDamageIfIShould(PlayerController user)
	{
		if (user.HasActiveBonusSynergy(CustomSynergyType.HEART_SHAPED_BOX))
		{
			AkSoundEngine.PostEvent("Play_OBJ_heart_heal_01", base.gameObject);
			user.healthHaver.ApplyHealing(0.5f);
		}
		else if (UnityEngine.Random.value < ChanceToDamagePlayerOnSuccess)
		{
			AmountToDamagePlayer += 0.5f;
			user.healthHaver.ApplyDamage(AmountToDamagePlayer, Vector2.zero, StringTableManager.GetItemsString("#LAMENTBOX_ENCNAME"), CoreDamageTypes.None, DamageCategory.Normal, true);
		}
	}

	private void CheckRitual(PlayerController user, bool shouldOpen)
	{
		if ((!shouldOpen && !ShouldUseRitualEveryUse) || !(UnityEngine.Random.value < DemonicRitualChance))
		{
			return;
		}
		bool flag = !user.CurrentRoom.IsSealed;
		FloodFillUtility.PreprocessContiguousCells(LastOwner.CurrentRoom, LastOwner.specRigidbody.UnitCenter.ToIntVector2(VectorConversions.Floor));
		IntVector2? targetCenter = user.CenterPosition.ToIntVector2(VectorConversions.Floor);
		int num = 0;
		NumEnemiesToSpawn = UnityEngine.Random.Range(2f, MaxEnemiesToSpawn);
		for (int i = 0; (float)i < NumEnemiesToSpawn; i++)
		{
			string guid = DevilEnemyGuid;
			if (AdditionalEnemyGuids.Length > 0)
			{
				int num2 = UnityEngine.Random.Range(-1, AdditionalEnemyGuids.Length);
				if (num2 >= 0)
				{
					guid = AdditionalEnemyGuids[num2];
				}
			}
			AIActor enemyPrefab = EnemyDatabase.GetOrLoadByGuid(guid);
			bool checkContiguous = true;
			CellValidator cellValidator = delegate(IntVector2 c)
			{
				if (checkContiguous && !FloodFillUtility.WasFilled(c))
				{
					return false;
				}
				for (int k = 0; k < enemyPrefab.Clearance.x; k++)
				{
					for (int l = 0; l < enemyPrefab.Clearance.y; l++)
					{
						if (GameManager.Instance.Dungeon.data.isTopWall(c.x + k, c.y + l))
						{
							return false;
						}
						if (targetCenter.HasValue)
						{
							if (IntVector2.Distance(targetCenter.Value, c.x + k, c.y + l) < 4f)
							{
								return false;
							}
							if (IntVector2.Distance(targetCenter.Value, c.x + k, c.y + l) > 20f)
							{
								return false;
							}
						}
					}
				}
				return true;
			};
			checkContiguous = true;
			IntVector2? randomAvailableCell = user.CurrentRoom.GetRandomAvailableCell(enemyPrefab.Clearance, enemyPrefab.PathableTiles, false, cellValidator);
			if (!randomAvailableCell.HasValue)
			{
				checkContiguous = false;
				randomAvailableCell = user.CurrentRoom.GetRandomAvailableCell(enemyPrefab.Clearance, enemyPrefab.PathableTiles, false, cellValidator);
			}
			if (randomAvailableCell.HasValue)
			{
				AIActor aIActor = AIActor.Spawn(enemyPrefab, randomAvailableCell.Value, user.CurrentRoom, true);
				aIActor.StartCoroutine(TimedKill(aIActor));
				num++;
				aIActor.HandleReinforcementFallIntoRoom();
			}
		}
		if (num <= 0)
		{
			return;
		}
		if (user.CurrentRoom.area.runtimePrototypeData != null)
		{
			bool flag2 = false;
			for (int j = 0; j < user.CurrentRoom.area.runtimePrototypeData.roomEvents.Count; j++)
			{
				RoomEventDefinition roomEventDefinition = user.CurrentRoom.area.runtimePrototypeData.roomEvents[j];
				if (roomEventDefinition.condition == RoomEventTriggerCondition.ON_ENEMIES_CLEARED && roomEventDefinition.action == RoomEventTriggerAction.UNSEAL_ROOM)
				{
					flag2 = true;
				}
			}
			if (!flag2)
			{
				user.CurrentRoom.area.runtimePrototypeData.roomEvents.Add(new RoomEventDefinition(RoomEventTriggerCondition.ON_ENEMIES_CLEARED, RoomEventTriggerAction.UNSEAL_ROOM));
			}
		}
		if (flag)
		{
			user.CurrentRoom.PreventStandardRoomReward = true;
		}
		user.CurrentRoom.SealRoom();
	}

	private PickupObject Open(PlayerController user)
	{
		DebrisObject debrisObject = GameManager.Instance.RewardManager.SpawnTotallyRandomItem(user.CenterPosition, ItemQuality.B);
		DoDamageIfIShould(user);
		if ((bool)debrisObject)
		{
			Vector2 vector = ((!debrisObject.sprite) ? (debrisObject.transform.position.XY() + new Vector2(0.5f, 0.5f)) : debrisObject.sprite.WorldCenter);
			GameObject gameObject = SpawnManager.SpawnVFX((GameObject)BraveResources.Load("Global VFX/VFX_BlackPhantomDeath"), vector, Quaternion.identity, false);
			if ((bool)gameObject && (bool)gameObject.GetComponent<tk2dSprite>())
			{
				tk2dSprite component = gameObject.GetComponent<tk2dSprite>();
				component.HeightOffGround = 5f;
				component.UpdateZDepth();
			}
			return debrisObject.GetComponentInChildren<PickupObject>();
		}
		return null;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
