using System;
using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class GungeonEggItem : PlayerItem
{
	public int m_numberElapsedFloors;

	public GameObject HealVFX;

	[EnemyIdentifier]
	public string GudetamaGuid;

	[PickupIdentifier]
	public int BabyDragunItemId;

	public DungeonPlaceableBehaviour BabyDragunPlaceable;

	public float TimeInFireToHatch = 4f;

	public bool DoShards;

	public ShardsModule Shards;

	private bool m_isBroken;

	private float m_elapsedInFire;

	private bool m_coroutineActive;

	protected override void Start()
	{
		base.Start();
		SpeculativeRigidbody speculativeRigidbody = base.specRigidbody;
		speculativeRigidbody.OnTriggerCollision = (SpeculativeRigidbody.OnTriggerDelegate)Delegate.Combine(speculativeRigidbody.OnTriggerCollision, new SpeculativeRigidbody.OnTriggerDelegate(HandleTriggerCollision));
	}

	private bool IsPointOnFire(Vector2 testPos)
	{
		IntVector2 key = (testPos / DeadlyDeadlyGoopManager.GOOP_GRID_SIZE).ToIntVector2(VectorConversions.Floor);
		if (DeadlyDeadlyGoopManager.allGoopPositionMap.ContainsKey(key))
		{
			DeadlyDeadlyGoopManager deadlyDeadlyGoopManager = DeadlyDeadlyGoopManager.allGoopPositionMap[key];
			return deadlyDeadlyGoopManager.IsPositionOnFire(testPos);
		}
		return false;
	}

	private void HatchToDragun()
	{
		m_isBroken = true;
		base.spriteAnimator.PlayAndDestroyObject("gungeon_egg_hatch");
		StartCoroutine(HandleDelayedShards());
		m_pickedUp = true;
		RoomHandler absoluteRoom = base.transform.position.GetAbsoluteRoom();
		GameObject gameObject = BabyDragunPlaceable.InstantiateObject(absoluteRoom, base.transform.position.IntXY() - absoluteRoom.area.basePosition + IntVector2.NegOne);
		gameObject.transform.position = base.transform.position + new Vector3(-0.25f, -0.5f, 0f);
		tk2dBaseSprite componentInChildren = gameObject.GetComponentInChildren<tk2dBaseSprite>();
		componentInChildren.UpdateZDepth();
		SpeculativeRigidbody componentInChildren2 = gameObject.GetComponentInChildren<SpeculativeRigidbody>();
		componentInChildren2.Reinitialize();
		DeadlyDeadlyGoopManager.DelayedClearGoopsInRadius(base.transform.position.XY() + new Vector2(0.25f, 0.5f), 3f);
	}

	private void HandleTriggerCollision(SpeculativeRigidbody specRigidbody, SpeculativeRigidbody sourceSpecRigidbody, CollisionData collisionData)
	{
		if (m_isBroken || !base.enabled || m_pickedUp)
		{
			return;
		}
		if (IsPointOnFire(specRigidbody.UnitCenter))
		{
			CanBeSold = false;
			HatchToDragun();
		}
		else if (m_numberElapsedFloors > 0 && !m_isBroken)
		{
			if ((bool)specRigidbody && (bool)specRigidbody.projectile && specRigidbody.projectile.Owner is PlayerController)
			{
				m_isBroken = true;
				CreateRewardItem();
				m_pickedUp = true;
				CanBeSold = false;
				base.spriteAnimator.PlayAndDestroyObject("gungeon_egg_hatch");
				StartCoroutine(HandleDelayedShards());
			}
		}
		else if (m_numberElapsedFloors == 0 && !m_isBroken && (bool)specRigidbody && (bool)specRigidbody.projectile && specRigidbody.projectile.Owner is PlayerController)
		{
			m_isBroken = true;
			AIActor orLoadByGuid = EnemyDatabase.GetOrLoadByGuid(GudetamaGuid);
			AIActor aIActor = AIActor.Spawn(orLoadByGuid, base.transform.position.XY().ToIntVector2(), base.transform.position.GetAbsoluteRoom());
			if ((bool)aIActor)
			{
				aIActor.healthHaver.TriggerInvulnerabilityPeriod(0.5f);
				aIActor.PreventAutoKillOnBossDeath = true;
			}
			m_pickedUp = true;
			base.spriteAnimator.PlayAndDestroyObject("gungeon_egg_hatch");
			StartCoroutine(HandleDelayedShards());
		}
	}

	private IEnumerator HandleDelayedShards()
	{
		yield return new WaitForSeconds(0.8f);
		if (DoShards)
		{
			Shards.HandleShardSpawns(base.sprite.WorldCenter, Vector2.up * 3f);
		}
	}

	public override bool CanBeUsed(PlayerController user)
	{
		if (user.healthHaver.GetCurrentHealthPercentage() >= 1f)
		{
			return false;
		}
		return base.CanBeUsed(user);
	}

	protected override void DoEffect(PlayerController user)
	{
		base.DoEffect(user);
		user.healthHaver.FullHeal();
		user.PlayEffectOnActor(HealVFX, Vector3.zero);
		AkSoundEngine.PostEvent("Play_OBJ_med_kit_01", base.gameObject);
	}

	protected void CreateRewardItem()
	{
		ItemQuality itemQuality = ItemQuality.D;
		if (m_numberElapsedFloors >= 4)
		{
			itemQuality = ItemQuality.S;
			if (m_numberElapsedFloors < 9)
			{
			}
		}
		else
		{
			switch (m_numberElapsedFloors)
			{
			case 1:
				itemQuality = ItemQuality.C;
				break;
			case 2:
				itemQuality = ItemQuality.B;
				break;
			case 3:
				itemQuality = ItemQuality.A;
				break;
			}
		}
		PickupObject itemOfTypeAndQuality = LootEngine.GetItemOfTypeAndQuality<PickupObject>(itemQuality, (!(UnityEngine.Random.value < 0.5f)) ? GameManager.Instance.RewardManager.GunsLootTable : GameManager.Instance.RewardManager.ItemsLootTable);
		if ((bool)itemOfTypeAndQuality)
		{
			LootEngine.SpawnItem(itemOfTypeAndQuality.gameObject, base.transform.position, Vector2.up, 0.1f);
		}
	}

	public override void Update()
	{
		base.Update();
		if (!m_pickedUp && !m_isBroken)
		{
			if (IsPointOnFire(base.specRigidbody.UnitCenter))
			{
				m_elapsedInFire += BraveTime.DeltaTime;
				if (m_elapsedInFire > TimeInFireToHatch)
				{
					HatchToDragun();
				}
			}
			else
			{
				m_elapsedInFire = 0f;
			}
		}
		if (!base.spriteAnimator.IsPlaying("gungeon_egg_hatch"))
		{
			if (m_numberElapsedFloors >= 2 && m_numberElapsedFloors < 4 && !base.spriteAnimator.IsPlaying("gungeon_egg_stir_2"))
			{
				base.spriteAnimator.Play("gungeon_egg_stir_2");
			}
			else if (m_numberElapsedFloors >= 4 && !base.spriteAnimator.IsPlaying("gungeon_egg_stir_3"))
			{
				base.spriteAnimator.Play("gungeon_egg_stir_3");
			}
		}
	}

	public override void Pickup(PlayerController player)
	{
		base.Pickup(player);
		player.OnNewFloorLoaded = (Action<PlayerController>)Delegate.Combine(player.OnNewFloorLoaded, new Action<PlayerController>(HandleLevelLoaded));
	}

	private void HandleLevelLoaded(PlayerController source)
	{
		if (!m_coroutineActive)
		{
			m_coroutineActive = true;
			StartCoroutine(DelayedProcessing());
		}
	}

	private IEnumerator DelayedProcessing()
	{
		while (Dungeon.IsGenerating)
		{
			yield return null;
		}
		m_numberElapsedFloors++;
		yield return new WaitForSeconds(1f);
		m_coroutineActive = false;
	}

	protected override void OnPreDrop(PlayerController user)
	{
		base.OnPreDrop(user);
		user.OnNewFloorLoaded = (Action<PlayerController>)Delegate.Remove(user.OnNewFloorLoaded, new Action<PlayerController>(HandleLevelLoaded));
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if ((bool)LastOwner)
		{
			PlayerController lastOwner = LastOwner;
			lastOwner.OnNewFloorLoaded = (Action<PlayerController>)Delegate.Remove(lastOwner.OnNewFloorLoaded, new Action<PlayerController>(HandleLevelLoaded));
		}
	}

	public override void MidGameSerialize(List<object> data)
	{
		base.MidGameSerialize(data);
		data.Add(m_numberElapsedFloors);
	}

	public override void MidGameDeserialize(List<object> data)
	{
		base.MidGameDeserialize(data);
		if (data.Count == 1)
		{
			m_numberElapsedFloors = (int)data[0];
		}
	}
}
