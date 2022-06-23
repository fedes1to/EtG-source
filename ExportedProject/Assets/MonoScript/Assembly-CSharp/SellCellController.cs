using System.Collections;
using Dungeonator;
using UnityEngine;

public class SellCellController : DungeonPlaceableBehaviour, IPlaceConfigurable
{
	public float SellValueModifier = 0.1f;

	public TalkDoerLite SellPitDweller;

	public GameObject SellExplosionVFX;

	public tk2dSprite CellTopSprite;

	public string ExplodedSellSpriteName;

	private bool m_isExploded;

	private int m_thingsSold;

	private int m_masteryRoundsSold;

	private bool m_currentlySellingAnItem;

	private float m_timeHovering;

	private void Start()
	{
		if ((bool)SellPitDweller && (bool)SellPitDweller.spriteAnimator)
		{
			SellPitDweller.spriteAnimator.alwaysUpdateOffscreen = true;
		}
	}

	public void AttemptSellItem(PickupObject targetItem)
	{
		if (!m_isExploded && !(targetItem == null) && targetItem.CanBeSold && !targetItem.IsBeingSold && !(targetItem is CurrencyPickup) && !(targetItem is KeyBulletPickup) && !(targetItem is HealthPickup) && base.specRigidbody.ContainsPoint(targetItem.sprite.WorldCenter, int.MaxValue, true))
		{
			StartCoroutine(HandleSoldItem(targetItem));
		}
	}

	private void HandleFlightCollider()
	{
		if (GameManager.Instance.IsLoadingLevel || !m_isExploded)
		{
			return;
		}
		for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
		{
			PlayerController playerController = GameManager.Instance.AllPlayers[i];
			if ((bool)playerController && !playerController.IsGhost && playerController.IsFlying && new Rect(base.transform.position.XY(), new Vector2(3f, 3f)).Contains(playerController.CenterPosition))
			{
				m_timeHovering += BraveTime.DeltaTime;
				if (m_timeHovering > 2f)
				{
					playerController.ForceFall();
					m_timeHovering = 0f;
				}
			}
		}
	}

	private IEnumerator HandleSellPitOpening()
	{
		if (GameManager.Instance.Dungeon.tileIndices.tilesetId != GlobalDungeonData.ValidTilesets.CATACOMBGEON)
		{
			yield break;
		}
		m_isExploded = true;
		SellPitDweller.PreventInteraction = true;
		SellPitDweller.PreventCoopInteraction = true;
		SellPitDweller.playerApproachRadius = -1f;
		yield return new WaitForSeconds(3f);
		Object.Instantiate(SellExplosionVFX, base.transform.position, Quaternion.identity);
		float elapsed = 0f;
		while (elapsed < 0.25f)
		{
			elapsed += BraveTime.DeltaTime;
			yield return null;
		}
		CellTopSprite.SetSprite(ExplodedSellSpriteName);
		for (int i = 1; i < GetWidth(); i++)
		{
			for (int j = 0; j < GetHeight(); j++)
			{
				IntVector2 intVector = base.transform.position.IntXY() + new IntVector2(i, j);
				if (GameManager.Instance.Dungeon.data.CheckInBoundsAndValid(intVector))
				{
					CellData cellData = GameManager.Instance.Dungeon.data[intVector];
					cellData.fallingPrevented = false;
				}
			}
		}
	}

	private void OnDisable()
	{
		if (!m_isExploded || !(CellTopSprite.CurrentSprite.name != ExplodedSellSpriteName))
		{
			return;
		}
		CellTopSprite.SetSprite(ExplodedSellSpriteName);
		for (int i = 1; i < GetWidth(); i++)
		{
			for (int j = 0; j < GetHeight(); j++)
			{
				IntVector2 intVector = base.transform.position.IntXY() + new IntVector2(i, j);
				if (GameManager.Instance.Dungeon.data.CheckInBoundsAndValid(intVector))
				{
					CellData cellData = GameManager.Instance.Dungeon.data[intVector];
					cellData.fallingPrevented = false;
				}
			}
		}
	}

	private IEnumerator HandleSoldItem(PickupObject targetItem)
	{
		targetItem.IsBeingSold = true;
		while (m_currentlySellingAnItem)
		{
			yield return null;
		}
		if (!targetItem || !targetItem.sprite || !base.specRigidbody.ContainsPoint(targetItem.sprite.WorldCenter, int.MaxValue, true))
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
		Vector3 finalOffset = Vector3.zero;
		tk2dBaseSprite targetSprite = targetItem.GetComponentInChildren<tk2dBaseSprite>();
		if ((bool)targetSprite)
		{
			finalOffset = targetSprite.GetBounds().extents;
		}
		while (elapsed < duration)
		{
			elapsed += BraveTime.DeltaTime;
			if (!targetItem || !targetItem.transform)
			{
				m_currentlySellingAnItem = false;
				yield break;
			}
			targetItem.transform.localScale = Vector3.Lerp(Vector3.one, new Vector3(0.01f, 0.01f, 1f), elapsed / duration);
			targetItem.transform.position = Vector3.Lerp(startPos, startPos + new Vector3(finalOffset.x, 0f, 0f), elapsed / duration);
			yield return null;
		}
		if (!targetItem || !targetItem.transform)
		{
			m_currentlySellingAnItem = false;
			yield break;
		}
		SellPitDweller.SendPlaymakerEvent("playerSoldSomething");
		int sellPrice = Mathf.Clamp(Mathf.CeilToInt((float)targetItem.PurchasePrice * SellValueModifier), 0, 200);
		if (targetItem.quality == PickupObject.ItemQuality.SPECIAL || targetItem.quality == PickupObject.ItemQuality.EXCLUDED)
		{
			sellPrice = 3;
		}
		LootEngine.SpawnCurrency(targetItem.sprite.WorldCenter, sellPrice);
		m_thingsSold++;
		if (targetItem.PickupObjectId == GlobalItemIds.MasteryToken_Castle || targetItem.PickupObjectId == GlobalItemIds.MasteryToken_Catacombs || targetItem.PickupObjectId == GlobalItemIds.MasteryToken_Gungeon || targetItem.PickupObjectId == GlobalItemIds.MasteryToken_Forge || targetItem.PickupObjectId == GlobalItemIds.MasteryToken_Mines)
		{
			m_masteryRoundsSold++;
		}
		if (targetItem is Gun && (bool)targetItem.GetComponentInParent<DebrisObject>())
		{
			Object.Destroy(targetItem.transform.parent.gameObject);
		}
		else
		{
			Object.Destroy(targetItem.gameObject);
		}
		if (m_thingsSold >= 3 && m_masteryRoundsSold > 0)
		{
			StartCoroutine(HandleSellPitOpening());
		}
		m_currentlySellingAnItem = false;
	}

	private void Update()
	{
		HandleFlightCollider();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	public void ConfigureOnPlacement(RoomHandler room)
	{
		for (int i = 1; i < GetWidth(); i++)
		{
			for (int j = 0; j < GetHeight(); j++)
			{
				IntVector2 intVector = base.transform.position.IntXY() + new IntVector2(i, j);
				if (GameManager.Instance.Dungeon.data.CheckInBoundsAndValid(intVector))
				{
					CellData cellData = GameManager.Instance.Dungeon.data[intVector];
					cellData.type = CellType.PIT;
					cellData.fallingPrevented = true;
				}
			}
		}
	}
}
