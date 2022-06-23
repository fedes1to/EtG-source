using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class EmptyBottleItem : PlayerItem
{
	public enum EmptyBottleContents
	{
		NONE,
		HALF_HEART,
		FULL_HEART,
		AMMO,
		FAIRY,
		ENEMY_SOUL,
		SPREAD_AMMO,
		BLANK,
		KEY
	}

	private EmptyBottleContents m_contents;

	public string EmptySprite;

	public string ContainsHeartSprite;

	public string ContainsHalfHeartSprite;

	public string ContainsAmmoSprite;

	public string ContainsFairySprite;

	public string ContainsSoulSprite;

	public string ContainsSpreadAmmoSprite;

	public string ContainsBlankSprite;

	public string ContainsKeySprite;

	public GameObject HealVFX;

	public GameObject AmmoVFX;

	public GameObject FairyVFX;

	public float SoulDamage = 30f;

	public GameObject OnBurstDamageVFX;

	public EmptyBottleContents Contents
	{
		get
		{
			return m_contents;
		}
		set
		{
			m_contents = value;
			UpdateSprite();
		}
	}

	public override bool CanBeUsed(PlayerController user)
	{
		if (m_contents == EmptyBottleContents.NONE)
		{
			if (!CanReallyBeUsed(user))
			{
				return false;
			}
		}
		else if (m_contents == EmptyBottleContents.ENEMY_SOUL)
		{
			if (user.CurrentRoom == null || !user.CurrentRoom.HasActiveEnemies(RoomHandler.ActiveEnemyType.All))
			{
				return false;
			}
		}
		else if (m_contents == EmptyBottleContents.FAIRY && user.healthHaver.GetCurrentHealthPercentage() >= 1f)
		{
			return false;
		}
		return base.CanBeUsed(user);
	}

	private bool BottleFullCanBeConsumed(PlayerController user)
	{
		switch (m_contents)
		{
		case EmptyBottleContents.NONE:
			if (!CanReallyBeUsed(user))
			{
				return false;
			}
			break;
		case EmptyBottleContents.HALF_HEART:
		case EmptyBottleContents.FULL_HEART:
			if (user.healthHaver.GetCurrentHealthPercentage() >= 1f)
			{
				return false;
			}
			break;
		case EmptyBottleContents.AMMO:
			if (user.CurrentGun == null || user.CurrentGun.ammo == user.CurrentGun.AdjustedMaxAmmo || !user.CurrentGun.CanGainAmmo || user.CurrentGun.InfiniteAmmo)
			{
				return false;
			}
			break;
		case EmptyBottleContents.FAIRY:
			if (user.healthHaver.GetCurrentHealthPercentage() >= 1f)
			{
				return false;
			}
			break;
		case EmptyBottleContents.ENEMY_SOUL:
			if (user.CurrentRoom == null || !user.CurrentRoom.HasActiveEnemies(RoomHandler.ActiveEnemyType.All))
			{
				return false;
			}
			break;
		case EmptyBottleContents.SPREAD_AMMO:
			if (user.CurrentGun == null || user.CurrentGun.ammo == user.CurrentGun.AdjustedMaxAmmo || !user.CurrentGun.CanGainAmmo || user.CurrentGun.InfiniteAmmo)
			{
				return false;
			}
			break;
		}
		return true;
	}

	private bool CanReallyBeUsed(PlayerController user)
	{
		if (!user)
		{
			return false;
		}
		if (user.CurrentRoom != null)
		{
			List<AIActor> activeEnemies = user.CurrentRoom.GetActiveEnemies(RoomHandler.ActiveEnemyType.All);
			if (activeEnemies != null)
			{
				for (int i = 0; i < activeEnemies.Count; i++)
				{
					AIActor aIActor = activeEnemies[i];
					if ((bool)aIActor && (bool)aIActor.encounterTrackable && aIActor.encounterTrackable.journalData.PrimaryDisplayName == "#GUNFAIRY_ENCNAME")
					{
						return true;
					}
				}
			}
		}
		List<DebrisObject> allDebris = StaticReferenceManager.AllDebris;
		if (allDebris != null)
		{
			for (int j = 0; j < allDebris.Count; j++)
			{
				DebrisObject debrisObject = allDebris[j];
				if (!debrisObject || !debrisObject.IsPickupObject)
				{
					continue;
				}
				float sqrMagnitude = (user.CenterPosition - debrisObject.transform.position.XY()).sqrMagnitude;
				if (sqrMagnitude > 25f)
				{
					continue;
				}
				HealthPickup component = debrisObject.GetComponent<HealthPickup>();
				AmmoPickup component2 = debrisObject.GetComponent<AmmoPickup>();
				KeyBulletPickup component3 = debrisObject.GetComponent<KeyBulletPickup>();
				SilencerItem component4 = debrisObject.GetComponent<SilencerItem>();
				if (((bool)component && component.armorAmount == 0 && (component.healAmount == 0.5f || component.healAmount == 1f)) || (bool)component2 || (bool)component3 || (bool)component4)
				{
					float num = Mathf.Sqrt(sqrMagnitude);
					if (num < 5f)
					{
						return true;
					}
				}
			}
		}
		if ((bool)user)
		{
			IPlayerInteractable lastInteractable = user.GetLastInteractable();
			if (lastInteractable is HeartDispenser)
			{
				HeartDispenser heartDispenser = lastInteractable as HeartDispenser;
				if ((bool)heartDispenser && HeartDispenser.CurrentHalfHeartsStored > 0)
				{
					return true;
				}
			}
		}
		return false;
	}

	protected override void OnPreDrop(PlayerController user)
	{
		if ((bool)user)
		{
			user.OnReceivedDamage -= HandleOwnerTookDamage;
		}
		base.OnPreDrop(user);
	}

	public override void Pickup(PlayerController player)
	{
		base.Pickup(player);
		player.OnReceivedDamage += HandleOwnerTookDamage;
	}

	private void HandleOwnerTookDamage(PlayerController sourcePlayer)
	{
		if ((bool)sourcePlayer && sourcePlayer.HasActiveBonusSynergy(CustomSynergyType.EMPTY_VESSELS) && Contents == EmptyBottleContents.NONE)
		{
			Contents = EmptyBottleContents.ENEMY_SOUL;
		}
	}

	public override void MidGameSerialize(List<object> data)
	{
		base.MidGameSerialize(data);
		data.Add((int)Contents);
	}

	public override void MidGameDeserialize(List<object> data)
	{
		base.MidGameDeserialize(data);
		if (data.Count == 1)
		{
			Contents = (EmptyBottleContents)data[0];
		}
	}

	private IEnumerator HandleSuck(tk2dSprite targetSprite)
	{
		float elapsed = 0f;
		float duration = 0.25f;
		PlayerController owner = LastOwner;
		if ((bool)targetSprite)
		{
			Vector3 startPosition = targetSprite.transform.position;
			while (elapsed < duration && (bool)owner)
			{
				elapsed += BraveTime.DeltaTime;
				if ((bool)targetSprite)
				{
					targetSprite.transform.localScale = Vector3.Lerp(Vector3.one, new Vector3(0.1f, 0.1f, 0.1f), elapsed / duration);
					targetSprite.transform.position = Vector3.Lerp(startPosition, owner.CenterPosition.ToVector3ZisY(), elapsed / duration);
				}
				yield return null;
			}
		}
		Object.Destroy(targetSprite.gameObject);
	}

	protected override void DoEffect(PlayerController user)
	{
		AkSoundEngine.PostEvent("Play_OBJ_bottle_cork_01", base.gameObject);
		if (Contents == EmptyBottleContents.NONE)
		{
			tk2dSpriteCollectionData tk2dSpriteCollectionData2 = null;
			int spriteId = -1;
			Vector3 position = Vector3.zero;
			AIActor aIActor = null;
			List<AIActor> activeEnemies = user.CurrentRoom.GetActiveEnemies(RoomHandler.ActiveEnemyType.All);
			if (activeEnemies != null)
			{
				for (int i = 0; i < activeEnemies.Count; i++)
				{
					if ((bool)activeEnemies[i] && (bool)activeEnemies[i].encounterTrackable && activeEnemies[i].encounterTrackable.journalData.PrimaryDisplayName == "#GUNFAIRY_ENCNAME")
					{
						aIActor = activeEnemies[i];
					}
				}
			}
			if ((bool)aIActor)
			{
				if ((bool)aIActor.sprite)
				{
					tk2dSpriteCollectionData2 = aIActor.sprite.Collection;
					spriteId = aIActor.sprite.spriteId;
					position = aIActor.transform.position;
				}
				aIActor.EraseFromExistence();
				Contents = EmptyBottleContents.FAIRY;
			}
			else
			{
				if ((bool)user)
				{
					IPlayerInteractable lastInteractable = user.GetLastInteractable();
					if (lastInteractable is HeartDispenser)
					{
						HeartDispenser heartDispenser = lastInteractable as HeartDispenser;
						if ((bool)heartDispenser && HeartDispenser.CurrentHalfHeartsStored > 0)
						{
							if (HeartDispenser.CurrentHalfHeartsStored > 1)
							{
								HeartDispenser.CurrentHalfHeartsStored -= 2;
								Contents = EmptyBottleContents.FULL_HEART;
							}
							else
							{
								HeartDispenser.CurrentHalfHeartsStored--;
								Contents = EmptyBottleContents.HALF_HEART;
							}
							return;
						}
					}
				}
				if (StaticReferenceManager.AllDebris != null)
				{
					DebrisObject debrisObject = null;
					float num = float.MaxValue;
					for (int j = 0; j < StaticReferenceManager.AllDebris.Count; j++)
					{
						DebrisObject debrisObject2 = StaticReferenceManager.AllDebris[j];
						if (!debrisObject2.IsPickupObject)
						{
							continue;
						}
						float sqrMagnitude = (user.CenterPosition - debrisObject2.transform.position.XY()).sqrMagnitude;
						if (sqrMagnitude > 25f)
						{
							continue;
						}
						HealthPickup component = debrisObject2.GetComponent<HealthPickup>();
						AmmoPickup component2 = debrisObject2.GetComponent<AmmoPickup>();
						KeyBulletPickup component3 = debrisObject2.GetComponent<KeyBulletPickup>();
						SilencerItem component4 = debrisObject2.GetComponent<SilencerItem>();
						if (((bool)component && component.armorAmount == 0 && (component.healAmount == 0.5f || component.healAmount == 1f)) || (bool)component2 || (bool)component3 || (bool)component4)
						{
							float num2 = Mathf.Sqrt(sqrMagnitude);
							if (num2 < num && num2 < 5f)
							{
								num = num2;
								debrisObject = debrisObject2;
							}
						}
					}
					if ((bool)debrisObject)
					{
						HealthPickup component5 = debrisObject.GetComponent<HealthPickup>();
						AmmoPickup component6 = debrisObject.GetComponent<AmmoPickup>();
						KeyBulletPickup component7 = debrisObject.GetComponent<KeyBulletPickup>();
						SilencerItem component8 = debrisObject.GetComponent<SilencerItem>();
						if ((bool)component5)
						{
							if ((bool)component5.sprite)
							{
								tk2dSpriteCollectionData2 = component5.sprite.Collection;
								spriteId = component5.sprite.spriteId;
								position = component5.transform.position;
							}
							if (component5.armorAmount == 0 && component5.healAmount == 0.5f)
							{
								Contents = EmptyBottleContents.HALF_HEART;
								Object.Destroy(component5.gameObject);
							}
							else if (component5.armorAmount == 0 && component5.healAmount == 1f)
							{
								Contents = EmptyBottleContents.FULL_HEART;
								Object.Destroy(component5.gameObject);
							}
						}
						else if ((bool)component6)
						{
							if ((bool)component6.sprite)
							{
								tk2dSpriteCollectionData2 = component6.sprite.Collection;
								spriteId = component6.sprite.spriteId;
								position = component6.transform.position;
							}
							Contents = ((component6.mode != AmmoPickup.AmmoPickupMode.SPREAD_AMMO) ? EmptyBottleContents.AMMO : EmptyBottleContents.SPREAD_AMMO);
							Object.Destroy(component6.gameObject);
						}
						else if ((bool)component7)
						{
							if ((bool)component7.sprite)
							{
								tk2dSpriteCollectionData2 = component7.sprite.Collection;
								spriteId = component7.sprite.spriteId;
								position = component7.transform.position;
							}
							Contents = EmptyBottleContents.KEY;
							Object.Destroy(component7.gameObject);
						}
						else if ((bool)component8)
						{
							if ((bool)component8.sprite)
							{
								tk2dSpriteCollectionData2 = component8.sprite.Collection;
								spriteId = component8.sprite.spriteId;
								position = component8.transform.position;
							}
							Contents = EmptyBottleContents.BLANK;
							Object.Destroy(component8.gameObject);
						}
					}
				}
			}
			if (tk2dSpriteCollectionData2 != null)
			{
				GameObject gameObject = new GameObject("sucked sprite");
				gameObject.transform.position = position;
				tk2dSprite targetSprite = tk2dSprite.AddComponent(gameObject, tk2dSpriteCollectionData2, spriteId);
				GameManager.Instance.Dungeon.StartCoroutine(HandleSuck(targetSprite));
			}
		}
		else if (BottleFullCanBeConsumed(user))
		{
			UseContainedItem(user);
		}
		else
		{
			ThrowContainedItem(user);
		}
	}

	private void ThrowContainedItem(PlayerController user)
	{
		switch (Contents)
		{
		case EmptyBottleContents.HALF_HEART:
			LootEngine.SpawnHealth(user.CenterPosition, 1, Random.insideUnitCircle.normalized);
			break;
		case EmptyBottleContents.FULL_HEART:
			LootEngine.SpawnHealth(user.CenterPosition, 2, Random.insideUnitCircle.normalized);
			break;
		case EmptyBottleContents.AMMO:
			LootEngine.SpawnItem((GameObject)BraveResources.Load("Ammo_Pickup"), user.CenterPosition.ToVector3ZUp(), Random.insideUnitCircle.normalized, 4f);
			break;
		case EmptyBottleContents.SPREAD_AMMO:
			LootEngine.SpawnItem((GameObject)BraveResources.Load("Ammo_Pickup_Spread"), user.CenterPosition.ToVector3ZUp(), Random.insideUnitCircle.normalized, 4f);
			break;
		case EmptyBottleContents.KEY:
			LootEngine.SpawnItem(PickupObjectDatabase.GetById(GlobalItemIds.Key).gameObject, user.CenterPosition.ToVector3ZUp(), Random.insideUnitCircle.normalized, 4f);
			break;
		case EmptyBottleContents.BLANK:
			LootEngine.SpawnItem(PickupObjectDatabase.GetById(GlobalItemIds.Blank).gameObject, user.CenterPosition.ToVector3ZUp(), Random.insideUnitCircle.normalized, 4f);
			break;
		}
		Contents = EmptyBottleContents.NONE;
	}

	private void UseContainedItem(PlayerController user)
	{
		switch (Contents)
		{
		case EmptyBottleContents.HALF_HEART:
			user.healthHaver.ApplyHealing(0.5f);
			AkSoundEngine.PostEvent("Play_OBJ_heart_heal_01", base.gameObject);
			user.PlayEffectOnActor(HealVFX, Vector3.zero);
			break;
		case EmptyBottleContents.FULL_HEART:
			user.healthHaver.ApplyHealing(1f);
			AkSoundEngine.PostEvent("Play_OBJ_heart_heal_01", base.gameObject);
			user.PlayEffectOnActor(HealVFX, Vector3.zero);
			break;
		case EmptyBottleContents.AMMO:
			if (user.CurrentGun != null && user.CurrentGun.AdjustedMaxAmmo > 0)
			{
				user.CurrentGun.GainAmmo(user.CurrentGun.AdjustedMaxAmmo);
				user.CurrentGun.ForceImmediateReload();
				AkSoundEngine.PostEvent("Play_OBJ_ammo_pickup_01", base.gameObject);
				user.PlayEffectOnActor(AmmoVFX, Vector3.zero);
				string string3 = StringTableManager.GetString("#AMMO_SINGLE_GUN_REFILLED_HEADER");
				string description = user.CurrentGun.GetComponent<EncounterTrackable>().journalData.GetPrimaryDisplayName() + " " + StringTableManager.GetString("#AMMO_SINGLE_GUN_REFILLED_BODY");
				tk2dBaseSprite tk2dBaseSprite3 = user.CurrentGun.GetSprite();
				if (!GameUIRoot.Instance.BossHealthBarVisible)
				{
					GameUIRoot.Instance.notificationController.DoCustomNotification(string3, description, tk2dBaseSprite3.Collection, tk2dBaseSprite3.spriteId);
				}
			}
			break;
		case EmptyBottleContents.SPREAD_AMMO:
		{
			float num = 0.5f;
			float num2 = 0.2f;
			user.CurrentGun.GainAmmo(Mathf.CeilToInt((float)user.CurrentGun.AdjustedMaxAmmo * num));
			for (int i = 0; i < user.inventory.AllGuns.Count; i++)
			{
				if ((bool)user.inventory.AllGuns[i] && user.CurrentGun != user.inventory.AllGuns[i])
				{
					user.inventory.AllGuns[i].GainAmmo(Mathf.FloorToInt((float)user.inventory.AllGuns[i].AdjustedMaxAmmo * num2));
				}
			}
			user.CurrentGun.ForceImmediateReload();
			if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
			{
				PlayerController otherPlayer = GameManager.Instance.GetOtherPlayer(user);
				if (!otherPlayer.IsGhost)
				{
					for (int j = 0; j < otherPlayer.inventory.AllGuns.Count; j++)
					{
						if ((bool)otherPlayer.inventory.AllGuns[j])
						{
							otherPlayer.inventory.AllGuns[j].GainAmmo(Mathf.FloorToInt((float)otherPlayer.inventory.AllGuns[j].AdjustedMaxAmmo * num2));
						}
					}
					otherPlayer.CurrentGun.ForceImmediateReload();
				}
			}
			AkSoundEngine.PostEvent("Play_OBJ_ammo_pickup_01", base.gameObject);
			user.PlayEffectOnActor(AmmoVFX, Vector3.zero);
			string @string = StringTableManager.GetString("#AMMO_SINGLE_GUN_REFILLED_HEADER");
			string string2 = StringTableManager.GetString("#AMMO_SPREAD_REFILLED_BODY");
			tk2dBaseSprite tk2dBaseSprite2 = user.CurrentGun.GetSprite();
			if (!GameUIRoot.Instance.BossHealthBarVisible)
			{
				GameUIRoot.Instance.notificationController.DoCustomNotification(@string, string2, tk2dBaseSprite2.Collection, tk2dBaseSprite2.spriteId);
			}
			break;
		}
		case EmptyBottleContents.FAIRY:
			AkSoundEngine.PostEvent("Play_NPC_faerie_heal_01", base.gameObject);
			user.PlayFairyEffectOnActor(ResourceCache.Acquire("Global VFX/VFX_Fairy_Fly") as GameObject, Vector3.zero, 4.5f, true);
			user.StartCoroutine(HandleHearts(user));
			break;
		case EmptyBottleContents.ENEMY_SOUL:
			user.CurrentRoom.ApplyActionToNearbyEnemies(user.transform.position.XY(), 100f, SoulProcessEnemy);
			break;
		case EmptyBottleContents.KEY:
			user.carriedConsumables.KeyBullets = user.carriedConsumables.KeyBullets + 1;
			break;
		case EmptyBottleContents.BLANK:
			user.Blanks++;
			break;
		}
		Contents = EmptyBottleContents.NONE;
	}

	private void SoulProcessEnemy(AIActor a, float distance)
	{
		if ((bool)a && a.IsNormalEnemy && (bool)a.healthHaver && !a.IsGone)
		{
			if ((bool)LastOwner)
			{
				a.healthHaver.ApplyDamage(SoulDamage, Vector2.zero, LastOwner.ActorName);
			}
			else
			{
				a.healthHaver.ApplyDamage(SoulDamage, Vector2.zero, "projectile");
			}
			if ((bool)OnBurstDamageVFX)
			{
				a.PlayEffectOnActor(OnBurstDamageVFX, Vector3.zero);
			}
		}
	}

	private IEnumerator HandleHearts(PlayerController targetPlayer)
	{
		float duration = 4.5f;
		int halfHeartsToHeal = Mathf.RoundToInt((targetPlayer.healthHaver.GetMaxHealth() - targetPlayer.healthHaver.GetCurrentHealth()) / 0.5f);
		float timeStep = duration / (float)halfHeartsToHeal;
		while (targetPlayer.healthHaver.GetCurrentHealth() < targetPlayer.healthHaver.GetMaxHealth())
		{
			targetPlayer.healthHaver.ApplyHealing(0.5f);
			yield return new WaitForSeconds(timeStep);
		}
	}

	private void UpdateSprite()
	{
		switch (Contents)
		{
		case EmptyBottleContents.NONE:
			base.spriteAnimator.Stop();
			base.sprite.SetSprite(EmptySprite);
			break;
		case EmptyBottleContents.HALF_HEART:
			base.spriteAnimator.Stop();
			base.sprite.SetSprite(ContainsHalfHeartSprite);
			break;
		case EmptyBottleContents.FULL_HEART:
			base.spriteAnimator.Stop();
			base.sprite.SetSprite(ContainsHeartSprite);
			break;
		case EmptyBottleContents.AMMO:
			base.spriteAnimator.Stop();
			base.sprite.SetSprite(ContainsAmmoSprite);
			break;
		case EmptyBottleContents.FAIRY:
			base.spriteAnimator.Stop();
			base.sprite.SetSprite(ContainsFairySprite);
			break;
		case EmptyBottleContents.ENEMY_SOUL:
			base.sprite.SetSprite(ContainsSoulSprite);
			base.spriteAnimator.Play("empty_bottle_soul");
			break;
		case EmptyBottleContents.SPREAD_AMMO:
			base.spriteAnimator.Stop();
			base.sprite.SetSprite(ContainsSpreadAmmoSprite);
			break;
		case EmptyBottleContents.KEY:
			base.spriteAnimator.Stop();
			base.sprite.SetSprite(ContainsKeySprite);
			break;
		case EmptyBottleContents.BLANK:
			base.spriteAnimator.Stop();
			base.sprite.SetSprite(ContainsBlankSprite);
			break;
		}
	}

	protected override void CopyStateFrom(PlayerItem other)
	{
		base.CopyStateFrom(other);
		EmptyBottleItem emptyBottleItem = other as EmptyBottleItem;
		if ((bool)emptyBottleItem)
		{
			m_contents = emptyBottleItem.m_contents;
			base.sprite.SetSprite(emptyBottleItem.sprite.spriteId);
		}
	}

	protected override void OnDestroy()
	{
		if ((bool)LastOwner)
		{
			LastOwner.OnReceivedDamage -= HandleOwnerTookDamage;
		}
		base.OnDestroy();
	}
}
