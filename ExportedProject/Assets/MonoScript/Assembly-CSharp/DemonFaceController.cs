using System;
using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class DemonFaceController : DungeonPlaceableBehaviour, IPlaceConfigurable
{
	public SpeculativeRigidbody interiorRigidbody;

	public LootData WaterRewardTable;

	private List<PlayerController> m_containedPlayers = new List<PlayerController>();

	public int RequiredCurrency = 100;

	public float RequiredCurse = 0.01f;

	private bool m_hasDrunkDeeplyFromTheSweetSweetWaters;

	private void Start()
	{
		SpeculativeRigidbody speculativeRigidbody = base.specRigidbody;
		speculativeRigidbody.OnEnterTrigger = (SpeculativeRigidbody.OnTriggerDelegate)Delegate.Combine(speculativeRigidbody.OnEnterTrigger, new SpeculativeRigidbody.OnTriggerDelegate(HandleTrigger));
		SpeculativeRigidbody speculativeRigidbody2 = base.specRigidbody;
		speculativeRigidbody2.OnExitTrigger = (SpeculativeRigidbody.OnTriggerExitDelegate)Delegate.Combine(speculativeRigidbody2.OnExitTrigger, new SpeculativeRigidbody.OnTriggerExitDelegate(HandleTriggerExit));
		SpeculativeRigidbody speculativeRigidbody3 = interiorRigidbody;
		speculativeRigidbody3.OnPreRigidbodyCollision = (SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate)Delegate.Combine(speculativeRigidbody3.OnPreRigidbodyCollision, new SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate(HandleRigidbodyCollision));
		SpeculativeRigidbody speculativeRigidbody4 = interiorRigidbody;
		speculativeRigidbody4.OnHitByBeam = (Action<BasicBeamController>)Delegate.Combine(speculativeRigidbody4.OnHitByBeam, new Action<BasicBeamController>(HandleBeam));
	}

	private void HandleRigidbodyCollision(SpeculativeRigidbody myRigidbody, PixelCollider myPixelCollider, SpeculativeRigidbody otherRigidbody, PixelCollider otherPixelCollider)
	{
		if ((bool)otherRigidbody.projectile)
		{
			otherRigidbody.projectile.ForceDestruction();
			PhysicsEngine.SkipCollision = true;
		}
	}

	private void HandleTriggerExit(SpeculativeRigidbody specRigidbody, SpeculativeRigidbody sourceSpecRigidbody)
	{
		if ((bool)specRigidbody)
		{
			PlayerController component = specRigidbody.GetComponent<PlayerController>();
			if ((bool)component)
			{
				m_containedPlayers.Remove(component);
			}
		}
	}

	private void HandleBeam(BasicBeamController obj)
	{
		if ((bool)obj.projectile && (obj.projectile.damageTypes | CoreDamageTypes.Water) == obj.projectile.damageTypes)
		{
			HitWithWater();
		}
	}

	private void HandleTrigger(SpeculativeRigidbody specRigidbody, SpeculativeRigidbody sourceSpecRigidbody, CollisionData collisionData)
	{
		PlayerController component = specRigidbody.GetComponent<PlayerController>();
		if ((bool)component)
		{
			m_containedPlayers.Add(component);
			bool flag = false;
			if (component.carriedConsumables.Currency >= RequiredCurrency)
			{
				flag = true;
			}
			if ((float)PlayerStats.GetTotalCurse() >= RequiredCurse)
			{
				flag = true;
			}
			if (flag)
			{
				GameManager.Instance.Dungeon.StartCoroutine(HandleEjection(component, true));
			}
			else
			{
				GameManager.Instance.Dungeon.StartCoroutine(HandleEjection(component, false));
			}
		}
		else
		{
			Projectile projectile = specRigidbody.projectile;
			if ((bool)projectile && (projectile.damageTypes | CoreDamageTypes.Water) == projectile.damageTypes)
			{
				HitWithWater();
			}
		}
	}

	private void WarpToBlackMarket(PlayerController triggerPlayer)
	{
		GameManager.Instance.platformInterface.AchievementUnlock(Achievement.REACH_BLACK_MARKET);
		for (int i = 0; i < GameManager.Instance.Dungeon.data.rooms.Count; i++)
		{
			RoomHandler roomHandler = GameManager.Instance.Dungeon.data.rooms[i];
			if (roomHandler.area.PrototypeRoomCategory == PrototypeDungeonRoom.RoomCategory.SPECIAL && roomHandler.area.PrototypeRoomName == "Black Market")
			{
				triggerPlayer.AttemptTeleportToRoom(roomHandler, true, true);
				if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
				{
					GameManager.Instance.GetOtherPlayer(triggerPlayer).AttemptTeleportToRoom(roomHandler, true);
				}
			}
		}
	}

	private IEnumerator HandleEjection(PlayerController triggerPlayer, bool success)
	{
		triggerPlayer.SetInputOverride("demon face");
		triggerPlayer.ForceStaticFaceDirection(Vector2.up);
		bool playerHasYellowChamber = false;
		for (int i = 0; i < triggerPlayer.passiveItems.Count; i++)
		{
			if (triggerPlayer.passiveItems[i] is YellowChamberItem)
			{
				playerHasYellowChamber = true;
				break;
			}
		}
		if (playerHasYellowChamber)
		{
			interiorRigidbody.spriteAnimator.Play();
			yield return new WaitForSeconds(1.75f);
		}
		triggerPlayer.FlatColorOverridden = true;
		triggerPlayer.ToggleGunRenderers(false, "face");
		triggerPlayer.ToggleHandRenderers(false, "face");
		triggerPlayer.ForceMoveInDirectionUntilThreshold(Vector2.up, triggerPlayer.CenterPosition.y + 1f, 0f, 2f);
		float ela = 0f;
		if (!triggerPlayer.IsDodgeRolling)
		{
			while (ela < 1f)
			{
				ela += BraveTime.DeltaTime;
				triggerPlayer.ChangeFlatColorOverride(new Color(0f, 0f, 0f, ela));
				yield return null;
			}
		}
		else
		{
			ela = 1f;
			triggerPlayer.ForceStopDodgeRoll();
			triggerPlayer.ChangeFlatColorOverride(new Color(0f, 0f, 0f, 1f));
		}
		triggerPlayer.ToggleGunRenderers(false, "face");
		triggerPlayer.ToggleHandRenderers(false, "face");
		while (ela < 1.5f)
		{
			ela += BraveTime.DeltaTime;
			yield return null;
		}
		if (success && 0 == 0)
		{
			WarpToBlackMarket(triggerPlayer);
			yield return new WaitForSeconds(0.625f);
		}
		triggerPlayer.ToggleGunRenderers(true, string.Empty);
		triggerPlayer.ToggleHandRenderers(true, string.Empty);
		triggerPlayer.usingForcedInput = false;
		triggerPlayer.FlatColorOverridden = false;
		triggerPlayer.ChangeFlatColorOverride(new Color(0f, 0f, 0f, 0f));
		triggerPlayer.ClearInputOverride("demon face");
		if (!success)
		{
			if (m_containedPlayers.Contains(triggerPlayer))
			{
				triggerPlayer.healthHaver.ApplyDamage(0.5f, Vector2.down, StringTableManager.GetItemsString("#DEMONFACE"), CoreDamageTypes.None, DamageCategory.Environment);
				triggerPlayer.knockbackDoer.ApplyKnockback(Vector2.down, 80f);
			}
		}
		else
		{
			yield return new WaitForSeconds(0.5f);
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	private void HitWithWater()
	{
		if (!m_hasDrunkDeeplyFromTheSweetSweetWaters)
		{
			m_hasDrunkDeeplyFromTheSweetSweetWaters = true;
			GameManager.Instance.Dungeon.StartCoroutine(ProvideDumbReward());
		}
	}

	private IEnumerator ProvideDumbReward()
	{
		yield return new WaitForSeconds(0.5f);
		PickupObject prefabItem = WaterRewardTable.GetSingleItemForPlayer(GameManager.Instance.PrimaryPlayer);
		if (prefabItem != null)
		{
			float x = prefabItem.GetComponent<tk2dBaseSprite>().GetBounds().center.x;
			DebrisObject debrisObject = LootEngine.SpawnItem(prefabItem.gameObject, base.specRigidbody.PixelColliders[base.specRigidbody.PixelColliders.Count - 1].UnitCenter + new Vector2(0f - x, 0f), Vector2.down, 4f);
			if ((bool)debrisObject.specRigidbody)
			{
				PhysicsEngine.Instance.RegisterOverlappingGhostCollisionExceptions(debrisObject.specRigidbody);
			}
		}
	}

	public void ConfigureOnPlacement(RoomHandler room)
	{
		room.OptionalDoorTopDecorable = ResourceCache.Acquire("Global Prefabs/Purple_Lantern") as GameObject;
	}
}
