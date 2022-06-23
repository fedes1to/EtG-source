using System.Collections;
using Dungeonator;
using UnityEngine;

public class BabyDragunJailController : DungeonPlaceableBehaviour, IPlayerInteractable
{
	public tk2dSprite CagedBabyDragun;

	public Transform CagedBabyDragunTalkPoint;

	public SpeculativeRigidbody SellRegionRigidbody;

	public int RequiredItems = 2;

	[PickupIdentifier]
	public int ItemID;

	private bool m_isOpen;

	private RoomHandler m_room;

	private int m_itemsEaten;

	private bool m_currentlySellingAnItem;

	private void Start()
	{
		m_isOpen = true;
		m_room = base.transform.position.GetAbsoluteRoom();
		m_room.RegisterInteractable(this);
	}

	private void Update()
	{
		if (Dungeon.IsGenerating)
		{
			return;
		}
		bool flag = false;
		for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
		{
			if (GameManager.Instance.AllPlayers[i].CurrentRoom == m_room)
			{
				flag = true;
				break;
			}
		}
		if (!flag || m_itemsEaten >= RequiredItems)
		{
			return;
		}
		for (int j = 0; j < StaticReferenceManager.AllDebris.Count; j++)
		{
			DebrisObject debrisObject = StaticReferenceManager.AllDebris[j];
			if ((bool)debrisObject && debrisObject.IsPickupObject && debrisObject.Static)
			{
				PickupObject componentInChildren = debrisObject.GetComponentInChildren<PickupObject>();
				if ((bool)componentInChildren && !(componentInChildren is GungeonEggItem))
				{
					AttemptSellItem(componentInChildren);
				}
			}
		}
		if (m_currentlySellingAnItem)
		{
			return;
		}
		for (int k = 0; k < StaticReferenceManager.AllNpcs.Count; k++)
		{
			TalkDoerLite talkDoerLite = StaticReferenceManager.AllNpcs[k];
			if ((bool)talkDoerLite && talkDoerLite.name.Contains("ResourcefulRat_Beaten"))
			{
				float magnitude = (talkDoerLite.specRigidbody.UnitCenter - CagedBabyDragun.WorldCenter).magnitude;
				if (magnitude < 3f)
				{
					RoomHandler.unassignedInteractableObjects.Remove(talkDoerLite);
					StartCoroutine(EatCorpse(talkDoerLite));
				}
			}
		}
	}

	private IEnumerator EatCorpse(TalkDoerLite targetCorpse)
	{
		float elapsed = 0f;
		float duration = 0.5f;
		Vector3 startPos = targetCorpse.transform.position;
		Vector3 finalOffset = CagedBabyDragun.WorldCenter - startPos.XY();
		tk2dBaseSprite targetSprite = targetCorpse.GetComponentInChildren<tk2dBaseSprite>();
		Object.Destroy(targetCorpse);
		Object.Destroy(targetCorpse.specRigidbody);
		CagedBabyDragun.spriteAnimator.PlayForDuration("baby_dragun_weak_eat", -1f, "baby_dragun_weak_idle");
		AkSoundEngine.PostEvent("Play_NPC_BabyDragun_Munch_01", base.gameObject);
		while (elapsed < duration)
		{
			elapsed += BraveTime.DeltaTime;
			if (!targetSprite || !targetSprite.transform)
			{
				m_currentlySellingAnItem = false;
				yield break;
			}
			targetSprite.transform.localScale = Vector3.Lerp(Vector3.one, new Vector3(0.01f, 0.01f, 1f), elapsed / duration);
			targetSprite.transform.position = Vector3.Lerp(startPos, startPos + finalOffset, elapsed / duration);
			yield return null;
		}
		if (!targetSprite || !targetSprite.transform)
		{
			m_currentlySellingAnItem = false;
			yield break;
		}
		Object.Destroy(targetSprite.gameObject);
		m_itemsEaten++;
		if (m_itemsEaten >= RequiredItems)
		{
			while (CagedBabyDragun.spriteAnimator.IsPlaying("baby_dragun_weak_eat"))
			{
				yield return null;
			}
			LootEngine.GivePrefabToPlayer(PickupObjectDatabase.GetById(ItemID).gameObject, GameManager.Instance.BestActivePlayer);
			Object.Destroy(base.gameObject);
		}
	}

	public void AttemptSellItem(PickupObject targetItem)
	{
		if (!(targetItem == null) && targetItem.CanBeSold && !targetItem.IsBeingSold && !(targetItem is CurrencyPickup) && !(targetItem is KeyBulletPickup) && !(targetItem is HealthPickup) && m_itemsEaten < RequiredItems && !m_currentlySellingAnItem && SellRegionRigidbody.ContainsPoint(targetItem.sprite.WorldCenter, int.MaxValue, true))
		{
			StartCoroutine(HandleSoldItem(targetItem));
		}
	}

	private IEnumerator HandleSoldItem(PickupObject targetItem)
	{
		targetItem.IsBeingSold = true;
		while (m_currentlySellingAnItem)
		{
			yield return null;
		}
		if (m_itemsEaten >= RequiredItems || !targetItem || !targetItem.sprite || !SellRegionRigidbody.ContainsPoint(targetItem.sprite.WorldCenter, int.MaxValue, true))
		{
			yield break;
		}
		m_currentlySellingAnItem = true;
		IPlayerInteractable ixable = null;
		if (targetItem is PassiveItem)
		{
			PassiveItem passiveItem = targetItem as PassiveItem;
			passiveItem.GetRidOfMinimapIcon();
			ixable = targetItem as PassiveItem;
		}
		else if (targetItem is Gun)
		{
			Gun gun = targetItem as Gun;
			gun.GetRidOfMinimapIcon();
			ixable = targetItem as Gun;
		}
		else if (targetItem is PlayerItem)
		{
			PlayerItem playerItem = targetItem as PlayerItem;
			playerItem.GetRidOfMinimapIcon();
			ixable = targetItem as PlayerItem;
		}
		if (ixable != null)
		{
			RoomHandler.unassignedInteractableObjects.Remove(ixable);
			GameManager.Instance.PrimaryPlayer.RemoveBrokenInteractable(ixable);
		}
		float elapsed = 0f;
		float duration = 0.5f;
		Vector3 startPos = targetItem.transform.position;
		Vector3 finalOffset = CagedBabyDragun.WorldCenter - startPos.XY();
		tk2dBaseSprite targetSprite = targetItem.GetComponentInChildren<tk2dBaseSprite>();
		CagedBabyDragun.spriteAnimator.PlayForDuration("baby_dragun_weak_eat", -1f, "baby_dragun_weak_idle");
		AkSoundEngine.PostEvent("Play_NPC_BabyDragun_Munch_01", base.gameObject);
		while (elapsed < duration)
		{
			elapsed += BraveTime.DeltaTime;
			if (!targetItem || !targetItem.transform)
			{
				m_currentlySellingAnItem = false;
				yield break;
			}
			targetItem.transform.localScale = Vector3.Lerp(Vector3.one, new Vector3(0.01f, 0.01f, 1f), elapsed / duration);
			targetItem.transform.position = Vector3.Lerp(startPos, startPos + finalOffset, elapsed / duration);
			yield return null;
		}
		if (!targetItem || !targetItem.transform)
		{
			m_currentlySellingAnItem = false;
			yield break;
		}
		m_itemsEaten++;
		if (m_itemsEaten >= RequiredItems)
		{
			while (CagedBabyDragun.spriteAnimator.IsPlaying("baby_dragun_weak_eat"))
			{
				yield return null;
			}
			LootEngine.GivePrefabToPlayer(PickupObjectDatabase.GetById(ItemID).gameObject, GameManager.Instance.BestActivePlayer);
			LootEngine.DoDefaultPurplePoof(CagedBabyDragun.WorldCenter);
			Object.Destroy(base.gameObject);
		}
		if (targetItem is Gun && (bool)targetItem.GetComponentInParent<DebrisObject>())
		{
			Object.Destroy(targetItem.transform.parent.gameObject);
		}
		else
		{
			Object.Destroy(targetItem.gameObject);
		}
		m_currentlySellingAnItem = false;
	}

	private void Talk(PlayerController interactor)
	{
		string key = ((m_itemsEaten != 0) ? "#BABYDRAGUN_FED_ONCE" : "#BABYDRAGUN_UNFED");
		TextBoxManager.ShowThoughtBubble(interactor.sprite.WorldTopCenter + new Vector2(0f, 0.5f), interactor.transform, 3f, StringTableManager.GetString(key), true, false, string.Empty);
	}

	public float GetDistanceToPoint(Vector2 point)
	{
		if (!m_isOpen)
		{
			return 100f;
		}
		Vector3 vector = BraveMathCollege.ClosestPointOnRectangle(point, CagedBabyDragun.WorldBottomLeft, CagedBabyDragun.WorldTopRight - CagedBabyDragun.WorldBottomLeft);
		return Vector2.Distance(point, vector) / 1.5f;
	}

	public void OnEnteredRange(PlayerController interactor)
	{
		SpriteOutlineManager.AddOutlineToSprite(CagedBabyDragun, Color.white);
	}

	public void OnExitRange(PlayerController interactor)
	{
		if (SpriteOutlineManager.HasOutline(CagedBabyDragun))
		{
			TextBoxManager.ClearTextBox(interactor.transform);
			SpriteOutlineManager.RemoveOutlineFromSprite(CagedBabyDragun);
		}
	}

	public void Interact(PlayerController interactor)
	{
		Talk(interactor);
	}

	public string GetAnimationState(PlayerController interactor, out bool shouldBeFlipped)
	{
		shouldBeFlipped = false;
		return string.Empty;
	}

	public float GetOverrideMaxDistance()
	{
		return -1f;
	}

	protected override void OnDestroy()
	{
		m_room.DeregisterInteractable(this);
		base.OnDestroy();
	}
}
