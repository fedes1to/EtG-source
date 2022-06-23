using System;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class ShopController : DungeonPlaceableBehaviour, IPlaceConfigurable
{
	[Header("Spawn Group 1")]
	public GenericLootTable shopItems;

	public Transform[] spawnPositions;

	[Header("Spawn Group 2")]
	public GenericLootTable shopItemsGroup2;

	public Transform[] spawnPositionsGroup2;

	[Header("Other Settings")]
	public tk2dSpriteAnimator speakerAnimator;

	public Transform speechPoint;

	public float ItemHeightOffGroundModifier;

	public GameObject shopItemShadowPrefab;

	protected List<GameObject> m_shopItems;

	protected List<ShopItemController> m_itemControllers;

	protected bool firstTime = true;

	[NonSerialized]
	public int StolenCount;

	protected string defaultTalkAction = "talk";

	protected string defaultIdleAction = "idle";

	protected RoomHandler m_room;

	public RoomHandler Room
	{
		get
		{
			return m_room;
		}
	}

	protected virtual void Start()
	{
		DoSetup();
	}

	public virtual void OnRoomEnter(PlayerController p)
	{
		if (!p.IsStealthed)
		{
			if (firstTime)
			{
				firstTime = false;
				OnInitialRoomEnter();
			}
			else
			{
				OnSequentialRoomEnter();
			}
		}
	}

	public virtual void OnRoomExit()
	{
		if (!GameManager.Instance.PrimaryPlayer || !GameManager.Instance.PrimaryPlayer.IsStealthed)
		{
			GunsmithTalk(StringTableManager.GetString("#SHOP_EXIT"));
		}
	}

	protected virtual void OnInitialRoomEnter()
	{
		GunsmithTalk(StringTableManager.GetString("#SHOP_ENTER"));
	}

	protected virtual void OnSequentialRoomEnter()
	{
		GunsmithTalk(StringTableManager.GetString("#SHOP_REENTER"));
	}

	protected virtual void GunsmithTalk(string message)
	{
		TextBoxManager.ShowTextBox(speechPoint.position, speechPoint, 5f, message, "shopkeep", false);
		speakerAnimator.PlayForDuration(defaultTalkAction, 2.5f, defaultIdleAction);
	}

	public virtual void OnBetrayalWarning()
	{
		speakerAnimator.PlayForDuration("scold", 1f, defaultIdleAction);
		TextBoxManager.ShowTextBox(speechPoint.position, speechPoint, 5f, StringTableManager.GetString("#SHOPKEEP_BETRAYAL_WARNING"), "shopkeep", false);
	}

	public virtual void PullGun()
	{
		speakerAnimator.Play("gun");
		defaultIdleAction = "gun_idle";
		defaultTalkAction = "gun_talk";
		TextBoxManager.ShowTextBox(speechPoint.position, speechPoint, 5f, StringTableManager.GetString("#SHOPKEEP_ANGRYTOWN"), "shopkeep", false);
		for (int i = 0; i < m_itemControllers.Count; i++)
		{
			m_itemControllers[i].CurrentPrice *= 2;
		}
		TalkDoer component = speakerAnimator.GetComponent<TalkDoer>();
		component.modules[0].stringKeys[0] = "#SHOPKEEP_ANGRY_CHAT";
		component.modules[0].usesAnimation = true;
		component.modules[0].animationDuration = 2.5f;
		component.modules[0].animationName = "gun_talk";
		component.defaultSpeechAnimName = "gun_talk";
		component.fallbackAnimName = "gun_idle";
	}

	public virtual void NotifyFailedPurchase(ShopItemController itemController)
	{
		TextBoxManager.ShowTextBox(speechPoint.position, speechPoint, 5f, StringTableManager.GetString("#SHOP_NOMONEY"), "shopkeep", false);
		if (defaultIdleAction == "idle")
		{
			speakerAnimator.PlayForDuration("shake", 2.5f, defaultIdleAction);
		}
		else
		{
			speakerAnimator.PlayForDuration("scold", 1f, defaultIdleAction);
		}
	}

	public virtual void ReturnToIdle(tk2dSpriteAnimator animator, tk2dSpriteAnimationClip clip)
	{
		animator.Play(defaultIdleAction);
		animator.AnimationCompleted = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>)Delegate.Remove(animator.AnimationCompleted, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>(ReturnToIdle));
	}

	public virtual void PurchaseItem(ShopItemController item, bool actualPurchase = true, bool allowSign = true)
	{
		if (actualPurchase)
		{
			if (defaultIdleAction == "gun_idle")
			{
				GunsmithTalk(StringTableManager.GetString("#SHOPKEEP_PURCHASE_ANGRY"));
			}
			else
			{
				GunsmithTalk(StringTableManager.GetString("#SHOP_PURCHASE"));
				speakerAnimator.PlayForDuration("nod", 1.5f, defaultIdleAction);
			}
		}
		if (!item.item.PersistsOnPurchase)
		{
			m_room.DeregisterInteractable(item);
			if (allowSign)
			{
				GameObject gameObject = (GameObject)UnityEngine.Object.Instantiate(BraveResources.Load("Global Prefabs/Sign_SoldOut"));
				gameObject.GetComponent<tk2dBaseSprite>().PlaceAtPositionByAnchor(item.sprite.WorldCenter, tk2dBaseSprite.Anchor.MiddleCenter);
				gameObject.transform.position = gameObject.transform.position.Quantize(0.0625f);
			}
		}
		UnityEngine.Object.Destroy(item.gameObject);
	}

	public virtual void ConfigureOnPlacement(RoomHandler room)
	{
		room.IsShop = true;
		m_room = room;
	}

	protected virtual void DoSetup()
	{
		m_shopItems = new List<GameObject>();
		m_room.Entered += OnRoomEnter;
		m_room.Exited += OnRoomExit;
		Func<GameObject, float, float> weightModifier = null;
		if (SecretHandshakeItem.NumActive > 0)
		{
			weightModifier = delegate(GameObject prefabObject, float sourceWeight)
			{
				PickupObject component6 = prefabObject.GetComponent<PickupObject>();
				float num2 = sourceWeight;
				if (component6 != null)
				{
					int quality = (int)component6.quality;
					num2 *= 1f + (float)quality / 10f;
				}
				return num2;
			};
		}
		for (int i = 0; i < spawnPositions.Length; i++)
		{
			GameObject item = shopItems.SelectByWeightWithoutDuplicatesFullPrereqs(m_shopItems, weightModifier);
			m_shopItems.Add(item);
		}
		m_itemControllers = new List<ShopItemController>();
		for (int j = 0; j < spawnPositions.Length; j++)
		{
			Transform transform = spawnPositions[j];
			if (m_shopItems[j] == null)
			{
				continue;
			}
			PickupObject component = m_shopItems[j].GetComponent<PickupObject>();
			if (!(component == null))
			{
				GameObject gameObject = new GameObject("Shop item " + j);
				Transform transform2 = gameObject.transform;
				transform2.parent = transform;
				transform2.localPosition = Vector3.zero;
				EncounterTrackable component2 = component.GetComponent<EncounterTrackable>();
				if (component2 != null)
				{
					GameManager.Instance.ExtantShopTrackableGuids.Add(component2.EncounterGuid);
				}
				ShopItemController shopItemController = gameObject.AddComponent<ShopItemController>();
				if (transform.name.Contains("SIDE") || transform.name.Contains("EAST"))
				{
					shopItemController.itemFacing = DungeonData.Direction.EAST;
				}
				else if (transform.name.Contains("WEST"))
				{
					shopItemController.itemFacing = DungeonData.Direction.WEST;
				}
				else if (transform.name.Contains("NORTH"))
				{
					shopItemController.itemFacing = DungeonData.Direction.NORTH;
				}
				if (!m_room.IsRegistered(shopItemController))
				{
					m_room.RegisterInteractable(shopItemController);
				}
				shopItemController.Initialize(component, this);
				m_itemControllers.Add(shopItemController);
			}
		}
		if (shopItemsGroup2 != null && spawnPositionsGroup2.Length > 0)
		{
			int count = m_shopItems.Count;
			for (int k = 0; k < spawnPositionsGroup2.Length; k++)
			{
				float num = 1f - (float)k * 0.25f;
				if (UnityEngine.Random.value < num)
				{
					GameObject rewardObjectShopStyle = GameManager.Instance.RewardManager.GetRewardObjectShopStyle(GameManager.Instance.PrimaryPlayer, false, false, m_shopItems);
					m_shopItems.Add(rewardObjectShopStyle);
				}
				else
				{
					m_shopItems.Add(null);
				}
			}
			for (int l = 0; l < spawnPositionsGroup2.Length; l++)
			{
				Transform transform3 = spawnPositionsGroup2[l];
				if (m_shopItems[count + l] == null)
				{
					continue;
				}
				PickupObject component3 = m_shopItems[count + l].GetComponent<PickupObject>();
				if (!(component3 == null))
				{
					GameObject gameObject2 = new GameObject("Shop 2 item " + l);
					Transform transform4 = gameObject2.transform;
					transform4.parent = transform3;
					transform4.localPosition = Vector3.zero;
					EncounterTrackable component4 = component3.GetComponent<EncounterTrackable>();
					if (component4 != null)
					{
						GameManager.Instance.ExtantShopTrackableGuids.Add(component4.EncounterGuid);
					}
					ShopItemController shopItemController2 = gameObject2.AddComponent<ShopItemController>();
					if (transform3.name.Contains("SIDE") || transform3.name.Contains("EAST"))
					{
						shopItemController2.itemFacing = DungeonData.Direction.EAST;
					}
					else if (transform3.name.Contains("WEST"))
					{
						shopItemController2.itemFacing = DungeonData.Direction.WEST;
					}
					else if (transform3.name.Contains("NORTH"))
					{
						shopItemController2.itemFacing = DungeonData.Direction.NORTH;
					}
					if (!m_room.IsRegistered(shopItemController2))
					{
						m_room.RegisterInteractable(shopItemController2);
					}
					shopItemController2.Initialize(component3, this);
					m_itemControllers.Add(shopItemController2);
				}
			}
		}
		List<ShopSubsidiaryZone> componentsInRoom = m_room.GetComponentsInRoom<ShopSubsidiaryZone>();
		for (int m = 0; m < componentsInRoom.Count; m++)
		{
			componentsInRoom[m].HandleSetup(this, m_room, m_shopItems, m_itemControllers);
		}
		TalkDoer component5 = speakerAnimator.GetComponent<TalkDoer>();
		if (component5 != null)
		{
			component5.usesCustomBetrayalLogic = true;
			component5.OnBetrayalWarning = (Action)Delegate.Combine(component5.OnBetrayalWarning, new Action(OnBetrayalWarning));
			component5.OnBetrayal = (Action)Delegate.Combine(component5.OnBetrayal, new Action(PullGun));
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
