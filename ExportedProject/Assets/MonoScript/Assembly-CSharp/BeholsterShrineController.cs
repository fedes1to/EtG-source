using System.Collections;
using Dungeonator;
using UnityEngine;

public class BeholsterShrineController : DungeonPlaceableBehaviour, IPlayerInteractable, IPlaceConfigurable
{
	public string displayTextKey;

	public string acceptOptionKey;

	public string declineOptionKey;

	public string spentOptionKey = "#SHRINE_GENERIC_SPENT";

	public Transform talkPoint;

	public tk2dSprite AlternativeOutlineTarget;

	[PickupIdentifier]
	public int Gun01ID;

	[PickupIdentifier]
	public int Gun02ID;

	[PickupIdentifier]
	public int Gun03ID;

	[PickupIdentifier]
	public int Gun04ID;

	[PickupIdentifier]
	public int Gun05ID;

	[PickupIdentifier]
	public int Gun06ID;

	public tk2dSprite Gun01Sprite;

	public tk2dSprite Gun02Sprite;

	public tk2dSprite Gun03Sprite;

	public tk2dSprite Gun04Sprite;

	public tk2dSprite Gun05Sprite;

	public tk2dSprite Gun06Sprite;

	public GameObject VFXStonePuff;

	private RoomHandler m_room;

	private int m_useCount;

	public void ConfigureOnPlacement(RoomHandler room)
	{
		m_room = room;
		m_room.OptionalDoorTopDecorable = ResourceCache.Acquire("Global Prefabs/Shrine_Lantern") as GameObject;
		UpdateSpriteVisibility();
	}

	private void UpdateSpriteVisibility()
	{
		UpdateSingleSpriteVisibility(Gun01Sprite, GameStatsManager.Instance.GetFlag(GungeonFlags.SHRINE_BEHOLSTER_GUN_01));
		UpdateSingleSpriteVisibility(Gun02Sprite, GameStatsManager.Instance.GetFlag(GungeonFlags.SHRINE_BEHOLSTER_GUN_02));
		UpdateSingleSpriteVisibility(Gun03Sprite, GameStatsManager.Instance.GetFlag(GungeonFlags.SHRINE_BEHOLSTER_GUN_03));
		UpdateSingleSpriteVisibility(Gun04Sprite, GameStatsManager.Instance.GetFlag(GungeonFlags.SHRINE_BEHOLSTER_GUN_04));
		UpdateSingleSpriteVisibility(Gun05Sprite, GameStatsManager.Instance.GetFlag(GungeonFlags.SHRINE_BEHOLSTER_GUN_05));
		UpdateSingleSpriteVisibility(Gun06Sprite, GameStatsManager.Instance.GetFlag(GungeonFlags.SHRINE_BEHOLSTER_GUN_06));
	}

	private void UpdateSingleSpriteVisibility(tk2dSprite gunSprite, bool visibility)
	{
		if (gunSprite.renderer.enabled != visibility)
		{
			gunSprite.renderer.enabled = visibility;
			if ((bool)VFXStonePuff)
			{
				GameObject gameObject = SpawnManager.SpawnVFX(VFXStonePuff, gunSprite.transform.position, Quaternion.identity);
				tk2dSprite component = gameObject.GetComponent<tk2dSprite>();
				component.HeightOffGround = 10f;
				component.UpdateZDepth();
				AkSoundEngine.PostEvent("Play_OBJ_item_spawn_01", base.gameObject);
			}
		}
	}

	public string GetAnimationState(PlayerController interactor, out bool shouldBeFlipped)
	{
		shouldBeFlipped = false;
		return string.Empty;
	}

	public float GetDistanceToPoint(Vector2 point)
	{
		if (base.sprite == null)
		{
			return 100f;
		}
		Vector3 vector = BraveMathCollege.ClosestPointOnRectangle(point, base.specRigidbody.UnitBottomLeft, base.specRigidbody.UnitDimensions);
		return Vector2.Distance(point, vector) / 1.5f;
	}

	public float GetOverrideMaxDistance()
	{
		return -1f;
	}

	public void OnEnteredRange(PlayerController interactor)
	{
		if (AlternativeOutlineTarget != null)
		{
			SpriteOutlineManager.AddOutlineToSprite(AlternativeOutlineTarget, Color.white);
		}
		else
		{
			SpriteOutlineManager.AddOutlineToSprite(base.sprite, Color.white);
		}
	}

	public void OnExitRange(PlayerController interactor)
	{
		if (AlternativeOutlineTarget != null)
		{
			SpriteOutlineManager.RemoveOutlineFromSprite(AlternativeOutlineTarget);
		}
		else
		{
			SpriteOutlineManager.RemoveOutlineFromSprite(base.sprite);
		}
	}

	private bool NeedsGun(int pickupID)
	{
		if (pickupID == Gun01ID && !GameStatsManager.Instance.GetFlag(GungeonFlags.SHRINE_BEHOLSTER_GUN_01))
		{
			return true;
		}
		if (pickupID == Gun02ID && !GameStatsManager.Instance.GetFlag(GungeonFlags.SHRINE_BEHOLSTER_GUN_02))
		{
			return true;
		}
		if (pickupID == Gun03ID && !GameStatsManager.Instance.GetFlag(GungeonFlags.SHRINE_BEHOLSTER_GUN_03))
		{
			return true;
		}
		if (pickupID == Gun04ID && !GameStatsManager.Instance.GetFlag(GungeonFlags.SHRINE_BEHOLSTER_GUN_04))
		{
			return true;
		}
		if (pickupID == Gun05ID && !GameStatsManager.Instance.GetFlag(GungeonFlags.SHRINE_BEHOLSTER_GUN_05))
		{
			return true;
		}
		if (pickupID == Gun06ID && !GameStatsManager.Instance.GetFlag(GungeonFlags.SHRINE_BEHOLSTER_GUN_06))
		{
			return true;
		}
		return false;
	}

	private bool CheckCanBeUsed(PlayerController interactor)
	{
		if (!interactor || !interactor.CurrentGun)
		{
			return false;
		}
		if (m_useCount > 10)
		{
			return false;
		}
		return NeedsGun(interactor.CurrentGun.PickupObjectId);
	}

	private void SetFlagForID(int id)
	{
		if (id == Gun01ID)
		{
			GameStatsManager.Instance.SetFlag(GungeonFlags.SHRINE_BEHOLSTER_GUN_01, true);
		}
		if (id == Gun02ID)
		{
			GameStatsManager.Instance.SetFlag(GungeonFlags.SHRINE_BEHOLSTER_GUN_02, true);
		}
		if (id == Gun03ID)
		{
			GameStatsManager.Instance.SetFlag(GungeonFlags.SHRINE_BEHOLSTER_GUN_03, true);
		}
		if (id == Gun04ID)
		{
			GameStatsManager.Instance.SetFlag(GungeonFlags.SHRINE_BEHOLSTER_GUN_04, true);
		}
		if (id == Gun05ID)
		{
			GameStatsManager.Instance.SetFlag(GungeonFlags.SHRINE_BEHOLSTER_GUN_05, true);
		}
		if (id == Gun06ID)
		{
			GameStatsManager.Instance.SetFlag(GungeonFlags.SHRINE_BEHOLSTER_GUN_06, true);
		}
		UpdateSpriteVisibility();
	}

	private void DoShrineEffect(PlayerController interactor)
	{
		SetFlagForID(interactor.CurrentGun.PickupObjectId);
		int num = 0;
		if (GameStatsManager.Instance.GetFlag(GungeonFlags.SHRINE_BEHOLSTER_GUN_01))
		{
			num++;
		}
		if (GameStatsManager.Instance.GetFlag(GungeonFlags.SHRINE_BEHOLSTER_GUN_02))
		{
			num++;
		}
		if (GameStatsManager.Instance.GetFlag(GungeonFlags.SHRINE_BEHOLSTER_GUN_03))
		{
			num++;
		}
		if (GameStatsManager.Instance.GetFlag(GungeonFlags.SHRINE_BEHOLSTER_GUN_04))
		{
			num++;
		}
		if (GameStatsManager.Instance.GetFlag(GungeonFlags.SHRINE_BEHOLSTER_GUN_05))
		{
			num++;
		}
		if (GameStatsManager.Instance.GetFlag(GungeonFlags.SHRINE_BEHOLSTER_GUN_06))
		{
			num++;
		}
		if (num == 6)
		{
			LootEngine.TryGiveGunToPlayer(PickupObjectDatabase.GetById(Gun01ID).gameObject, interactor);
			LootEngine.TryGiveGunToPlayer(PickupObjectDatabase.GetById(Gun02ID).gameObject, interactor);
			LootEngine.TryGiveGunToPlayer(PickupObjectDatabase.GetById(Gun03ID).gameObject, interactor);
			LootEngine.TryGiveGunToPlayer(PickupObjectDatabase.GetById(Gun04ID).gameObject, interactor);
			LootEngine.TryGiveGunToPlayer(PickupObjectDatabase.GetById(Gun05ID).gameObject, interactor);
			LootEngine.TryGiveGunToPlayer(PickupObjectDatabase.GetById(Gun06ID).gameObject, interactor);
			StartCoroutine(HandleShrineCompletionVisuals());
			m_useCount = 100;
			interactor.inventory.GunChangeForgiveness = true;
			for (int i = 0; i < 100; i++)
			{
				Gun targetGunWithChange = interactor.inventory.GetTargetGunWithChange(i);
				if (targetGunWithChange.PickupObjectId == Gun01ID)
				{
					if (i != 0)
					{
						interactor.inventory.ChangeGun(i);
					}
					break;
				}
			}
			interactor.inventory.GunChangeForgiveness = false;
		}
		else
		{
			interactor.inventory.DestroyCurrentGun();
		}
	}

	private IEnumerator HandleShrineCompletionVisuals()
	{
		AkSoundEngine.PostEvent("Play_OBJ_shrine_accept_01", base.gameObject);
		yield return new WaitForSeconds(0.5f);
		GameStatsManager.Instance.SetFlag(GungeonFlags.SHRINE_BEHOLSTER_GUN_01, false);
		UpdateSpriteVisibility();
		yield return new WaitForSeconds(0.2f);
		GameStatsManager.Instance.SetFlag(GungeonFlags.SHRINE_BEHOLSTER_GUN_02, false);
		UpdateSpriteVisibility();
		yield return new WaitForSeconds(0.2f);
		GameStatsManager.Instance.SetFlag(GungeonFlags.SHRINE_BEHOLSTER_GUN_03, false);
		UpdateSpriteVisibility();
		yield return new WaitForSeconds(0.2f);
		GameStatsManager.Instance.SetFlag(GungeonFlags.SHRINE_BEHOLSTER_GUN_04, false);
		UpdateSpriteVisibility();
		yield return new WaitForSeconds(0.2f);
		GameStatsManager.Instance.SetFlag(GungeonFlags.SHRINE_BEHOLSTER_GUN_05, false);
		UpdateSpriteVisibility();
		yield return new WaitForSeconds(0.2f);
		GameStatsManager.Instance.SetFlag(GungeonFlags.SHRINE_BEHOLSTER_GUN_06, false);
		UpdateSpriteVisibility();
	}

	private IEnumerator HandleShrineConversation(PlayerController interactor)
	{
		TextBoxManager.ShowStoneTablet(text: StringTableManager.GetLongString(displayTextKey), worldPosition: talkPoint.position, parent: talkPoint, duration: -1f);
		int selectedResponse = -1;
		interactor.SetInputOverride("shrineConversation");
		yield return null;
		bool canUse = CheckCanBeUsed(interactor);
		if (canUse)
		{
			string @string = StringTableManager.GetString(acceptOptionKey);
			GameUIRoot.Instance.DisplayPlayerConversationOptions(interactor, null, @string, StringTableManager.GetString(declineOptionKey));
		}
		else
		{
			GameUIRoot.Instance.DisplayPlayerConversationOptions(interactor, null, StringTableManager.GetString(declineOptionKey), string.Empty);
		}
		while (!GameUIRoot.Instance.GetPlayerConversationResponse(out selectedResponse))
		{
			yield return null;
		}
		interactor.ClearInputOverride("shrineConversation");
		TextBoxManager.ClearTextBox(talkPoint);
		if (canUse && selectedResponse == 0)
		{
			DoShrineEffect(interactor);
		}
		ResetForReuse();
	}

	private void ResetForReuse()
	{
		m_useCount--;
	}

	private IEnumerator HandleSpentText(PlayerController interactor)
	{
		TextBoxManager.ShowStoneTablet(talkPoint.position, talkPoint, -1f, StringTableManager.GetLongString(spentOptionKey));
		int selectedResponse = -1;
		interactor.SetInputOverride("shrineConversation");
		GameUIRoot.Instance.DisplayPlayerConversationOptions(interactor, null, StringTableManager.GetString(declineOptionKey), string.Empty);
		while (!GameUIRoot.Instance.GetPlayerConversationResponse(out selectedResponse))
		{
			yield return null;
		}
		interactor.ClearInputOverride("shrineConversation");
		TextBoxManager.ClearTextBox(talkPoint);
	}

	public void Interact(PlayerController interactor)
	{
		if (TextBoxManager.HasTextBox(talkPoint))
		{
			return;
		}
		if (m_useCount > 0)
		{
			if (!string.IsNullOrEmpty(spentOptionKey))
			{
				StartCoroutine(HandleSpentText(interactor));
			}
		}
		else
		{
			m_useCount++;
			StartCoroutine(HandleShrineConversation(interactor));
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
