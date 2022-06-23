using System;
using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using HutongGames.PlayMaker;
using UnityEngine;

public class SecretFloorInteractableController : DungeonPlaceableBehaviour, IPlayerInteractable
{
	public bool IsResourcefulRatPit;

	public bool GoesToRatFloor;

	public string targetLevelName;

	public List<InteractableLock> WorldLocks;

	public SpeculativeRigidbody FlightCollider;

	public GlobalDungeonData.ValidTilesets TargetTileset = GlobalDungeonData.ValidTilesets.SEWERGEON;

	public TileIndexGrid OverridePitIndex;

	private bool m_hasOpened;

	public tk2dSpriteAnimator cryoAnimator;

	public string cryoArriveAnimation;

	public string cyroDepartAnimation;

	private FsmBool m_cryoBool;

	private FsmBool m_normalBool;

	private float m_timeHovering;

	private bool m_isLoading;

	public override GameObject InstantiateObject(RoomHandler targetRoom, IntVector2 loc, bool deferConfiguration = false)
	{
		if (IsResourcefulRatPit)
		{
			IntVector2 intVector = loc + targetRoom.area.basePosition;
			int num = intVector.x;
			int num2 = intVector.x + placeableWidth;
			int num3 = intVector.y;
			int num4 = intVector.y + placeableHeight;
			if (GoesToRatFloor)
			{
				num++;
				num3++;
				num2--;
				num4--;
			}
			for (int i = num; i < num2; i++)
			{
				for (int j = num3; j < num4; j++)
				{
					CellData cellData = GameManager.Instance.Dungeon.data.cellData[i][j];
					cellData.type = CellType.PIT;
					cellData.fallingPrevented = true;
					if (OverridePitIndex != null)
					{
						cellData.cellVisualData.HasTriggeredPitVFX = true;
						cellData.cellVisualData.PitVFXCooldown = float.MaxValue;
						if (j == intVector.y + placeableHeight - 1)
						{
							cellData.cellVisualData.pitOverrideIndex = OverridePitIndex.topIndices.GetIndexByWeight();
						}
						else
						{
							cellData.cellVisualData.pitOverrideIndex = OverridePitIndex.centerIndices.GetIndexByWeight();
						}
					}
				}
			}
		}
		return base.InstantiateObject(targetRoom, loc, deferConfiguration);
	}

	private void Start()
	{
		RoomHandler absoluteParentRoom = GetAbsoluteParentRoom();
		for (int i = 0; i < WorldLocks.Count; i++)
		{
			absoluteParentRoom.RegisterInteractable(WorldLocks[i]);
		}
		if (IsResourcefulRatPit)
		{
			SpeculativeRigidbody speculativeRigidbody = base.specRigidbody;
			speculativeRigidbody.OnEnterTrigger = (SpeculativeRigidbody.OnTriggerDelegate)Delegate.Combine(speculativeRigidbody.OnEnterTrigger, new SpeculativeRigidbody.OnTriggerDelegate(HandleTriggerEntered));
			SpeculativeRigidbody speculativeRigidbody2 = base.specRigidbody;
			speculativeRigidbody2.OnExitTrigger = (SpeculativeRigidbody.OnTriggerExitDelegate)Delegate.Combine(speculativeRigidbody2.OnExitTrigger, new SpeculativeRigidbody.OnTriggerExitDelegate(HandleTriggerExited));
			SpeculativeRigidbody speculativeRigidbody3 = base.specRigidbody;
			speculativeRigidbody3.OnTriggerCollision = (SpeculativeRigidbody.OnTriggerDelegate)Delegate.Combine(speculativeRigidbody3.OnTriggerCollision, new SpeculativeRigidbody.OnTriggerDelegate(HandleTriggerRemain));
		}
		if ((bool)FlightCollider)
		{
			SpeculativeRigidbody flightCollider = FlightCollider;
			flightCollider.OnTriggerCollision = (SpeculativeRigidbody.OnTriggerDelegate)Delegate.Combine(flightCollider.OnTriggerCollision, new SpeculativeRigidbody.OnTriggerDelegate(HandleFlightCollider));
		}
		TalkDoerLite componentInChildren = absoluteParentRoom.hierarchyParent.GetComponentInChildren<TalkDoerLite>();
		if ((bool)componentInChildren && componentInChildren.name.Contains("CryoButton"))
		{
			componentInChildren.OnGenericFSMActionA = (Action)Delegate.Combine(componentInChildren.OnGenericFSMActionA, new Action(SwitchToCryoElevator));
			componentInChildren.OnGenericFSMActionB = (Action)Delegate.Combine(componentInChildren.OnGenericFSMActionB, new Action(RescindCryoElevator));
			m_cryoBool = componentInChildren.playmakerFsm.FsmVariables.GetFsmBool("IS_CRYO");
			m_normalBool = componentInChildren.playmakerFsm.FsmVariables.GetFsmBool("IS_NORMAL");
			m_cryoBool.Value = false;
			m_normalBool.Value = true;
		}
	}

	private void RescindCryoElevator()
	{
		m_cryoBool.Value = false;
		m_normalBool.Value = true;
		if ((bool)cryoAnimator && !string.IsNullOrEmpty(cyroDepartAnimation))
		{
			cryoAnimator.Play(cyroDepartAnimation);
		}
	}

	private void SwitchToCryoElevator()
	{
		m_cryoBool.Value = true;
		m_normalBool.Value = false;
		if ((bool)cryoAnimator && !string.IsNullOrEmpty(cryoArriveAnimation))
		{
			cryoAnimator.Play(cryoArriveAnimation);
		}
	}

	private void HandleFlightCollider(SpeculativeRigidbody specRigidbody, SpeculativeRigidbody sourceSpecRigidbody, CollisionData collisionData)
	{
		if (GameManager.Instance.IsLoadingLevel || !IsValidForUse())
		{
			return;
		}
		PlayerController component = specRigidbody.GetComponent<PlayerController>();
		if (!component || !component.IsFlying || m_isLoading || GameManager.Instance.IsLoadingLevel || string.IsNullOrEmpty(targetLevelName))
		{
			return;
		}
		m_timeHovering += BraveTime.DeltaTime;
		if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
		{
			PlayerController otherPlayer = GameManager.Instance.GetOtherPlayer(component);
			if (component.IsFlying && !otherPlayer.IsFlying && !otherPlayer.IsGhost)
			{
				m_timeHovering = 0f;
			}
		}
		if (m_timeHovering > 0.5f)
		{
			m_isLoading = true;
			GameManager.Instance.LoadCustomLevel(targetLevelName);
		}
	}

	private void HandleTriggerRemain(SpeculativeRigidbody specRigidbody, SpeculativeRigidbody sourceSpecRigidbody, CollisionData collisionData)
	{
		if (IsValidForUse() && !m_isLoading)
		{
			PlayerController component = specRigidbody.GetComponent<PlayerController>();
			StartCoroutine(FrameDelayedTriggerRemainCheck(component));
		}
	}

	private IEnumerator FrameDelayedTriggerRemainCheck(PlayerController targetPlayer)
	{
		yield return null;
		if ((bool)targetPlayer && (targetPlayer.IsFalling || targetPlayer.IsFlying) && m_cryoBool != null && m_cryoBool.Value)
		{
			m_isLoading = true;
			Pixelator.Instance.FadeToBlack(1f);
			GameUIRoot.Instance.HideCoreUI(string.Empty);
			GameUIRoot.Instance.ToggleLowerPanels(false, false, string.Empty);
			AkSoundEngine.PostEvent("Stop_MUS_All", base.gameObject);
			GameManager.DoMidgameSave(TargetTileset);
			float delay = 1.5f;
			targetPlayer.specRigidbody.Velocity = Vector2.zero;
			targetPlayer.LevelToLoadOnPitfall = "midgamesave";
			GameManager.Instance.DelayedLoadCharacterSelect(delay, true, true);
		}
	}

	private void HandleTriggerEntered(SpeculativeRigidbody specRigidbody, SpeculativeRigidbody sourceSpecRigidbody, CollisionData collisionData)
	{
		PlayerController component = specRigidbody.GetComponent<PlayerController>();
		if ((bool)component)
		{
			if (m_cryoBool != null && m_cryoBool.Value)
			{
				component.LevelToLoadOnPitfall = "midgamesave";
			}
			else
			{
				component.LevelToLoadOnPitfall = targetLevelName;
			}
		}
	}

	private void HandleTriggerExited(SpeculativeRigidbody specRigidbody, SpeculativeRigidbody sourceSpecRigidbody)
	{
		PlayerController component = specRigidbody.GetComponent<PlayerController>();
		if ((bool)component)
		{
			component.LevelToLoadOnPitfall = string.Empty;
		}
	}

	private void Update()
	{
		if (m_hasOpened || !IsResourcefulRatPit || !IsValidForUse())
		{
			return;
		}
		m_hasOpened = true;
		base.spriteAnimator.Play();
		IntVector2 intVector = base.transform.position.IntXY(VectorConversions.Floor);
		int num = intVector.x;
		int num2 = intVector.x + placeableWidth;
		int num3 = intVector.y;
		int num4 = intVector.y + placeableHeight;
		if (GoesToRatFloor)
		{
			num++;
			num3++;
			num2--;
			num4--;
		}
		for (int i = num; i < num2; i++)
		{
			for (int j = num3; j < num4; j++)
			{
				if (!GoesToRatFloor || ((i != intVector.x + 1 || j != intVector.y + 1) && (i != intVector.x + 1 || j != intVector.y + placeableHeight - 2) && (i != intVector.x + placeableWidth - 2 || j != intVector.y + 1) && (i != intVector.x + placeableWidth - 2 || j != intVector.y + placeableHeight - 2)))
				{
					CellData cellData = GameManager.Instance.Dungeon.data.cellData[i][j];
					cellData.fallingPrevented = false;
				}
			}
		}
	}

	private bool IsValidForUse()
	{
		bool result = true;
		for (int i = 0; i < WorldLocks.Count; i++)
		{
			if (WorldLocks[i].IsLocked || WorldLocks[i].spriteAnimator.IsPlaying(WorldLocks[i].spriteAnimator.CurrentClip))
			{
				result = false;
			}
		}
		return result;
	}

	public void OnEnteredRange(PlayerController interactor)
	{
		if ((bool)this)
		{
			SpriteOutlineManager.AddOutlineToSprite(base.sprite, Color.white, 0.1f);
			base.sprite.UpdateZDepth();
		}
	}

	public void OnExitRange(PlayerController interactor)
	{
		if ((bool)this)
		{
			SpriteOutlineManager.RemoveOutlineFromSprite(base.sprite);
			base.sprite.UpdateZDepth();
		}
	}

	public float GetDistanceToPoint(Vector2 point)
	{
		if (IsResourcefulRatPit)
		{
			return 1000f;
		}
		if (!IsValidForUse())
		{
			return 1000f;
		}
		Bounds bounds = base.sprite.GetBounds();
		bounds.SetMinMax(bounds.min + base.transform.position, bounds.max + base.transform.position);
		float num = Mathf.Max(Mathf.Min(point.x, bounds.max.x), bounds.min.x);
		float num2 = Mathf.Max(Mathf.Min(point.y, bounds.max.y), bounds.min.y);
		return Mathf.Sqrt((point.x - num) * (point.x - num) + (point.y - num2) * (point.y - num2));
	}

	public float GetOverrideMaxDistance()
	{
		return -1f;
	}

	public void Interact(PlayerController player)
	{
		if (IsValidForUse())
		{
			GameManager.Instance.LoadCustomLevel(targetLevelName);
		}
	}

	public string GetAnimationState(PlayerController interactor, out bool shouldBeFlipped)
	{
		shouldBeFlipped = false;
		return string.Empty;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
